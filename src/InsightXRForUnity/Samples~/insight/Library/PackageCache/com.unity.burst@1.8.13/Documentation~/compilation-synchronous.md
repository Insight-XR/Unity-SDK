# Synchronous compilation

By default, when in Play mode, the Burst compiler compiles jobs asynchronously. To force synchronous compilation, set the [`CompileSynchronously`](xref:Unity.Burst.BurstCompileAttribute.CompileSynchronously) property to `true`, which compiles your method on the first schedule.

```c#
[BurstCompile(CompileSynchronously = true)]
public struct MyJob : IJob
{
    // ...
}
```

If you don't set this property, on the first call of a job, Burst compiles it asynchronously in the background, and runs a managed C# job in the mean time. This minimizes any frame hitching and keeps the experience responsive.

However, when you set `CompileSynchronously = true`, no asynchronous compilation can happen. This means that it might take longer for Burst to compile. This pause for compilation affects the current running frame, which means that hitches can happen and it might provide an unresponsive experience for users. 

In general, only use `CompileSynchronously = true` in the following situations:

* If you have a long running job that only runs once. The performance of the compiled code might outweigh the downsides doing the compilation synchronously.
* If you're profiling a Burst job and want to test the code from the Burst compiler. When you do this, perform a warmup to discard any timing measurements from the first call to the job. This is because the profiling data includes the compilation time and skews the result.
* To aid with debugging the difference between managed and Burst compiled code. 