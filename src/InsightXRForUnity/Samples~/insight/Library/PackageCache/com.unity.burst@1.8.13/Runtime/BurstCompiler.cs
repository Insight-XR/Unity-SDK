using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine.Scripting;
using System.Linq;
using System.Text;

namespace Unity.Burst
{
    /// <summary>
    /// The burst compiler runtime frontend.
    /// </summary>
    ///
    public static class BurstCompiler
    {
        /// <summary>
        /// Check if the LoadAdditionalLibrary API is supported by the current version of Unity
        /// </summary>
        /// <returns>True if the LoadAdditionalLibrary API can be used by the current version of Unity</returns>
        public static bool IsLoadAdditionalLibrarySupported()
        {
            return IsApiAvailable("LoadBurstLibrary");
        }

#if UNITY_EDITOR
        static unsafe BurstCompiler()
        {
            // Store pointers to Log and Compile callback methods.
            // For more info about why we need to do this, see comments in CallbackStubManager.
            string GetFunctionPointer<TDelegate>(TDelegate callback)
            {
                GCHandle.Alloc(callback); // Ensure delegate is never garbage-collected.
                var callbackFunctionPointer = Marshal.GetFunctionPointerForDelegate(callback);
                return "0x" + callbackFunctionPointer.ToInt64().ToString("X16");
            }

            EagerCompileLogCallbackFunctionPointer = GetFunctionPointer<LogCallbackDelegate>(EagerCompileLogCallback);
            ManagedResolverFunctionPointer = GetFunctionPointer<ManagedFnPtrResolverDelegate>(ManagedResolverFunction);
            ProgressCallbackFunctionPointer = GetFunctionPointer<ProgressCallbackDelegate>(ProgressCallback);
            ProfileBeginCallbackFunctionPointer = GetFunctionPointer<ProfileBeginCallbackDelegate>(ProfileBeginCallback);
            ProfileEndCallbackFunctionPointer = GetFunctionPointer<ProfileEndCallbackDelegate>(ProfileEndCallback);
        }
#endif

        private class CommandBuilder
        {
            private StringBuilder _builder;
            private bool _hasArgs;

            public CommandBuilder()
            {
                _builder = new StringBuilder();
                _hasArgs = false;
            }

            public CommandBuilder Begin(string cmd)
            {
                _builder.Clear();
                _hasArgs = false;
                _builder.Append(cmd);
                return this;
            }

            public CommandBuilder With(string arg)
            {
                if (!_hasArgs) _builder.Append(' ');
                _hasArgs = true;
                _builder.Append(arg);
                return this;
            }

            public CommandBuilder With(IntPtr arg)
            {
                if (!_hasArgs) _builder.Append(' ');
                _hasArgs = true;
                _builder.AppendFormat("0x{0:X16}", arg.ToInt64());
                return this;
            }

            public CommandBuilder And(char sep = '|')
            {
                _builder.Append(sep);
                return this;
            }

            public string SendToCompiler()
            {
                return SendRawCommandToCompiler(_builder.ToString());
            }
        }

        [ThreadStatic]
        private static CommandBuilder _cmdBuilder;

        private static CommandBuilder BeginCompilerCommand(string cmd)
        {
            if (_cmdBuilder == null)
            {
                _cmdBuilder = new CommandBuilder();
            }

            return _cmdBuilder.Begin(cmd);
        }

#if BURST_INTERNAL
        [ThreadStatic]
        public static Func<object, IntPtr> InternalCompiler;
#endif

        /// <summary>
        /// Internal variable setup by BurstCompilerOptions.
        /// </summary>
#if BURST_INTERNAL

        [ThreadStatic] // As we are changing this boolean via BurstCompilerOptions in btests and we are running multithread tests
                       // we would change a global and it would generate random errors, so specifically for btests, we are using a TLS.
        public
#else
        internal
#endif
        static bool _IsEnabled;

        /// <summary>
        /// Gets a value indicating whether Burst is enabled.
        /// </summary>
#if UNITY_EDITOR || BURST_INTERNAL
        public static bool IsEnabled => _IsEnabled;
#else
        public static bool IsEnabled => _IsEnabled && BurstCompilerHelper.IsBurstGenerated;
#endif

