# NoAlias attribute

Use the [`[NoAlias]`](xref:Unity.Burst.NoAliasAttribute) attribute to give Burst additional information on the aliasing of pointers and structs. 

In most use cases, you won't need to use the `[NoAlias]` attribute. You don't need to use it with [`[NativeContainer]`](https://docs.unity3d.com/ScriptReference/Unity.Collections.LowLevel.Unsafe.NativeContainerAttribute.html) attributed structs, or with fields in job structs. This is because the Burst compiler infers the no-alias information.

The `[NoAlias]` attribute is exposed so that you can construct complex data structures where Burst can't infer the aliasing. If you use the `[NoAlias]` attribute on a pointer that could alias with another, it might result in undefined behavior and make it hard to track down bugs.

You can use this attribute in the following ways: 

* On a function parameter it signifies that the parameter doesn't alias with any other parameter to the function.
* On a struct field it signifies that the field doesn't alias with any other field of the struct.
* On a struct it signifies that the address of the struct can't appear within the struct itself.
* On a function return value it signifies that the returned pointer doesn't alias with any other pointer returned from the same function.

## NoAlias function parameter

The following is an example of aliasing:

```c#
int Foo(ref int a, ref int b)
{
    b = 13;
    a = 42;
    return b;
}
```

For this, Burst produces the following assembly:

```x86asm
mov     dword ptr [rdx], 13
mov     dword ptr [rcx], 42
mov     eax, dword ptr [rdx]
ret
```

This means that Burst does the following:

* Stores 13 into `b`.
* Stores 42 into `a`.
* Reloads the value from `b` to return it.

Burst has to reload `b` because it doesn't know whether `a` and `b` are backed by the same memory or not.

Add the `[NoAlias]` attribute to the code to change this:

```c#
int Foo([NoAlias] ref int a, ref int b)
{
    b = 13;
    a = 42;
    return b;
}
```

For this, Burst produces the following assembly:

```x86asm
mov     dword ptr [rdx], 13
mov     dword ptr [rcx], 42
mov     eax, 13
ret
```

In this case, the load from `b` has been replaced with moving the constant 13 into the return register.

## NoAlias struct field

The following example is the same as the previous, but applied to a struct:

```c#
struct Bar
{
    public NativeArray<int> a;
    public NativeArray<float> b;
}

int Foo(ref Bar bar)
{
    bar.b[0] = 42.0f;
    bar.a[0] = 13;
    return (int)bar.b[0];
}
```

For this, Burst produces the following assembly:

```x86asm
mov     rax, qword ptr [rcx + 16]
mov     dword ptr [rax], 1109917696
mov     rcx, qword ptr [rcx]
mov     dword ptr [rcx], 13
cvttss2si       eax, dword ptr [rax]
ret
```

In this case, Burst does the following:

* Loads the address of the data in `b` into `rax`.
* Stores 42 into it (`1109917696` is `0x42280000`, which is `42.0f`).
* Loads the address of the data in `a` into `rcx`.
* Stores 13 into it.
* Reloads the data in `b` and converts it to an integer for returning.

If you know that the two `NativeArrays` aren't backed by the same memory, you can change the code to the following:

```c#
struct Bar
{
    [NoAlias]
    public NativeArray<int> a;

    [NoAlias]
    public NativeArray<float> b;
}

int Foo(ref Bar bar)
{
    bar.b[0] = 42.0f;
    bar.a[0] = 13;
    return (int)bar.b[0];
}
```

If you attribute both `a` and `b` with `[NoAlias]` it tells Burst that they don't alias with each other within the struct, which produces the following assembly:

```x86asm
mov     rax, qword ptr [rcx + 16]
mov     dword ptr [rax], 1109917696
mov     rax, qword ptr [rcx]
mov     dword ptr [rax], 13
mov     eax, 42
ret
```

This means that Burst can return the integer constant 42.

## NoAlias struct

Burst assumes that the pointer to a struct doesn't appear within the struct itself. However, there are cases where this isn't true: 

```c#
unsafe struct CircularList
{
    public CircularList* next;

    public CircularList()
    {
        // The 'empty' list just points to itself.
        next = this;
    }
}
```

Lists are one of the few structures where it's normal to have the pointer to the struct accessible from somewhere within the struct itself.

The following example indicates where `[NoAlias]` on a struct can help:

```c#
unsafe struct Bar
{
    public int i;
    public void* p;
}

float Foo(ref Bar bar)
{
    *(int*)bar.p = 42;
    return ((float*)bar.p)[bar.i];
}
```

This produces the following assembly:

```x86asm
mov     rax, qword ptr [rcx + 8]
mov     dword ptr [rax], 42
mov     rax, qword ptr [rcx + 8]
mov     ecx, dword ptr [rcx]
movss   xmm0, dword ptr [rax + 4*rcx]
ret
```

In this case, Burst:
* Loads `p` into `rax`.
* Stores 42 into `p`.
* Loads `p` into `rax` again.
* Loads `i` into `ecx`.
* Returns the index into `p` by `i`.

In this situation, Burst loads `p` twice. This is because it doesn't know if `p` points to the address of the struct `bar`. Once it stores 42 into `p` it has to reload the address of `p` from `bar`, which is a costly operation.

Add `[NoAlias]` to prevent this:

```c#
[NoAlias]
unsafe struct Bar
{
    public int i;
    public void* p;
}

float Foo(ref Bar bar)
{
    *(int*)bar.p = 42;
    return ((float*)bar.p)[bar.i];
}
```

This produces the following assembly:

```x86asm
mov     rax, qword ptr [rcx + 8]
mov     dword ptr [rax], 42
mov     ecx, dword ptr [rcx]
movss   xmm0, dword ptr [rax + 4*rcx]
ret
```

In this situation, Burst only loads the address of `p` once, because `[NoAlias]` tells it that `p` can't be the pointer to `bar`.

## NoAlias function return

Some functions can only return a unique pointer. For instance, `malloc` only returns a unique pointer. In this case, `[return:NoAlias]` gives some useful information to Burst.

>[!IMPORTANT]
>Only use `[return: NoAlias]` on functions that are guaranteed to produce a unique pointer. For example, with bump-allocations, or with things like `malloc`. Burst aggressively inlines functions for performance considerations, so with small functions, Burst inlines them into their parents to produce the same result without the attribute.

The following example uses a bump allocator backed with a stack allocation:

```c#
// Only ever returns a unique address into the stackalloc'ed memory.
// We've made this no-inline because Burst will always try and inline
// small functions like these, which would defeat the purpose of this
// example
[MethodImpl(MethodImplOptions.NoInlining)]
unsafe int* BumpAlloc(int* alloca)
{
    int location = alloca[0]++;
    return alloca + location;
}

unsafe int Func()
{
    int* alloca = stackalloc int[128];

    // Store our size at the start of the alloca.
    alloca[0] = 1;

    int* ptr1 = BumpAlloc(alloca);
    int* ptr2 = BumpAlloc(alloca);

    *ptr1 = 42;
    *ptr2 = 13;

    return *ptr1;
}
```

This produces the following assembly:

```x86asm
push    rsi
push    rdi
push    rbx
sub     rsp, 544
lea     rcx, [rsp + 36]
movabs  rax, offset memset
mov     r8d, 508
xor     edx, edx
call    rax
mov     dword ptr [rsp + 32], 1
movabs  rbx, offset "BumpAlloc(int* alloca)"
lea     rsi, [rsp + 32]
mov     rcx, rsi
call    rbx
mov     rdi, rax
mov     rcx, rsi
call    rbx
mov     dword ptr [rdi], 42
mov     dword ptr [rax], 13
mov     eax, dword ptr [rdi]
add     rsp, 544
pop     rbx
pop     rdi
pop     rsi
ret
```

The key things that Burst does:

* Has `ptr1` in `rdi`.
* Has `ptr2` in `rax`.
* Stores 42 into `ptr1`.
* Stores 13 into `ptr2`.
* Loads `ptr1` again to return it.

If you add the `[return: NoAlias]` attribute:

```c#
[MethodImpl(MethodImplOptions.NoInlining)]
[return: NoAlias]
unsafe int* BumpAlloc(int* alloca)
{
    int location = alloca[0]++;
    return alloca + location;
}

unsafe int Func()
{
    int* alloca = stackalloc int[128];

    // Store our size at the start of the alloca.
    alloca[0] = 1;

    int* ptr1 = BumpAlloc(alloca);
    int* ptr2 = BumpAlloc(alloca);

    *ptr1 = 42;
    *ptr2 = 13;

    return *ptr1;
}
```

It produces the following assembly:

```x86asm
push    rsi
push    rdi
push    rbx
sub     rsp, 544
lea     rcx, [rsp + 36]
movabs  rax, offset memset
mov     r8d, 508
xor     edx, edx
call    rax
mov     dword ptr [rsp + 32], 1
movabs  rbx, offset "BumpAlloc(int* alloca)"
lea     rsi, [rsp + 32]
mov     rcx, rsi
call    rbx
mov     rdi, rax
mov     rcx, rsi
call    rbx
mov     dword ptr [rdi], 42
mov     dword ptr [rax], 13
mov     eax, 42
add     rsp, 544
pop     rbx
pop     rdi
pop     rsi
ret
```

In this case, Burst doesn't reload `ptr2`, and moves 42 into the return register.
