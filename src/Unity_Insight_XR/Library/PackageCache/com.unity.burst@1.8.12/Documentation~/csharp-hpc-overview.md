## HPC# overview

Burst uses a high performance subset of C# called High Performance C# (HPC#).

## Supported C# features in HPC#

HPC# supports most expressions and statements in C#. It supports the following:

|**Supported feature**|**Notes**|
|---|---|
|Extension methods.||
|Instance methods of structs.||
|Unsafe code and pointer manipulation.||
|Loading from static read-only fields.|For more information, see the documentation on [Static read-only fields and static constructors](csharp-static-read-only-support.md).|
|Regular C# control flows.|`if`<br/>`else`<br/>`switch`<br/>`case`<br/>`for`<br/>`while`<br/>`break`<br/>`continue`|
|`ref` and `out` parameters||
|`fixed` statements||
|Some [IL opcodes](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes?view=net-6.0).|`cpblk`<br/> `initblk`<br/> `sizeof`|
|`DLLImport` and internal calls.|For more information, see the documentation on [`DLLImport` and internal calls](csharp-burst-intrinsics-dllimport.md).|
|`try` and `finally` keywords. Burst also supports the associated [IDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable?view=net-6.0) patterns, `using` and `foreach`.|If an exception happens in Burst, the behavior is different from .NET. In .NET, if an exception occurs inside a `try` block, control flow goes to the `finally` block. However, in Burst, if an exception happens inside or outside a `try` block, the exception throws as if any `finally` blocks do not exist. Invoking `foreach` calls is supported by Burst, but there is a `foreach` edge case that burst currently does not support (see ["Foreach and While" section](#foreach-and-while) for more details).|
|Strings and `ProfilerMarker`.|For more information, see the documentation on [Support for Unity Profiler markers](debugging-profiling-tools.md#profiler-markers).|
|`throw` expressions.| Burst only supports simple `throw` patterns, for example, `throw new ArgumentException("Invalid argument")`. When you use simple patterns like this, Burst extracts the static string exception message and includes it in the generated code.|
|Strings and `Debug.Log`.|Only partially supported. For more information, see the documentation on [String support and `Debug.Log`](csharp-string-support.md). |

Burst also provides alternatives for some C# constructions not directly accessible to HPC#:

* [Function pointers](csharp-function-pointers.md) as an alternative to using delegates within HPC#
* [Shared static](csharp-shared-static.md) to access static mutable data from both C# and HPC#

### Exception expressions

Burst supports `throw` expressions for exceptions. Exceptions thrown in the **editor** can be caught by managed code, and are reported in the console window. Exceptions thrown in **player builds** will always cause the application to abort. Thus with Burst you should only use exceptions for exceptional behavior. To ensure that code doesn't end up relying on exceptions for things like general control flow, Burst produces the following warning on code that tries to `throw` within a method not attributed with `[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]`:

> Burst warning BC1370: An exception was thrown from a function without the correct [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")] guard. Exceptions only work in the editor and so should be protected by this guard

<a name="foreach-and-while"></a>
### Foreach and While

Burst supports invoking `foreach` and `while`. However, there is an edge case which is currently unsupported - methods that take one or more generic collection parameters `T: IEnumerable<U>` and invoke `foreach` or `while` on at least one of the collections in the method body. 
To illustrate, the following example methods exemplify this limitation:
```c#
public static void IterateThroughConcreteCollection(NativeArray<int> list)
{
    foreach (var element in list)
    {
        // This works
    }
}

public static void IterateThroughGenericCollection<S>(S list) where S : struct, IEnumerable<int>
{
    foreach (var element in list)
    {
        // This doesn't work
    }
}
```
Note that the uppermost method `IterateThroughConcreteCollection()`'s parameter is specified to be a *concrete* collection type, in this case `NativeArray<int>`. Because it's concrete iterating through it inside the method will compile in Burst. 
In the method `IterateThroughGenericCollection()` below it, however, the parameter is specified to be a *generic* collection type `S`. Iterating through `S` inside the method will therefore *not* compile in Burst. It will instead throw the following error:
> Can't call the method (method name) on the generic interface object type (object name). This may be because you are trying to do a foreach over a generic collection of type IEnumerable.
 

## Unsupported C# features in HPC#

HPC# doesn't support the following C# features:

* Catching exceptions `catch` in a `try`/`catch`.
* Storing to static fields except via [Shared Static](csharp-shared-static.md)
* Any methods related to managed objects, for example, string methods.
