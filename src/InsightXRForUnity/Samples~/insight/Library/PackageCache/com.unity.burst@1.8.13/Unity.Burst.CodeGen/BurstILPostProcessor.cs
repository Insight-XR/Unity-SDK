using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;

// TODO: Once DOTS has the latest 2022.2 editor merged into their fork, this can be UNITY_2022_2_OR_NEWER!
#if UNITY_2023_1_OR_NEWER
using ILPostProcessorToUse = zzzUnity.Burst.CodeGen.ILPostProcessing;
#else
using ILPostProcessorToUse = zzzUnity.Burst.CodeGen.ILPostProcessingLegacy;
#endif

/// Deliberately named zzzUnity.Burst.CodeGen, as we need to ensure its last in the chain
namespace zzzUnity.Burst.CodeGen
{
    /// <summary>
    /// Postprocessor used to replace calls from C# to [BurstCompile] functions to direct calls to
    /// Burst native code functions without having to go through a C# delegate.
    /// </summary>
    internal class BurstILPostProcessor : ILPostProcessor
    {
        private sealed class CachedAssemblyResolver : AssemblyResolver
        {
            private Dictionary<string, AssemblyDefinition> _cache = new Dictionary<string, AssemblyDefinition>();
            private Dictionary<string, string> _knownLocations = new Dictionary<string, string>();

            public void RegisterKnownLocation(string path)
            {
                var k = Path.GetFileNameWithoutExtension(path);
                // If an assembly is referenced multiple times, resolve to the first one
                if (!_knownLocations.ContainsKey(k))
                {
                    _knownLocations.Add(k, path);
                }
            }

            public override AssemblyDefinition Resolve(AssemblyNameReference name)
            {
                if (!_cache.TryGetValue(name.FullName, out var definition))
                {
                    if (_knownLocations.TryGetValue(name.Name, out var path))
                    {
                        definition = LoadFromFile(path);
                    }
                    else
                    {
                        definition = base.Resolve(name);
                    }

                    _cache.Add(name.FullName, definition);
                }

                return definition;
            }
        }

        public bool IsDebugging;
        public int DebuggingLevel;

        private void SetupDebugging()
        {
            // This can be setup to get more diagnostics
            var debuggingStr = Environment.GetEnvironmentVariable("UNITY_BURST_DEBUG");
            IsDebugging = debuggingStr != null;
            if (IsDebugging)
            {
                Log("[com.unity.burst] Extra debugging is turned on.");
                int debuggingLevel;
                int.TryParse(debuggingStr, out debuggingLevel);
                if (debuggingLevel <= 0) debuggingLevel = 1;
                DebuggingLevel = debuggingLevel;
            }
        }

        private static SequencePoint FindBestSequencePointFor(MethodDefinition method, Instruction instruction)
        {
            var sequencePoints = method.DebugInformation?.GetSequencePointMapping().Values.OrderBy(s => s.Offset).ToList();
            if (sequencePoints == null || !sequencePoints.Any())
                return null;

            for (int i = 0; i != sequencePoints.Count-1; i++)
            {
                if (sequencePoints[i].Offset < instruction.Offset &&
                    sequencePoints[i + 1].Offset > instruction.Offset)
                    return sequencePoints[i];
            }

            return sequencePoints.FirstOrDefault();
        }

        private static DiagnosticMessage MakeDiagnosticError(MethodDefinition method, Instruction errorLocation, string message)
        {
            var m = new DiagnosticMessage { DiagnosticType = DiagnosticType.Error };
            var sPoint = errorLocation != null ? FindBestSequencePointFor(method, errorLocation) : null;
            if (sPoint!=null)
            {
                m.Column = sPoint.StartColumn;
                m.Line = sPoint.StartLine;
                m.File = sPoint.Document.Url;
            }
            m.MessageData = message;
            return m;
        }

        public override unsafe ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            var diagnostics = new List<DiagnosticMessage>();
            if (!WillProcess(compiledAssembly))
                return new ILPostProcessResult(null, diagnostics);

            bool wasModified = false;
            SetupDebugging();
            bool debugging = IsDebugging && DebuggingLevel >= 2;

            var inMemoryAssembly = compiledAssembly.InMemoryAssembly;


            var peData = inMemoryAssembly.PeData;
            var pdbData = inMemoryAssembly.PdbData;


            var loader = new CachedAssemblyResolver();
            var folders = new HashSet<string>();
            var isForEditor = compiledAssembly.Defines?.Contains("UNITY_EDITOR") ?? false;
            foreach (var reference in compiledAssembly.References)
            {
                loader.RegisterKnownLocation(reference);
                folders.Add(Path.Combine(Environment.CurrentDirectory, Path.GetDirectoryName(reference)));
            }
            var folderList = folders.OrderBy(x => x).ToList();
            foreach (var folder in folderList)
            {
                loader.AddSearchDirectory(folder);
            }

            var clock = Stopwatch.StartNew();
            if (debugging)
            {
                Log($"Start processing assembly {compiledAssembly.Name}, IsForEditor: {isForEditor}, Folders: {string.Join("\n", folderList)}");
            }

            var ilPostProcessing = new ILPostProcessorToUse(loader, isForEditor,
                (m,i,s) => { diagnostics.Add(MakeDiagnosticError(m, i, s)); },
                IsDebugging ? Log : (LogDelegate)null, DebuggingLevel);
            var functionPointerProcessing = new FunctionPointerInvokeTransform(loader,
                (m,i,s) => { diagnostics.Add(MakeDiagnosticError(m, i, s)); },
                IsDebugging ? Log : (LogDelegate)null, DebuggingLevel);
            try
            {
                // For IL Post Processing, use the builtin symbol reader provider
                var assemblyDefinition = loader.LoadFromStream(new MemoryStream(peData), new MemoryStream(pdbData), new PortablePdbReaderProvider() );
                wasModified |= ilPostProcessing.Run(assemblyDefinition);
                wasModified |= functionPointerProcessing.Run(assemblyDefinition);
                if (wasModified)
                {
                    var peStream = new MemoryStream();
                    var pdbStream = new MemoryStream();
                    var writeParameters = new WriterParameters
                    {
                        SymbolWriterProvider = new PortablePdbWriterProvider(),
                        WriteSymbols = true,
                        SymbolStream = pdbStream
                    };

                    assemblyDefinition.Write(peStream, writeParameters);
                    peStream.Flush();
                    pdbStream.Flush();

                    peData = peStream.ToArray();
                    pdbData = pdbStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Internal compiler error for Burst ILPostProcessor on {compiledAssembly.Name}. Exception: {ex}");
            }

            if (debugging)
            {
                Log($"End processing assembly {compiledAssembly.Name} in {clock.Elapsed.TotalMilliseconds}ms.");
            }

            if (wasModified && !diagnostics.Any(d => d.DiagnosticType == DiagnosticType.Error))
            {
                return new ILPostProcessResult(new InMemoryAssembly(peData, pdbData), diagnostics);
            }
            return new ILPostProcessResult(null, diagnostics);
        }

        private static void Log(string message)
        {
            Console.WriteLine($"{nameof(BurstILPostProcessor)}: {message}");
        }

        public override ILPostProcessor GetInstance()
        {
            return this;
        }

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            return compiledAssembly.References.Any(f => Path.GetFileName(f) == "Unity.Burst.dll");
        }
    }
}