        /// <summary>
        /// Gets the global options for the burst compiler.
        /// </summary>
        public static readonly BurstCompilerOptions Options = new BurstCompilerOptions(true);

        /// <summary>
        /// Sets the execution mode for all jobs spawned from now on.
        /// </summary>
        /// <param name="mode">Specifiy the required execution mode</param>
        public static void SetExecutionMode(BurstExecutionEnvironment mode)
        {
            Burst.LowLevel.BurstCompilerService.SetCurrentExecutionMode((uint)mode);
        }
        /// <summary>
        /// Retrieve the current execution mode that is configured.
        /// </summary>
        /// <returns>Currently configured execution mode</returns>
        public static BurstExecutionEnvironment GetExecutionMode()
        {
            return (BurstExecutionEnvironment)Burst.LowLevel.BurstCompilerService.GetCurrentExecutionMode();
        }

        /// <summary>
        /// Compile the following delegate with burst and return a new delegate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="delegateMethod"></param>
        /// <returns></returns>
        /// <remarks>NOT AVAILABLE, unsafe to use</remarks>
        internal static unsafe T CompileDelegate<T>(T delegateMethod) where T : class
        {
            // We have added support for runtime CompileDelegate in 2018.2+
            void* function = Compile(delegateMethod, false);
            object res = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer((IntPtr)function, delegateMethod.GetType());
            return (T)res;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void VerifyDelegateIsNotMulticast<T>(T delegateMethod) where T : class
        {
            var delegateKind = delegateMethod as Delegate;
            if (delegateKind.GetInvocationList().Length > 1)
            {
                throw new InvalidOperationException($"Burst does not support multicast delegates, please use a regular delegate for `{delegateMethod}'");
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void VerifyDelegateHasCorrectUnmanagedFunctionPointerAttribute<T>(T delegateMethod) where T : class
        {
            var attrib = delegateMethod.GetType().GetCustomAttribute<System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute>();
            if (attrib == null || attrib.CallingConvention != CallingConvention.Cdecl)
            {
#if !BURST_INTERNAL
                UnityEngine.Debug.LogWarning($"The delegate type {delegateMethod.GetType().FullName} should be decorated with [UnmanagedFunctionPointer(CallingConvention.Cdecl)] to ensure runtime interoperabilty between managed code and Burst-compiled code.");
#endif
            }
        }

        /// <summary>
        /// DO NOT USE - deprecated.
        /// </summary>
        /// <param name="burstMethodHandle">The Burst method to compile.</param>
        /// <param name="managedMethodHandle">The fallback managed method to use.</param>
        /// <param name="delegateTypeHandle">The type of the delegate used to execute these methods.</param>
        /// <returns>Nothing</returns>
        [Obsolete("This method will be removed in a future version of Burst")]
        public static unsafe IntPtr CompileILPPMethod(RuntimeMethodHandle burstMethodHandle, RuntimeMethodHandle managedMethodHandle, RuntimeTypeHandle delegateTypeHandle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compile an IL Post-Processed method.
        /// </summary>
        /// <param name="burstMethodHandle">The Burst method to compile.</param>
        /// <returns>A token that must be passed to <see cref="GetILPPMethodFunctionPointer2"/> to get an actual executable function pointer.</returns>
        public static unsafe IntPtr CompileILPPMethod2(RuntimeMethodHandle burstMethodHandle)
        {
            if (burstMethodHandle.Value == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(burstMethodHandle));
            }

            OnCompileILPPMethod2?.Invoke();

            var burstMethod = (MethodInfo)MethodBase.GetMethodFromHandle(burstMethodHandle);

            return (IntPtr)Compile(new FakeDelegate(burstMethod), burstMethod, isFunctionPointer: true, isILPostProcessing: true);
        }

        internal static Action OnCompileILPPMethod2;

        /// <summary>
        /// DO NOT USE - deprecated.
        /// </summary>
        /// <param name="ilppMethod">The result of a previous call to <see cref="CompileILPPMethod"/>.</param>
        /// <returns>Nothing.</returns>
        [Obsolete("This method will be removed in a future version of Burst")]
        public static unsafe void* GetILPPMethodFunctionPointer(IntPtr ilppMethod)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// For a previous call to <see cref="CompileILPPMethod2"/>, get the actual executable function pointer.
        /// </summary>
        /// <param name="ilppMethod">The result of a previous call to <see cref="CompileILPPMethod"/>.</param>
        /// <param name="managedMethodHandle">The fallback managed method to use.</param>
        /// <param name="delegateTypeHandle">The type of the delegate used to execute these methods.</param>
        /// <returns>A pointer into an executable region, for running the function pointer.</returns>
        public static unsafe void* GetILPPMethodFunctionPointer2(IntPtr ilppMethod, RuntimeMethodHandle managedMethodHandle, RuntimeTypeHandle delegateTypeHandle)
        {
            if (ilppMethod == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(ilppMethod));
            }

            if (managedMethodHandle.Value == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(managedMethodHandle));
            }

            if (delegateTypeHandle.Value == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(delegateTypeHandle));
            }

            // If we are in the editor, we need to route a command to the compiler to start compiling the deferred ILPP compilation.
            // Otherwise if we're in Burst's internal testing, or in a player build, we already actually have the actual executable
            // pointer address, and we just return that.
#if UNITY_EDITOR
            var managedMethod = (MethodInfo)MethodBase.GetMethodFromHandle(managedMethodHandle);
            var delegateType = Type.GetTypeFromHandle(delegateTypeHandle);
            var managedFallbackDelegate = Delegate.CreateDelegate(delegateType, managedMethod);

            var handle = GCHandle.Alloc(managedFallbackDelegate);

            var result =
                BeginCompilerCommand(BurstCompilerOptions.CompilerCommandILPPCompilation)
                    .With(ilppMethod).And()
                    .With(ManagedResolverFunctionPointer).And()
                    .With(GCHandle.ToIntPtr(handle))
                    .SendToCompiler();

            return new IntPtr(Convert.ToInt64(result, 16)).ToPointer();
#else
            return ilppMethod.ToPointer();
#endif
        }

