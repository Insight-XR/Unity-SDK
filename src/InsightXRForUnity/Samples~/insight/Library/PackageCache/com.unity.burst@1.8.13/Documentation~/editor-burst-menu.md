# Burst menu reference

In the Editor, use the settings in the Burst menu to control how Burst works. To access this menu, go to **Jobs &gt; Burst**. The following settings are available:

|**Setting**|**Function**|
|---|---|
|**Enable Compilation**| Enable this setting to activate Burst compilation. When you enable this setting, Burst compiles jobs and Burst custom delegates that you tag with the attribute `[BurstCompile]`.|
|**Enable Safety Checks**| Choose what safety checks Burst should use. For more information see the [Enable Safety Checks setting](#safety-checks) section of this documentation.|
|Off| Disable safety checks across all Burst jobs and function-pointers. Only use this setting if you want more realistic profiling results from in-Editor captures. When you reload the Editor, this setting always resets to **On**. |
|On| Enable safety checks on code that uses collection containers (e.g `NativeArray<T>`). Checks include job data dependency and container indexes out of bounds. This is the default setting.|
|Force On| Force safety checks on even for jobs and function-pointers that have [`DisableSafetyChecks = true`](xref:Unity.Burst.BurstCompileAttribute.DisableSafetyChecks). Use this setting to rule out any problems that safety checks might have caught.|
|**Synchronous Compilation**| Enable this setting to compile Burst synchronously. For more information, see [Synchronous compilation](compilation-synchronous.md).|
|**Native Debug Mode Compilation**| Enable this setting to deactivate optimizations on all code that Burst compiles. This makes it easier to debug via a native debugger. For more information, see [Native Debugging tools](debugging-profiling-tools.md#native-debugging). |
|**Show Timings**| Enable this setting to log the time it takes to JIT compile a job in the Editor and display it in the Console. For more information see the [Show Timings setting](#show-timings) section of this documentation.|
|**Open Inspector**| Opens the [Burst Inspector window](editor-burst-inspector.md).|

<a name="safety-checks"></a>

## Enable Safety Checks setting

To disable Burst's safety check code, use [DisableSafetyChecks](xref:Unity.Burst.BurstCompileAttribute.DisableSafetyChecks). This results in faster code generation, however make sure that you use containers in a safe fashion.

To disable safety checks on a job or function-pointer set `DisableSafetyChecks` to `true`:

```c#
[BurstCompile(DisableSafetyChecks = true)]
public struct MyJob : IJob
{
    // ...
}
```

Burst ignores code marked explicitly with `DisableSafetyChecks = true` when it safety checks your code if you set **Enable Safety Checks** to **On** in the Editor. Select **Force On** to make Burst to safety check all code, including code marked with `DisableSafetyChecks = true`.

<a name="show-timings"></a>

## Show Timings setting

When you enable the **Show Timings** setting, Unity logs an output in the [Console window](https://docs.unity3d.com/Manual/Console.html) for each library of entry points that Burst compiles. Burst batches the compilation into units of methods-per-assembly, and groups multiple entry-points together in a single compilation task. This output is useful if you want to report outliers in compilation to the Burst compiler team (via the [Burst forum](https://forum.unity.com/forums/burst.629/)). 

Unity splits Burst's output into the following major sections:

* Method discovery (where Burst works out what it needs to compile)
* Front end (where Burst turns C# IL into an LLVM IR module)
* Middle end (where Burst specializes, optimizes, and cleans up the module)
* Back-end (where Burst turns the LLVM IR module into a native DLL)

The compile time in the front end and optimizer is linear to the amount operations that it needs to compile. More functions and more instructions means a longer compile time. The more generic functions you have, the higher the front end performance timings, because generic resolutions have non-zero costs.

The compile time in the back-end scales with the number of entry-points in the module. This is because each entry point is in its own native object file.

If the optimizer takes a significant amount of time, use `[BurstCompile(OptimizeFor = OptimizeFor.FastCompilation)]` which reduces the optimizations that Burst does, but compiles things much faster. Profile the job before and after to make sure that this tradeoff is right for that entry-point.