# Calling Burst-compiled code

You can call Burst-compiled methods direct from managed code. Calling generic methods or methods whose declaring type is generic isn't supported, otherwise the rules as for [function pointers](csharp-function-pointers.md) apply. However, you don't need to worry about the extra boiler plate needed for function pointers.

The following example shows a Burst-compiled utility class. Because it uses structs, it passes by reference per the [function pointer](csharp-function-pointers.md) rules.

```c#
[BurstCompile]
public static class MyBurstUtilityClass
{
    [BurstCompile]
    public static void BurstCompiled_MultiplyAdd(in float4 mula, in float4 mulb, in float4 add, out float4 result)
    {
        result = mula * mulb + add;
    }
}
```
Use this method from managed code like so:

```c#
public class MyMonoBehaviour : MonoBehaviour
{
    void Start()
    {
        var mula = new float4(1, 2, 3, 4);
        var mulb = new float4(-1,1,-1,1);
        var add = new float4(99,0,0,0);
        MyBurstUtilityClass.BurstCompiled_MultiplyAdd(mula, mulb, add, out var result);
        Debug.Log(result);
    }
}
```

If you attach this script to an object and run it, `float4(98f, 2f, -3f, 4f)` is printed to the log. 

## Code transformation

Burst uses IL Post Processing to automatically transform the code into a function pointer and call. For more information, see the documentation on [Function pointers](csharp-function-pointers.md).

To disable the direct call transformation, add`DisableDirectCall = true` to the BurstCompile options. This prevents the Post Processor from running on the code:

```c#
[BurstCompile]
public static class MyBurstUtilityClass
{
    [BurstCompile(DisableDirectCall = true)]
    public static void BurstCompiled_MultiplyAdd(in float4 mula, in float4 mulb, in float4 add, out float4 result)
    {
        result = mula * mulb + add;
    }
}
```