        /// <summary>
        /// DO NOT USE - deprecated.
        /// </summary>
        /// <param name="handle">A runtime method handle.</param>
        /// <returns>Nothing.</returns>
        [Obsolete("This method will be removed in a future version of Burst")]
        public static unsafe void* CompileUnsafeStaticMethod(RuntimeMethodHandle handle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compile the following delegate into a function pointer with burst, invokable from a Burst Job or from regular C#.
        /// </summary>
        /// <typeparam name="T">Type of the delegate of the function pointer</typeparam>
        /// <param name="delegateMethod">The delegate to compile</param>
        /// <returns>A function pointer invokable from a Burst Job or from regular C#</returns>
        public static unsafe FunctionPointer<T> CompileFunctionPointer<T>(T delegateMethod) where T : class
        {
            VerifyDelegateIsNotMulticast<T>(delegateMethod);
            VerifyDelegateHasCorrectUnmanagedFunctionPointerAttribute<T>(delegateMethod);
            // We have added support for runtime CompileDelegate in 2018.2+
            void* function = Compile(delegateMethod, true);
            return new FunctionPointer<T>(new IntPtr(function));
        }

        [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
        internal class StaticTypeReinitAttribute : Attribute
        {
            public readonly Type reinitType;

            public StaticTypeReinitAttribute(Type toReinit)
            {
                reinitType = toReinit;
            }
        }

        private static unsafe void* Compile(object delegateObj, bool isFunctionPointer)
        {
            if (!(delegateObj is Delegate)) throw new ArgumentException("object instance must be a System.Delegate", nameof(delegateObj));
            var delegateMethod = (Delegate)delegateObj;
            return Compile(delegateMethod, delegateMethod.Method, isFunctionPointer, false);
        }

        private static unsafe void* Compile(object delegateObj, MethodInfo methodInfo, bool isFunctionPointer, bool isILPostProcessing)
        {
            if (delegateObj == null) throw new ArgumentNullException(nameof(delegateObj));

            if (delegateObj.GetType().IsGenericType)
            {
                throw new InvalidOperationException($"The delegate type `{delegateObj.GetType()}` must be a non-generic type");
            }
            if (!methodInfo.IsStatic)
            {
                throw new InvalidOperationException($"The method `{methodInfo}` must be static. Instance methods are not supported");
            }
            if (methodInfo.IsGenericMethod)
            {
                throw new InvalidOperationException($"The method `{methodInfo}` must be a non-generic method");
            }

#if ENABLE_IL2CPP
            if (isFunctionPointer && !isILPostProcessing &&
                methodInfo.GetCustomAttributes().All(s => s.GetType().Name != "MonoPInvokeCallbackAttribute"))
            {
                UnityEngine.Debug.Log($"The method `{methodInfo}` must have `MonoPInvokeCallback` attribute to be compatible with IL2CPP!");
            }
#endif

            void* function;

#if BURST_INTERNAL
            // Internally in Burst tests, we callback the C# method instead
            function = (void*)InternalCompiler(delegateObj);
#else

            Delegate managedFallbackDelegateMethod = null;

            if (!isILPostProcessing)
            {
                managedFallbackDelegateMethod = delegateObj as Delegate;
            }

            var delegateMethod = delegateObj as Delegate;

#if UNITY_EDITOR
            string defaultOptions;

            // In case Burst is disabled entirely from the command line
            if (BurstCompilerOptions.ForceDisableBurstCompilation)
            {
                if (isILPostProcessing)
                {
                    return null;
                }
                else
                {
                    GCHandle.Alloc(managedFallbackDelegateMethod);
                    function = (void*)Marshal.GetFunctionPointerForDelegate(managedFallbackDelegateMethod);
                    return function;
                }
            }

            if (isILPostProcessing)
            {
                defaultOptions = "--" + BurstCompilerOptions.OptionJitIsForFunctionPointer + "\n";
            }
            else if (isFunctionPointer)
            {
                defaultOptions = "--" + BurstCompilerOptions.OptionJitIsForFunctionPointer + "\n";
                // Make sure that the delegate will never be collected
                var delHandle = GCHandle.Alloc(managedFallbackDelegateMethod);
                defaultOptions += "--" + BurstCompilerOptions.OptionJitManagedDelegateHandle + "0x" + ManagedResolverFunctionPointer + "|" + "0x" + GCHandle.ToIntPtr(delHandle).ToInt64().ToString("X16");
            }
            else
            {
                defaultOptions = "--" + BurstCompilerOptions.OptionJitEnableSynchronousCompilation;
            }

            string extraOptions;
            // The attribute is directly on the method, so we recover the underlying method here
            if (Options.TryGetOptions(methodInfo, out extraOptions, isForILPostProcessing: isILPostProcessing))
            {
                if (!string.IsNullOrWhiteSpace(extraOptions))
                {
                    defaultOptions += "\n" + extraOptions;
                }

                var delegateMethodId = Unity.Burst.LowLevel.BurstCompilerService.CompileAsyncDelegateMethod(delegateObj, defaultOptions);
                function = Unity.Burst.LowLevel.BurstCompilerService.GetAsyncCompiledAsyncDelegateMethod(delegateMethodId);
            }
#else
            // The attribute is directly on the method, so we recover the underlying method here
            if (BurstCompilerOptions.HasBurstCompileAttribute(methodInfo))
            {
                if (Options.EnableBurstCompilation && BurstCompilerHelper.IsBurstGenerated)
                {
                    var delegateMethodId = Unity.Burst.LowLevel.BurstCompilerService.CompileAsyncDelegateMethod(delegateObj, string.Empty);
                    function = Unity.Burst.LowLevel.BurstCompilerService.GetAsyncCompiledAsyncDelegateMethod(delegateMethodId);
                }
                else
                {
                    // If this is for direct-call, and we're in a player, with Burst disabled, then we should return null,
                    // since we don't actually have a managedFallbackDelegateMethod at this point.
                    if (isILPostProcessing)
                    {
                        return null;
                    }

                    // Make sure that the delegate will never be collected
                    GCHandle.Alloc(managedFallbackDelegateMethod);
                    // If we are in a standalone player, and burst is disabled and we are actually
                    // trying to load a function pointer, in that case we need to support it
                    // so we are then going to use the managed function directly
                    // NOTE: When running under IL2CPP, this could lead to a `System.NotSupportedException : To marshal a managed method, please add an attribute named 'MonoPInvokeCallback' to the method definition.`
                    // so in that case, the method needs to have `MonoPInvokeCallback`
                    // but that's a requirement for IL2CPP, not an issue with burst
                    function = (void*)Marshal.GetFunctionPointerForDelegate(managedFallbackDelegateMethod);
                }
            }
#endif
            else
            {
                throw new InvalidOperationException($"Burst cannot compile the function pointer `{methodInfo}` because the `[BurstCompile]` attribute is missing");
            }
#endif
            // Should not happen but in that case, we are still trying to generated an error
            // It can be null if we are trying to compile a function in a standalone player
            // and the function was not compiled. In that case, we need to output an error
            if (function == null)
            {
                throw new InvalidOperationException($"Burst failed to compile the function pointer `{methodInfo}`");
            }

            // When burst compilation is disabled, we are still returning a valid stub function pointer (the a pointer to the managed function)
            // so that CompileFunctionPointer actually returns a delegate in all cases
            return function;
        }

        /// <summary>
        /// Lets the compiler service know we are shutting down, called by the event on OnDomainUnload, if EditorApplication.quitting was called
        /// </summary>
        internal static void Shutdown()
        {
#if UNITY_EDITOR
            SendCommandToCompiler(BurstCompilerOptions.CompilerCommandShutdown);
#endif
        }

#if UNITY_EDITOR
        internal static void SetDefaultOptions()
        {
            SendCommandToCompiler(BurstCompilerOptions.CompilerCommandSetDefaultOptions, Options.GetOptions(isForCompilerClient: true));
        }
#endif

#if UNITY_EDITOR
        // We need this to be queried each domain reload in a static constructor so that it is called on the main thread only!
        internal static readonly bool IsScriptDebugInfoEnabled = UnityEditor.Compilation.CompilationPipeline.IsScriptDebugInfoEnabled();

        private sealed class DomainReloadStateSingleton : UnityEditor.ScriptableSingleton<DomainReloadStateSingleton>
        {
            public bool AlreadyLoaded = false;
            public bool IsScriptDebugInfoEnabled = false;
        }

        internal static bool WasScriptDebugInfoEnabledAtDomainReload => DomainReloadStateSingleton.instance.IsScriptDebugInfoEnabled;

        internal static void DomainReload()
        {
            const string parameterSeparator = "***";
            const string assemblySeparator = "```";

            var isScriptDebugInfoEnabled = IsScriptDebugInfoEnabled;

            var cmdBuilder =
                BeginCompilerCommand(BurstCompilerOptions.CompilerCommandDomainReload)
                    .With(ProgressCallbackFunctionPointer)
                    .With(parameterSeparator)
                    .With(EagerCompileLogCallbackFunctionPointer)
                    .With(parameterSeparator)
                    .With(isScriptDebugInfoEnabled ? "Debug" : "Release")
                    .With(parameterSeparator);

            // We need to send the list of assemblies if
            // (a) we have never done that before in this Editor instance, or
            // (b) we have done it before, but now the scripting code optimization mode has changed
            //     from Debug to Release or vice-versa.
            // This is because these are the two cases in which CompilerClient will be
            // destroyed and recreated.
            if (!DomainReloadStateSingleton.instance.AlreadyLoaded ||
                DomainReloadStateSingleton.instance.IsScriptDebugInfoEnabled != isScriptDebugInfoEnabled)
            {
                // Gather list of assemblies to compile (only actually used at Editor startup)
                var assemblyNames = UnityEditor.Compilation.CompilationPipeline
                    .GetAssemblies(UnityEditor.Compilation.AssembliesType.Editor)
                    .Where(x => File.Exists(x.outputPath)) // If C# compilation fails, it won't exist on disk
                    .Select(x => $"{x.name}|{string.Join(";", x.defines)}");

                foreach (var assemblyName in assemblyNames)
                {
                    cmdBuilder.With(assemblyName)
                              .With(assemblySeparator);
                }

                DomainReloadStateSingleton.instance.AlreadyLoaded = true;
                DomainReloadStateSingleton.instance.IsScriptDebugInfoEnabled = IsScriptDebugInfoEnabled;
            }

            cmdBuilder.SendToCompiler();
        }

        internal static string VersionNotify(string version)
        {
            return SendCommandToCompiler(BurstCompilerOptions.CompilerCommandVersionNotification, version);
        }

        internal static string GetTargetCpuFromHost()
        {
            return SendCommandToCompiler(BurstCompilerOptions.CompilerCommandGetTargetCpuFromHost);
        }
#endif

        /// <summary>
        /// Cancel any compilation being processed by the JIT Compiler in the background.
        /// </summary>
        internal static void Cancel()
        {
#if UNITY_EDITOR
            SendCommandToCompiler(BurstCompilerOptions.CompilerCommandCancel);
#endif
        }

        /// <summary>
        /// Check if there is any job pending related to the last compilation ID.
        /// </summary>
        internal static bool IsCurrentCompilationDone()
        {
#if UNITY_EDITOR
            return SendCommandToCompiler(BurstCompilerOptions.CompilerCommandIsCurrentCompilationDone) == "True";
#else
            return true;
#endif
        }

        internal static void Enable()
        {
#if UNITY_EDITOR
            SendCommandToCompiler(BurstCompilerOptions.CompilerCommandEnableCompiler);
#endif
        }

        internal static void Disable()
        {
#if UNITY_EDITOR
            SendCommandToCompiler(BurstCompilerOptions.CompilerCommandDisableCompiler);
#endif
        }

        internal static bool IsHostEditorArm()
        {
#if UNITY_EDITOR
            return SendCommandToCompiler(BurstCompilerOptions.CompilerCommandIsArmTestEnv)=="true";
#else
            return false;
#endif
        }

        internal static void TriggerUnsafeStaticMethodRecompilation()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var reinitAttributes = asm.GetCustomAttributes().Where(
                    x => x.GetType().FullName == "Unity.Burst.BurstCompiler+StaticTypeReinitAttribute"
                    );
                foreach (var attribute in reinitAttributes)
                {
                    var ourAttribute = attribute as StaticTypeReinitAttribute;
                    var type = ourAttribute.reinitType;
                    var method = type.GetMethod("Constructor",BindingFlags.Static|BindingFlags.Public);
                    method.Invoke(null, new object[] { });
                }
            }
        }

