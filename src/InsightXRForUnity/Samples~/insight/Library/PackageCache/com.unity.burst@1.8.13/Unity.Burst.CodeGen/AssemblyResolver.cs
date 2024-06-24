using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;

namespace zzzUnity.Burst.CodeGen
{
    /// <summary>
    /// Provides an assembly resolver with deferred loading and a custom metadata resolver.
    /// </summary>
    /// <remarks>
    /// This class is not thread safe. It needs to be protected outside.
    /// </remarks>
#if BURST_COMPILER_SHARED
    public
#else
    internal
#endif
    class AssemblyResolver : BaseAssemblyResolver
    {
        private readonly ReadingMode _readingMode;

        public AssemblyResolver(ReadingMode readingMode = ReadingMode.Deferred)
        {
            _readingMode = readingMode;

            // We remove all setup by Cecil by default (it adds '.' and 'bin')
            ClearSearchDirectories();

            LoadDebugSymbols = false;       // We don't bother loading the symbols by default now, since we use SRM to handle symbols in a more thread safe manner
                                            // this is to maintain compatibility with the patch-assemblies path (see BclApp.cs), used by dots runtime
        }

        public bool LoadDebugSymbols { get; set; }

        protected void ClearSearchDirectories()
        {
            foreach (var dir in GetSearchDirectories())
            {
                RemoveSearchDirectory(dir);
            }
        }

        public AssemblyDefinition LoadFromFile(string path)
        {
            return AssemblyDefinition.ReadAssembly(path, CreateReaderParameters());
        }

        public AssemblyDefinition LoadFromStream(Stream peStream, Stream pdbStream = null, ISymbolReaderProvider customSymbolReader=null)
        {
            peStream.Position = 0;
            if (pdbStream != null)
            {
                pdbStream.Position = 0;
            }
            var readerParameters = CreateReaderParameters();
            if (customSymbolReader != null)
            {
                readerParameters.ReadSymbols = true;
                readerParameters.SymbolReaderProvider = customSymbolReader;
            }
            try
            {
                readerParameters.SymbolStream = pdbStream;
                return AssemblyDefinition.ReadAssembly(peStream, readerParameters);
            }
            catch
            {
                readerParameters.ReadSymbols = false;
                readerParameters.SymbolStream = null;
                peStream.Position = 0;
                if (pdbStream != null)
                {
                    pdbStream.Position = 0;
                }
                return AssemblyDefinition.ReadAssembly(peStream, readerParameters);
            }
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            var readerParameters = CreateReaderParameters();
            AssemblyDefinition assemblyDefinition;

            try
            {
                assemblyDefinition = Resolve(name, readerParameters);
            }
            catch (Exception ex)
            {
                if (readerParameters.ReadSymbols == true)
                {
                    // Attempt to load without symbols
                    readerParameters.ReadSymbols = false;
                    assemblyDefinition = Resolve(name, readerParameters);
                }
                else
                {
                    throw new AssemblyResolutionException(
                        name,
                        new Exception($"Failed to resolve assembly '{name}' in directories: {string.Join(Environment.NewLine, GetSearchDirectories())}", ex));
                }
            }

            return assemblyDefinition;
        }

        public bool TryResolve(AssemblyNameReference name, out AssemblyDefinition assembly)
        {
            try
            {
                assembly = Resolve(name);
                return true;
            }
            catch (AssemblyResolutionException)
            {
                assembly = null;
                return false;
            }
        }

        public new void AddSearchDirectory(string directory)
        {
            if (!GetSearchDirectories().Contains(directory))
            {
                base.AddSearchDirectory(directory);
            }
        }

        private ReaderParameters CreateReaderParameters()
        {
            var readerParams = new ReaderParameters
            {
                InMemory = true,
                AssemblyResolver = this,
                MetadataResolver =  new CustomMetadataResolver(this),
                ReadSymbols = LoadDebugSymbols     // We no longer use cecil to read symbol information, prefering SRM thread safe methods, so I`m being explicit here in case the default changes
            };

            if (LoadDebugSymbols)
            {
                readerParams.SymbolReaderProvider = new CustomSymbolReaderProvider(null);
            }

            readerParams.ReadingMode = _readingMode;

            return readerParams;
        }

