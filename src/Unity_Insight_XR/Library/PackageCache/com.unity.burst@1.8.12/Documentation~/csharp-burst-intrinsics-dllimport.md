## DllImport and internal calls

To call native functions, use [`[DllImport]`](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.dllimportattribute?view=net-6.0):

```c#
[DllImport("MyNativeLibrary")]
public static extern int Foo(int arg);
```

Burst also supports internal calls implemented inside Unity:

```c#
// In UnityEngine.Mathf
[MethodImpl(MethodImplOptions.InternalCall)]
public static extern int ClosestPowerOfTwo(int value);
```

`DllImport` is only supported for [native plug-ins](https://docs.unity3d.com/Manual/NativePlugins.html), not platform-dependent libraries like `kernel32.dll`.

For all `DllImport` and internal calls, you can only use the following types as parameter or return types:

|**Type**|**Supported type**|
|---|---|
|Built-in and intrinsic types|`byte` / `sbyte`<br/>`short` / `ushort`<br/>`int` / `uint`<br/>`long` / `ulong`<br/>`float`<br/>`double`<br/>`System.IntPtr` / `System.UIntPtr`<br/>`Unity.Burst.Intrinsics.v64` / `Unity.Burst.Intrinsics.v128` / `Unity.Burst.Intrinsics.v256`|
|Pointers and references|`sometype*` : Pointer to any of the other types in this list<br/>`ref sometype` : Reference to any of the other types in this list
|Handle structs|`unsafe struct MyStruct { void* Ptr; }` : Struct containing a single pointer field<br/>`unsafe struct MyStruct { int Value; }` : Struct containing a single integer field

> [!NOTE]
>Passing structs by value isn't supported; you need to pass them through a pointer or reference. The only exception is that handle structs are supported. These are structs that contain a single field of pointer or integer type.
