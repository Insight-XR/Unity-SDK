# Getting started

Burst is primarily designed to work with Unity's [job system](https://docs.unity3d.com/Manual/JobSystem.html). To start using the Burst compiler in your code, decorate a [Job struct](https://docs.unity3d.com/ScriptReference/Unity.Jobs.IJob.html) with the [`[BurstCompile]`](xref:Unity.Burst.BurstCompileAttribute) attribute. Add the `[BurstCompile]` attribute to the type and the static method you want Burst to compile.


```c#
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class MyBurst2Behavior : MonoBehaviour
{
    void Start()
    {
        var input = new NativeArray<float>(10, Allocator.Persistent);
        var output = new NativeArray<float>(1, Allocator.Persistent);
        for (int i = 0; i < input.Length; i++)
            input[i] = 1.0f * i;

        var job = new MyJob
        {
            Input = input,
            Output = output
        };
        job.Schedule().Complete();

        Debug.Log("The result of the sum is: " + output[0]);
        input.Dispose();
        output.Dispose();
    }

    // Using BurstCompile to compile a Job with Burst

    [BurstCompile]
    private struct MyJob : IJob
    {
        [ReadOnly]
        public NativeArray<float> Input;

        [WriteOnly]
        public NativeArray<float> Output;

        public void Execute()
        {
            float result = 0.0f;
            for (int i = 0; i < Input.Length; i++)
            {
                result += Input[i];
            }
            Output[0] = result;
        }
    }
}
```

## Limitations

Burst supports most C# expressions and statements, with a few exceptions. For more information, see [C# language support](csharp-language-support.md).

## Compilation

Burst compiles your code [just-in-time (JIT)](https://en.wikipedia.org/wiki/Just-in-time_compilation) while in Play mode in the Editor, and [ahead-of-time (AOT)](https://en.wikipedia.org/wiki/Ahead-of-time_compilation) when your application runs in a Player. For more information on compilation, see [Burst compilation](compilation.md)

## Command line options

You can pass the following options to the Unity Editor on the command line to control Burst:

- `--burst-disable-compilation` disables Burst.
- `--burst-force-sync-compilation` force Burst to compile synchronously. For more information, see [Burst compilation](compilation.md).
