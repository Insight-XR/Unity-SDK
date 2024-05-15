# C#/.NET System namespace support

Burst provides support for some of the `System` namespace, transforming these into Burst compatible variants in the Burst compiler.

## `System.Math`

Burst supports all methods that `System.Math` declares, with the following exceptions:

 - `double IEEERemainder(double x, double y)` is only supported when Api Compatibility Level is set to .NET Standard 2.1 in project settings

## `System.IntPtr`

Burst supports all methods of `System.IntPtr`/`System.UIntPtr`, including the static fields `IntPtr.Zero` and `IntPtr.Size`

## `System.Threading.Interlocked`

Burst supports atomic memory intrinsics for all methods provided by `System.Threading.Interlocked` (for example, `Interlocked.Increment`).

Make sure that the source location of the interlocked methods are naturally aligned. For example, the alignment of the pointer is a multiple of the pointed-to-type:

```c#
[StructLayout(LayoutKind.Explicit)]
struct Foo
{
    [FieldOffset(0)] public long a;
    [FieldOffset(5)] public long b;

    public long AtomicReadAndAdd()
    {
        return Interlocked.Read(ref a) + Interlocked.Read(ref b);
    }
}
```

If the pointer to the struct `Foo` has an alignment of 8, which is the natural alignment of a `long` value, the `Interlocked.Read` of `a` would be successful because it lies on a naturally aligned address. However, `b` would not be successful and undefined behavior happens at the load of `b` as a result.

## `System.Threading.Thread`

Burst supports the `MemoryBarrier` method of `System.Threading.Thread`.

## `System.Threading.Volatile`

Burst supports the non-generic variants of `Read` and `Write` provided by `System.Threading.Volatile`.
