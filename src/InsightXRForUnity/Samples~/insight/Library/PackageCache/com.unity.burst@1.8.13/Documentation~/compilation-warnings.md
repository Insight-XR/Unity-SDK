# Compilation warnings 

This page describes common compilation warnings, and how to fix them.

## IgnoreWarning attribute

The [`Unity.Burst.CompilerServices.IgnoreWarningAttribute`](xref:Unity.Burst.CompilerServices.IgnoreWarningAttribute) attribute lets you suppress warnings for a specific function that is being compiled from Burst. However, the warnings that the Burst compiler generates are very important to pay attention to, so this attribute should be used sparingly and only when necessary. The sections below describe the specific situations in which you might want to suppress warnings.

## BC1370

Warning BC1370 produces the message:

> An exception was thrown from a function without the correct `[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]` guard...

This warning happens if Unity encounters a throw in code that `[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]` doesn't guard. In the Editor, thrown exceptions will be caught and logged to the Console, but in a Player build, a `throw` becomes an abort, which crashes your application. Burst warns you about these exceptions, and advises you to place them in functions guarded with `[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]`, because functions guarded with that attribute will not be included in Player builds. However, if you want to purposely throw an exception to crash your application, use the `IgnoreWarningAttribute` to suppress the warnings that Burst provides on the `throw`:

```c#
[IgnoreWarning(1370)]
int DoSomethingMaybe(int x)
{
    if (x < 0) throw new Exception("Dang - sorry I crashed your game!");

    return x * x;
}
```

> [!NOTE]
> This warning is only produced for exceptions that persist into Player builds. Editor-only or debug-only exception throws that aren't compiled into Player builds will not trigger this warning.

## BC1371

Warning BC1371 produces the message:

> A call to the method 'xxx' has been discarded, due to its use as an argument to a discarded method...

To understand this warning, consider the following example:

```c#
[BurstDiscard]
static void DoSomeManagedStuff(int x)
{
    ///.. only run when Burst compilation is disabled
}

// A function that computes some result which we need to pass to managed code
int BurstCompiledCode(int x,int y)
{
    return y+2*x;
}

[BurstCompile]
void BurstMethod()
{
    var myValue = BurstCompiledCode(1,3);
    DoSomeManagedStuff(myValue);
}
```

When Unity compiles your C# code in release mode, it optimizes and removes the local variable `myValue`. This means that Burst receives something like the following code :

```c#
[BurstCompile]
void BurstedMethod()
{
    DoSomeManagedStuff(BurstCompiledCode(1,3));
}
```

This makes Burst generate the warning, because Burst discards `DoSomeManagedStuff`, along with the `BurstCompiledCode` argument. This means that the `BurstCompiledCode` function is no longer executed, which generates the warning.

If this isn't what you intended then ensure the variable has multiple uses. For example: 

```c#

void IsUsed(ref int x)
{
    // Dummy function to prevent removal
}

[BurstCompile]
void BurstedMethod()
{
    var myValue = BurstCompiledCode(1,3);
    DoSomeManagedStuff(myValue);
    IsUsed(ref myValue);
}
```

Alternatively, if you're happy that the code is being discarded correctly, ignore the warning on the `BurstedMethod` like so: 

```c#
[IgnoreWarning(1371)]
[BurstCompile]
void BurstMethod()
{
    var myValue = BurstCompiledCode(1,3);
    DoSomeManagedStuff(myValue);
}
```
