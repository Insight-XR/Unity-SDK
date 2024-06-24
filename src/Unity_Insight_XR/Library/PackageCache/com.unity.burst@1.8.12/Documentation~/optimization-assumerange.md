# AssumeRange attribute

Use the [`AssumeRange`](xref:Unity.Burst.CompilerServices.AssumeRangeAttribute) attribute to tell Burst that a given scalar-integer lies within a certain constrained range. If Burst has this information, it can improve the performance of your application. The following code is an example of this:

```c#
[return:AssumeRange(0u, 13u)]
static uint WithConstrainedRange([AssumeRange(0, 26)] int x)
{
    return (uint)x / 2u;
}
```

This example tells Burst the following:

* The variable `x` is in the closed-interval range `[0..26]`, or more plainly that `x >= 0 && x <= 26`.
* The return value from `WithConstrainedRange` is in the closed-interval range `[0..13]`, or more plainly that `x >= 0 && x <= 13`.

Burst uses these assumptions to create better code generation. However, there are some restrictions:

* You can only place these on scalar-integer (signed or unsigned) types.
* The type of the range arguments must match the type being attributed.

Burst has deductions for the `.Length` property of `NativeArray` and `NativeSlice` which indicates that these always return non-negative integers:

```c#
static bool IsLengthNegative(NativeArray<float> na)
{
    // Burst always replaces this with the constant false
    return na.Length < 0;
}
```

For example, if you have a container like the following:

```c#
struct MyContainer
{
    public int Length;
    
    // Some other data...
}
```

The following example shows how to tell Burst that `Length` is always a positive integer:

```c#
struct MyContainer
{
    private int _length;

    [return: AssumeRange(0, int.MaxValue)]
    private int LengthGetter()
    {
        return _length;
    }

    public int Length
    {
        get => LengthGetter();
        set => _length = value;
    }

    // Some other data...
}
```