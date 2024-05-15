#if UNITY_EDITOR && ENABLE_BURST_AOT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEditor.Scripting;
using UnityEditor.Scripting.ScriptCompilation;
using UnityEditor.Scripting.Compilers;
using UnityEditor.UnityLinker;
using UnityEditor.Utils;
using UnityEngine;
using CompilerMessageType = UnityEditor.Scripting.Compilers.CompilerMessageType;
using Debug = UnityEngine.Debug;
using System.Runtime.InteropServices;

#if UNITY_EDITOR_OSX
using System.ComponentModel;
using Unity.Burst.LowLevel;
using UnityEditor.Callbacks;
#endif

namespace Unity.Burst.Editor
{
    using static BurstCompilerOptions;

    internal class TargetCpus
    {
        public List<BurstTargetCpu> Cpus;

        public TargetCpus()
        {
            Cpus = new List<BurstTargetCpu>();
        }

        public TargetCpus(BurstTargetCpu single)
        {
            Cpus = new List<BurstTargetCpu>(1)
            {
                single
            };
        }

        public bool IsX86()
        {
            foreach (var cpu in Cpus)
            {
                switch (cpu)
                {
                    case BurstTargetCpu.X86_SSE2:
                    case BurstTargetCpu.X86_SSE4:
                        return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            var result = "";

            var first = true;

            foreach (var cpu in Cpus)
            {
                if (first)
                {
                    result += $"{cpu}";
                    first = false;
                }
                else
                {
                    result += $", {cpu}";
                }
            }

            return result;
        }

        public TargetCpus Clone()
        {
            var copy = new TargetCpus
            {
                Cpus = new List<BurstTargetCpu>(Cpus.Count)
            };

            foreach (var cpu in Cpus)
            {
                copy.Cpus.Add(cpu);
            }

            return copy;
        }
    }

    #if !ENABLE_GENERATE_NATIVE_PLUGINS_FOR_ASSEMBLIES_API
    internal class LinkXMLGenerator : IUnityLinkerProcessor
    {
        public int callbackOrder => 1;
        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            var linkXml = Path.GetFullPath(Path.Combine("Temp", BurstAotCompiler.BurstLinkXmlName));

            return linkXml;
        }

        public void OnBeforeRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {
        }

        public void OnAfterRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {
        }
    }
    #endif

#if ENABLE_GENERATE_NATIVE_PLUGINS_FOR_ASSEMBLIES_API
    internal class BurstAOTCompilerPostprocessor : IGenerateNativePluginsForAssemblies
#else
    internal class BurstAOTCompilerPostprocessor : IPostBuildPlayerScriptDLLs
#endif
    {
        public int callbackOrder => 0;

#if ENABLE_GENERATE_NATIVE_PLUGINS_FOR_ASSEMBLIES_API
        public IGenerateNativePluginsForAssemblies.PrepareResult PrepareOnMainThread(IGenerateNativePluginsForAssemblies.PrepareArgs args)
        {
            if (ForceDisableBurstCompilation)
                return new();
            DoSetup(args.report);
            var target = BurstPlatformAotSettings.ResolveTarget(settings.summary.platform);
            return new()
            {
                additionalInputFiles = new[]
                {
                    // Any files in this list will be scanned for changes, and any changes in these files will trigger
                    // a rerun of the Burst compiler on the player build (even if the script assemblies have not changed).
                    //
                    // We add the settings so that changing any Burst setting will trigger a rebuild.
                    BurstPlatformAotSettings.GetPath(target),

                    // Like above, but specifically for settings unique to player-builds (like SDK versions and like)
                    // Those settings are extracted in `DoSetup`
                    BurstAotCompiler.BurstAOTSettings.GetPath(target),

                    // We don't want to scan every file in the Burst package (though every file could potentially change
                    // behavior). When working on Burst code locally, you may need to select "Clean Build" in the Build
                    // settings window to force a rebuild to pick up the changes.
                    //
                    // But we add the compiler executable to have at least on file in the package. This should be good
                    // enough for users. Because any change in Burst will come with a change of the Burst package
                    // version, which will change the pathname for this file (which will then trigger a rebuild, even
                    // if the contents have not changed).
                    BurstLoader.BclConfiguration.ExecutablePath,
                },
                displayName = "Running Burst Compiler"
            };
        }

        public IGenerateNativePluginsForAssemblies.GenerateResult GenerateNativePluginsForAssemblies(IGenerateNativePluginsForAssemblies.GenerateArgs args)
        {
            if (ForceDisableBurstCompilation)
                return new ();
            if (Directory.Exists(BurstAotCompiler.OutputBaseFolder))
                Directory.Delete(BurstAotCompiler.OutputBaseFolder, true);
            var assemblies = args.assemblyFiles.Select(path => new Assembly(
                Path.GetFileNameWithoutExtension(path),
                path,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<Assembly>(),
                Array.Empty<string>(),
                UnityEditor.Compilation.AssemblyFlags.None))
                // We don't run Burst on UnityEngine assemblies, so we skip them to save time
                .Where(a => !a.name.StartsWith("UnityEngine."))
                .ToArray();
            return new () { generatedPlugins = DoGenerate(assemblies).ToArray() };
        }
#else
        public void OnPostBuildPlayerScriptDLLs(BuildReport report)
        {
            if (ForceDisableBurstCompilation)
            {
                return;
            }

            var step = report.BeginBuildStep("burst");
            try
            {
                DoSetup(report);

                DoGenerate(BurstAotCompiler.GetPlayerAssemblies(report))
                    .ToList(); // Force enumeration
            }
            finally
            {
                report.EndBuildStep(step);
            }
        }
#endif

        private BurstAotCompiler.BurstAOTSettings settings;

        public void DoSetup(BuildReport report)
        {
            settings = new BurstAotCompiler.BurstAOTSettings()
            {
                summary = report.summary,
                productName = PlayerSettings.productName
            };
            settings.aotSettingsForTarget = BurstPlatformAotSettings.GetOrCreateSettings(settings.summary.platform);
            settings.isSupported = BurstAotCompiler.IsSupportedPlatform(settings.summary.platform, settings.aotSettingsForTarget);
            if (settings.isSupported)
            {
                settings.targetPlatform = BurstAotCompiler.GetTargetPlatformAndDefaultCpu(settings.summary.platform,
                    out settings.targetCpus, settings.aotSettingsForTarget);
                settings.combinations =
                    BurstAotCompiler.CollectCombinations(settings.targetPlatform, settings.targetCpus,
                        settings.summary);
                settings.scriptingBackend =
#if UNITY_2021_2_OR_NEWER
                    PlayerSettings.GetScriptingBackend(NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(settings.summary.platform)));
#else
                    PlayerSettings.GetScriptingBackend(BuildPipeline.GetBuildTargetGroup(settings.summary.platform));
#endif
#if UNITY_IOS
                if (settings.targetPlatform == TargetPlatform.iOS)
                {
                    settings.extraOptions = new List<string>();
                    settings.extraOptions.Add(GetOption(OptionLinkerOptions, $"min-ios-version={PlayerSettings.iOS.targetOSVersionString}"));
                    settings.extraOptions.Add(GetOption(OptionPlatformConfiguration, PlayerSettings.iOS.targetOSVersionString));
                }
#endif
#if UNITY_TVOS
                if (settings.targetPlatform == TargetPlatform.tvOS)
                {
                    settings.extraOptions = new List<string>();
                    settings.extraOptions.Add(GetOption(OptionLinkerOptions, $"min-tvos-version={PlayerSettings.tvOS.targetOSVersionString}"));
                    settings.extraOptions.Add(GetOption(OptionPlatformConfiguration, PlayerSettings.tvOS.targetOSVersionString));
                }
#endif
#if UNITY_VISIONOS
                if (settings.targetPlatform == TargetPlatform.visionOS || settings.targetPlatform == TargetPlatform.visionSimulator)
                {
                    settings.extraOptions = new List<string>();
#if UNITY_2023_3_OR_NEWER
                    var targetOSVersionString = PlayerSettings.VisionOS.targetOSVersionString;
#else
                    var playerSettings = typeof(PlayerSettings);
                    var visionOs = playerSettings.GetNestedType("VisionOS");
                    if (visionOs == null) throw new Exception("Editor does not appear to support visionOS");
                    var targetOSVersionStringProperty = visionOs.GetProperty("targetOSVersionString", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (targetOSVersionStringProperty == null) throw new Exception("Property `targetOSVersionString` not found");
                    var targetOSVersionString = targetOSVersionStringProperty.GetValue(null, null);
#endif
                    settings.extraOptions.Add(GetOption(OptionLinkerOptions, $"min-visionos-version={targetOSVersionString}"));
                    settings.extraOptions.Add(GetOption(OptionPlatformConfiguration, targetOSVersionString));
                }
#endif
#if UNITY_2022_2_OR_NEWER && UNITY_ANDROID
                if (settings.targetPlatform == TargetPlatform.Android)
                {
                    // Enable Armv9 security features (PAC/BTI) if needed
                    settings.aotSettingsForTarget.EnableArmv9SecurityFeatures = PlayerSettings.Android.enableArmv9SecurityFeatures;
                    if (PlayerSettings.Android.enableArmv9SecurityFeatures)
                    {
                        settings.extraOptions ??= new List<string>();
                        settings.extraOptions.Add(GetOption(OptionPlatformConfiguration, "armv9-sec"));
                    }
                }
#endif
                if (settings.targetPlatform == TargetPlatform.UWP)
                {
                    settings.extraOptions = new List<string>();

                    if (!string.IsNullOrEmpty(EditorUserBuildSettings.wsaUWPVisualStudioVersion))
                    {
                        settings.extraOptions.Add(GetOption(OptionLinkerOptions, $"vs-version={EditorUserBuildSettings.wsaUWPVisualStudioVersion}"));
                    }

                    if (!string.IsNullOrEmpty(EditorUserBuildSettings.wsaUWPSDK))
                    {
                        settings.extraOptions.Add(GetOption(OptionLinkerOptions, $"target-sdk-version={EditorUserBuildSettings.wsaUWPSDK}"));
                    }

                    settings.extraOptions.Add(GetOption(OptionPlatformConfiguration, $"{EditorUserBuildSettings.wsaUWPVisualStudioVersion}:{EditorUserBuildSettings.wsaUWPSDK}:{EditorUserBuildSettings.wsaMinUWPSDK}"));
                }

#if PLATFORM_QNX
                if (settings.targetPlatform == TargetPlatform.QNX)
                {
                    settings.extraOptions = new List<string>();
                    settings.extraOptions.Add(GetOption(OptionPlatformConfiguration, GetQNXTargetOsVersion()));
                }
#endif

                settings.Save();
            }
        }

        private static string GetQNXTargetOsVersion()
        {
            var flags = System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Static |
                        System.Reflection.BindingFlags.FlattenHierarchy;
            var property = typeof(EditorUserBuildSettings).GetProperty("selectedQnxOsVersion", flags);
            if (null == property)
            {
                return "NOT_FOUND";
            }
            var value = (int)property.GetValue(null, null);
            switch (value)
            {
                case /*UnityEditor.QNXOsVersion.Neutrino70*/ 0: return "Neutrino70";
                case /*UnityEditor.QNXOsVersion.Neutrino71*/ 1: return "Neutrino71";
                default: return $"UNKNOWN_{value}";
            }
        }

        public IEnumerable<string> DoGenerate(Assembly[] assemblies)
        {
            if (!settings.isSupported)
                return Array.Empty<string>();
            return BurstAotCompiler.OnPostBuildPlayerScriptDLLsImpl(settings, assemblies);
        }
    }

#if !ENABLE_GENERATE_NATIVE_PLUGINS_FOR_ASSEMBLIES_API
    internal class BurstAndroidGradlePostprocessor : IPostGenerateGradleAndroidProject
    {
        int IOrderedCallback.callbackOrder => 1;

