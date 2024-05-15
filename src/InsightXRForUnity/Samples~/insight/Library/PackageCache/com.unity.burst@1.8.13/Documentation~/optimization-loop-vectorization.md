# Loop vectorization

Burst uses [loop vectorization](https://llvm.org/docs/Vectorizers.html#loop-vectorizer) to improve the performance of your code. It uses this technique to loop over multiple values at the same time, rather than looping over single values at a time, which speeds up the performance of your code. For example:

``` c#
[MethodImpl(MethodImplOptions.NoInlining)]
private static unsafe void Bar([NoAlias] int* a, [NoAlias] int* b, int count)
{
    for (var i = 0; i < count; i++)
    {
        a[i] += b[i];
    }
}

public static unsafe void Foo(int count)
{
    var a = stackalloc int[count];
    var b = stackalloc int[count];

    Bar(a, b, count);
}
```

Burst converts the scalar loop in `Bar` into a vectorized loop. Then, instead of looping over a single value at a time, it generates code that loops over multiple values at the same time, which produces faster code. 

This is the `x64` assembly Burst generates for `AVX2` for the loop in `Bar` above:

```x86asm
.LBB1_4:
    vmovdqu    ymm0, ymmword ptr [rdx + 4*rax]
    vmovdqu    ymm1, ymmword ptr [rdx + 4*rax + 32]
    vmovdqu    ymm2, ymmword ptr [rdx + 4*rax + 64]
    vmovdqu    ymm3, ymmword ptr [rdx + 4*rax + 96]
    vpaddd     ymm0, ymm0, ymmword ptr [rcx + 4*rax]
    vpaddd     ymm1, ymm1, ymmword ptr [rcx + 4*rax + 32]
    vpaddd     ymm2, ymm2, ymmword ptr [rcx + 4*rax + 64]
    vpaddd     ymm3, ymm3, ymmword ptr [rcx + 4*rax + 96]
    vmovdqu    ymmword ptr [rcx + 4*rax], ymm0
    vmovdqu    ymmword ptr [rcx + 4*rax + 32], ymm1
    vmovdqu    ymmword ptr [rcx + 4*rax + 64], ymm2
    vmovdqu    ymmword ptr [rcx + 4*rax + 96], ymm3
    add        rax, 32
    cmp        r8, rax
    jne        .LBB1_4
```

Burst has unrolled and vectorized the loop into four `vpaddd` instructions, which calculate eight integer additions each, for a total of 32 integer additions per loop iteration.

## Loop vectorization intrinsics

Burst includes experimental intrinsics to express loop vectorization assumptions: `Loop.ExpectVectorized` and `Loop.ExpectNotVectorized`. Burst then validates the loop vectorization at compile-time. This is useful in a situation where you might break the auto vectorization. For example, if you introduce a branch to the code:

``` c#
[MethodImpl(MethodImplOptions.NoInlining)]
private static unsafe void Bar([NoAlias] int* a, [NoAlias] int* b, int count)
{
    for (var i = 0; i < count; i++)
    {
        if (a[i] > b[i])
        {
            break;
        }

        a[i] += b[i];
    }
}
```

This changes the assembly to the following:

```x86asm
.LBB1_3:
    mov        r9d, dword ptr [rcx + 4*r10]
    mov        eax, dword ptr [rdx + 4*r10]
    cmp        r9d, eax
    jg        .LBB1_4
    add        eax, r9d
    mov        dword ptr [rcx + 4*r10], eax
    inc        r10
    cmp        r8, r10
    jne        .LBB1_3
```

This isn't ideal because the loop is scalar and only has 1 integer addition per loop iteration. It can be difficult to spot this happening in your code, so use the experimental intrinsics `Loop.ExpectVectorized` and `Loop.ExpectNotVectorized` to express loop vectorization assumptions. Burst then validates the loop vectorization at compile-time.

Because the intrinsics are experimental, you need to use the `UNITY_BURST_EXPERIMENTAL_LOOP_INTRINSICS` preprocessor define to enable them.

The following example shows the original `Bar` example with the `Loop.ExpectVectorized` intrinsic:

``` c#
[MethodImpl(MethodImplOptions.NoInlining)]
private static unsafe void Bar([NoAlias] int* a, [NoAlias] int* b, int count)
{
    for (var i = 0; i < count; i++)
    {
        Unity.Burst.CompilerServices.Loop.ExpectVectorized();

        a[i] += b[i];
    }
}
```

Burst then validates at compile-time whether the loop is vectorized. If the loop isn't vectorized, Burst emits a compiler error. The following example produces an error:

``` c#
[MethodImpl(MethodImplOptions.NoInlining)]
private static unsafe void Bar([NoAlias] int* a, [NoAlias] int* b, int count)
{
    for (var i = 0; i < count; i++)
    {
        Unity.Burst.CompilerServices.Loop.ExpectVectorized();

        if (a[i] > b[i])
        {
            break;
        }

        a[i] += b[i];
    }
}
```

Burst emits the following error at compile-time:

>LoopIntrinsics.cs(6,9): Burst error BC1321: The loop is not vectorized where it was expected that it is vectorized.

>[!IMPORTANT]
>These intrinsics don't work inside `if` statements. Burst doesn't prevent this from happening, so you won't see a compile-time error for this.