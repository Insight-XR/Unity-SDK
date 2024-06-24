# SkipLocalsInit attribute

Use [`SkipLocalsInitAttribute`](xref:Unity.Burst.CompilerServices.SkipLocalsInitAttribute), to tell Burst that any stack allocations within a method don't have to be initialized to zero.

In C# all local variables are initialized to zero by default. This is useful because it means an entire class of bugs surrounding undefined data disappears. But this can impact runtime performance, because initializing this data to zero takes work:

```c#
static unsafe int DoSomethingWithLUT(int* data);

static unsafe int DoSomething(int size)
{
    int* data = stackalloc int[size];

    // Initialize every field of data to be an incrementing set of values.
    for (int i = 0; i < size; i++)
    {
        data[i] = i;
    }

    // Use the data elsewhere.
    return DoSomethingWithLUT(data);
}
```

The X86 assembly for this is:

```x86asm
        push    rbp
        .seh_pushreg rbp
        push    rsi
        .seh_pushreg rsi
        push    rdi
        .seh_pushreg rdi
        mov     rbp, rsp
        .seh_setframe rbp, 0
        .seh_endprologue
        mov     edi, ecx
        lea     r8d, [4*rdi]
        lea     rax, [r8 + 15]
        and     rax, -16
        movabs  r11, offset __chkstk
        call    r11
        sub     rsp, rax
        mov     rsi, rsp
        sub     rsp, 32
        movabs  rax, offset burst.memset.inline.X64_SSE4.i32@@32
        mov     rcx, rsi
        xor     edx, edx
        xor     r9d, r9d
        call    rax
        add     rsp, 32
        test    edi, edi
        jle     .LBB0_7
        mov     eax, edi
        cmp     edi, 8
        jae     .LBB0_3
        xor     ecx, ecx
        jmp     .LBB0_6
.LBB0_3:
        mov     ecx, eax
        and     ecx, -8
        movabs  rdx, offset __xmm@00000003000000020000000100000000
        movdqa  xmm0, xmmword ptr [rdx]
        mov     rdx, rsi
        add     rdx, 16
        movabs  rdi, offset __xmm@00000004000000040000000400000004
        movdqa  xmm1, xmmword ptr [rdi]
        movabs  rdi, offset __xmm@00000008000000080000000800000008
        movdqa  xmm2, xmmword ptr [rdi]
        mov     rdi, rcx
        .p2align        4, 0x90
.LBB0_4:
        movdqa  xmm3, xmm0
        paddd   xmm3, xmm1
        movdqu  xmmword ptr [rdx - 16], xmm0
        movdqu  xmmword ptr [rdx], xmm3
        paddd   xmm0, xmm2
        add     rdx, 32
        add     rdi, -8
        jne     .LBB0_4
        cmp     rcx, rax
        je      .LBB0_7
        .p2align        4, 0x90
.LBB0_6:
        mov     dword ptr [rsi + 4*rcx], ecx
        inc     rcx
        cmp     rax, rcx
        jne     .LBB0_6
.LBB0_7:
        sub     rsp, 32
        movabs  rax, offset "DoSomethingWithLUT"
        mov     rcx, rsi
        call    rax
        nop
        mov     rsp, rbp
        pop     rdi
        pop     rsi
        pop     rbp
        ret
```

In this example, the `movabs  rax, offset burst.memset.inline.X64_SSE4.i32@@32` line means that you've had to inject a memset to zero out the data. In the above example, you know that the array is entirely initialized in the following loop, but Burst doesn't know that. 

To fix this problem, use [`Unity.Burst.CompilerServices.SkipLocalsInitAttribute`](xref:Unity.Burst.CompilerServices.SkipLocalsInitAttribute), which tells Burst that any stack allocations within a method don't have to be initialized to zero. 

>[!NOTE]
>Only use this attribute if you're certain that you won't run into undefined behavior bugs. 

For example:

```c#
using Unity.Burst.CompilerServices;

static unsafe int DoSomethingWithLUT(int* data);

[SkipLocalsInit]
static unsafe int DoSomething(int size)
{
    int* data = stackalloc int[size];

    // Initialize every field of data to be an incrementing set of values.
    for (int i = 0; i < size; i++)
    {
        data[i] = i;
    }

    // Use the data elsewhere.
    return DoSomethingWithLUT(data);
}
```

The assembly after adding the `[SkipLocalsInit]` on the method is:

```x86asm
        push    rbp
        .seh_pushreg rbp
        mov     rbp, rsp
        .seh_setframe rbp, 0
        .seh_endprologue
        mov     edx, ecx
        lea     eax, [4*rdx]
        add     rax, 15
        and     rax, -16
        movabs  r11, offset __chkstk
        call    r11
        sub     rsp, rax
        mov     rcx, rsp
        test    edx, edx
        jle     .LBB0_7
        mov     r8d, edx
        cmp     edx, 8
        jae     .LBB0_3
        xor     r10d, r10d
        jmp     .LBB0_6
.LBB0_3:
        mov     r10d, r8d
        and     r10d, -8
        movabs  rax, offset __xmm@00000003000000020000000100000000
        movdqa  xmm0, xmmword ptr [rax]
        mov     rax, rcx
        add     rax, 16
        movabs  rdx, offset __xmm@00000004000000040000000400000004
        movdqa  xmm1, xmmword ptr [rdx]
        movabs  rdx, offset __xmm@00000008000000080000000800000008
        movdqa  xmm2, xmmword ptr [rdx]
        mov     r9, r10
        .p2align        4, 0x90
.LBB0_4:
        movdqa  xmm3, xmm0
        paddd   xmm3, xmm1
        movdqu  xmmword ptr [rax - 16], xmm0
        movdqu  xmmword ptr [rax], xmm3
        paddd   xmm0, xmm2
        add     rax, 32
        add     r9, -8
        jne     .LBB0_4
        cmp     r10, r8
        je      .LBB0_7
        .p2align        4, 0x90
.LBB0_6:
        mov     dword ptr [rcx + 4*r10], r10d
        inc     r10
        cmp     r8, r10
        jne     .LBB0_6
.LBB0_7:
        sub     rsp, 32
        movabs  rax, offset "DoSomethingWithLUT"
        call    rax
        nop
        mov     rsp, rbp
        pop     rbp
        ret
```

The call to memset is now gone, because you've told Burst that any stack allocations within a method don't have to be initialized to zero. 

