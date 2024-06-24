# Debugging and profiling tools

The following sections describe how to debug and profile your Burst-compiled code in the Editor and in player builds.

>[!TIP]
>Before attempting to debug Burst-compiled code, enable script debugging for the Editor, or a player build by following the steps in [Debug C# code in Unity](xref:ManagedCodeDebugging). Although you can theoretically debug Burst-compiled code even when the script compilation mode is set to Release, in practice it doesn't work reliably. Breakpoints might be skipped, and variables might not appear in the Locals window, for example.

<a name="debugging-in-editor"></a>

## Debugging Burst-compiled code in the Editor

To debug Burst-compiled code in the Editor, you can either use a managed debugger, or a native debugger. This section explains both options.

### Attach a managed debugger

You can attach a managed debugger such as Visual Studio, Visual Studio for Mac, or JetBrains Rider. This is the same type of debugger you can use to debug regular managed C# code in your Unity project. The ways of attaching a debugger differ depending on the version of Unity you're using:

- **Unity 2022.2+**: When you place a breakpoint inside Burst-compiled code, and you have a managed debugger attached, Unity disables Burst automatically for that code path. This allows you to use a managed debugger to debug the managed version of your code. When you remove all breakpoints from that code path, Unity re-enables Burst for that code path.

- **Unity 2022.1 and older**: Disable Burst, either with the global option in the Editor [Burst menu](editor-burst-menu.md) (**Jobs** &gt; **Burst** &gt; **Enable Compilation**), or comment out the `[BurstCompile]` attribute from the specific entry-point that you want to debug.

### Attach a native debugger

You can attach a native debugger such as Visual Studio or Xcode. Before doing so, you need to disable Burst optimizations. You can do this in the following ways:
    
- Use the **Native Debug Mode Compilation** setting in the Editor [Burst menu](editor-burst-menu.md) (**Jobs** &gt; **Burst** &gt; **Native Debug Mode Compilation**). **Important:** This setting disables optimizations across all jobs, which impacts the performance of Burst code. If you want to disable optimizations only for a specific job, use the other option in this list.
    
- Add the `Debug = true` flag to your job, which disables optimizations and enables debugging on that specific job:

    ```c#
    [BurstCompile(Debug = true)]
    public struct MyJob : IJob
    {
        // ...
    }
    ```

    >[!TIP]
    >Player builds pick up the `Debug` flag, so you can also use this to debug a player build. 

To attach a native debugger to the Unity Editor process, see the [native debugging](#native-debugging) section below.

<a name="debugging-in-player"></a>

## Debugging Burst-compiled code in a player build

Because of the way that Unity builds the code for a player, you need to tell the debugging tool where to find the symbols. To do this, point the tool to the folder that contains the `lib_burst_generated` files, which is usually in the `Plugins` folder.

To debug Burst-compiled code in a player build, you need to attach a native debugger (such as Visual Studio or Xcode) to the player process. Before doing so, you need to:

- Enable symbol generation. You can do this in either of two ways:

    - Enable the **Development Build** option before you build the player, or

    - Enable the **Force Debug Information** option in [Burst AOT Player Settings](building-aot-settings.md)

- Disable Burst optimizations. You can do this in either of two ways:

    - Disable the **Enable Optimizations** option in [Burst AOT Player Settings](building-aot-settings.md). **Important:** This setting disables optimizations across all jobs, which impacts the performance of Burst code.  If you want to disable optimizations only for a specific job, use the other option in this list.

    - Add the `Debug = true` flag to your job, which disables optimizations and enables debugging on that specific job:

        ```c#
        [BurstCompile(Debug = true)]
        public struct MyJob : IJob
        {
            // ...
        }
        ```

To attach a native debugger to the player process, see the [native debugging](#native-debugging) section below.

<a name="native-debugging"></a>

## Native debugging

Follow the instructions above to setup native debugging correctly for the [Editor](#debugging-in-editor) or a [player build](#debugging-in-player). Then, attach a native debugger such as Visual Studio or Xcode.

### Native debugging limitations

* Native debuggers can't discover lambda captures on `Entity.ForEach`, so you can't inspect variables originating from these.
* Structs that use [`[StructLayout(LayoutKind=Explicit)]`](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.layoutkind?view=net-6.0) and have overlapping fields are represented by a struct that hides one of the overlaps. 

Types that are nested, are namespaced in C/C++ style. e.g.

```c#
namespace Pillow
{
	public struct Spot
	{
		public struct SubSpot
		{
            public int a;
            public int b;
        }
		public int a;
		public int b;
		public SubSpot sub;
	}
```

You would refer to SubSpot as Pillow::Spot::SubSpot in this case (for instance if you were trying to cast a pointer in a debugger watch window).

### Code-based breakpoints

Burst supports code-based breakpoints through the [`System.Diagnostics.Debugger.Break`](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.debugger.break?view=net-6.0) method. This method generates a debug trap in your code. You must attach a debugger to your code so that it can intercept the break. Breakpoints trigger whether you've attached a debugger or not. 

Burst adds information to track local variables, function parameters and breakpoints. If your debugger supports conditional breakpoints, use these over adding breakpoints in your code, because they only fire when you've attached a debugger.

## Profiling Burst-compiled code

### Profiling using standalone profiling tools

You can use profiling tools (such as Instruments or Superluminal) to profile Burst-compiled code in a player build. Because of the way that Unity builds the code for a player, you need to tell the profiling tool where to find the symbols. To do this, point the tool to the folder that contains the `lib_burst_generated` files, which is usually in the `Plugins` folder.

<a name="profiler-markers"></a>

### Unity Profiler markers

To improve the data you get from Unity Profiler (either for Burst-compiled code running in the Editor or in an attached player), you can create Unity Profiler markers from Burst code by calling `new ProfilerMarker("MarkerName")`:

```c#
[BurstCompile]
private static class ProfilerMarkerWrapper
{
    private static readonly ProfilerMarker StaticMarker = new ProfilerMarker("TestStaticBurst");

    [BurstCompile(CompileSynchronously = true)]
    public static int CreateAndUseProfilerMarker(int start)
    {
        using (StaticMarker.Auto())
        {
            var p = new ProfilerMarker("TestBurst");
            p.Begin();
            var result = 0;
            for (var i = start; i < start + 100000; i++)
            {
                result += i;
            }
            p.End();
            return result;
        }
    }
}
```
