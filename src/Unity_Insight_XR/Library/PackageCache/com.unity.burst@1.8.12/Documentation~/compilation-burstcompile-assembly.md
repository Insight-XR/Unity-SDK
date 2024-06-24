# Assembly level BurstCompile

Use the `BurstCompile` attribute on an assembly to set options for all Burst jobs and function-pointers within the assembly:

```c#
[assembly: BurstCompile(CompileSynchronously = true)]
```

For example, if an assembly just contains game code which needs to run quickly, you can use:

```c#
[assembly: BurstCompile(OptimizeFor = OptimizeFor.FastCompilation)]
```

This means that Burst compiles the code as fast as it possibly can, which means that you can iterate on the game code much more quickly. It also means that other assemblies compile as they did before, which gives you more control on how Burst works with your code.

Assembly-level `BurstCompile` attributes iterate with any job or function-pointer attribute, and also with any globally set options from the Burst Editor menu. Burst prioritizes assembly level attributes in the following order:

1. [Editor menu settings](editor-burst-menu.md) take precedence. For example, if you enable **Native Debug Compilation** from the Burst menu, Burst always compiles your code ready for debugging. 
1. Burst checks any `BurstCompile` attribute on a job or function-pointer. If you have `CompileSynchronously = true` in `BurstCompile`, then Burst compiles synchronously
1. Otherwise, Burst sources any remaining settings from any assembly level attribute.

For example:

```c#
[assembly: BurstCompile(OptimizeFor = OptimizeFor.FastCompilation)]

// This job will be optimized for fast-compilation, because the per-assembly BurstCompile asked for it
[BurstCompile]
struct AJob : IJob
{
    // ...
}

// This job will be optimized for size, because the per-job BurstCompile asked for it
[BurstCompile(OptimizeFor = OptimizeFor.Size)]
struct BJob : IJob
{
    // ...
}
```