        void IPostGenerateGradleAndroidProject.OnPostGenerateGradleAndroidProject(string path)
        {
            var aotSettingsForTarget = BurstPlatformAotSettings.GetOrCreateSettings(BuildTarget.Android);
            // Early exit if burst is not activated
            if (BurstCompilerOptions.ForceDisableBurstCompilation || !aotSettingsForTarget.EnableBurstCompilation)
            {
                return;
            }

            // Copy bursted .so's from tempburstlibs to the actual location in the gradle project
            var sourceLocation = Path.GetFullPath(Path.Combine("Temp", "StagingArea", "tempburstlibs"));
            var targetLocation = Path.GetFullPath(Path.Combine(path, "src", "main", "jniLibs"));
            FileUtil.CopyDirectoryRecursive(sourceLocation, targetLocation, true);
        }
    }

    // For static builds, there are two different approaches:
    // Postprocessing adds the libraries after Unity is done building,
    // for platforms that need to build a project file, etc.
    // Preprocessing simply adds the libraries to the Unity build,
    // for platforms where Unity can directly build an app.
    internal class StaticPreProcessor : IPreprocessBuildWithReport
    {
        private const string TempSourceLibrary = @"Temp/StagingArea/SourcePlugins";
        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildReport report)
        {
            var aotSettingsForTarget = BurstPlatformAotSettings.GetOrCreateSettings(report.summary.platform);

            // Early exit if burst is not activated
            if (BurstCompilerOptions.ForceDisableBurstCompilation || !aotSettingsForTarget.EnableBurstCompilation)
            {
                return;
            }

            if(report.summary.platform == BuildTarget.Switch)
            {
                if(!Directory.Exists(TempSourceLibrary))
                {
                    Directory.CreateDirectory(TempSourceLibrary);
                    Directory.CreateDirectory(TempSourceLibrary);
                }

                BurstAotCompiler.WriteStaticLinkCppFile(TempSourceLibrary);
            }
        }
    }
#endif

    /// <summary>
    /// Integration of the burst AOT compiler into the Unity build player pipeline
    /// </summary>
    internal class BurstAotCompiler
    {
#if ENABLE_GENERATE_NATIVE_PLUGINS_FOR_ASSEMBLIES_API
        // When using the new player build API, don't write to Temp/StagingArea.
        // We still need code in Unity to support old versions of Burst not using the new API.
        // for that case, we will just pick up files written to the Temp/StagingArea.
        // So in order to not pick up files twice, use a different output location for the new
        // API.
        internal const string OutputBaseFolder = @"Temp/BurstOutput/";
#else
        private const string OutputBaseFolder = @"Temp/StagingArea/";
#endif
        private const string TempStagingManaged = OutputBaseFolder + @"Data/Managed/";
        private const string LibraryPlayerScriptAssemblies = "Library/PlayerScriptAssemblies";
        private const string TempManagedSymbols = @"Temp/ManagedSymbols/";
        internal const string BurstLinkXmlName = "burst.link.xml";

        internal struct BurstAOTSettings
        {
            public BuildSummary summary;
            public BurstPlatformAotSettings aotSettingsForTarget;
            public TargetPlatform targetPlatform;
            public TargetCpus targetCpus;
            public List<BurstAotCompiler.BurstOutputCombination> combinations;
            public ScriptingImplementation scriptingBackend;
            public string productName;
            public bool isSupported;
            public List<string> extraOptions;

            // Hash any fields that might have an effect on whether Bursted code needs to be recompiled
            // Note that the BurstPlatformAotSettings are saved and used separately, so they don't need to
            // be included in this hash.
            public Hash128 Hash()
            {
                var hc = new Hash128();
                hc.Append((int)(summary.options & (BuildOptions.InstallInBuildFolder | BuildOptions.Development)));
                hc.Append((int)targetPlatform);
                hc.Append(isSupported ? 1 : 0);

                if (targetCpus?.Cpus != null)
                {
                    hc.Append(targetCpus.Cpus.Count);
                    foreach (var cpu in targetCpus.Cpus)
                    {
                        hc.Append((int)cpu);
                    }
                }

                if (combinations != null)
                {
                    hc.Append(combinations.Count);
                    foreach (var comb in combinations)
                    {
                        comb.HashInto(ref hc);
                    }
                }

                if (extraOptions != null)
                {
                    hc.Append(extraOptions.Count);
                    foreach (var opt in extraOptions)
                    {
                        hc.Append(opt);
                    }
                }

                hc.Append((int)scriptingBackend);
                hc.Append(productName);

                return hc;
            }

            public void Save()
            {
                var path = GetPath(BurstPlatformAotSettings.ResolveTarget(summary.platform));
                var hash = Hash();
                try
                {
                    if (File.Exists(path))
                    {
                        // If the hashes match, don't touch the file so Bee will consider it unchanged
                        var bytes = File.ReadAllBytes(path);
                        var storedHash = new Hash128(BitConverter.ToUInt64(bytes, 0), BitConverter.ToUInt64(bytes, 8));
                        if (storedHash == hash)
                        {
                            return;
                        }
                    }

                    using (var f = new BufferedStream(File.OpenWrite(path)))
                    {
                        f.Write(BitConverter.GetBytes(hash.u64_0), 0, 8);
                        f.Write(BitConverter.GetBytes(hash.u64_1), 0, 8);
                    }
                }
                catch (Exception)
                {
                    // If we for some reason fail to save the settings, delete the existing ones (if any) to be sure
                    // we invalidate the cache and cause a recompilation
                    try
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                    catch (Exception)
                    {
                        // Welp
                    }
                }
            }

