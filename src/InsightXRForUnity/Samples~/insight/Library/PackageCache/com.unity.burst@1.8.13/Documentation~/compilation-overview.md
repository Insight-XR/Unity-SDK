# Compilation overview

Burst compiles your code in different ways depending on its context.

* In Play mode, Burst compiles your code [just-in-time (JIT)](https://en.wikipedia.org/wiki/Just-in-time_compilation). 
* When you build and run your application to a player, Burst compiles [ahead-of-time (AOT)](https://en.wikipedia.org/wiki/Ahead-of-time_compilation).

## Just-in-time compilation

While your application is in Play mode in the Editor, Burst compiles your code asynchronously at the point that Unity uses it. This means that your code runs under the default [Mono compiler](https://docs.unity3d.com/Manual/Mono.html) until Burst completes its work in the background. 

To force Unity to compile your code synchronously while in the Editor, see [Synchronous compilation](compilation-synchronous.md)

## Ahead-of-time compilation

When you build your project, Burst compiles all the supported code ahead-of-time (AOT) into a native library which Unity ships with your application. To control how Burst compiles AOT, use the [Player Settings window](building-aot-settings.md). Depending on the platform you want to build for, AOT compilation might require access to linker tools. For more information, see [Building your project](building-projects.md).

## Further resources

* [Synchronous compilation](compilation-synchronous.md)
* [BurstCompile attribute](compilation-burstcompile.md)