# Processor specific SIMD extensions

Burst exposes all Intel SIMD intrinsics from SSE up to and including AVX2 in the [`Unity.Burst.Intrinsics.X86`](xref:Unity.Burst.Intrinsics.X86) family of nested classes. The [`Unity.Burst.Intrinsics.Arm.Neon`](xref:Unity.Burst.Intrinsics.Arm.Neon) class provides intrinsics for Arm Neon's Armv7, Armv8, and Armv8.2 (RDMA, crypto, dotprod).

## Organizing your code

You should statically import these intrinsics because they contain plain static functions:

```c#
using static Unity.Burst.Intrinsics.X86;
using static Unity.Burst.Intrinsics.X86.Sse;
using static Unity.Burst.Intrinsics.X86.Sse2;
using static Unity.Burst.Intrinsics.X86.Sse3;
using static Unity.Burst.Intrinsics.X86.Ssse3;
using static Unity.Burst.Intrinsics.X86.Sse4_1;
using static Unity.Burst.Intrinsics.X86.Sse4_2;
using static Unity.Burst.Intrinsics.X86.Popcnt;
using static Unity.Burst.Intrinsics.X86.Avx;
using static Unity.Burst.Intrinsics.X86.Avx2;
using static Unity.Burst.Intrinsics.X86.Fma;
using static Unity.Burst.Intrinsics.X86.F16C;
using static Unity.Burst.Intrinsics.X86.Bmi1;
using static Unity.Burst.Intrinsics.X86.Bmi2;
using static Unity.Burst.Intrinsics.Arm.Neon;
```

Burst CPU intrinsics are translated into specific CPU instructions. However, Burst has a special compiler pass which makes sure that your CPU target set in `Burst AOT Settings` is compatible with the intrinsics used in your code. This ensures you don't try to call unsupported instructions (for example, AArch64 Neon on an Intel CPU or AVX2 instructions on an SSE4 CPU), which causes the process to abort with an "Invalid instruction" exception. A compiler error is generated if the check fails.

However, if you want to provide several code paths with different CPU targets, or to make sure your intrinsics code is compatible with any target CPU, you can wrap your intrinsics code with the followinf property checks:

* [IsNeonSupported](xref:Unity.Burst.Intrinsics.Arm.Neon.IsNeonSupported)
* [IsNeonArmv82FeaturesSupported](xref:Unity.Burst.Intrinsics.Arm.Neon.IsNeonArmv82FeaturesSupported)
* [IsNeonCryptoSupported](xref:Unity.Burst.Intrinsics.Arm.Neon.IsNeonCryptoSupported)
* [IsNeonDotProdSupported](xref:Unity.Burst.Intrinsics.Arm.Neon.IsNeonDotProdSupported)
* [IsNeonRDMASupported](xref:Unity.Burst.Intrinsics.Arm.Neon.IsNeonRDMASupported)

For example:

```c#
if (IsAvx2Supported)
{
    // Code path for AVX2 instructions
}
else if (IsSse42Supported)
{
    // Code path for SSE4.2 instructions
}
else if (IsNeonArmv82FeaturesSupported)
{
    // Code path for Armv8.2 Neon instructions
}
else if (IsNeonSupported)
{
    // Code path for Arm Neon instructions
}
else
{
    // Fallback path for everything else
}
```

These branches don't affect performance. Burst evaluates the `IsXXXSupported` properties at compile-time and eliminates unsupported branches as dead code, while the active branch stays there without the if check. Later feature levels implicitly include the previous ones, so you should organize tests from most recent to oldest. Burst emits compile-time errors if you've used intrinsics that aren't part of the current compilation target. Burst doesn't bracket these with a feature level test, which helps you to narrow in on what to put inside a feature test.

If you run your application in .NET, Mono or IL2CPP without Burst enabled, all the `IsXXXSupported` properties return `false`. However, if you skip the test you can still run a reference version of most intrinsics in Mono (exceptions listed below), which is helpful if you need to use the managed debugger. Reference implementations are slow and only intended for managed debugging.

>[!IMPORTANT]
>There isn't a reference managed implementation of Arm Neon intrinsics. This means that you can't use the technique mentioned in the previous paragraph to step through the intrinsics in Mono. FMA intrinsics that operate on doubles don't have a software fallback because of the inherit complexity in emulating fused 64-bit floating point math.






Intrinsics use the types `v64` (Arm only), `v128` and `v256`, which represent a 64-bit, 128-bit or 256-bit vector respectively. For example, given a `NativeArray<float>` and a `Lut` lookup table of v128 shuffle masks, a code fragment like this performs lane left packing, demonstrating the use of vector load/store reinterpretation and direct intrinsic calls:

```c#
v128 a = Input.ReinterpretLoad<v128>(i);
v128 mask = cmplt_ps(a, Limit);
int m = movemask_ps(a);
v128 packed = shuffle_epi8(a, Lut[m]);
Output.ReinterpretStore(outputIndex, packed);
outputIndex += popcnt_u32((uint)m);
```

## Intel intrinsics

The Intel intrinsics API mirrors the [C/C++ Intel instrinsics API](https://software.intel.com/sites/landingpage/IntrinsicsGuide/), with a the following differences:

* All 128-bit vector types (`__m128`, `__m128i` and `__m128d`) are collapsed into `v128`
* All 256-bit vector types (`__m256`, `__m256i` and `__m256d`) are collapsed into `v256`
* All `_mm` prefixes on instructions and macros are dropped, because C# has namespaces
* All bitfield constants (for example, rounding mode selection) are replaced with C# bitflag enum values

## Arm Neon intrinsics

The Arm Neon intrinsics API mirrors the [Arm C Language Extensions](https://developer.arm.com/architectures/instruction-sets/simd-isas/neon/intrinsics), with the following differences:

* All vector types are collapsed into `v64` and `v128`, becoming typeless. This means that the vector type must contain expected element types and count when calling an API.
* The `*x2`, `*x3`, `*x4` vector types aren't supported.
* `poly*` types aren't supported.
* `reinterpret*` functions aren't supported (they aren't needed because of the usage of `v64` and `v128` vector types).
* Intrinsic usage is only supported on Armv8 (64-bit) hardware.

Burst's CPU intrinsics use typeless vectors. Because of this, Burst doesn't perform any type checks. For example, if you call an intrinsic which processes 4 ints on a vector that was initialized with 4 floats, then there's no compiler error. The vector types have fields that represent every element type, in a union-like struct, which gives you flexibility to use these intrinsics in a way that best fits your code.

Arm Neon C intrinsics (ACLE) use typed vectors, for example int32x4_t, and has special APIs (for example, `reinterpret_\*`) to convert to a vector of another element type. Burst CPU intrinsics vectors are typeless, so these APIs are not needed. The following APIs provide the equivalent functionality:

* [v64 (Arm Neon only)](xref:Unity.Burst.Intrinsics.v64)
* [v128](xref:Unity.Burst.Intrinsics.v128)
* [v256](xref:Unity.Burst.Intrinsics.v256)
	

For a categorized index of Arm Neon intrinsics supported in Burst, see the [Arm Neon intrinsics reference](csharp-burst-intrinsics-neon.md).