            public static string GetPath(BuildTarget? target)
            {
                var root = "Library/BurstCache";
                if (target.HasValue)
                {
                    return $"{root}/AotSettings_{target.Value}.hash";
                }
                else
                {
                    return $"{root}/AotSettings.hash";
                }
            }
        }

        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        internal static IEnumerable<string> OnPostBuildPlayerScriptDLLsImpl(BurstAOTSettings settings, Assembly[] playerAssemblies)
        {
            var buildTarget = settings.summary.platform;

            string burstMiscAlongsidePath = "";
            if ((settings.summary.options & BuildOptions.InstallInBuildFolder) == 0)
            {
                burstMiscAlongsidePath = BurstPlatformAotSettings.FetchOutputPath(settings.summary, settings.productName);
            }

            HashSet<string> assemblyDefines = new HashSet<string>();

            // Early exit if burst is not activated or the platform is not supported
            if (BurstCompilerOptions.ForceDisableBurstCompilation || !settings.aotSettingsForTarget.EnableBurstCompilation)
            {
                return Array.Empty<string>();
            }

            var isDevelopmentBuild = (settings.summary.options & BuildOptions.Development) != 0;

            var commonOptions = new List<string>();
            var stagingFolder = Path.GetFullPath(TempStagingManaged);

            // grab the location of the root of the player folder - for handling nda platforms that require keys
            var keyFolder = BuildPipeline.GetPlaybackEngineDirectory(buildTarget, BuildOptions.None);
            commonOptions.Add(GetOption(OptionAotKeyFolder, keyFolder));
            commonOptions.Add(GetOption(OptionAotDecodeFolder, Path.Combine(Environment.CurrentDirectory, "Library", "Burst")));

            // Extract the TargetPlatform and Cpus from the current build settings
            commonOptions.Add(GetOption(OptionPlatform, settings.targetPlatform));

            // --------------------------------------------------------------------------------------------------------
            // 1) Calculate AssemblyFolders
            // These are the folders to look for assembly resolution
            // --------------------------------------------------------------------------------------------------------
            var assemblyFolders = new List<string> { stagingFolder };

            foreach (var assembly in playerAssemblies)
                AddAssemblyFolder(assembly.outputPath, stagingFolder, buildTarget, assemblyFolders);

            if (buildTarget == BuildTarget.WSAPlayer || buildTarget == BuildTarget.GameCoreXboxOne || buildTarget == BuildTarget.GameCoreXboxSeries)
            {
                // On UWP, not all assemblies are copied to StagingArea, so we want to
                // find all directories that we can reference assemblies from
                // If we don't do this, we will crash with AssemblyResolutionException
                // when following type references.
                foreach (var assembly in playerAssemblies)
                {
                    foreach (var assemblyRef in assembly.compiledAssemblyReferences)
                        AddAssemblyFolder(assemblyRef, stagingFolder, buildTarget, assemblyFolders);
                }
            }

            if (settings.extraOptions != null)
            {
                commonOptions.AddRange(settings.extraOptions);
            }

            // Copy assembly used during staging to have a trace
            if (BurstLoader.IsDebugging)
            {
                try
                {
                    var copyAssemblyFolder = Path.Combine(Environment.CurrentDirectory, "Logs", "StagingAssemblies");
                    try
                    {
                        if (Directory.Exists(copyAssemblyFolder)) Directory.Delete(copyAssemblyFolder);
                    }
                    catch
                    {
                    }

                    if (!Directory.Exists(copyAssemblyFolder)) Directory.CreateDirectory(copyAssemblyFolder);
                    foreach (var file in Directory.EnumerateFiles(stagingFolder))
                    {
                        File.Copy(file, Path.Combine(copyAssemblyFolder, Path.GetFileName(file)));
                    }
                }
                catch
                {
                }
            }

            // --------------------------------------------------------------------------------------------------------
            // 2) Calculate root assemblies
            // These are the assemblies that the compiler will look for methods to compile
            // This list doesn't typically include .NET runtime assemblies but only assemblies compiled as part
            // of the current Unity project
            // --------------------------------------------------------------------------------------------------------
            var rootAssemblies = new List<string>();
            foreach (var playerAssembly in playerAssemblies)
            {
#if ENABLE_GENERATE_NATIVE_PLUGINS_FOR_ASSEMBLIES_API
                var playerAssemblyPath = Path.GetFullPath(playerAssembly.outputPath);
#else
                // the file at path `playerAssembly.outputPath` is actually not on the disk
                // while it is in the staging folder because OnPostBuildPlayerScriptDLLs is being called once the files are already
                // transferred to the staging folder, so we are going to work from it but we are reusing the file names that we got earlier
                var playerAssemblyPath = Path.Combine(stagingFolder, Path.GetFileName(playerAssembly.outputPath));
#endif

                if (!File.Exists(playerAssemblyPath))
                {
                    Debug.LogWarning($"Unable to find player assembly: {playerAssembly.outputPath}");
                }
                else
                {
                    rootAssemblies.Add(playerAssemblyPath);
                    commonOptions.Add(GetOption(OptionAssemblyDefines, $"{playerAssembly.name};{string.Join(";", playerAssembly.defines)}"));
                }
            }

            commonOptions.AddRange(rootAssemblies.Select(root => GetOption(OptionRootAssembly, root)));

            // --------------------------------------------------------------------------------------------------------
            // 4) Compile each combination
            //
            // Here bcl.exe is called for each target CPU combination
            // --------------------------------------------------------------------------------------------------------

            string debugLogFile = null;
            if (BurstLoader.IsDebugging)
            {
                // Reset log files
                try
                {
                    var logDir = Path.Combine(Environment.CurrentDirectory, "Logs");
                    debugLogFile = Path.Combine(logDir, "burst_bcl_editor.log");
                    if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
                    File.WriteAllText(debugLogFile, string.Empty);
                }
                catch
                {
                    debugLogFile = null;
                }
            }

            if ((settings.summary.options & BuildOptions.InstallInBuildFolder) == 0)
            {
                CreateFolderForMiscFiles(burstMiscAlongsidePath);
            }

            // Log the targets generated by BurstReflection.FindExecuteMethods
            foreach (var combination in settings.combinations)
            {
                // Gets the output folder
                var stagingOutputFolder = Path.GetFullPath(Path.Combine(OutputBaseFolder, combination.OutputPath));
                var outputFilePrefix = Path.Combine(stagingOutputFolder, combination.LibraryName);

                var options = new List<string>(commonOptions)
                {
                    GetOption(OptionAotOutputPath, outputFilePrefix),
                    GetOption(OptionTempDirectory, Path.Combine(Environment.CurrentDirectory, "Temp", "Burst"))
                };

                foreach (var cpu in combination.TargetCpus.Cpus)
                {
                    options.Add(GetOption(OptionTarget, cpu));
                }

                if (settings.targetPlatform == TargetPlatform.iOS || settings.targetPlatform == TargetPlatform.tvOS || settings.targetPlatform == TargetPlatform.Switch || settings.targetPlatform == TargetPlatform.visionOS)
                {
                    options.Add(GetOption(OptionStaticLinkage));
#if ENABLE_GENERATE_NATIVE_PLUGINS_FOR_ASSEMBLIES_API
                    WriteStaticLinkCppFile($"{OutputBaseFolder}/{combination.OutputPath}");
#endif
                }

                if (settings.targetPlatform == TargetPlatform.Windows)
                {
                    options.Add(GetOption(OptionLinkerOptions, $"PdbAltPath=\"{settings.productName}_{combination.OutputPath}/{Path.GetFileNameWithoutExtension(combination.LibraryName)}.pdb\""));
                }

#if UNITY_2022_2_OR_NEWER && UNITY_ANDROID
                if (settings.targetPlatform == TargetPlatform.Android)
                {
                    // Enable Armv9 security features (PAC/BTI) if needed
                    if (settings.aotSettingsForTarget.EnableArmv9SecurityFeatures)
                        options.Add(GetOption(OptionBranchProtection, "Standard"));
                }
#endif

                options.AddRange(assemblyFolders.Select(assemblyFolder => GetOption(OptionAotAssemblyFolder, assemblyFolder)));

                // Set the flag to print a message on missing MonoPInvokeCallback attribute on IL2CPP only
                if (settings.scriptingBackend == ScriptingImplementation.IL2CPP)
                {
                    options.Add(GetOption(OptionPrintLogOnMissingPInvokeCallbackAttribute));
                }

                // Log the targets generated by BurstReflection.FindExecuteMethods
                if (BurstLoader.IsDebugging && debugLogFile != null)
                {
                    try
                    {
                        var writer = new StringWriter();
                        writer.WriteLine("-----------------------------------------------------------");
                        writer.WriteLine("Combination: " + combination);
                        writer.WriteLine("-----------------------------------------------------------");

                        foreach (var option in options)
                        {
                            writer.WriteLine(option);
                        }

                        writer.WriteLine("Assemblies in AssemblyFolders:");
                        foreach (var assemblyFolder in assemblyFolders)
                        {
                            writer.WriteLine("|- Folder: " + assemblyFolder);
                            foreach (var assemblyOrDll in Directory.EnumerateFiles(assemblyFolder, "*.dll"))
                            {
                                var fileInfo = new FileInfo(assemblyOrDll);
                                writer.WriteLine("   |- " + assemblyOrDll +  " Size: " + fileInfo.Length + " Date: " + fileInfo.LastWriteTime);
                            }
                        }

                        File.AppendAllText(debugLogFile, writer.ToString());
                    }
                    catch
                    {
                        // ignored
                    }
                }

                // Allow burst to find managed symbols in the backup location in case the symbols are stripped in the build location
                options.Add(GetOption(OptionAotPdbSearchPaths, TempManagedSymbols));

                if (isDevelopmentBuild && Environment.GetEnvironmentVariable("UNITY_BURST_ENABLE_SAFETY_CHECKS_IN_PLAYER_BUILD") != null)
                {
                    options.Add("--global-safety-checks-setting=ForceOn");
                }


                options.Add(GetOption(OptionGenerateLinkXml, Path.Combine("Temp", BurstLinkXmlName)));

                if (!string.IsNullOrWhiteSpace(settings.aotSettingsForTarget.DisabledWarnings))
                {
                    options.Add(GetOption(OptionDisableWarnings, settings.aotSettingsForTarget.DisabledWarnings));
                }

                if (isDevelopmentBuild || settings.aotSettingsForTarget.EnableDebugInAllBuilds)
                {
                    if (!isDevelopmentBuild)
                    {
                        Debug.LogWarning(
                            "Symbols are being generated for burst compiled code, please ensure you intended this - see Burst AOT settings.");
                    }

                    options.Add(GetOption(OptionDebug,
                        (settings.aotSettingsForTarget.DebugDataKind == DebugDataKind.Full) && (!combination.WorkaroundFullDebugInfo) ? "Full" : "LineOnly"));
                }

                if (!settings.aotSettingsForTarget.EnableOptimisations)
                {
                    options.Add(GetOption(OptionDisableOpt));
                }
                else
                {
                    switch (settings.aotSettingsForTarget.OptimizeFor)
                    {
                        case OptimizeFor.Default:
                        case OptimizeFor.Balanced:
                            options.Add(GetOption(OptionOptLevel, 2));
                            break;
                        case OptimizeFor.Performance:
                            options.Add(GetOption(OptionOptLevel, 3));
                            break;
                        case OptimizeFor.Size:
                            options.Add(GetOption(OptionOptForSize));
                            options.Add(GetOption(OptionOptLevel, 3));
                            break;
                        case OptimizeFor.FastCompilation:
                            options.Add(GetOption(OptionOptLevel, 1));
                            break;
                    }
                }

                if (BurstLoader.IsDebugging)
                {
                    options.Add(GetOption("debug-logging"));
                }

                // Add list of assemblies to ignore
                var disabledAssemblies = BurstAssemblyDisable.GetDisabledAssemblies(BurstAssemblyDisable.DisableType.Player, BurstPlatformAotSettings.ResolveTarget(buildTarget).ToString());
                foreach (var discard in disabledAssemblies)
                {
                    options.Add(GetOption(OptionDiscardAssemblies, discard));
                }

                // Write current options to the response file
                var responseFile = Path.GetTempFileName();
                File.WriteAllLines(responseFile, options);

                if (BurstLoader.IsDebugging)
                {
                    Debug.Log($"bcl.exe {OptionBurstcSwitch} @{responseFile}\n\nResponse File:\n" + string.Join("\n", options));
                }

                try
                {
                    var burstcSwitch = OptionBurstcSwitch;

                    if (!string.IsNullOrEmpty(
                            Environment.GetEnvironmentVariable("UNITY_BURST_DISABLE_INCREMENTAL_PLAYER_BUILDS")))
                    {
                        burstcSwitch = "";
                    }

                    if (BurstLoader.BclConfiguration.IsExecutableNative)
                    {
                        BclRunner.RunNativeProgram(
                            BurstLoader.BclConfiguration.ExecutablePath,
                            $"{burstcSwitch} {BclRunner.EscapeForShell("@" + responseFile)}",
                            new BclOutputErrorParser());
                    }
                    else
                    {
                        BclRunner.RunManagedProgram(
                            BurstLoader.BclConfiguration.ExecutablePath,
                            $"{burstcSwitch} {BclRunner.EscapeForShell("@" + responseFile)}",
                            new BclOutputErrorParser());
                    }

                    // Additionally copy the pdb to the root of the player build so run in editor also locates the symbols
                    var pdbPath = $"{Path.Combine(stagingOutputFolder, combination.LibraryName)}.pdb";
                    if (File.Exists(pdbPath))
                    {
                        var dstPath = Path.Combine(OutputBaseFolder, $"{combination.LibraryName}.pdb");
                        File.Copy(pdbPath, dstPath, overwrite: true);
                    }
                }
                catch (BuildFailedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new BuildFailedException(e);
                }
            }

            PostProcessCombinations(settings.targetPlatform, settings.combinations, settings.summary);

            var pdbsRemainInBuild = isDevelopmentBuild || settings.aotSettingsForTarget.EnableDebugInAllBuilds || settings.targetPlatform == TargetPlatform.UWP;

            // Finally move out any symbols/misc files from the final output
            if ((settings.summary.options & BuildOptions.InstallInBuildFolder) == 0)
            {
                return CollateMiscFiles(settings.combinations, burstMiscAlongsidePath, pdbsRemainInBuild);
            }

            return Array.Empty<string>();
        }

