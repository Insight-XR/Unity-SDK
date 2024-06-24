# BurstCompile attribute

To improve the performance of Burst, you can change how it behaves when it compiles a job with the [`[BurstCompile]`](xref:Unity.Burst.BurstCompileAttribute) attribute. Use it do the following: 

* Use a different accuracy for math functions (for example, sin, cos).
* Relax the order of math computations so that Burst can rearrange the floating point calculations.
* Force a synchronous compilation of a job (only for [just-in-time compilation](compilation.md)).

For example, you can use the `[BurstCompile]` attribute to change the [floating precision](xref:Unity.Burst.FloatPrecision) and [float mode](xref:Unity.Burst.FloatMode) of Burst like so: 

    [BurstCompile(FloatPrecision.Med, FloatMode.Fast)]

## FloatPrecision

Use the [`FloatPrecision`](xref:Unity.Burst.FloatPrecision) enumeration to define Burst's floating precision accuracy.

Float precision is measured in ulp (unit in the last place or unit of least precision). This is the space between floating-point numbers: the value the least significant digit represents if it's 1. `Unity.Burst.FloatPrecision` provides the following accuracy: 

* `FloatPrecision.Standard`: Default value, which is the same as `FloatPrecision.Medium`. This provides an accuracy of 3.5 ulp. 
* `FloatPrecision.High`: Provides an accuracy of 1.0 ulp.
* `FloatPrecision.Medium`: Provides an accuracy of 3.5 ulp.
* `FloatPrecision.Low`: Has an accuracy defined per function, and functions might specify a restricted range of valid inputs.

**Note:** In previous versions of the Burst API, the `FloatPrecision` enum was named `Accuracy`.

### FloatPrecision.Low

If you use the [`FloatPrecision.Low`](xref:Unity.Burst.FloatPrecision) mode, the following functions have a precision of 350.0 ulp. All other functions inherit the ulp from `FloatPrecision.Medium`.

* `Unity.Mathematics.math.sin(x)`
* `Unity.Mathematics.math.cos(x)`
* `Unity.Mathematics.math.exp(x)`
* `Unity.Mathematics.math.exp2(x)`	
* `Unity.Mathematics.math.exp10(x)`	
* `Unity.Mathematics.math.log(x)`
* `Unity.Mathematics.math.log2(x)`
* `Unity.Mathematics.math.log10(x)`	
* `Unity.Mathematics.math.pow(x, y)`
    * Negative `x` to the power of a fractional `y` aren't supported.
* `Unity.Mathematics.math.fmod(x, y)`

## FloatMode

Use the [`FloatMode`](xref:Unity.Burst.FloatMode) enumeration to define Burst's floating point math mode. It provides the following modes:


* `FloatMode.Default`: Defaults to `FloatMode.Strict` mode.
* `FloatMode.Strict`: Burst doesn't perform any re-arrangement of the calculation and respects special floating point values (such as denormals, NaN). This is the default value.
* `FloatMode.Fast`: Burst can perform instruction re-arrangement and use dedicated or less precise hardware SIMD instructions.
* `FloatMode.Deterministic`: Unsupported. Deterministic mode is reserved for a future iteration of Burst. 

For hardware that can support Multiply and Add (e.g mad `a * b + c`) into a single instruction, you can use `FloatMode.Fast` to enable this optimization. However, the reordering of these instructions might lead to a lower accuracy.

Use `FloatMode.Fast` for scenarios where the exact order of the calculation and the uniform handling of NaN values aren't required.