        internal static void TriggerRecompilation()
        {
#if UNITY_EDITOR
            SetDefaultOptions();

            // This is done separately from CompilerCommandTriggerRecompilation below,
            // because CompilerCommandTriggerRecompilation will cause all jobs to re-request
            // their function pointers from Burst, and we need to have actually triggered
            // compilation by that point.
            SendCommandToCompiler(BurstCompilerOptions.CompilerCommandTriggerSetupRecompilation);

            SendCommandToCompiler(BurstCompilerOptions.CompilerCommandTriggerRecompilation, Options.RequiresSynchronousCompilation.ToString());
#endif
        }

        internal static void UnloadAdditionalLibraries()
        {
            SendCommandToCompiler(BurstCompilerOptions.CompilerCommandUnloadBurstNatives);
        }

        internal static void InitialiseDebuggerHooks()
        {
            if (IsApiAvailable("BurstManagedDebuggerPluginV1") && String.IsNullOrEmpty(Environment.GetEnvironmentVariable("BURST_DISABLE_DEBUGGER_HOOKS")))
            {
                SendCommandToCompiler(SendCommandToCompiler(BurstCompilerOptions.CompilerCommandRequestInitialiseDebuggerCommmand));
            }
        }

        internal static bool IsApiAvailable(string apiName)
        {
            return SendCommandToCompiler(BurstCompilerOptions.CompilerCommandIsNativeApiAvailable, apiName) == "True";
        }

