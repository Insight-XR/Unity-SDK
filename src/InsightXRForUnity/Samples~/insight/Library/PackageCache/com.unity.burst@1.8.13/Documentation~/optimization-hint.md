# Hint intrinsics

Use the [`Hint`](xref:Unity.Burst.CompilerServices.Hint) intrinsics to add information to your code which helps with Burst optimization. It has the following methods:

* [`Unity.Burst.CompilerServices.Hint.Likely`](xref:Unity.Burst.CompilerServices.Hint.Likely*): Tells Burst that a Boolean condition is likely to be true.
* [`Unity.Burst.CompilerServices.Hint.Unlikely`](xref:Unity.Burst.CompilerServices.Hint.Unlikely*): Tells Burst that a Boolean condition is unlikely to be true.
* [`Unity.Burst.CompilerServices.Hint.Assume`](xref:Unity.Burst.CompilerServices.Hint.Assume*): Tells Burst that it can assume a Boolean condition is true.

## Likely intrinsic

The `Likely` intrinsic is most useful to tell Burst which branch condition has a high probability of being true. This means that Burst can focus on the branch in question for optimization purposes:

```c#
if (Unity.Burst.CompilerServices.Hint.Likely(b))
{
    // Any code in here will be optimized by Burst with the assumption that we'll probably get here!
}
else
{
    // Whereas the code in here will be kept out of the way of the optimizer.
}
```

## Unlikely intrinsic

The `Unlikely` intrinsic tells Burst the opposite of the `Likely` intrinsic: the condition is unlikely to be true, and it should optimize against it:

```c#
if (Unity.Burst.CompilerServices.Hint.Unlikely(b))
{
    // Whereas the code in here will be kept out of the way of the optimizer.
}
else
{
    // Any code in here will be optimized by Burst with the assumption that we'll probably get here!
}
```

The `Likely` and `Unlikely` intrinsics make sure that Burst places the code most likely to be hit after the branching condition in the binary. This means that the code has a high probability of being in the instruction cache. Burst can also hoist the code out of the likely branch and spend extra time optimizing it, and not spend as much time looking at the unlikely code.

An example of an unlikely branch is to check if result of an allocation is valid. The allocation is valid most of all the time, so you want the code to be fast with that assumption, but you want an error case to fall back to.

## Assume intrinsic

The `Assume` intrinsic is powerful. Use it with caution because it tells Burst that a condition is always true.

>[!WARNING]
>When you use `Assume`, Burst assumes the value is true without checking whether it's true.

```c#
Unity.Burst.CompilerServices.Hint.Assume(b);

if (b)
{
    // Burst has been told that b is always true, so this branch will always be taken.
}
else
{
    // Any code in here will be removed from the program because b is always true!
}
```

Use the `Assume` intrinsic to arbitrarily tell Burst that something is true. For example, you can use `Assume` to tell Burst to assume that a loop end is always a multiple of 16, which means that it can provide perfect vectorization without any scalar spilling for that loop. You could also use it to tell Burst that a value isn't `NaN`, or it's negative.
