# Aliasing and the job system

Unity's job system infrastructure has some limitations on what can alias within a job struct:

* Structs attributed with [`[NativeContainer]`](https://docs.unity3d.com/ScriptReference/Unity.Collections.LowLevel.Unsafe.NativeContainerAttribute.html) (for example, [`NativeArray`](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html) and [`NativeSlice`](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeSlice_1.html)) that are members of a job struct don't alias.
* Job struct members with the [`[NativeDisableContainerSafetyRestriction]`](https://docs.unity3d.com/ScriptReference/Unity.Collections.LowLevel.Unsafe.NativeDisableContainerSafetyRestrictionAttribute.html) attribute can alias with other members. This is because this attribute explicitly opts in to this kind of aliasing.
* Pointers to structs attributed with `[NativeContainer]` can't appear in other structs attributed with `[NativeContainer]`. For example, you can't have a `NativeArray<NativeSlice<T>>`. 

The following example job shows how these limitations work in practice:

```c#
[BurstCompile]
private struct MyJob : IJob
{
    public NativeArray<float> a;
    public NativeArray<float> b;
    public NativeSlice<int> c;

    [NativeDisableContainerSafetyRestriction]
    public NativeArray<byte> d;

    public void Execute() { ... }
}
```

* `a`, `b`, and `c` don't alias with each other.
* `d` can alias with `a`, `b`, or `c`.

>[!TIP]
>If you're used to working with C/C++'s [Type Based Alias Analysis (TBAA)](https://en.wikipedia.org/wiki/Alias_analysis#Type-based_alias_analysis), then you might assume that because `d` has a different type from `a`, `b`, or `c`, it shouldn't alias. However, in C#, pointers don't have any assumptions that pointing to a different type results in no aliasing. This is why `d` is assumed to alias with `a`, `b`, or `c`. 