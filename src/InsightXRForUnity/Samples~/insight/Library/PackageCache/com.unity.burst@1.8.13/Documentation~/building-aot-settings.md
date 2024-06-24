# Burst AOT Player Settings reference

To control Burst's AOT compilation, use the [Player Settings window](https://docs.unity3d.com/Manual/class-PlayerSettings.html) (**Edit &gt; Player Settings &gt; Burst AOT Settings**). These settings override the Burst settings in the [Jobs menu](editor-burst-menu.md) when you make a build of your project.

![Burst AOT Settings](images/burst_aot_settings.png)

|**Setting**|**Function**|
|---|---|
|**Target Platform**| Displays the current platform. To change the platform, go to **File &gt; Build Settings**. You can set these Player Settings per platform.|
|**Enable Burst Compilation**| Enable this setting to turn Burst compilation on. Disable this setting to deactivate Burst compilation for the selected platform.|
|**Enable Optimizations**| Enable this setting to activate Burst optimizations.|
|**Force Debug Information**| Enable this setting to make Burst generate debug information. This adds debug symbols to your project, even in release builds of your project, so that when you load it in a debugger you can see file and line information.|
|**Use Platform SDK Linker**<br/>(Windows, macOS, and Linux builds only)| Disables cross compilation support. When you enable this setting, you must use platform-specific tools for your target platform. Only enable this setting for debugging purposes. For more information, see [Platforms with cross compilation disabled](building-projects.md#cross-compilation).|
|**Target 32Bit CPU Architectures**<br/>(Displayed if the architecture is supported)| Select the CPU architectures that you want to use for 32 bit builds. By default, SSE2 and SSE4 are selected.|
|**Target 64Bit CPU Architectures**<br/>(Displayed if the architecture is supported)| Select the CPU architectures that you want to use for 64-bit builds. By default, SSE2 and SSE4 are selected.|
|**Target Arm 64Bit CPU Architectures**<br/>(Displayed if the architecture is supported)| Select the CPU architectures that you want to use for Arm 64-bit builds. By default, ARMV8A is selected.|
|**Optimize For**| Select which optimization settings to compile Burst code for. For more information see [`OptimizeFor`](xref:Unity.Burst.OptimizeFor).|
|Performance|Optimizes the job to run as fast as possible.|
|Size|Optimizes to make the code generation as small as possible.|
|Fast Compilation|Compiles code as fast as possible, with minimal optimization. Burst doesn't perform any  vectorization, inlining, or loop optimizations. |
|Balanced</br>(Default)|Optimizes for code that runs fast but keeps compile time as low as possible.|
|**Disabled Warnings**| Specify a semi-colon separated list of Burst warning numbers to disable the warnings for a player build. Unity shares this setting across all platforms. This can be useful if you want to ignore specific [compilation warnings](compilation-warnings.md) while testing your application. |

The **CPU Architecture** setting is only supported for Windows, macOS, Linux and Android. Unity builds a Player that supports the CPU architectures you've selected. Burst generates a special dispatch into the module, so that the code generated detects the CPU the target platform uses and selects the appropriate CPU architecture at runtime.

## Optimize For setting

>[!NOTE]
>Any [OptimizeFor](xref:Unity.Burst.OptimizeFor) setting is the global default optimization setting for any Burst job or function-pointer. If any assembly level `BurstCompile`, or a specific Burst job or function-pointer has an `OptimizeFor` setting, it overrides the global optimization setting for those jobs.

To control how Burst optimizes your code, use the **Optimize For** setting in the Editor, or use the [`OptimizeFor`](xref:Unity.Burst.OptimizeFor) field:

```c#
[BurstCompile(OptimizeFor = OptimizeFor.FastCompilation)]
public struct MyJob : IJob
{
    // ...
}
```