        internal static int RequestSetProtocolVersion(int version)
        {
            // Ask editor for the maximum version of the protocol we support, then inform the rest of the systems the negotiated version
            var editorVersion = SendCommandToCompiler(BurstCompilerOptions.CompilerCommandRequestSetProtocolVersionEditor, $"{version}");
            if (string.IsNullOrEmpty(editorVersion) || !int.TryParse(editorVersion, out var result))
            {
                result=0;
            }
            SendCommandToCompiler(BurstCompilerOptions.CompilerCommandSetProtocolVersionBurst, $"{result}");
            return result;
        }



#if UNITY_EDITOR
        private unsafe delegate void LogCallbackDelegate(void* userData, int logType, byte* message, byte* fileName, int lineNumber);

        private static unsafe void EagerCompileLogCallback(void* userData, int logType, byte* message, byte* fileName, int lineNumber)
        {
            if (EagerCompilationLoggingEnabled)
            {
                BurstRuntime.Log(message, logType, fileName, lineNumber);
            }
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr ManagedFnPtrResolverDelegate(IntPtr handleVal);

        private static IntPtr ManagedResolverFunction(IntPtr handleVal)
        {
            var delegateObj = GCHandle.FromIntPtr(handleVal).Target;
            var fnptr = Marshal.GetFunctionPointerForDelegate(delegateObj);
            return fnptr;
        }

        internal static bool EagerCompilationLoggingEnabled = false;

        private static readonly string EagerCompileLogCallbackFunctionPointer;
        private static readonly string ManagedResolverFunctionPointer;
#endif

        internal static void Initialize(string[] assemblyFolders, string[] ignoreAssemblies)
        {
#if UNITY_EDITOR
            var glued = new string[2];
            glued[0] = SafeStringArrayHelper.SerialiseStringArraySafe(assemblyFolders);
            glued[1] = SafeStringArrayHelper.SerialiseStringArraySafe(ignoreAssemblies);
            var optionsSet = SafeStringArrayHelper.SerialiseStringArraySafe(glued);
            SendCommandToCompiler(BurstCompilerOptions.CompilerCommandInitialize, optionsSet);
#endif
        }

        internal static void NotifyCompilationStarted(string[] assemblyFolders, string[] ignoreAssemblies)
        {
#if UNITY_EDITOR
            var glued = new string[2];
            glued[0] = SafeStringArrayHelper.SerialiseStringArraySafe(assemblyFolders);
            glued[1] = SafeStringArrayHelper.SerialiseStringArraySafe(ignoreAssemblies);
            var optionsSet = SafeStringArrayHelper.SerialiseStringArraySafe(glued);
            SendCommandToCompiler(BurstCompilerOptions.CompilerCommandNotifyCompilationStarted, optionsSet);
#endif
        }

        internal static void NotifyAssemblyCompilationNotRequired(string assemblyName)
        {
#if UNITY_EDITOR
            SendCommandToCompiler(BurstCompilerOptions.CompilerCommandNotifyAssemblyCompilationNotRequired, assemblyName);
#endif
        }

        internal static void NotifyAssemblyCompilationFinished(string assemblyName, string[] defines)
        {
#if UNITY_EDITOR
            BeginCompilerCommand(BurstCompilerOptions.CompilerCommandNotifyAssemblyCompilationFinished)
                .With(assemblyName).And()
                .With(string.Join(";", defines))
                .SendToCompiler();
#endif
        }

        internal static void NotifyCompilationFinished()
        {
#if UNITY_EDITOR
            SendCommandToCompiler(BurstCompilerOptions.CompilerCommandNotifyCompilationFinished);
#endif
        }

        internal static string AotCompilation(string[] assemblyFolders, string[] assemblyRoots, string options)
        {
            var result = "failed";
#if UNITY_EDITOR
            result = SendCommandToCompiler(
                BurstCompilerOptions.CompilerCommandAotCompilation,
                BurstCompilerOptions.SerialiseCompilationOptionsSafe(assemblyRoots, assemblyFolders, options));
#endif
            return result;
        }

#if UNITY_EDITOR
        private static readonly string ProgressCallbackFunctionPointer;

        private delegate void ProgressCallbackDelegate(int current, int total);

        private static void ProgressCallback(int current, int total)
        {
            OnProgress?.Invoke(current, total);
        }

        internal static event Action<int, int> OnProgress;
#endif

        internal static void SetProfilerCallbacks()
        {
#if UNITY_EDITOR
            BeginCompilerCommand(BurstCompilerOptions.CompilerCommandSetProfileCallbacks)
                .With(ProfileBeginCallbackFunctionPointer).And(';')
                .With(ProfileEndCallbackFunctionPointer)
                .SendToCompiler();
#endif
        }

#if UNITY_EDITOR
        internal delegate void ProfileBeginCallbackDelegate(string markerName, string metadataName, string metadataValue);
        internal delegate void ProfileEndCallbackDelegate(string markerName);

        private static readonly string ProfileBeginCallbackFunctionPointer;
        private static readonly string ProfileEndCallbackFunctionPointer;

        private static void ProfileBeginCallback(string markerName, string metadataName, string metadataValue) => OnProfileBegin?.Invoke(markerName, metadataName, metadataValue);
        private static void ProfileEndCallback(string markerName) => OnProfileEnd?.Invoke(markerName);

        internal static event ProfileBeginCallbackDelegate OnProfileBegin;
        internal static event ProfileEndCallbackDelegate OnProfileEnd;
#endif


        private static string SendRawCommandToCompiler(string command)
        {
            var results = Unity.Burst.LowLevel.BurstCompilerService.GetDisassembly(DummyMethodInfo, command);
            if (!string.IsNullOrEmpty(results))
                return results.TrimStart('\n');
            return "";
        }

        private static string SendCommandToCompiler(string commandName, string commandArgs = null)
        {
            if (commandName == null) throw new ArgumentNullException(nameof(commandName));

            if (commandArgs == null)
            {
                // If there are no arguments then there's no reason to go through the builder
                return SendRawCommandToCompiler(commandName);
            }

            // Otherwise use the builder for building the final command
            return BeginCompilerCommand(commandName)
                    .With(commandArgs)
                    .SendToCompiler();
        }

        private static readonly MethodInfo DummyMethodInfo = typeof(BurstCompiler).GetMethod(nameof(DummyMethod), BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// Dummy empty method for being able to send a command to the compiler
        /// </summary>
        private static void DummyMethod() { }

#if !UNITY_EDITOR && !BURST_INTERNAL
        /// <summary>
        /// Internal class to detect at standalone player time if AOT settings were enabling burst.
        /// </summary>
        [BurstCompile]
        internal static class BurstCompilerHelper
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate bool IsBurstEnabledDelegate();
            private static readonly IsBurstEnabledDelegate IsBurstEnabledImpl = new IsBurstEnabledDelegate(IsBurstEnabled);

            [BurstCompile]
            [AOT.MonoPInvokeCallback(typeof(IsBurstEnabledDelegate))]
            private static bool IsBurstEnabled()
            {
                bool result = true;
                DiscardedMethod(ref result);
                return result;
            }

            [BurstDiscard]
            private static void DiscardedMethod(ref bool value)
            {
                value = false;
            }

            private static unsafe bool IsCompiledByBurst(Delegate del)
            {
                var delegateMethodId = Unity.Burst.LowLevel.BurstCompilerService.CompileAsyncDelegateMethod(del, string.Empty);
                // We don't try to run the method, having a pointer is already enough to tell us that burst was active for AOT settings
                return Unity.Burst.LowLevel.BurstCompilerService.GetAsyncCompiledAsyncDelegateMethod(delegateMethodId) != (void*)0;
            }

            /// <summary>
            /// Gets a boolean indicating whether burst was enabled for standalone player, used only at runtime.
            /// </summary>
            public static readonly bool IsBurstGenerated = IsCompiledByBurst(IsBurstEnabledImpl);
        }
#endif // !UNITY_EDITOR && !BURST_INTERNAL

        /// <summary>
        /// Fake delegate class to make BurstCompilerService.CompileAsyncDelegateMethod happy
        /// so that it can access the underlying static method via the property get_Method.
        /// So this class is not a delegate.
        /// </summary>
        private class FakeDelegate
        {
            public FakeDelegate(MethodInfo method)
            {
                Method = method;
            }

            [Preserve]
            public MethodInfo Method { get; }
        }
    }
}
