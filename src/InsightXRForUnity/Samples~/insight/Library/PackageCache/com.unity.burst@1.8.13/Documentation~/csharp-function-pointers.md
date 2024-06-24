# Function pointers

To work with dynamic functions that process data based on other data states, use [`FunctionPointer<T>`](xref:Unity.Burst.FunctionPointer`1). Because Burst treats delegates as managed objects, you can't use [C# delegates](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/) to work with dynamic functions.

## Support details

Function pointers don't support generic delegates. Also, avoid wrapping [`BurstCompiler.CompileFunctionPointer<T>`](xref:Unity.Burst.BurstCompiler.CompileFunctionPointer``1(``0)) within another open generic method. If you do this, Burst can't apply required attributes to the delegate, perform additional safety analysis, or perform potential optimizations. 

Argument and return types are subject to the same restrictions as `DllImport` and internal calls. For more information, see the documentation on [DllImport and internal calls](csharp-burst-intrinsics-dllimport.md).

### Interoperability with IL2CPP

Interoperability of function pointers with IL2CPP requires `System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute` on the delegate. Set the calling convention to `CallingConvention.Cdecl`. Burst automatically adds this attribute to delegates that are used with [`BurstCompiler.CompileFunctionPointer<T>`](xref:Unity.Burst.BurstCompiler.CompileFunctionPointer``1(``0)).

## Using function pointers

To use function pointers, identify the static functions that you want Burst to compile and do the following:

1. Add a `[BurstCompile]` attribute to these functions
1. Add a `[BurstCompile]` attribute to the containing type. This helps the Burst compiler find the static methods that have `[BurstCompile]` attribute
1. Declare a delegate to create the "interface" of these functions
1. Add a `[MonoPInvokeCallbackAttribute]` attribute to the functions. You need to add this so that IL2CPP works with these functions. For example:

    ```c#
    // Instruct Burst to look for static methods with [BurstCompile] attribute
    [BurstCompile]
    class EnclosingType {
        [BurstCompile]
        [MonoPInvokeCallback(typeof(Process2FloatsDelegate))]
        public static float MultiplyFloat(float a, float b) => a * b;
    
        [BurstCompile]
        [MonoPInvokeCallback(typeof(Process2FloatsDelegate))]
        public static float AddFloat(float a, float b) => a + b;
    
        // A common interface for both MultiplyFloat and AddFloat methods
        public delegate float Process2FloatsDelegate(float a, float b);
    }
    ```

1. Compile these function pointers from regular C# code:

    ```c#
        // Contains a compiled version of MultiplyFloat with Burst
        FunctionPointer<Process2FloatsDelegate> mulFunctionPointer = BurstCompiler.CompileFunctionPointer<Process2FloatsDelegate>(MultiplyFloat);
    
        // Contains a compiled version of AddFloat with Burst
        FunctionPointer<Process2FloatsDelegate> addFunctionPointer = BurstCompiler.    CompileFunctionPointer<Process2FloatsDelegate>(AddFloat);
    ```

### Using function pointers in a job

To use the function pointers directly from a job, pass them to the job struct:

```c#
    // Invoke the function pointers from HPC# jobs
    var resultMul = mulFunctionPointer.Invoke(1.0f, 2.0f);
    var resultAdd = addFunctionPointer.Invoke(1.0f, 2.0f);
``` 

Burst compiles function pointers asynchronously for jobs by default. To force a synchronous compilation of function pointers use `[BurstCompile(SynchronousCompilation = true)]`.

### Using function pointers in C# code

To use these function pointers from regular C# code, cache the `FunctionPointer<T>.Invoke` property (which is the delegate instance) to a static field to get the best performance:

```c#
    private readonly static Process2FloatsDelegate mulFunctionPointerInvoke = BurstCompiler.CompileFunctionPointer<Process2FloatsDelegate>(MultiplyFloat).Invoke;

    // Invoke the delegate from C#
    var resultMul = mulFunctionPointerInvoke(1.0f, 2.0f);
```

Using Burst-compiled function pointers from C# might be slower than their pure C# version counterparts if the function is too small compared to the overhead of [`P/Invoke`](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke) interop.


## Performance considerations

Where possible, you use a job over a function pointer to run Burst compiled code, because jobs are more optimal. Burst provides better aliasing calculations for jobs because the job safety system has more optimizations by default.

You also can't pass most of the `[NativeContainer]` structs like `NativeArray` directly to function pointers and must use a job struct to do so. Native container structs contain managed objects for safety checks that the Burst compiler can work around when compiling jobs, but not for function pointers.

The following example shows a bad example of how to use function pointers in Burst. The function pointer computes `math.sqrt` from an input pointer and stores it to an output pointer. `MyJob` feeds this function pointer sources from two `NativeArray`s which isn't optimal:

```c#
///Bad function pointer example
[BurstCompile]
public class MyFunctionPointers
{
    public unsafe delegate void MyFunctionPointerDelegate(float* input, float* output);

    [BurstCompile]
    public static unsafe void MyFunctionPointer(float* input, float* output)
    {
        *output = math.sqrt(*input);
    }
}

[BurstCompile]
struct MyJob : IJobParallelFor
{
     public FunctionPointer<MyFunctionPointers.MyFunctionPointerDelegate> FunctionPointer;

    [ReadOnly] public NativeArray<float> Input;
    [WriteOnly] public NativeArray<float> Output;

    public unsafe void Execute(int index)
    {
        var inputPtr = (float*)Input.GetUnsafeReadOnlyPtr();
        var outputPtr = (float*)Output.GetUnsafePtr();
        FunctionPointer.Invoke(inputPtr + index, outputPtr + index);
    }
}
```

This example isn't optimal for the following reasons:

* Burst can't vectorize the function pointer because it's being fed a single scalar element. This means that 4-8x performance is lost from a lack of vectorization.
* The `MyJob` knows that the `Input` and `Output` native arrays can't alias, but this information isn't communicated to the function pointer.
* There is a non-zero overhead to constantly branching to a function pointer somewhere else in memory.

To use a function pointer in an optimal way, always process batches of data in the function pointer, like so:

```c#
[BurstCompile]
public class MyFunctionPointers
{
    public unsafe delegate void MyFunctionPointerDelegate(int count, float* input, float* output);

    [BurstCompile]
    public static unsafe void MyFunctionPointer(int count, float* input, float* output)
    {
        for (int i = 0; i < count; i++)
        {
            output[i] = math.sqrt(input[i]);
        }
    }
}

[BurstCompile]
struct MyJob : IJobParallelForBatch
{
     public FunctionPointer<MyFunctionPointers.MyFunctionPointerDelegate> FunctionPointer;

    [ReadOnly] public NativeArray<float> Input;
    [WriteOnly] public NativeArray<float> Output;

    public unsafe void Execute(int index, int count)
    {
        var inputPtr = (float*)Input.GetUnsafeReadOnlyPtr() + index;
        var outputPtr = (float*)Output.GetUnsafePtr() + index;
        FunctionPointer.Invoke(count, inputPtr, outputPtr);
    }
}
```

Thee modified `MyFunctionPointer` takes a `count` of elements to process, and loops over the `input` and `output` pointers to do a lot of calculations. The `MyJob` becomes an `IJobParallelForBatch`, and the `count` is passed directly into the function pointer. This is better for performance because of the following reasons:

* Burst vectorizes the `MyFunctionPointer` call.
* Because Burst processes `count` items per function pointer, any overhead of calling the function pointer is reduced by `count` times. For example, if you run a batch of 128, the function pointer overhead is 1/128th per `index` of what it was previously.
* Batching results in a 1.53x performance gain over not batching.

However, to get the best possible performance, use a job. This gives Burst the most visibility over what you want it to do, and the most opportunities to optimize:

```c#
[BurstCompile]
struct MyJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float> Input;
    [WriteOnly] public NativeArray<float> Output;

    public unsafe void Execute(int index)
    {
        Output[i] = math.sqrt(Input[i]);
    }
}
```

This runs 1.26x faster than the batched function pointer example, and 1.93x faster than the non-batched function pointer examples. Burst has perfect aliasing knowledge and can make the broadest modifications to the above. This code is also a lot simpler than either of the function pointer cases.