        internal static string NormalizeFilePath(string path)
        {
            try
            {
                return Path.GetFullPath(new Uri(path).LocalPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not normalize file path: {path}", ex);
            }
        }

        private class CustomMetadataResolver : MetadataResolver
        {
            public CustomMetadataResolver(IAssemblyResolver assemblyResolver) : base(assemblyResolver)
            {
            }

            public override MethodDefinition Resolve(MethodReference method)
            {
                if (method is MethodDefinition methodDef)
                {
                    return methodDef;
                }

                if (method.GetElementMethod() is MethodDefinition methodDef2)
                {
                    return methodDef2;
                }

                return base.Resolve(method);
            }
        }

        /// <summary>
        /// Custom implementation of <see cref="ISymbolReaderProvider"/> to:
        /// - to load pdb/mdb through a MemoryStream to avoid locking the file on the disk
        /// - catch any exceptions while loading the symbols and report them back
        /// </summary>
        private class CustomSymbolReaderProvider : ISymbolReaderProvider
        {
            private readonly Action<string, Exception> _logException;

            public CustomSymbolReaderProvider(Action<string, Exception> logException)
            {
                _logException = logException;
            }

            public ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName)
            {
                if (string.IsNullOrWhiteSpace(fileName)) return null;

                string pdbFileName = fileName;
                try
                {
                    fileName = NormalizeFilePath(fileName);
                    pdbFileName = GetPdbFileName(fileName);

                    if (File.Exists(pdbFileName))
                    {
                        var pdbStream = ReadToMemoryStream(pdbFileName);
                        if (IsPortablePdb(pdbStream))
                            return new SafeDebugReaderProvider(new PortablePdbReaderProvider().GetSymbolReader(module, pdbStream));

                        return new SafeDebugReaderProvider(new NativePdbReaderProvider().GetSymbolReader(module, pdbStream));
                    }
                }
                catch (Exception ex) when (_logException != null)
                {
                    _logException?.Invoke($"Unable to load symbol `{pdbFileName}`", ex);
                    return null;
                }
                return null;
            }

            private static MemoryStream ReadToMemoryStream(string filename)
            {
                return new MemoryStream(File.ReadAllBytes(filename));
            }

            public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
            {
                throw new NotSupportedException();
            }

            private static string GetPdbFileName(string assemblyFileName)
            {
                return Path.ChangeExtension(assemblyFileName, ".pdb");
            }

            private static bool IsPortablePdb(Stream stream)
            {
                if (stream.Length < 4L)
                    return false;
                long position = stream.Position;
                try
                {
                    return (int)new BinaryReader(stream).ReadUInt32() == 1112167234;
                }
                finally
                {
                    stream.Position = position;
                }
            }

            /// <summary>
            /// This class is a wrapper around <see cref="ISymbolReader"/> to protect
            /// against failure while trying to read debug information in Mono.Cecil
            /// </summary>
            private class SafeDebugReaderProvider : ISymbolReader
            {
                private readonly ISymbolReader _reader;

                public SafeDebugReaderProvider(ISymbolReader reader)
                {
                    _reader = reader;
                }


                public void Dispose()
                {
                    try
                    {
                        _reader.Dispose();
                    }
                    catch
                    {
                        // ignored
                    }
                }

                public ISymbolWriterProvider GetWriterProvider()
                {
                    // We are not protecting here as we are not suppose to write to PDBs
                    return _reader.GetWriterProvider();
                }

                public bool ProcessDebugHeader(ImageDebugHeader header)
                {
                    try
                    {
                        return _reader.ProcessDebugHeader(header);
                    }
                    catch
                    {
                        // ignored
                    }

                    return false;
                }

                public MethodDebugInformation Read(MethodDefinition method)
                {
                    try
                    {
                        return _reader.Read(method);
                    }
                    catch
                    {
                        // ignored
                    }
                    return null;
                }
            }
        }
    }
}