        private static void AddAssemblyFolder(string assemblyRef, string stagingFolder, BuildTarget buildTarget,
            List<string> assemblyFolders)
        {
            // Exclude folders with assemblies already compiled in the `folder`
            var assemblyName = Path.GetFileName(assemblyRef);
            if (assemblyName != null && File.Exists(Path.Combine(stagingFolder, assemblyName)))
            {
                return;
            }

            var directory = Path.GetDirectoryName(assemblyRef);
            if (directory != null)
            {
                var fullPath = Path.GetFullPath(directory);
                if (IsMonoReferenceAssemblyDirectory(fullPath) || IsDotNetStandardAssemblyDirectory(fullPath))
                {
                    // Don't pass reference assemblies to burst because they contain methods without implementation
                    // If burst accidentally resolves them, it will emit calls to burst_abort.
                    fullPath = Path.Combine(EditorApplication.applicationContentsPath, "MonoBleedingEdge/lib/mono");
#if UNITY_2021_2_OR_NEWER
                    // In 2021.2 we got multiple mono distributions, per platform.
                    fullPath = Path.Combine(fullPath, "unityaot-" + BuildTargetDiscovery.GetPlatformProfileSuffix(buildTarget));
#else
                                fullPath = Path.Combine(fullPath, "unityaot");
#endif
                    fullPath = Path.GetFullPath(fullPath); // GetFullPath will normalize path separators to OS native format
                    if (!assemblyFolders.Contains(fullPath))
                        assemblyFolders.Add(fullPath);

                    fullPath = Path.Combine(fullPath, "Facades");
                    if (!assemblyFolders.Contains(fullPath))
                        assemblyFolders.Add(fullPath);
                }
                else if (!assemblyFolders.Contains(fullPath))
                {
                    assemblyFolders.Add(fullPath);
                }
            }
        }

        private static void CreateFolderForMiscFiles(string finalFolder)
        {
            try
            {
                if (Directory.Exists(finalFolder)) Directory.Delete(finalFolder,true);
            }
            catch
            {
            }
            Directory.CreateDirectory(finalFolder);
        }

        private static IEnumerable<string> CollateMiscFiles(List<BurstOutputCombination> combinations, string finalFolder, bool retainPdbs)
        {
            foreach (var combination in combinations)
            {
                var inputPath = Path.GetFullPath(Path.Combine(OutputBaseFolder, combination.OutputPath));
                var outputPath = Path.Combine(finalFolder, combination.OutputPath);
                Directory.CreateDirectory(outputPath);
                if (!Directory.Exists(inputPath))
                    continue;
                var files = Directory.GetFiles(inputPath);
                var directories = Directory.GetDirectories(inputPath);
                foreach (var fileName in files)
                {
                    var lowerCase = fileName.ToLower();
                    if ( (!retainPdbs && lowerCase.EndsWith(".pdb")) || lowerCase.EndsWith(".dsym") || lowerCase.EndsWith(".txt"))
                    {
                        // Move the file out of the staging area so its not included in the build
                        File.Move(fileName, Path.Combine(outputPath, Path.GetFileName(fileName)));
                    }
                    else if (!combination.CollateDirectory)
                    {
                        yield return fileName;
                    }
                }
                foreach (var fileName in directories)
                {
                    var lowerCase = fileName.ToLower();
                    if ( (!retainPdbs && lowerCase.EndsWith(".pdb")) || lowerCase.EndsWith(".dsym") || lowerCase.EndsWith(".txt"))
                    {
                        // Move the folder out of the staging area so its not included in the build
                        Directory.Move(fileName, Path.Combine(outputPath, Path.GetFileName(fileName)));
                    }
                    else if (!combination.CollateDirectory)
                    {
                        yield return fileName;
                    }
                }

                if (combination.CollateDirectory)
                    yield return inputPath;
            }
        }

        private static bool AndroidHasX86(AndroidArchitecture architecture)
        {
            // Deal with rename that occured
            AndroidArchitecture val;
            if (AndroidArchitecture.TryParse("X86", out val))
            {
                return (architecture & val)!=0;
            }
            else if (AndroidArchitecture.TryParse("x86", out val))
            {
                return (architecture & val)!=0;
            }
            return false;
        }
        private static bool AndroidHasX86_64(AndroidArchitecture architecture)
        {
            // Deal with rename that occured
            AndroidArchitecture val;
            if (AndroidArchitecture.TryParse("X86_64", out val))
            {
                return (architecture & val)!=0;
            }
            else if (AndroidArchitecture.TryParse("x86_64", out val))
            {
                return (architecture & val)!=0;
            }
            return false;
        }

        private enum SimulatorPlatforms
        {
            iOS,
            tvOS
        }
        private static bool IsForSimulator(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.iOS:
                    return IsForSimulator(SimulatorPlatforms.iOS);
                case BuildTarget.tvOS:
                    return IsForSimulator(SimulatorPlatforms.tvOS);
                default:
                    return false;
            }
        }
        private static bool IsForSimulator(TargetPlatform targetPlatform)
        {
            switch (targetPlatform)
            {
                case TargetPlatform.iOS:
                    return IsForSimulator(SimulatorPlatforms.iOS);
                case TargetPlatform.tvOS:
                    return IsForSimulator(SimulatorPlatforms.tvOS);
                default:
                    return false;
            }
        }
        private static bool IsForSimulator(SimulatorPlatforms simulatorPlatforms)
        {
            switch (simulatorPlatforms)
            {
                case SimulatorPlatforms.iOS:
                    return UnityEditor.PlayerSettings.iOS.sdkVersion == iOSSdkVersion.SimulatorSDK;
                case SimulatorPlatforms.tvOS:
                    return UnityEditor.PlayerSettings.tvOS.sdkVersion == tvOSSdkVersion.Simulator;
            }

            return false;
        }

