# BurstDiscard attribute

If you're running C# code not inside Burst-compiled code, you might want to use managed objects, but not compile these portions of code within Burst. To do this, use the `[BurstDiscard]` attribute on a method:

```c#
[BurstCompile]
public struct MyJob : IJob
{
    public void Execute()
    {
        // Only executed when running from a full .NET runtime
        // this method call will be discard when compiling this job with
        // [BurstCompile] attribute
        MethodToDiscard();
    }

    [BurstDiscard]
    private static void MethodToDiscard(int arg)
    {
        Debug.Log($"This is a test: {arg}");
    }
}
```
>[!NOTE]
>A method with `[BurstDiscard]` can't have a return value.

You can use a `ref` or `out` parameter, which indicates whether the code is running on Burst or managed:

```c#
[BurstDiscard]
private static void SetIfManaged(ref bool b) => b = false;

private static bool IsBurst()
{
    var b = true;
    SetIfManaged(ref b);
    return b;
}
```

