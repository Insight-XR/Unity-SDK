# Burst Intrinsics Common class

The [`Unity.Burst.Intrinsics.Common`](xref:Unity.Burst.Intrinsics.Common) intrinsics are for functionality shared across the hardware targets that Burst supports.

## Pause

[`Unity.Burst.Intrinsics.Common.Pause`](xref:Unity.Burst.Intrinsics.Common.Pause) is an intrinsic that requests that the CPU pause the current thread. It maps to `pause` on x86, and `yield` on ARM.

Use it to stop spin locks over contending on an atomic access, which reduces contention and power on that section of code.

## Prefetch

The `Unity.Burst.Intrinsics.Common.Prefetch` is an experimental intrinsic that provides a hint that Burst should prefetch the memory location into the cache.

Because the intrinsic is experimental, you must use the `UNITY_BURST_EXPERIMENTAL_PREFETCH_INTRINSIC` preprocessor define to get access to it.

## umul128

Use the [`Unity.Burst.Intrinsics.Common.umul128`](xref:Unity.Burst.Intrinsics.Common.umul128*) intrinsic to access 128-bit unsigned multiplication. These multiplies are useful for hashing functions. It maps 1:1 with hardware instructions on x86 and ARM targets.

## InterlockedAnd & InterlockedOr

The `Unity.Burst.Intrinsics.Common.InterlockedAnd` and `Unity.Burst.Intrinsics.Common.InterlockedOr` are experimental intrinsics that provides atomic and/or operations on `int`, `uint`, `long`, and `ulong` types.

Because these intrinsics are experimental, you must use the `UNITY_BURST_EXPERIMENTAL_ATOMIC_INTRINSICS` preprocessor define to get access to them.