        public static void WriteStaticLinkCppFile(string dir)
        {
            Directory.CreateDirectory(dir);
            string cppPath = Path.Combine(dir, "lib_burst_generated.cpp");
            // Additionally we need a small cpp file (weak symbols won't unfortunately override directly from the libs
            //presumably due to link order?
            File.WriteAllText(cppPath, @"
extern ""C""
{
    void Staticburst_initialize(void* );
    void* StaticBurstStaticMethodLookup(void* );

    int burst_enable_static_linkage = 1;
    void burst_initialize(void* i) { Staticburst_initialize(i); }
    void* BurstStaticMethodLookup(void* i) { return StaticBurstStaticMethodLookup(i); }
}
");
        }

        /// <summary>
        /// Collect CPU combinations for the specified TargetPlatform and TargetCPU
        /// </summary>
        /// <param name="targetPlatform">The target platform (e.g Windows)</param>
        /// <param name="targetCpus">The target CPUs (e.g X64_SSE4)</param>
        /// <param name="report">Error reporting</param>
        /// <returns>The list of CPU combinations</returns>
        internal static List<BurstOutputCombination> CollectCombinations(TargetPlatform targetPlatform, TargetCpus targetCpus, BuildSummary summary)
        {
            var combinations = new List<BurstOutputCombination>();

            if (targetPlatform == TargetPlatform.macOS)
            {
                // NOTE: OSX has a special folder for the plugin
                // Declared in GetStagingAreaPluginsFolder
                // PlatformDependent\OSXPlayer\Extensions\Managed\OSXDesktopStandalonePostProcessor.cs
                var outputPath = Path.Combine(Path.GetFileName(summary.outputPath), "Contents", "Plugins");

                // Based on : PlatformDependent/OSXPlayer/Extension/OSXStandaloneBuildWindowExtension.cs
                var aotSettings = BurstPlatformAotSettings.GetOrCreateSettings(BuildTarget.StandaloneOSX);
                var buildTargetName = BuildPipeline.GetBuildTargetName(BuildTarget.StandaloneOSX);
                var architecture = EditorUserBuildSettings.GetPlatformSettings(buildTargetName, "Architecture").ToLowerInvariant();
                switch (architecture)
                {
                    case "x64":
                        combinations.Add(new BurstOutputCombination(outputPath, aotSettings.GetDesktopCpu64Bit()));
                        break;
                    case "arm64":
                        // According to
                        // https://web.archive.org/web/20220504192056/https://github.com/llvm/llvm-project/blob/main/llvm/include/llvm/Support/AArch64TargetParser.def#L240
                        // M1 is equivalent to Armv8.5-A, so it supports everything from HALFFP target
                        // (there's no direct confirmation on crypto because it's not mandatory)
                        combinations.Add(new BurstOutputCombination(outputPath, new TargetCpus(BurstTargetCpu.ARMV8A_AARCH64_HALFFP)));
                        break;
                    default:
                        combinations.Add(new BurstOutputCombination(Path.Combine(outputPath, "x64"), aotSettings.GetDesktopCpu64Bit()));
                        combinations.Add(new BurstOutputCombination(Path.Combine(outputPath, "arm64"), new TargetCpus(BurstTargetCpu.ARMV8A_AARCH64_HALFFP)));
                        break;
                }
            }
            else if (targetPlatform == TargetPlatform.iOS || targetPlatform == TargetPlatform.tvOS || targetPlatform == TargetPlatform.visionOS || targetPlatform == TargetPlatform.visionSimulator)
            {
                if (IsForSimulator(targetPlatform))
                {
                    Debug.LogWarning("Burst Does not currently support the simulator, burst is disabled for this build.");
                }
                else if (Application.platform != RuntimePlatform.OSXEditor)
                {
                    Debug.LogWarning("Burst Cross Compilation to iOS/tvOS for standalone player, is only supported on OSX Editor at this time, burst is disabled for this build.");
                }
                else
                {
                    // Looks like a way to detect iOS CPU capabilities in runtime (like getauxval()) is sysctlbyname()
                    // https://developer.apple.com/documentation/kernel/1387446-sysctlbyname/determining_instruction_set_characteristics
                    // TODO: add support for it when needed, for now using the lowest common denominator
                    // https://web.archive.org/web/20220504192056/https://github.com/llvm/llvm-project/blob/main/llvm/include/llvm/Support/AArch64TargetParser.def#L240
                    // This LLVM code implies A11 is the first Armv8.2-A CPU
                    // However, it doesn't support dotprod, so we can't consider it equivalent to our HALFFP variant
                    // A13 (equivalent to Armv8.4-A) and M1 seem to be the first CPUs we can claim HALFFP compatible
                    // Since we need to support older CPUs, have to use the "basic" Armv8A here
                    combinations.Add(new BurstOutputCombination("StaticLibraries", new TargetCpus(BurstTargetCpu.ARMV8A_AARCH64)));
                }
            }
            else if (targetPlatform == TargetPlatform.Android)
            {
                // TODO: would be better to query AndroidNdkRoot (but thats not exposed from unity)
                string ndkRoot = null;
                var targetAPILevel = PlayerSettings.Android.GetMinTargetAPILevel();
#if UNITY_ANDROID
                ndkRoot = UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath;
#else
                // 2019.1 now has an embedded ndk
                if (EditorPrefs.HasKey("NdkUseEmbedded"))
                {
                    if (EditorPrefs.GetBool("NdkUseEmbedded"))
                    {
                        ndkRoot = Path.Combine(BuildPipeline.GetPlaybackEngineDirectory(BuildTarget.Android, BuildOptions.None), "NDK");
                    }
                    else
                    {
                        ndkRoot = EditorPrefs.GetString("AndroidNdkRootR16b");
                    }
                }
#endif

                // If we still don't have a valid root, try the old key
                if (string.IsNullOrEmpty(ndkRoot))
                {
                    ndkRoot = EditorPrefs.GetString("AndroidNdkRoot");
                }

                // Verify the directory at least exists, if not we fall back to ANDROID_NDK_ROOT current setting
                if (!string.IsNullOrEmpty(ndkRoot) && !Directory.Exists(ndkRoot))
                {
                    ndkRoot = null;
                }

                // Always set the ANDROID_NDK_ROOT (if we got a valid result from above), so BCL knows where to find the Android toolchain and its the one the user expects
                if (!string.IsNullOrEmpty(ndkRoot))
                {
                    Environment.SetEnvironmentVariable("ANDROID_NDK_ROOT", ndkRoot);
                }

                Environment.SetEnvironmentVariable("BURST_ANDROID_MIN_API_LEVEL", $"{targetAPILevel}");

                // Setting tempburstlibs/ as the interim target directory
                // Don't target libs/ directly because incremental build pipeline doesn't expect the so's at that path
                // Rather, so's are copied to the actual location in the gradle project in BurstAndroidGradlePostprocessor
                var androidTargetArch = PlayerSettings.Android.targetArchitectures;
                if ((androidTargetArch & AndroidArchitecture.ARMv7) != 0)
                {
                    combinations.Add(new BurstOutputCombination("tempburstlibs/armeabi-v7a", new TargetCpus(BurstTargetCpu.ARMV7A_NEON32), collateDirectory: true));
                }

                if ((androidTargetArch & AndroidArchitecture.ARM64) != 0)
                {
                    var aotSettingsForTarget = BurstPlatformAotSettings.GetOrCreateSettings(summary.platform);
                    combinations.Add(new BurstOutputCombination("tempburstlibs/arm64-v8a", aotSettingsForTarget.GetAndroidCpuArm64(), collateDirectory: true));
                }
#if UNITY_2019_4_OR_NEWER
                if (AndroidHasX86(androidTargetArch))
                {
                    combinations.Add(new BurstOutputCombination("tempburstlibs/x86", new TargetCpus(BurstTargetCpu.X86_SSE4), collateDirectory: true));
                }
                if (AndroidHasX86_64(androidTargetArch))
                {
                    combinations.Add(new BurstOutputCombination("tempburstlibs/x86_64", new TargetCpus(BurstTargetCpu.X64_SSE4), collateDirectory: true));
                }
#endif
            }
            else if (targetPlatform == TargetPlatform.UWP)
            {
                var aotSettingsForTarget = BurstPlatformAotSettings.GetOrCreateSettings(summary.platform);

                if (EditorUserBuildSettings.wsaUWPBuildType == WSAUWPBuildType.ExecutableOnly)
                {
                    combinations.Add(new BurstOutputCombination($"Plugins/{GetUWPTargetArchitecture()}", targetCpus, collateDirectory: true));
                }
                else
                {
                    combinations.Add(new BurstOutputCombination("Plugins/x64", aotSettingsForTarget.GetDesktopCpu64Bit(), collateDirectory: true));
                    combinations.Add(new BurstOutputCombination("Plugins/x86", aotSettingsForTarget.GetDesktopCpu32Bit(), collateDirectory: true));
                    combinations.Add(new BurstOutputCombination("Plugins/ARM", new TargetCpus(BurstTargetCpu.THUMB2_NEON32), collateDirectory: true));
                    combinations.Add(new BurstOutputCombination("Plugins/ARM64", new TargetCpus(BurstTargetCpu.ARMV8A_AARCH64), collateDirectory: true));
                }
            }
#if !UNITY_2022_2_OR_NEWER
            else if (targetPlatform == TargetPlatform.Lumin)
            {
                // Set the LUMINSDK_UNITY so bcl.exe will be able to find the SDK
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LUMINSDK_UNITY")))
                {
                    var sdkRoot = EditorPrefs.GetString("LuminSDKRoot");
                    if (!string.IsNullOrEmpty(sdkRoot))
                    {
                        Environment.SetEnvironmentVariable("LUMINSDK_UNITY", sdkRoot);
                    }
                }
                combinations.Add(new BurstOutputCombination("Data/Plugins/", targetCpus));
            }
#endif
            else if (targetPlatform == TargetPlatform.Switch)
            {
                combinations.Add(new BurstOutputCombination("NativePlugins/", targetCpus));
            }
            else
            {
                if (targetPlatform == TargetPlatform.Windows)
                {
                    // This is what is expected by PlatformDependent\Win\Plugins.cpp
                    if (targetCpus.IsX86())
                    {
                        combinations.Add(new BurstOutputCombination("Data/Plugins/x86", targetCpus, collateDirectory: true));
                    }
                    else
                    {
                        var windowsArchitecture = GetWindows64BitTargetArchitecture();
                        if (string.Equals(windowsArchitecture, "ARM64", StringComparison.OrdinalIgnoreCase))
                        {
                            combinations.Add(new BurstOutputCombination("Data/Plugins/ARM64", targetCpus, collateDirectory: true, workaroundBrokenDebug: true));
                        }
                        else
                        {
                            combinations.Add(new BurstOutputCombination("Data/Plugins/x86_64", targetCpus, collateDirectory: true));
                        }
                    }
                }
                else
                {
                    // Safeguard
                    combinations.Add(new BurstOutputCombination("Data/Plugins/", targetCpus));
                }
            }

            return combinations;
        }

        private static void PostProcessCombinations(TargetPlatform targetPlatform, List<BurstOutputCombination> combinations, BuildSummary summary)
        {
            if (targetPlatform == TargetPlatform.macOS && combinations.Count > 1)
            {
                // Figure out which files we need to lipo
                string outputSymbolsDir = null;
                var outputDir = Path.Combine(OutputBaseFolder, Path.GetFileName(summary.outputPath), "Contents", "Plugins");

                var sliceCount = combinations.Count;
                var binarySlices = new string[sliceCount];
                var debugSymbolSlices = new string[sliceCount];

                for (int i = 0; i < sliceCount; i++)
                {
                    var slice = combinations[i];

                    var binaryFileName = slice.LibraryName + ".bundle";
                    var binaryPath = Path.Combine(OutputBaseFolder, slice.OutputPath, binaryFileName);
                    binarySlices[i] = binaryPath;

                    // Only attempt to lipo symbols if they actually exist
                    var dsymPath = binaryPath + ".dsym";
                    var debugSymbolsPath = Path.Combine(dsymPath, "Contents", "Resources", "DWARF", binaryFileName);
                    if (File.Exists(debugSymbolsPath))
                    {
                        if (string.IsNullOrWhiteSpace(outputSymbolsDir))
                        {
                            // Copy over the symbols from the first combination for metadata files which we aren't merging, like Info.plist
                            var outputDsymPath = Path.Combine(outputDir, binaryFileName + ".dsym");
                            CopyDirectory(dsymPath, outputDsymPath, true);

                            outputSymbolsDir = Path.Combine(outputDsymPath, "Contents", "Resources", "DWARF");
                        }

                        debugSymbolSlices[i] = debugSymbolsPath;
                    }
                }

                // lipo combinations together
                var outBinaryFileName = combinations[0].LibraryName + ".bundle";
                RunLipo(binarySlices, Path.Combine(outputDir, outBinaryFileName));

                if (!string.IsNullOrWhiteSpace(outputSymbolsDir))
                    RunLipo(debugSymbolSlices, Path.Combine(outputSymbolsDir, outBinaryFileName));

                // Remove single-slice binary so they don't end up in the build
                for (int i = 0; i < sliceCount; i++)
                    Directory.Delete(Path.GetDirectoryName(binarySlices[i]), true);

                // Since we have combined the files, we need to adjust combinations for the next step
                var outFolder = Path.GetDirectoryName(combinations[0].OutputPath);  // remove platform folder
                combinations.Clear();
                combinations.Add(new BurstOutputCombination(outFolder, new TargetCpus()));
            }
        }

        private static void RunLipo(string[] inputFiles, string outputFile)
        {
            var outputDir = Path.GetDirectoryName(outputFile);
            Directory.CreateDirectory(outputDir);

            var cmdLine = new StringBuilder();
            foreach (var input in inputFiles)
            {
                if (string.IsNullOrEmpty(input))
                    continue;

                cmdLine.Append(BclRunner.EscapeForShell(input));
                cmdLine.Append(' ');
            }

            cmdLine.Append("-create -output ");
            cmdLine.Append(BclRunner.EscapeForShell(outputFile));

            string lipoPath;

            var currentEditorPlatform = Application.platform;
            switch (currentEditorPlatform)
            {
                case RuntimePlatform.LinuxEditor:
                    lipoPath = Path.Combine(BurstLoader.BclConfiguration.FolderPath, "hostlin", "llvm-lipo");
                    break;

                case RuntimePlatform.OSXEditor:
                    lipoPath = Path.Combine(BurstLoader.BclConfiguration.FolderPath, "hostmac", "llvm-lipo");
                    break;

                case RuntimePlatform.WindowsEditor:
                    lipoPath = Path.Combine(BurstLoader.BclConfiguration.FolderPath, "hostwin", "llvm-lipo.exe");
                    break;

                default:
                    throw new NotSupportedException("Unknown Unity editor platform: " + currentEditorPlatform);
            }

            BclRunner.RunNativeProgram(lipoPath, cmdLine.ToString(), null);
        }

        internal static Assembly[] GetPlayerAssemblies(BuildReport report)
        {
            // We need to build the list of root assemblies based from the "PlayerScriptAssemblies" folder.
            // This is so we compile the versions of the library built for the individual platforms, not the editor version.
            var oldOutputDir = EditorCompilationInterface.GetCompileScriptsOutputDirectory();
            try
            {
                EditorCompilationInterface.SetCompileScriptsOutputDirectory(LibraryPlayerScriptAssemblies);

                var shouldIncludeTestAssemblies = report.summary.options.HasFlag(BuildOptions.IncludeTestAssemblies);

#if UNITY_2021_1_OR_NEWER
                // Workaround that with 'Server Build' ticked in the build options, since there is no 'AssembliesType.Server'
                // enum, we need to manually add the BuildingForHeadlessPlayer compilation option.

#if UNITY_2022_1_OR_NEWER
                var isHeadless = report.summary.subtarget == (int)StandaloneBuildSubtarget.Server;
#elif UNITY_2021_2_OR_NEWER
                // A really really really gross hack - thanks Cristian Mazo! Querying the BuildOptions.EnableHeadlessMode is
                // obselete, but accessing its integer value is not... Note: this is just the temporary workaround to unblock
                // us (as of 1st June 2021, I say this with **much hope** that it is indeed temporary!).
                var isHeadless = report.summary.options.HasFlag((BuildOptions)16384);
#else
                var isHeadless = report.summary.options.HasFlag(BuildOptions.EnableHeadlessMode);
#endif
                if (isHeadless)
                {
                    var compilationOptions = EditorCompilationInterface.GetAdditionalEditorScriptCompilationOptions();
                    compilationOptions |= EditorScriptCompilationOptions.BuildingForHeadlessPlayer;

                    if (shouldIncludeTestAssemblies)
                    {
                        compilationOptions |= EditorScriptCompilationOptions.BuildingIncludingTestAssemblies;
                    }

                    return CompilationPipeline.ToAssemblies(CompilationPipeline.GetScriptAssemblies(EditorCompilationInterface.Instance, compilationOptions));
                }
                else
                {
                    return CompilationPipeline.GetAssemblies(shouldIncludeTestAssemblies ? AssembliesType.Player : AssembliesType.PlayerWithoutTestAssemblies);
                }
#else
                // Workaround that with 'Server Build' ticked in the build options, since there is no 'AssembliesType.Server'
                // enum, we need to manually add the 'UNITY_SERVER' define to the player assembly search list.
                if (report.summary.options.HasFlag(BuildOptions.EnableHeadlessMode))
                {
                    var compilationOptions = EditorCompilationInterface.GetAdditionalEditorScriptCompilationOptions();
                    if (shouldIncludeTestAssemblies)
                    {
                        compilationOptions |= EditorScriptCompilationOptions.BuildingIncludingTestAssemblies;
                    }

                    return CompilationPipeline.GetPlayerAssemblies(EditorCompilationInterface.Instance, compilationOptions, new string[] { "UNITY_SERVER" });
                }
                else
                {
                    return CompilationPipeline.GetAssemblies(shouldIncludeTestAssemblies ? AssembliesType.Player : AssembliesType.PlayerWithoutTestAssemblies);
                }
#endif
            }
            finally
            {
                EditorCompilationInterface.SetCompileScriptsOutputDirectory(oldOutputDir);  // restore output directory back to original value
            }
        }

        private static bool IsMonoReferenceAssemblyDirectory(string path)
        {
            var editorDir = Path.GetFullPath(EditorApplication.applicationContentsPath);
            return path.IndexOf(editorDir, StringComparison.OrdinalIgnoreCase) != -1 && path.IndexOf("MonoBleedingEdge", StringComparison.OrdinalIgnoreCase) != -1 && path.IndexOf("-api", StringComparison.OrdinalIgnoreCase) != -1;
        }

        private static bool IsDotNetStandardAssemblyDirectory(string path)
        {
            var editorDir = Path.GetFullPath(EditorApplication.applicationContentsPath);
            return path.IndexOf(editorDir, StringComparison.OrdinalIgnoreCase) != -1 && path.IndexOf("netstandard", StringComparison.OrdinalIgnoreCase) != -1 && path.IndexOf("shims", StringComparison.OrdinalIgnoreCase) != -1;
        }

        internal static TargetPlatform GetTargetPlatformAndDefaultCpu(BuildTarget target, out TargetCpus targetCpu, BurstPlatformAotSettings aotSettingsForTarget)
        {
            var platform = TryGetTargetPlatform(target, out targetCpu, aotSettingsForTarget);
            if (!platform.HasValue)
            {
                throw new NotSupportedException("The target platform " + target + " is not supported by the burst compiler");
            }
            return platform.Value;
        }

        internal static bool IsSupportedPlatform(BuildTarget target, BurstPlatformAotSettings aotSettingsForTarget)
        {
            return TryGetTargetPlatform(target, out var _, aotSettingsForTarget).HasValue;
        }

        private static TargetPlatform? TryGetTargetPlatform(BuildTarget target, out TargetCpus targetCpus, BurstPlatformAotSettings aotSettingsForTarget)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                    targetCpus = aotSettingsForTarget.GetDesktopCpu32Bit();
                    return TargetPlatform.Windows;
                case BuildTarget.StandaloneWindows64:
                    var windowsArchitecture = GetWindows64BitTargetArchitecture();

                    if (string.Equals(windowsArchitecture, "x64", StringComparison.OrdinalIgnoreCase))
                    {
                        targetCpus = aotSettingsForTarget.GetDesktopCpu64Bit();
                    }
                    else if (string.Equals(windowsArchitecture, "ARM64", StringComparison.OrdinalIgnoreCase))
                    {
                        targetCpus = new TargetCpus(BurstTargetCpu.ARMV8A_AARCH64);
                    }
                    else
                    {
                        throw new InvalidOperationException("Unknown Windows 64 Bit CPU architecture: " + windowsArchitecture);
                    }
                    return TargetPlatform.Windows;
                case BuildTarget.StandaloneOSX:
                    targetCpus = aotSettingsForTarget.GetDesktopCpu64Bit();
                    return TargetPlatform.macOS;
                case BuildTarget.StandaloneLinux64:
                    targetCpus = aotSettingsForTarget.GetDesktopCpu64Bit();
                    return TargetPlatform.Linux;
                case BuildTarget.WSAPlayer:
                    {
                        var uwpArchitecture = GetUWPTargetArchitecture();
                        if (string.Equals(uwpArchitecture, "x64", StringComparison.OrdinalIgnoreCase))
                        {
                            targetCpus = aotSettingsForTarget.GetDesktopCpu64Bit();
                        }
                        else if (string.Equals(uwpArchitecture, "x86", StringComparison.OrdinalIgnoreCase))
                        {
                            targetCpus = aotSettingsForTarget.GetDesktopCpu32Bit();
                        }
                        else if (string.Equals(uwpArchitecture, "ARM", StringComparison.OrdinalIgnoreCase))
                        {
                            targetCpus = new TargetCpus(BurstTargetCpu.THUMB2_NEON32);
                        }
                        else if (string.Equals(uwpArchitecture, "ARM64", StringComparison.OrdinalIgnoreCase))
                        {
                            targetCpus = new TargetCpus(BurstTargetCpu.ARMV8A_AARCH64);
                        }
                        else
                        {
                            throw new InvalidOperationException("Unknown UWP CPU architecture: " + uwpArchitecture);
                        }

                        return TargetPlatform.UWP;
                    }
                case BuildTarget.GameCoreXboxOne:
                    targetCpus = new TargetCpus(BurstTargetCpu.AVX);
                    return TargetPlatform.GameCoreXboxOne;
                case BuildTarget.GameCoreXboxSeries:
                    targetCpus = new TargetCpus(BurstTargetCpu.AVX2);
                    return TargetPlatform.GameCoreXboxSeries;
                case BuildTarget.PS4:
                    targetCpus = new TargetCpus(BurstTargetCpu.X64_SSE4);
                    return TargetPlatform.PS4;
                case BuildTarget.Android:
                    targetCpus = new TargetCpus(BurstTargetCpu.ARMV7A_NEON32);
                    return TargetPlatform.Android;
                case BuildTarget.iOS:
                    targetCpus = new TargetCpus(BurstTargetCpu.ARMV7A_NEON32);
                    return TargetPlatform.iOS;
                case BuildTarget.tvOS:
                    targetCpus = new TargetCpus(BurstTargetCpu.ARMV8A_AARCH64);
                    return TargetPlatform.tvOS;
#if !UNITY_2022_2_OR_NEWER
                case BuildTarget.Lumin:
                    targetCpus = new TargetCpus(BurstTargetCpu.ARMV8A_AARCH64);
                    return TargetPlatform.Lumin;
#endif
                case BuildTarget.Switch:
                    targetCpus = new TargetCpus(BurstTargetCpu.ARMV8A_AARCH64);
                    return TargetPlatform.Switch;
                case BuildTarget.PS5:
                    targetCpus = new TargetCpus(BurstTargetCpu.AVX2);
                    return TargetPlatform.PS5;
            }

#if UNITY_2023_3_OR_NEWER
            const int buildTargetVisionOS = (int)BuildTarget.VisionOS;
#else
            const int buildTargetVisionOS = 47;
#endif
            if ((int)target == buildTargetVisionOS)
            {
                targetCpus = new TargetCpus(BurstTargetCpu.ARMV8A_AARCH64);
                // An SDK Version of "0" indicates that the visionOS simulator SDK is not being
                // used, so assume this is the device SDK instead.
                return GetVisionSdkVersion() == 0 ? TargetPlatform.visionOS : TargetPlatform.visionSimulator;
            }

#if UNITY_2022_1_OR_NEWER
            const int qnxTarget = (int)BuildTarget.QNX;
#else
            const int qnxTarget = 46;
#endif
            if (qnxTarget == (int)target)
            {
                // QNX is supported on 2019.4 (shadow branch), 2020.3 (shadow branch) and 2022.1+ (official).
                var qnxArchitecture = GetQNXTargetArchitecture();
                if ("Arm64" == qnxArchitecture)
                {
                    targetCpus = new TargetCpus(BurstTargetCpu.ARMV8A_AARCH64);
                }
                else if ("X64" == qnxArchitecture)
                {
                    targetCpus = new TargetCpus(BurstTargetCpu.X64_SSE4);
                }
                else if ("X86" == qnxArchitecture)
                {
                    targetCpus = new TargetCpus(BurstTargetCpu.X86_SSE4);
                }
                else if ("Arm32" == qnxArchitecture)
                {
                    targetCpus = new TargetCpus(BurstTargetCpu.ARMV7A_NEON32);
                }
                else
                {
                    throw new InvalidOperationException("Unknown QNX CPU architecture: " + qnxArchitecture);
                }
                return TargetPlatform.QNX;
            }

#if UNITY_2021_2_OR_NEWER
            const int embeddedLinuxTarget = (int)BuildTarget.EmbeddedLinux;
#else
            const int embeddedLinuxTarget = 45;
#endif
            if (embeddedLinuxTarget == (int)target)
            {
                //EmbeddedLinux is supported on 2019.4 (shadow branch), 2020.3 (shadow branch) and 2021.2+ (official).
                var embeddedLinuxArchitecture = GetEmbeddedLinuxTargetArchitecture();
                if ("Arm64" == embeddedLinuxArchitecture)
                {
                    targetCpus = new TargetCpus(BurstTargetCpu.ARMV8A_AARCH64);
                }
                else if ("X64" == embeddedLinuxArchitecture)
                {
                    targetCpus = new TargetCpus(BurstTargetCpu.X64_SSE2); //lowest supported for now
                }
                else if (("X86" == embeddedLinuxArchitecture) || ("Arm32" == embeddedLinuxArchitecture))
                {
                    //32bit platforms cannot be support with the current SDK/Toolchain combination.
                    //i686-embedded-linux-gnu/8.3.0\libgcc.a(_moddi3.o + _divdi3.o): contains a compressed section, but zlib is not available
                    //_moddi3.o + _divdi3.o are required by LLVM for 64bit operations on 32bit platforms.
                    throw new InvalidOperationException($"No EmbeddedLinux Burst Support on {embeddedLinuxArchitecture} architecture.");
                }
                else
                {
                    throw new InvalidOperationException("Unknown EmbeddedLinux CPU architecture: " + embeddedLinuxArchitecture);
                }
                return TargetPlatform.EmbeddedLinux;
            }

