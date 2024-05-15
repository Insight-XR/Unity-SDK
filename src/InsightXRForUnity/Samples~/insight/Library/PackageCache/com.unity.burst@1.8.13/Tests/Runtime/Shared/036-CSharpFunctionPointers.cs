// Doesn't work with IL2CPP yet - waiting for Unity fix to land.
#if BURST_INTERNAL //|| UNITY_2021_2_OR_NEWER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Unity.Burst;
using UnityBenchShared;
#if BURST_INTERNAL
using System.IO;
using System.Reflection;
using Burst.Compiler.IL.Aot;
#endif

namespace Burst.Compiler.IL.Tests
{
    [RestrictPlatform("Mono on linux crashes to what appears to be a mono bug", Platform.Linux, exclude: true)]
    internal class TestCSharpFunctionPointers
    {
        [TestCompiler]
        public static unsafe int TestCSharpFunctionPointer()
        {
            delegate* unmanaged[Cdecl]<int, int> callback = &TestCSharpFunctionPointerCallback;
            return TestCSharpFunctionPointerHelper(callback);
        }

        private static unsafe int TestCSharpFunctionPointerHelper(delegate* unmanaged[Cdecl]<int, int> callback)
        {
            return callback(5);
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static int TestCSharpFunctionPointerCallback(int value) => value + 1;

        [TestCompiler]
        public static unsafe int TestCSharpFunctionPointerCastingParameterPtrFromVoid()
        {
            delegate* unmanaged[Cdecl]<void*, int> callback = &TestCSharpFunctionPointerCallbackVoidPtr;
            delegate* unmanaged[Cdecl]<int*, int> callbackCasted = callback;

            int i = 5;

            return callbackCasted(&i);
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static unsafe int TestCSharpFunctionPointerCallbackVoidPtr(void* value) => *((int*)value) + 1;

        [TestCompiler]
        public static unsafe int TestCSharpFunctionPointerCastingParameterPtrToVoid()
        {
            delegate* unmanaged[Cdecl]<int*, int> callback = &TestCSharpFunctionPointerCallbackIntPtr;
            delegate* unmanaged[Cdecl]<void*, int> callbackCasted = (delegate* unmanaged[Cdecl]<void*, int>)callback;

            int i = 5;

            return callbackCasted(&i);
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static unsafe int TestCSharpFunctionPointerCallbackIntPtr(int* value) => *value + 1;

        [TestCompiler]
        public static unsafe int TestCSharpFunctionPointerCastingToAndFromVoidPtr()
        {
            delegate* unmanaged[Cdecl]<int*, int> callback = &TestCSharpFunctionPointerCallbackIntPtr;
            void* callbackAsVoidPtr = callback;
            delegate* unmanaged[Cdecl]<int*, int> callbackCasted = (delegate* unmanaged[Cdecl]<int*, int>)callbackAsVoidPtr;

            int i = 5;

            return callbackCasted(&i);
        }

        public struct CSharpFunctionPointerProvider : IArgumentProvider
        {
            public unsafe object Value
            {
                get
                {
                    delegate* unmanaged[Cdecl]<int, int> callback = &TestCSharpFunctionPointerCallback;
                    return (IntPtr)callback;
                }
            }
        }

        [TestCompiler(typeof(CSharpFunctionPointerProvider))]
        public static unsafe int TestCSharpFunctionPointerPassedInFromOutside(IntPtr callbackAsIntPtr)
        {
            delegate* unmanaged[Cdecl]<int, int> callback = (delegate* unmanaged[Cdecl]<int, int>)callbackAsIntPtr;
            return TestCSharpFunctionPointerHelper(callback);
        }

        private struct TestCSharpFunctionPointerWithStructParameterStruct
        {
            public int X;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static int TestCSharpFunctionPointerWithStructParameterCallback(TestCSharpFunctionPointerWithStructParameterStruct value) => value.X + 1;

        public struct CSharpFunctionPointerWithStructParameterProvider : IArgumentProvider
        {
            public unsafe object Value
            {
                get
                {
                    delegate* unmanaged[Cdecl]<TestCSharpFunctionPointerWithStructParameterStruct, int> callback = &TestCSharpFunctionPointerWithStructParameterCallback;
                    return (IntPtr)callback;
                }
            }
        }

        [TestCompiler(typeof(CSharpFunctionPointerWithStructParameterProvider))]
        public static unsafe int TestCSharpFunctionPointerPassedInFromOutsideWithStructParameter(IntPtr untypedFp)
        {
            return TestHashingFunctionPointerTypeHelper((delegate* unmanaged[Cdecl]<TestCSharpFunctionPointerWithStructParameterStruct, int>)untypedFp);
        }

        private static unsafe int TestHashingFunctionPointerTypeHelper(delegate* unmanaged[Cdecl]<TestCSharpFunctionPointerWithStructParameterStruct, int> fp)
        {
            return fp(new TestCSharpFunctionPointerWithStructParameterStruct { X = 42 });
        }

        [TestCompiler(ExpectCompilerException = true, ExpectedDiagnosticId = DiagnosticId.ERR_CalliNonCCallingConventionNotSupported)]
        public static unsafe int TestCSharpFunctionPointerInvalidCallingConvention()
        {
            delegate*<int, int> callback = &TestCSharpFunctionPointerInvalidCallingConventionCallback;
            return callback(5);
        }

        private static int TestCSharpFunctionPointerInvalidCallingConventionCallback(int value) => value + 1;

        [TestCompiler]
        public static unsafe int TestCSharpFunctionPointerMissingBurstCompileAttribute()
        {
            delegate* unmanaged[Cdecl]<int, int> callback = &TestCSharpFunctionPointerCallbackMissingBurstCompileAttribute;
            return callback(5);
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static int TestCSharpFunctionPointerCallbackMissingBurstCompileAttribute(int value) => value + 1;

        [Test]
        public unsafe void TestFunctionPointerReturnedFromBurstFunction()
        {
#if BURST_INTERNAL
            var libraryCacheFolderName = Path.Combine(
                Path.GetDirectoryName(GetType().Assembly.Location),
                nameof(TestCSharpFunctionPointers),
                nameof(TestFunctionPointerReturnedFromBurstFunction));
            if (Directory.Exists(libraryCacheFolderName))
            {
                Directory.Delete(libraryCacheFolderName, true);
            }
            using var globalContext = new Server.GlobalContext(libraryCacheFolderName);
            var jitOptions = new AotCompilerOptions();
            using var methodCompiler = new Helpers.MethodCompiler(globalContext, jitOptions.BackendName, name => IntPtr.Zero);

            BurstCompiler.InternalCompiler = del =>
            {
                var getMethod = del.GetType().GetMethod("get_Method", BindingFlags.Public | BindingFlags.Instance);
                var methodInfo = (MethodInfo)getMethod.Invoke(del, new object[0]);
                var compiledResult = methodCompiler.CompileMethod(methodInfo, jitOptions);

                return compiledResult.FunctionPointer;
            };
#endif

            var fp = BurstCompiler.CompileFunctionPointer<DelegateWithCSharpFunctionPointerReturn>(EntryPointWithCSharpFunctionPointerReturn);

            var fpInner = fp.Invoke();

            delegate* unmanaged[Cdecl]<float, float, float, float, float, float, float> callback = (delegate* unmanaged[Cdecl]<float, float, float, float, float, float, float>)fpInner;

            var result = callback(1, 2, 4, 8, 16, 32);

            Assert.AreEqual((float)(1 + 2 + 4 + 8 + 16 + 32), result);
        }

        [BurstCompile(CompileSynchronously = true)]
        private static unsafe IntPtr EntryPointWithCSharpFunctionPointerReturn()
        {
            delegate* unmanaged[Cdecl]<float, float, float, float, float, float, float> fp = &EntryPointWithCSharpFunctionPointerReturnHelper;
            return (IntPtr)fp;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static unsafe float EntryPointWithCSharpFunctionPointerReturnHelper(float p1, float p2, float p3, float p4, float p5, float p6)
        {
            return p1 + p2 + p3 + p4 + p5 + p6;
        }

        private unsafe delegate IntPtr DelegateWithCSharpFunctionPointerReturn();

        // Note that there are 6 float parameters to try to catch any issues with calling conventions.
        private unsafe delegate float DelegateWithCSharpFunctionPointerReturnHelper(float p1, float p2, float p3, float p4, float p5, float p6);

        // Note that this test previously had a `out int i` parameter, but a bugfix in Roslyn
        // means that ref parameters in UnmanagedCallersOnly methods now result in a compilation error:
        // https://github.com/dotnet/roslyn/issues/57025
        // So we've updated this test to use a pointer.
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static unsafe void TestCSharpFunctionPointerCallbackWithOut(int* i)
        {
            TestCSharpFunctionPointerCallbackWithOut(out *i);
        }

        private static void TestCSharpFunctionPointerCallbackWithOut(out int i)
        {
            i = 42;
        }

        [TestCompiler]
        public static unsafe int TestCSharpFunctionPointerWithOut()
        {
            delegate* unmanaged[Cdecl]<int*, void> callback = &TestCSharpFunctionPointerCallbackWithOut;

            int i;
            callback(&i);

            return i;
        }

#if BURST_TESTS_ONLY
        [DllImport("burst-dllimport-native")]
        private static extern unsafe int callFunctionPointer(delegate* unmanaged[Cdecl]<int, int> f);

        // Ignored on wasm since dynamic linking is not supported at present.
        // Override result on Mono because it throws a StackOverflowException for some reason related to the function pointer.
        // We should use OverrideResultOnMono, but OverrideResultOnMono still runs the managed version, which causes a crash,
        // so we use OverrideManagedResult.
        [TestCompiler(IgnoreOnPlatform = Backend.TargetPlatform.Wasm, OverrideManagedResult = 43)]
        public static unsafe int TestPassingFunctionPointerToNativeCode()
        {
            return callFunctionPointer(&TestCSharpFunctionPointerCallback);
        }
#endif
    }
}

// This attribute is also included in com.unity.burst/Tests/Runtime/FunctionPointerTests.cs,
// so we want to exclude it here when we're running inside Unity otherwise we'll get a
// duplicate definition error.
#if BURST_TESTS_ONLY && NETFRAMEWORK
// UnmanagedCallersOnlyAttribute is new in .NET 5.0. This attribute is required
// when you declare an unmanaged function pointer with an explicit calling convention.
// Fortunately, Roslyn lets us declare the attribute class ourselves, and it will be used.
// Users will need this same declaration in their own projects, in order to use
// C# 9.0 function pointers.
namespace System.Runtime.InteropServices
{
    [AttributeUsage(System.AttributeTargets.Method, Inherited = false)]
    public sealed class UnmanagedCallersOnlyAttribute : Attribute
    {
        public Type[] CallConvs;
    }
}
#endif
#endif