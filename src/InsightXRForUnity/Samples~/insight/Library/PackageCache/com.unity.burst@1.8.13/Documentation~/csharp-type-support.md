# C#/.NET type support

Burst works on a subset of .NET that doesn't let you use any managed objects or reference types in your code (classes in C#).

The following sections gives more details about the constructs that Burst supports, and any limitations they have.

* [Built-in types](#built-in)
* [Array types](#arrays)
* [Struct types](#structs)
* [Generic types](#generics)
* [Vector types](#vectors)
* [Enum types](#enums)
* [Pointer types](#pointers)
* [Span types](#spans)
* [Tuple types](#tuples)

<a name="built-in"></a>

## Built-in types

### Supported built-in types

Burst supports the following built-in types:

* `bool`
* `byte`/`sbyte`
* `double`
* `float`
* `int`/`uint`
* `long`/`ulong`
* `short`/`ushort`

### Unsupported built-in types

Burst doesn't support the following built-in types:

* `char`
* `decimal`
* `string` because this is a managed type

<a name="arrays"></a>

## Array types

### Supported array types

Burst supports read-only managed arrays loaded from static read-only fields:

```c#
[BurstCompile]
public struct MyJob : IJob {
    private static readonly int[] _preComputeTable = new int[] { 1, 2, 3, 4 };

    public int Index { get; set; }

    public void Execute()
    {
        int x = _preComputeTable[0];
        int z = _preComputeTable[Index];
    }
}
```

However, accessing a static read-only managed array has the following restrictions:

* You can only use the read-only static managed array directly and can't pass it around, for example as a method argument.
* C# code that doesn't use jobs shouldn't modify the read-only static array's elements. This is because the Burst compiler makes a read-only copy of the data at compilation time.
* Multi-dimensional arrays aren't supported.

If you've used an unsupported static constructor, Burst produces the error `BC1361`.

For more information on how Burst initializes arrays, see [Static readonly fields and static constructors](csharp-static-read-only-support.md).

### Unsupported array types

Burst doesn't support managed arrays. Instead, use a native container such as [NativeArray<T>](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html).


<a name="structs"></a>

## Struct types

### Supported structs

Burst supports the following structs:

* Regular structs with any field with supported types
* Structs with fixed array fields

>[!NOTE]
>Structs with an explicit layout might generate non-optimal native code.

### Supported struct layout

Burst supports the following struct layouts:

* `LayoutKind.Sequential`
* `LayoutKind.Explicit` 
* `StructLayoutAttribute.Pack` 
* `StructLayoutAttribute.Size` 

Burst supports `System.IntPtr` and `System.UIntPtr` natively as intrinsic structs that directly represent pointers.

<a name="generics"></a>

## Generic types

Burst supports generic types used with structs. It supports full instantiation of generic calls for generic types that have interface constraints, for example when a struct with a generic parameter needs to implement an interface.

>[!NOTE]
> There are restrictions if you use [generic jobs](compilation-generic-jobs.md).

<a name="vectors"></a>

## Vector types

Burst can translate vector types from [`Unity.Mathematics`](https://docs.unity3d.com/Packages/com.unity.mathematics@latest) to native SIMD vector types with the following first class support for optimizations:

* `bool2`/`bool3`/`bool4`
* `uint2`/`uint3`/`uint4`
* `int2`/`int3`/`int4`
* `float2`/`float3`/`float4`

>[!TIP]
> For performance reasons, use the 4 wide types (`bool4`, `uint4`, `float4`, `int4`, ) over the other types.

<a name="enums"></a>

## Enum types

### Supported enum types

Burst supports all enums including enums that have a specific storage type, for example, `public enum MyEnum : short`.

### Unsupported enums
Burst doesn't support `Enum` methods, for example [`Enum.HasFlag`](https://docs.microsoft.com/en-us/dotnet/api/system.enum.hasflag?view=net-6.0).

<a name="pointers"></a>

## Pointer types

Burst supports any pointer types to any Burst supported types

<a name="spans"></a>

## Span types

Burst supports [`Span<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.span-1?view=net-6.0) and [`ReadOnlySpan<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.readonlyspan-1?view=net-6.0) types in the Unity Editors that support them.

You can only use span types in Burst jobs or function-pointers, but not across the interface to them. This is because in C#'s implementation of the span types it supports taking spans into managed data types (like a managed array). For example, the following code is invalid:

```c#
[BurstCompile]
public static void SomeFunctionPointer(Span<int> span) {}
```

This is because `Span` is used across the managed and Burst boundary. In Burst, span types respect any safety check setting, and only perform performance-intensive checks when safety checks are enabled.

    
<a name="tuples"></a>
    
## Tuple types

Burst supports value tuples [`ValueTuple<T1,T2>`](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/value-tuples) in Burst-compiled jobs or static methods, but not across the interface to them. This is because value tuples are of struct layout [LayoutKind.Auto](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.layoutkind?view=net-7.0). Burst does not support [LayoutKind.Auto](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.layoutkind?view=net-7.0) (to see a list of struct layouts Burst supports see the section [Struct types](#structs)).
However, one can use a regular struct to emulate a tuple like so:


```c#
[BurstCompile]
private struct MyTuple
{
    public int item1;
    public float item2;
}
```

