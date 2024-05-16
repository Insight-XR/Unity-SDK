using System;
using System.Runtime.InteropServices;
using AOT;
using NUnit.Framework;
using Unity.Burst;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_2021_2_OR_NEWER
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
#if UNITY_EDITOR
using OverloadedFunctionPointers;
#endif
#endif

[TestFixture, BurstCompile]
public class FunctionPointerTests
{
    [BurstCompile(CompileSynchronously = true)]
    private static T StaticFunctionNoArgsGenericReturnType<T>()
    {
        return default;
    }

    private delegate int DelegateNoArgsIntReturnType();

    [Test]
    public void TestCompileFunctionPointerNoArgsGenericReturnType()
    {
        Assert.Throws<InvalidOperationException>(
            () => BurstCompiler.CompileFunctionPointer<DelegateNoArgsIntReturnType>(StaticFunctionNoArgsGenericReturnType<int>),
            "The method `Int32 StaticFunctionNoArgsGenericReturnType[Int32]()` must be a non-generic method");
    }

    [BurstCompile(CompileSynchronously = true)]
    private static int StaticFunctionConcreteReturnType()
    {
        return default;
    }

    private delegate T DelegateGenericReturnType<T>();

    [Test]
    public void TestCompileFunctionPointerDelegateNoArgsGenericReturnType()
    {
        Assert.Throws<InvalidOperationException>(
            () => BurstCompiler.CompileFunctionPointer<DelegateGenericReturnType<int>>(StaticFunctionConcreteReturnType),
            "The delegate type `FunctionPointerTests+DelegateGenericReturnType`1[System.Int32]` must be a non-generic type");
    }

    private static class GenericClass<T>
    {
        public delegate int DelegateNoArgsIntReturnType();
    }

    [Test]
    public void TestCompileFunctionPointerDelegateNoArgsGenericDeclaringType()
    {
        Assert.Throws<InvalidOperationException>(
            () => BurstCompiler.CompileFunctionPointer<GenericClass<int>.DelegateNoArgsIntReturnType>(StaticFunctionConcreteReturnType),
            "The delegate type `FunctionPointerTests+GenericClass`1+DelegateNoArgsIntReturnType[System.Int32]` must be a non-generic type");
    }

// Doesn't work with IL2CPP yet - waiting for Unity fix to land. Once it does, remove `&& UNITY_EDITOR`
#if UNITY_2021_2_OR_NEWER && UNITY_EDITOR
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    [BurstCompile]
    private static int CSharpFunctionPointerCallback(int value) => value * 2;

    [BurstCompile(CompileSynchronously = true)]
    public unsafe struct StructWithCSharpFunctionPointer : IJob
    {
        [NativeDisableUnsafePtrRestriction]
        [ReadOnly]
        public IntPtr Callback;

        [ReadOnly]
        public NativeArray<int> Input;

        [WriteOnly]
        public NativeArray<int> Output;

        public void Execute()
        {
            delegate* unmanaged[Cdecl]<int, int> callback = (delegate* unmanaged[Cdecl]<int, int>)Callback;
            Output[0] = callback(Input[0]);
        }
    }

    [Test]
    public unsafe void CSharpFunctionPointerInsideJobStructTest()
    {
        using (var input = new NativeArray<int>(new int[1] { 40 }, Allocator.Persistent))
        using (var output = new NativeArray<int>(new int[1], Allocator.Persistent))
        {
            delegate* unmanaged[Cdecl]<int, int> callback = &CSharpFunctionPointerCallback;

            var job = new StructWithCSharpFunctionPointer
            {
                Callback = (IntPtr)callback,
                Input = input,
                Output = output
            };

            job.Run();

            Assert.AreEqual(40 * 2, output[0]);
        }
    }

    [Test]
    public unsafe void CSharpFunctionPointerInStaticMethodSignature()
    {
        var fp = BurstCompiler.CompileFunctionPointer<DelegateWithCSharpFunctionPointerParameter>(EntryPointWithCSharpFunctionPointerParameter);
        delegate* unmanaged[Cdecl]<int, int> callback = &CSharpFunctionPointerCallback;

        var result = fp.Invoke((IntPtr)callback);

        Assert.AreEqual(10, result);
    }

    [BurstCompile(CompileSynchronously = true)]
    private static unsafe int EntryPointWithCSharpFunctionPointerParameter(IntPtr callback)
    {
        delegate* unmanaged[Cdecl]<int, int> typedCallback = (delegate* unmanaged[Cdecl]<int, int>)callback;
        return typedCallback(5);
    }

    private unsafe delegate int DelegateWithCSharpFunctionPointerParameter(IntPtr callback);

    [Test]
    public unsafe void FunctionPointerReturnedFromBurstFunction()
    {
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
    [BurstCompile(CompileSynchronously = true)]
    private static unsafe float EntryPointWithCSharpFunctionPointerReturnHelper(float p1, float p2, float p3, float p4, float p5, float p6)
    {
        return p1 + p2 + p3 + p4 + p5 + p6;
    }

    [BurstCompile]
    [UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
    static long UnmanagedFunction(long burstCount) => 1;

    [BurstCompile]
    static unsafe void GetUnmanagedCallableWithReturn(out Callable fn)
    {
        fn = Callable.Create<long, long>(&UnmanagedFunction);
    }

    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]

    public void CallOverloadedFunctionWithFpArg()
    {
        GetUnmanagedCallableWithReturn(out var a);
        Assert.AreEqual(3, a.Value);
    }

    private delegate int Doer(int x);

    static int DoCompileFunctionPointerNestedStaticMethod(int x)
    {
        [BurstCompile]
        static int DoIt(int x) => x * 2 - 1;

        return BurstCompiler.CompileFunctionPointer<Doer>(DoIt).Invoke(x);
    }

    [Test]
    public void TestCompileFunctionPointerNestedStaticMethod()
    {
        Assert.AreEqual(3, DoCompileFunctionPointerNestedStaticMethod(2));
    }

    private unsafe delegate IntPtr DelegateWithCSharpFunctionPointerReturn();

    // Note that there are 6 float parameters to try to catch any issues with calling conventions.
    private unsafe delegate float DelegateWithCSharpFunctionPointerReturnHelper(float p1, float p2, float p3, float p4, float p5, float p6);
#endif

    [Test]
    public void TestDelegateWithCustomAttributeThatIsNotUnmanagedFunctionPointerAttribute()
    {
        var fp = BurstCompiler.CompileFunctionPointer<TestDelegateWithCustomAttributeThatIsNotUnmanagedFunctionPointerAttributeDelegate>(TestDelegateWithCustomAttributeThatIsNotUnmanagedFunctionPointerAttributeHelper);

        var result = fp.Invoke(42);

        Assert.AreEqual(43, result);
    }
    [BurstCompile(CompileSynchronously = true)]
    private static int TestDelegateWithCustomAttributeThatIsNotUnmanagedFunctionPointerAttributeHelper(int x) => x + 1;

    [MyCustomAttribute("Foo")]
    private delegate int TestDelegateWithCustomAttributeThatIsNotUnmanagedFunctionPointerAttributeDelegate(int x);

    private sealed class MyCustomAttributeAttribute : Attribute
    {
        public MyCustomAttributeAttribute(string param) { }
    }
}

#if UNITY_2021_2_OR_NEWER
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