            targetCpus = new TargetCpus(BurstTargetCpu.Auto);
            return null;
        }

        private static string GetWindows64BitTargetArchitecture()
        {
            var buildTargetName = BuildPipeline.GetBuildTargetName(BuildTarget.StandaloneWindows64);
            var architecture = EditorUserBuildSettings.GetPlatformSettings(buildTargetName, "Architecture").ToLowerInvariant();

            if (string.Equals(architecture, "x64", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(architecture, "ARM64", StringComparison.OrdinalIgnoreCase))
            {
                return architecture;
            }

            // Default to x64 if editor user build setting is garbage
            return "x64";
        }

        private static string GetUWPTargetArchitecture()
        {
            var architecture = EditorUserBuildSettings.wsaArchitecture;

            if (string.Equals(architecture, "x64", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(architecture, "x86", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(architecture, "ARM", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(architecture, "ARM64", StringComparison.OrdinalIgnoreCase))
            {
                return architecture;
            }

            // Default to x64 if editor user build setting is garbage
            return "x64";
        }

        private static string GetEmbeddedLinuxTargetArchitecture()
        {
            var flags = System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Static |
                        System.Reflection.BindingFlags.FlattenHierarchy;
            var property = typeof(EditorUserBuildSettings).GetProperty("selectedEmbeddedLinuxArchitecture", flags);
            if (null == property)
            {
                return "NOT_FOUND";
            }
            var value = (int)property.GetValue(null, null);
            switch (value)
            {
                case /*UnityEditor.EmbeddedLinuxArchitecture.Arm64*/ 0: return "Arm64";
                case /*UnityEditor.EmbeddedLinuxArchitecture.Arm32*/ 1: return "Arm32";
                case /*UnityEditor.EmbeddedLinuxArchitecture.X64*/   2: return "X64";
                case /*UnityEditor.EmbeddedLinuxArchitecture.X86*/   3: return "X86";
                default: return $"UNKNOWN_{value}";
            }
        }

        private static string GetQNXTargetArchitecture()
        {
            var flags = System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Static |
                        System.Reflection.BindingFlags.FlattenHierarchy;
            var property = typeof(EditorUserBuildSettings).GetProperty("selectedQnxArchitecture", flags);
            if (null == property)
            {
                return "NOT_FOUND";
            }
            var value = (int)property.GetValue(null, null);
            switch (value)
            {
                case /*UnityEditor.QNXArchitecture.Arm64*/ 0: return "Arm64";
                case /*UnityEditor.QNXArchitecture.Arm32*/ 1: return "Arm32";
                case /*UnityEditor.QNXArchitecture.X64*/   2: return "X64";
                case /*UnityEditor.QNXArchitecture.X86*/   3: return "X86";
                default: return $"UNKNOWN_{value}";
            }
        }

        private static int GetVisionSdkVersion()
        {
            var playerSettings = typeof(PlayerSettings);
            var visionOs = playerSettings.GetNestedType("VisionOS");
            if (visionOs == null) throw new Exception("Editor does not appear to support visionOS");

            var property = visionOs.GetProperty("sdkVersion",  System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (property == null) throw new Exception("Property `sdkVersion` not found");
            var sdkVersion = property.GetValue(null, null);
            return (int)sdkVersion;
        }

        /// <summary>
        /// Defines an output path (for the generated code) and the target CPU
        /// </summary>
        internal struct BurstOutputCombination
        {
            public readonly TargetCpus TargetCpus;
            public readonly string OutputPath;
            public readonly string LibraryName;
            public readonly bool CollateDirectory;
            public readonly bool WorkaroundFullDebugInfo;

            public BurstOutputCombination(string outputPath, TargetCpus targetCpus, string libraryName = DefaultLibraryName, bool collateDirectory = false, bool workaroundBrokenDebug=false)
            {
                TargetCpus = targetCpus.Clone();
                OutputPath = outputPath;
                LibraryName = libraryName;
                CollateDirectory = collateDirectory;
                WorkaroundFullDebugInfo = workaroundBrokenDebug;
            }

            public void HashInto(ref Hash128 hc)
            {
                hc.Append(TargetCpus.Cpus.Count);
                foreach (var cpu in TargetCpus.Cpus)
                {
                    hc.Append((int)cpu);
                }

                hc.Append(OutputPath);
                hc.Append(LibraryName);
                hc.Append(CollateDirectory ? 1 : 0);
                hc.Append(WorkaroundFullDebugInfo ? 1 : 0);
            }

            public override string ToString()
            {
                return $"{nameof(TargetCpus)}: {TargetCpus}, {nameof(OutputPath)}: {OutputPath}, {nameof(LibraryName)}: {LibraryName}";
            }
        }

        private class BclRunner
        {
            private static readonly Regex MatchVersion = new Regex(@"com.unity.burst@(\d+.*?)[\\/]");

            public static void RunManagedProgram(string exe, string args, CompilerOutputParserBase parser)
            {
                RunManagedProgram(exe, args, Application.dataPath + "/..", parser);
            }

            private static void RunManagedProgram(
              string exe,
              string args,
              string workingDirectory,
              CompilerOutputParserBase parser)
            {
                Program p;
                bool unreliableExitCode = false;
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                    {
                        // For Windows on Arm 64, we need to be explicit and specify the architecture we want our
                        //managed process to run with - unfortunately, doing this means the exit code of the process
                        //no longer reflects if compilation was successful, so unreliableExitCode works around this
                        args = "/c start /machine arm64 /b /WAIT " + exe + " " + args;
                        exe = "cmd";
                        unreliableExitCode = true;
                    }
                    ProcessStartInfo si = new ProcessStartInfo()
                    {
                        Arguments = args,
                        CreateNoWindow = true,
                        FileName = exe,
                    };
                    p = new Program(si);
                }
                else
                {
                    p = (Program) new ManagedProgram(MonoInstallationFinder.GetMonoInstallation("MonoBleedingEdge"), (string) null, exe, args, false, null);
                }

                RunProgram(p, exe, args, workingDirectory, parser, unreliableExitCode);
            }

            public static void RunNativeProgram(string exe, string args, CompilerOutputParserBase parser)
            {
                RunNativeProgram(exe, args, Application.dataPath + "/..", parser);
            }

            private static void RunNativeProgram(string exePath, string arguments, string workingDirectory, CompilerOutputParserBase parser)
            {
                // On non Windows platform, make sure that the command is executable
                // This is a workaround - occasionally the execute bits are lost from our package
                if (Application.platform != RuntimePlatform.WindowsEditor && Path.IsPathRooted(exePath))
                {
                    var escapedExePath = EscapeForShell(exePath, singleQuoteWrapped: true);
                    var shArgs = $"-c '[ ! -x {escapedExePath} ] && chmod 755 {escapedExePath}'";

                    var p = new Program(new ProcessStartInfo("sh", shArgs) { CreateNoWindow = true});
                    p.GetProcessStartInfo().WorkingDirectory = workingDirectory;
                    p.Start();
                    p.WaitForExit();
                }

                var startInfo = new ProcessStartInfo(exePath, arguments);
                startInfo.CreateNoWindow = true;

                RunProgram(new Program(startInfo), exePath, arguments, workingDirectory, parser, false);
            }

            public static string EscapeForShell(string s, bool singleQuoteWrapped = false)
            {
                // On Windows it's enough to enclose the path in double quotes (double quotes are not allowed in paths)
                if (Application.platform == RuntimePlatform.WindowsEditor) return $"\"{s}\"";

                // On non-windows platforms we enclose in single-quotes and escape any existing single quotes with: '\'':
                //    John's Folder => 'John'\''s Folder'
                var sb = new StringBuilder();
                var escaped = s.Replace("'", "'\\''");
                sb.Append('\'');
                sb.Append(escaped);
                sb.Append('\'');

                // If the outer-context is already wrapped in single-quotes, we need to double escape things:
                //    John's Folder => 'John'\''s Folder'
                //                  => '\''John'\''\'\'''\''s Folder'\''
                if (singleQuoteWrapped)
                {
                    // Pain
                    return sb.ToString().Replace("'", "'\\''");
                }

                return sb.ToString();
            }



            public static void RunProgram(
              Program p,
              string exe,
              string args,
              string workingDirectory,
              CompilerOutputParserBase parser,
              bool unreliableExitCode)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                using (p)
                {
                    p.GetProcessStartInfo().WorkingDirectory = workingDirectory;
                    p.Start();
                    p.WaitForExit();
                    stopwatch.Stop();

                    Console.WriteLine("{0} exited after {1} ms.", (object)exe, (object)stopwatch.ElapsedMilliseconds);
                    IEnumerable<UnityEditor.Scripting.Compilers.CompilerMessage> compilerMessages = null;
                    string[] errorOutput = p.GetErrorOutput();
                    string[] standardOutput = p.GetStandardOutput();
                    if (parser != null)
                    {
                        compilerMessages = parser.Parse(errorOutput, standardOutput, true, "n/a (burst)");
                    }

                    var errorMessageBuilder = new StringBuilder();

                    if (compilerMessages != null)
                    {
                        foreach (UnityEditor.Scripting.Compilers.CompilerMessage compilerMessage in compilerMessages)
                        {
                            switch (compilerMessage.type)
                            {
                                case CompilerMessageType.Warning:
                                    Debug.LogWarning(compilerMessage.message, compilerMessage.file, compilerMessage.line, compilerMessage.column);
                                    break;
                                case CompilerMessageType.Error:
                                    Debug.LogPlayerBuildError(compilerMessage.message, compilerMessage.file, compilerMessage.line, compilerMessage.column);
                                    break;
                            }
                        }
                    }

                    var exitFailed = p.ExitCode != 0;
                    if (unreliableExitCode)
                    {
                        // Bcl +burstc will emit a Done as the final stdout if everything was ok.
                        exitFailed = (standardOutput.Length==0) || (standardOutput[standardOutput.Length - 1].Trim() != "Done");
                    }

                    if (exitFailed)
                    {
                        // We try to output the version in the heading error if we can
                        var matchVersion = MatchVersion.Match(exe);
                        errorMessageBuilder.Append(matchVersion.Success ?
                            "Burst compiler (" + matchVersion.Groups[1].Value + ") failed running" :
                            "Burst compiler failed running");
                        errorMessageBuilder.AppendLine();
                        errorMessageBuilder.AppendLine();
                        // Don't output the path if we are not burst-debugging or the exe exist
                        if (BurstLoader.IsDebugging || !File.Exists(exe))
                        {
                            errorMessageBuilder.Append(exe).Append(" ").Append(args);
                            errorMessageBuilder.AppendLine();
                            errorMessageBuilder.AppendLine();
                        }

                        errorMessageBuilder.AppendLine("stdout:");
                        foreach (string str in standardOutput)
                            errorMessageBuilder.AppendLine(str);
                        errorMessageBuilder.AppendLine("stderr:");
                        foreach (string str in errorOutput)
                            errorMessageBuilder.AppendLine(str);

                        throw new BuildFailedException(errorMessageBuilder.ToString());
                    }
                    Console.WriteLine(p.GetAllOutput());
                }
            }
        }

        /// <summary>
        /// Internal class used to parse bcl output errors
        /// </summary>
        private class BclOutputErrorParser : CompilerOutputParserBase
        {
            // Format of an error message:
            //
            //C:\work\burst\src\Burst.Compiler.IL.Tests\Program.cs(17,9): error: Loading a managed string literal is not supported by burst
            // at Buggy.NiceBug() (at C:\work\burst\src\Burst.Compiler.IL.Tests\Program.cs:17)
            //
            //
            //                                                                [1]    [2]         [3]        [4]         [5]
            //                                                                path   line        col        type        message
            private static readonly Regex MatchLocation = new Regex(@"^(.*?)\((\d+)\s*,\s*(\d+)\):\s*([\w\s]+)\s*:\s*(.*)");

            // Matches " at "
            private static readonly Regex MatchAt = new Regex(@"^\s+at\s+");

            public override IEnumerable<UnityEditor.Scripting.Compilers.CompilerMessage> Parse(
                string[] errorOutput,
                string[] standardOutput,
                bool compilationHadFailure,
                string assemblyName)
            {
                var messages = new List<UnityEditor.Scripting.Compilers.CompilerMessage>();
                var textBuilder = new StringBuilder();
                for (var i = 0; i < errorOutput.Length; i++)
                {
                    string line = errorOutput[i];

                    var message = new UnityEditor.Scripting.Compilers.CompilerMessage {assemblyName = assemblyName};

                    // If we are able to match a location, we can decode it including the following attached " at " lines
                    textBuilder.Clear();

                    var match = MatchLocation.Match(line);
                    if (match.Success)
                    {
                        var path = match.Groups[1].Value;
                        int.TryParse(match.Groups[2].Value, out message.line);
                        int.TryParse(match.Groups[3].Value, out message.column);
                        if (match.Groups[4].Value.Contains("error"))
                        {
                            message.type = CompilerMessageType.Error;
                        }
                        else
                        {
                            message.type = CompilerMessageType.Warning;
                        }
                        message.file = !string.IsNullOrEmpty(path) ? path : "unknown";
                        // Replace '\' with '/' to let the editor open the file
                        message.file = message.file.Replace('\\', '/');

                        // Make path relative to project path path
                        var projectPath = Path.GetDirectoryName(Application.dataPath)?.Replace('\\', '/');
                        if (projectPath != null && message.file.StartsWith(projectPath))
                        {
                            message.file = message.file.Substring(projectPath.EndsWith("/") ? projectPath.Length : projectPath.Length + 1);
                        }

                        // debug
                        // textBuilder.AppendLine("line: " + message.line + " column: " + message.column + " error: " + message.type + " file: " + message.file);
                        textBuilder.Append(match.Groups[5].Value);
                    }
                    else
                    {
                        // Don't output any blank line
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }
                        // Otherwise we output an error, but without source location information
                        // so that at least the user can see it directly in the log errors
                        message.type = CompilerMessageType.Error;
                        message.line = 0;
                        message.column = 0;
                        message.file = "unknown";


                        textBuilder.Append(line);
                    }

                    // Collect attached location call context information ("at ...")
                    // we do it for both case (as if we have an exception in bcl we want to print this in a single line)
                    bool isFirstAt = true;
                    for (int j = i + 1; j < errorOutput.Length; j++)
                    {
                        var nextLine = errorOutput[j];

                        // Empty lines are ignored by the stack trace parser.
                        if (string.IsNullOrWhiteSpace(nextLine))
                        {
                            i++;
                            continue;
                        }

                        if (MatchAt.Match(nextLine).Success)
                        {
                            i++;
                            if (isFirstAt)
                            {
                                textBuilder.AppendLine();
                                isFirstAt = false;
                            }
                            textBuilder.AppendLine(nextLine);
                        }
                        else
                        {
                            break;
                        }
                    }
                    message.message = textBuilder.ToString();

                    messages.Add(message);
                }
                return messages;
            }

            protected override string GetErrorIdentifier()
            {
                throw new NotImplementedException(); // as we overriding the method Parse()
            }

            protected override Regex GetOutputRegex()
            {
                throw new NotImplementedException(); // as we overriding the method Parse()
            }
        }

#if UNITY_EDITOR_OSX && !ENABLE_GENERATE_NATIVE_PLUGINS_FOR_ASSEMBLIES_API
        private class StaticLibraryPostProcessor
        {
            private const string TempSourceLibrary = @"Temp/StagingArea/StaticLibraries";
            [PostProcessBuildAttribute(1)]
            public static void OnPostProcessBuild(BuildTarget target, string path)
            {
                // Early out if we are building for the simulator, as we don't
				//currently generate burst libraries that will work for that.
                if (IsForSimulator(target))
                {
                    return;
                }
                // We only support AOT compilation for ios from a macos host (we require xcrun and the apple tool chains)
                //for other hosts, we simply act as if burst is not being used (an error will be generated by the build aot step)
                //this keeps the behaviour consistent with how it was before static linkage was introduced
                if (target == BuildTarget.iOS)
                {
                    var aotSettingsForTarget = BurstPlatformAotSettings.GetOrCreateSettings(BuildTarget.iOS);

                    // Early exit if burst is not activated
                    if (!aotSettingsForTarget.EnableBurstCompilation)
                    {
                        return;
                    }
                    PostAddStaticLibraries(path);
                }
                if (target == BuildTarget.tvOS)
                {
                    var aotSettingsForTarget = BurstPlatformAotSettings.GetOrCreateSettings(BuildTarget.tvOS);

                    // Early exit if burst is not activated
                    if (!aotSettingsForTarget.EnableBurstCompilation)
                    {
                        return;
                    }
                    PostAddStaticLibraries(path);
                }
#if UNITY_2023_3_OR_NEWER
                if (target == BuildTarget.visionOS)
                {
                    var aotSettingsForTarget = BurstPlatformAotSettings.GetOrCreateSettings(BuildTarget.visionOS);

                    // Early exit if burst is not activated
                    if (!aotSettingsForTarget.EnableBurstCompilation)
                    {
                        return;
                    }
                    PostAddStaticLibraries(path);
                }
#endif
            }

            private static void PostAddStaticLibraries(string path)
            {
                var assm = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly =>
                    assembly.GetName().Name == "UnityEditor.iOS.Extensions.Xcode");
                Type PBXType = assm?.GetType("UnityEditor.iOS.Xcode.PBXProject");
                Type PBXSourceTree = assm?.GetType("UnityEditor.iOS.Xcode.PBXSourceTree");
                if (PBXType != null && PBXSourceTree != null)
                {
                    var project = Activator.CreateInstance(PBXType, null);

                    var _sGetPBXProjectPath = PBXType.GetMethod("GetPBXProjectPath");
                    var _ReadFromFile = PBXType.GetMethod("ReadFromFile");
                    var _sGetUnityTargetName = PBXType.GetMethod("GetUnityTargetName");
                    var _AddFileToBuild = PBXType.GetMethod("AddFileToBuild");
                    var _AddFile = PBXType.GetMethod("AddFile");
                    var _WriteToString = PBXType.GetMethod("WriteToString");

                    var sourcetree = new EnumConverter(PBXSourceTree).ConvertFromString("Source");

                    string sPath = (string)_sGetPBXProjectPath?.Invoke(null, new object[] { path });
                    _ReadFromFile?.Invoke(project, new object[] { sPath });

                    var _TargetGuidByName = PBXType.GetMethod("GetUnityFrameworkTargetGuid");
                    string g = (string) _TargetGuidByName?.Invoke(project, null);

                    var srcPath = TempSourceLibrary;
                    var dstPath = "Libraries";
                    var dstCopyPath = Path.Combine(path, dstPath);

                    var burstCppLinkFile = "lib_burst_generated.cpp";

                    var libName = $"{DefaultLibraryName}.a";
                    var libSrcPath = Path.Combine(srcPath, libName);
                    var libExists = File.Exists(libSrcPath);

                    if (!libExists)
                    {
                        return; // No libs, so don't write the cpp either
                    }

                    File.Copy(libSrcPath, Path.Combine(dstCopyPath, libName));
                    AddLibToProject(project, _AddFileToBuild, _AddFile, sourcetree, g, dstPath, libName);

                    // Additionally we need a small cpp file (weak symbols won't unfortunately override directly from the libs
                    //presumably due to link order?
                    WriteStaticLinkCppFile(dstCopyPath);
                    string cppPath = Path.Combine(dstPath, burstCppLinkFile);
                    string fileg = (string)_AddFile?.Invoke(project, new object[] { cppPath, cppPath, sourcetree });
                    _AddFileToBuild?.Invoke(project, new object[] { g, fileg });

                    string pstring = (string)_WriteToString?.Invoke(project, null);
                    File.WriteAllText(sPath, pstring);
                }
            }

            private static void AddLibToProject(object project, System.Reflection.MethodInfo _AddFileToBuild, System.Reflection.MethodInfo _AddFile, object sourcetree, string g, string dstPath, string lib32Name)
            {
                string fg = (string)_AddFile?.Invoke(project,
                    new object[] { Path.Combine(dstPath, lib32Name), Path.Combine(dstPath, lib32Name), sourcetree });
                _AddFileToBuild?.Invoke(project, new object[] { g, fg });
            }
        }
#endif
    }
}
#endif
