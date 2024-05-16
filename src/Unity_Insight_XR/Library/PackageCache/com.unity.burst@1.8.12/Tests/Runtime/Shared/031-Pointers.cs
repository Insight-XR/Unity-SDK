using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityBenchShared;

namespace Burst.Compiler.IL.Tests
{
    internal class Pointers
    {
        [TestCompiler(1)]
        [TestCompiler(4)]
        [TestCompiler(5)]
        public static int CheckAddressOf(int a)
        {
            var value = new MyIntValue(a);
            ref int intValue = ref value.GetValuePtr();
            return intValue * 10 + 1;
        }

        public struct MyIntValue
        {
            public MyIntValue(int value)
            {
                Value = value;
            }

            public int Value;

            public unsafe ref int GetValuePtr()
            {
                fixed (void* ptr = &this)
                {
                    return ref *(int*) ptr;
                }
            }
        }

        [TestCompiler(0, MyCastEnum.Value2)]
        [TestCompiler(1, MyCastEnum.Value0)]
        [TestCompiler(2, MyCastEnum.Value3)]
        public static unsafe MyCastEnum PointerCastEnum(int value, MyCastEnum newValue)
        {
            var ptvalue = new IntPtr(&value);
            var pEnum = (MyCastEnum*) ptvalue;
            *pEnum = newValue;
            return *pEnum;
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [TestCompiler(50, 50)]
        public static unsafe bool PointerIEquatable(IntPtr a, IntPtr b)
        {
            return a.Equals(b); // This
        }
#endif

        [TestCompiler(0, 0)]
        [TestCompiler(0, 1)]
        [TestCompiler(1, 0)]
        public static unsafe bool PointerCompare(IntPtr a, IntPtr b)
        {
            return a == b;
        }

        [TestCompiler(0)]
        [TestCompiler(1)]
        [TestCompiler(2)]
        public static unsafe bool RawPointerCompare(IntPtr value)
        {
            return (void*)value == (void*)1;
        }

        [TestCompiler(0)]
        [TestCompiler(1)]
        [TestCompiler(42424242)]
        [TestCompiler(0xC0FFEE4DEADBEEF)]
        public static unsafe int PointerHash(IntPtr value)
        {
            return value.GetHashCode();
        }

        [TestCompiler(0)]
        [TestCompiler(1)]
        [TestCompiler(42424242)]
        public static unsafe IntPtr PointerToPointer(IntPtr value)
        {
            return new IntPtr(value.ToPointer());
        }

        [TestCompiler(0, ExpectCompilerException = true, ExpectedDiagnosticId = DiagnosticId.ERR_CallingManagedMethodNotSupported)]
        public static unsafe int PointerToString(IntPtr value)
        {
            return value.ToString().Length;
        }

        [TestCompiler(1)]
        [TestCompiler(255)]
        [TestCompiler(12351235)]
        public static unsafe int PointerAdd(int a)
        {
            var pA = (byte*)&a;
            var pDest = pA + 3;
            *pDest = (byte)a;
            return a;
        }

        [TestCompiler(1)]
        [TestCompiler(255)]
        [TestCompiler(12351235)]
        public static unsafe int PointerSub(int a)
        {
            var pA = (byte*)&a;
            var pDest = pA + 3;
            *(pDest - 1) = (byte)a;
            return a;
        }

        [TestCompiler]
        public static unsafe int PointerPointerSub()
        {
            var value = new StructForPointerPointerSub();
            int* pa = &value.A;
            int* pb = &value.B;
            var auto = (pb - pa);
            return (int)auto;
        }

        [TestCompiler]
        public static unsafe int WhileWithPointer()
        {
            var check = new CheckPointers { X = 1, Y = 2, Z = 3, W = 4 };
            int* pstart = &check.X;
            int* pend = &check.W;
            int result = 0;
            while (pstart <= pend)
            {
                result += *pstart;
                pstart++;
            }

            return result;
        }

        struct StructForPointerPointerSub
        {
            public int A;
            public int B;
        }



        [TestCompiler(1)]
        [TestCompiler(255)]
        [TestCompiler(12351235)]
        public static IntPtr IntPtrConstructor(int a)
        {
            return new IntPtr(a);
        }

        [TestCompiler(1U)]
        [TestCompiler(255U)]
        [TestCompiler(12351235U)]
        public static UIntPtr UIntPtrConstructor(uint a)
        {
            return new UIntPtr(a);
        }

        [TestCompiler(1)]
        [TestCompiler(255)]
        [TestCompiler(12351235)]
        public static int IntPtrToInt32(int a)
        {
            return new IntPtr(a).ToInt32();
        }

        [TestCompiler(1)]
        [TestCompiler(255)]
        [TestCompiler(12351235)]
        public static long IntPtrToInt64(int a)
        {
            return new IntPtr(a).ToInt64();
        }

        [TestCompiler(OverrideOn32BitNative = 4)]
        public static int IntPtrSize()
        {
            return IntPtr.Size;
        }

        // asserted in IntPtrProcessor
        [TestCompiler(OverrideOn32BitNative = true)]
        public static bool IntPtrSizeCompared()
        {
            return IntPtr.Size == 4;
        }

        [TestCompiler]
        public static IntPtr IntPtrZero()
        {
            return IntPtr.Zero;
        }

        [TestCompiler(1)]
        [TestCompiler(5)]
        public static IntPtr IntPtrAdd(IntPtr a)
        {
            return IntPtr.Add(a, 1);
        }


        [TestCompiler(1)]
        [TestCompiler(5)]
        public static IntPtr IntPtrAdd2(IntPtr a)
        {
            return a + 1;
        }

        [TestCompiler(1)]
        [TestCompiler(5)]
        public static IntPtr IntPtrSub(IntPtr a)
        {
            return IntPtr.Subtract(a, 1);
        }


        [TestCompiler(1)]
        [TestCompiler(5)]
        public static IntPtr IntPtrSub2(IntPtr a)
        {
            return a - 1;
        }

        [TestCompiler]
        public static UIntPtr UIntPtrZero()
        {
            return UIntPtr.Zero;
        }

        [TestCompiler(1U)]
        [TestCompiler(5U)]
        public static UIntPtr UIntPtrAdd(UIntPtr a)
        {
            return UIntPtr.Add(a, 1);
        }

        [TestCompiler(1U)]
        [TestCompiler(5U)]
        public static UIntPtr UIntPtrSubstract(UIntPtr a)
        {
            return UIntPtr.Subtract(a, 1);
        }

        [TestCompiler(1)]
        public static unsafe int PointerAccess(int a)
        {
            var value = a;
            var pValue = &value;
            pValue[0] = a + 5;
            return value;
        }

        [TestCompiler(0)] // Keep it at 0 only!
        public static unsafe int PointerAccess2(int a)
        {
            int value = 15;
            var pValue = &value;
            pValue[a] = value + 5;
            return value;
        }

        [TestCompiler(0)] // Keep it at 0 only!
        public static unsafe float PointerAccess3(int a)
        {
            float value = 15.0f;
            var pValue = &value;
            pValue[a] = value + 5.0f;
            return value;
        }

        [TestCompiler(0)]
        public static unsafe int PointerCompareViaInt(int a)
        {
            int b;
            if (&a == &b)
                return 1;
            else
                return 0;
        }

        [TestCompiler(0)]
        public static unsafe int IntPtrCompare(int a)
        {
            int b;
            IntPtr aPtr = (IntPtr)(&a);
            IntPtr bPtr = (IntPtr)(&b);
            if (aPtr == bPtr)
                return 1;
            else
                return 0;
        }

        [TestCompiler(typeof(IntPtrZeroProvider), 1)]
        [TestCompiler(typeof(IntPtrOneProvider), 2)]
        public static unsafe int UnsafeCompare(int* a, int b)
        {
            if (a == null)
            {
                return 1 + b;
            }

            return 2 + b;
        }

        unsafe struct NativeQueueBlockHeader
        {
#pragma warning disable 0649
            public byte* nextBlock;
            public int itemsInBlock;
#pragma warning restore 0649
        }

        [TestCompiler]
        public static unsafe void PointerCastWithStruct()
        {

            byte* currentWriteBlock = null;
            if (currentWriteBlock != null && ((NativeQueueBlockHeader*) currentWriteBlock)->itemsInBlock == 100)
            {
                ((NativeQueueBlockHeader*) currentWriteBlock)->itemsInBlock = 5;
            }
        }

        private class IntPtrZeroProvider : IArgumentProvider
        {
            public object Value => IntPtr.Zero;
        }

        private class IntPtrOneProvider : IArgumentProvider
        {
            public object Value => new IntPtr(1);
        }

        [TestCompiler]
        public static unsafe int FixedField()
        {
            var fixedStruct = new MyStructWithFixed();
            fixedStruct.Values[0] = 1;
            fixedStruct.Values[1] = 2;
            fixedStruct.Values[2] = 3;
            fixedStruct.Values[9] = 9;

            int result = 0;
            for (int i = 0; i < 10; i++)
            {
                result += fixedStruct.Values[i];
            }
            return result;
        }

        [TestCompiler(typeof(MyStructWithFixedProvider), 1)]
        //[TestCompiler(typeof(MyStructWithFixedProvider), 2)]
        public static unsafe int FixedFieldViaPointer(ref MyStructWithFixed fixedStruct, int i)
        {
            fixed (MyStructWithFixed* check = &fixedStruct)
            {
                int* data = check->Values;
                return data[i];
            }
        }

        [TestCompiler(typeof(MyStructWithFixedProvider))]
        public static unsafe int FixedInt32AndRefInt32(ref MyStructWithFixed fixedStruct)
        {
            fixed (int* data = &fixedStruct.Value)
            {
                // We do a call to ProcessInt after with a ref int
                // to check that we don't collide with the PinnedType introduced by the previous
                // fixed statement
                ProcessInt(ref *data);
            }

            return fixedStruct.Value;
        }

        private static void ProcessInt(ref int value)
        {
            value += 5;
        }

        public unsafe struct ConditionalTestStruct
        {
            public void* a;
            public void* b;
        }

        public unsafe struct PointerConditional : IJob, IDisposable
        {
            public ConditionalTestStruct* t;

            public void Execute()
            {
                t->b = t->a != null ? t->a : null;
            }


            public struct Provider : IArgumentProvider
            {
                public object Value
                {
                    get
                    {
                        var value = new PointerConditional();
                        value.t = (ConditionalTestStruct*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<ConditionalTestStruct>(), 4, Allocator.Persistent);
                        value.t->a = (void*)0x12345678;
                        value.t->b = null;
                        return value;
                    }
                }
            }

            public void Dispose()
            {
                UnsafeUtility.Free(t, Allocator.Persistent);
            }
        }

        [TestCompiler(typeof(PointerConditional.Provider))]
        public static unsafe bool TestConditionalPointer([NoAlias] ref PointerConditional job)
        {
            job.Execute();
            return job.t->a == job.t->b;
        }

#if BURST_TESTS_ONLY
        // Disabled on .Net 7 due to Unsafe.ByteOffset
        [TestCompiler(IgnoreOnNetCore = true)]
        public static int TestFieldOffset()
        {
            var t = default(StructWithFields);
            return (int)Unsafe.ByteOffset(ref Unsafe.As<int, bool>(ref t.a), ref t.d);
        }
#endif

        public struct StructWithFields
        {
            public int a;
            public int b;
            public bool c;
            public bool d;
            public bool e;
            public bool f;
        }

        public unsafe struct MyStructWithFixed
        {
            public fixed int Values[10];
            public int Value;
        }

        private struct MyStructWithFixedProvider : IArgumentProvider
        {
            public unsafe object Value
            {
                get
                {
                    var field = new MyStructWithFixed();
                    for (int i = 0; i < 10; i++)
                    {
                        field.Values[i] = (i + 1) * 5;
                    }

                    field.Value = 1235;

                    return field;
                }
            }
        }

        [TestCompiler(0)]
        public static unsafe void TestCellVisibleInternal(int length)
        {
            int3* cellVisibleRequest = (int3*)0;
            bool*cellVisibleResult = (bool*)0;
            int3* visibleCells = (int3*)0;
            IsCellVisibleInternal(cellVisibleRequest, cellVisibleResult, visibleCells, length, length);
        }

        static unsafe void IsCellVisibleInternal(int3* cellVisibleRequest, bool* cellVisibleResult, int3* visibleCells, int requestLength, int visibleCellsLength)
        {
            for (int r = 0; r < requestLength; r++)
            {
                cellVisibleResult[r] = false;
                for (int i = 0; i < visibleCellsLength; i++)
                {
                    if (visibleCells[i].x == cellVisibleRequest[r].x && visibleCells[i].y == cellVisibleRequest[r].y && visibleCells[i].z == cellVisibleRequest[r].z)
                    {
                        cellVisibleResult[r] = true;
                        break;
                    }
                }
            }
        }

        public enum MyCastEnum
        {
            Value0 = 0,
            Value1 = 1,
            Value2 = 2,
            Value3 = 3,
        }

        public struct CheckPointers
        {
            public int X;
            public int Y;
            public int Z;
            public int W;
        }

        // From https://github.com/Unity-Technologies/ECSJobDemos/issues/244
        [TestCompiler]
        public static unsafe int InitialiseViaCastedPointer()
        {
            int value = 0;

            void* ptr = &value;

            byte* asBytePtr = (byte*)ptr;

            ((int*)asBytePtr)[0] = -1;

            return value;
        }

        [TestCompiler(1)]
        public static unsafe int PointerWriteArg(int a)
        {
            return (int)TestPointerAndGeneric<float>((int*) a);
        }

        private static unsafe int* TestPointerAndGeneric<T>(int* p) where T : struct
        {
            p = (int*)(IntPtr)26;
            return p;
        }


        [TestCompiler(ExpectedDiagnosticId = DiagnosticId.WRN_ExceptionThrownInNonSafetyCheckGuardedFunction)]
        public static void TestBlobAssetReferenceData()
        {
            var blob = new BlobAssetReferenceData(IntPtr.Zero);
            blob.Validate();
        }


        [StructLayout(LayoutKind.Explicit, Size = 16)]
        internal unsafe struct BlobAssetHeader
        {
            [FieldOffset(0)] public void* ValidationPtr;
            [FieldOffset(8)] public int Length;
            [FieldOffset(12)] public Allocator Allocator;
        }

        internal unsafe struct BlobAssetReferenceData
        {
            [NativeDisableUnsafePtrRestriction]
            public byte* _ptr;

            public BlobAssetReferenceData(IntPtr zero)
            {
                _ptr = (byte*)zero;
            }

            internal BlobAssetHeader* Header => ((BlobAssetHeader*)_ptr) - 1;

            public void Validate()
            {
                if (_ptr != null)
                    if (Header->ValidationPtr != _ptr)
                        throw new InvalidOperationException("The BlobAssetReference is not valid. Likely it has already been unloaded or released");
            }
        }

        internal unsafe struct StackAllocCheck
        {
            public int* ptr;

            [MethodImpl(MethodImplOptions.NoInlining)]
            public void AddToPtr(int* otherPtr)
            {
                *otherPtr = 42;
                *ptr += 1;
                *ptr += *otherPtr;
            }

            public class Provider : IArgumentProvider
            {
                public object Value => new StackAllocCheck();
            }
        }

        [TestCompiler(typeof(StackAllocCheck.Provider))]
        public static unsafe bool StackAllocAliasCheck([NoAlias] ref StackAllocCheck stackAllocCheck)
        {
            int* ptr = stackalloc int[1];
            *ptr = 13;

            stackAllocCheck.ptr = ptr;

            stackAllocCheck.AddToPtr(ptr);

            if (*ptr != 86)
            {
                return false;
            }

            *stackAllocCheck.ptr = -4;
            *ptr += 1;
            *ptr += *stackAllocCheck.ptr;

            if (*ptr != -6)
            {
                return false;
            }

            return true;
        }

        [TestCompiler(1)]
        public static unsafe int NativeIntAddCheck(int a)
        {
            return (int)(&a + 1) - (int)&a;
        }

        public unsafe struct PointerArithmetic : IJob, IDisposable
        {
            [NativeDisableUnsafePtrRestriction] public int** pointers;

            public void Execute()
            {
                pointers[10] = pointers[10] + +1;
                pointers[20] = pointers[20] - +1;
                pointers[30] = pointers[30] - -1;
                pointers[40] = pointers[40] + -1;
            }

            public struct Provider : IArgumentProvider
            {
                public object Value
                {
                    get
                    {
                        var value = new PointerArithmetic();
                        value.pointers = (int**)UnsafeUtility.Malloc(1000*sizeof(int*), 8, Allocator.Persistent);
                        UnsafeUtility.MemClear(value.pointers, 1000 * sizeof(int*));
                        return value;
                    }
                }
            }

            public void Dispose()
            {
                UnsafeUtility.Free(pointers, Allocator.Persistent);
            }
        }

        // The arithmetic test has been split to make it easier to see the mismatched value (rather than true!=false)
        // According to : https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/unsafe-code#pointer-types
        // Conversion between pointers and integrals is "Implementation Defined".


        [TestCompiler(typeof(PointerArithmetic.Provider))]
        public static unsafe Int64 TestArithmeticPointerA(ref PointerArithmetic job)
        {
            job.Execute();
            if (sizeof(int*) == 4)
                return (Int64)(UInt32)(job.pointers[10]);   // Workaround IL2CPP 32bit Bug : https://fogbugz.unity3d.com/f/cases/1254635/
            return (Int64)job.pointers[10];
        }

        [TestCompiler(typeof(PointerArithmetic.Provider))]
        public static unsafe Int64 TestArithmeticPointerB(ref PointerArithmetic job)
        {
            job.Execute();
            if (sizeof(int*) == 4)
                return (Int64)(UInt32)(job.pointers[20]);   // Workaround IL2CPP 32bit Bug : https://fogbugz.unity3d.com/f/cases/1254635/
            return (Int64)job.pointers[20];
        }

        [TestCompiler(typeof(PointerArithmetic.Provider))]
        public static unsafe Int64 TestArithmeticPointerC(ref PointerArithmetic job)
        {
            job.Execute();
            if (sizeof(int*) == 4)
                return (Int64)(UInt32)(job.pointers[30]);   // Workaround IL2CPP 32bit Bug : https://fogbugz.unity3d.com/f/cases/1254635/
            return (Int64)job.pointers[30];
        }

        [TestCompiler(typeof(PointerArithmetic.Provider))]
        public static unsafe Int64 TestArithmeticPointerD(ref PointerArithmetic job)
        {
            job.Execute();
            if (sizeof(int*) == 4)
                return (Int64)(UInt32)(job.pointers[40]);   // Workaround IL2CPP 32bit Bug : https://fogbugz.unity3d.com/f/cases/1254635/
            return (Int64)job.pointers[40];
        }

        private struct TestData
        {
            public int3 Min;
            public int Size;
        }

        [TestCompiler]
        public static unsafe int TestPointerWithIn()
        {
            var foo = stackalloc TestData[1];

            *foo = new TestData { Min = new int3(0, 1, 2), Size = 3 };

            return SubFunctionWithInPointer(in foo);
        }

        private static unsafe int SubFunctionWithInPointer(in TestData* node)
        {
            int3 data = node->Min;

            return node->Size + data.x + data.y + data.z;
        }

        /* System.Buffer::Memmove - Disabled on .Net 7 due to  :

            Framework
            IL_000e: conv.ovf.u8 args(IL_000d(ldarg.3))
            IL_000f: call System.Void System.Buffer::Memmove(System.Byte*,System.Byte*,System.UInt64) args(IL_000b(ldarg.1), IL_000c(ldarg.0), IL_000e(conv.ovf.u8))

            .Net 7 (note reference no pointer...)
            IL_000e: conv.ovf.u args(IL_000d(ldarg.3))
            IL_000f: call System.Void System.Buffer::Memmove(System.Byte&,System.Byte&,System.UIntPtr) args(IL_000b(ldarg.1), IL_000c(ldarg.0), IL_000e(conv.ovf.u))
        */
        [TestCompiler(IgnoreOnNetCore = true)]
        public static unsafe int TestSystemBufferMemoryCopy()
        {
            var a = stackalloc int[2];
            a[0] = 42;
            System.Buffer.MemoryCopy(a + 0, a + 1, UnsafeUtility.SizeOf<int>(), UnsafeUtility.SizeOf<int>());
            return a[1];
        }

        [TestCompiler(0ul, byte.MinValue)]
        [TestCompiler(0ul, byte.MaxValue)]
        public static unsafe IntPtr PointerMathAddPNTypesByte(UInt64 p,byte a)
        {
            var pointer = (byte*)p;
            return new IntPtr(pointer + a);   // Pointer LHS
        }

        [TestCompiler(0ul, byte.MinValue)]
        [TestCompiler(0ul, byte.MaxValue)]
        public static unsafe IntPtr PointerMathAddNPTypesByte(UInt64 p,byte a)
        {
            var pointer = (byte*)p;
            return new IntPtr(a + pointer);   // Pointer RHS
        }

        [TestCompiler(0ul, byte.MinValue)]
        [TestCompiler(0ul, byte.MaxValue)]
        public static unsafe IntPtr PointerMathSubPNTypesByte(UInt64 p,byte a)
        {
            var pointer = (byte*)p;
            return new IntPtr(pointer - a);   // Pointer LHS (no RHS since not legal in C#)
        }

        [TestCompiler(0ul, sbyte.MinValue)]
        [TestCompiler(0ul, sbyte.MaxValue)]
        public static unsafe IntPtr PointerMathAddPNTypesSByte(UInt64 p,sbyte a)
        {
            var pointer = (sbyte*)p;
            return new IntPtr(pointer + a);   // Pointer LHS
        }

        [TestCompiler(0ul, sbyte.MinValue)]
        [TestCompiler(0ul, sbyte.MaxValue)]
        public static unsafe IntPtr PointerMathAddNPTypesSByte(UInt64 p,sbyte a)
        {
            var pointer = (sbyte*)p;
            return new IntPtr(a + pointer);   // Pointer RHS
        }

        [TestCompiler(0ul, sbyte.MinValue)]
        [TestCompiler(0ul, sbyte.MaxValue)]
        public static unsafe IntPtr PointerMathSubPNTypesSByte(UInt64 p,sbyte a)
        {
            var pointer = (sbyte*)p;
            return new IntPtr(pointer - a);   // Pointer LHS (no RHS since not legal in C#)
        }

        [TestCompiler(0ul, short.MinValue)]
        [TestCompiler(0ul, short.MaxValue)]
        public static unsafe IntPtr PointerMathAddPNTypesShort(UInt64 p,short a)
        {
            var pointer = (short*)p;
            return new IntPtr(pointer + a);   // Pointer LHS
        }

        [TestCompiler(0ul, short.MinValue)]
        [TestCompiler(0ul, short.MaxValue)]
        public static unsafe IntPtr PointerMathAddNPTypesShort(UInt64 p,short a)
        {
            var pointer = (short*)p;
            return new IntPtr(a + pointer);   // Pointer RHS
        }

        [TestCompiler(0ul, short.MinValue)]
        [TestCompiler(0ul, short.MaxValue)]
        public static unsafe IntPtr PointerMathSubPNTypesShort(UInt64 p,short a)
        {
            var pointer = (short*)p;
            return new IntPtr(pointer - a);   // Pointer LHS (no RHS since not legal in C#)
        }

        [TestCompiler(0ul, ushort.MinValue)]
        [TestCompiler(0ul, ushort.MaxValue)]
        public static unsafe IntPtr PointerMathAddPNTypesUShort(UInt64 p,ushort a)
        {
            var pointer = (ushort*)p;
            return new IntPtr(pointer + a);   // Pointer LHS
        }

        [TestCompiler(0ul, ushort.MinValue)]
        [TestCompiler(0ul, ushort.MaxValue)]
        public static unsafe IntPtr PointerMathAddNPTypesUShort(UInt64 p,ushort a)
        {
            var pointer = (ushort*)p;
            return new IntPtr(a + pointer);   // Pointer RHS
        }

        [TestCompiler(0ul, ushort.MinValue)]
        [TestCompiler(0ul, ushort.MaxValue)]
        public static unsafe IntPtr PointerMathSubPNTypesUShort(UInt64 p,ushort a)
        {
            var pointer = (ushort*)p;
            return new IntPtr(pointer - a);   // Pointer LHS (no RHS since not legal in C#)
        }

        [TestCompiler(0ul, int.MinValue)]
        [TestCompiler(0ul, int.MaxValue)]
        public static unsafe IntPtr PointerMathAddPNTypesInt(UInt64 p,int a)
        {
            var pointer = (int*)p;
            return new IntPtr(pointer + a);   // Pointer LHS
        }

        [TestCompiler(0ul, int.MinValue)]
        [TestCompiler(0ul, int.MaxValue)]
        public static unsafe IntPtr PointerMathAddNPTypesInt(UInt64 p,int a)
        {
            var pointer = (int*)p;
            return new IntPtr(a + pointer);   // Pointer RHS
        }

        [TestCompiler(0ul, int.MinValue)]
        [TestCompiler(0ul, int.MaxValue)]
        public static unsafe IntPtr PointerMathSubPNTypesInt(UInt64 p,int a)
        {
            var pointer = (int*)p;
            return new IntPtr(pointer - a);   // Pointer LHS (no RHS since not legal in C#)
        }

        [TestCompiler(0ul, uint.MinValue)]
        [TestCompiler(0ul, uint.MaxValue)]
        public static unsafe IntPtr PointerMathAddPNTypesUInt(UInt64 p,uint a)
        {
            var pointer = (uint*)p;
            return new IntPtr(pointer + a);   // Pointer LHS
        }

        [TestCompiler(0ul, uint.MinValue)]
        [TestCompiler(0ul, uint.MaxValue)]
        public static unsafe IntPtr PointerMathAddNPTypesUInt(UInt64 p,uint a)
        {
            var pointer = (uint*)p;
            return new IntPtr(a + pointer);   // Pointer RHS
        }

        [TestCompiler(0ul, uint.MinValue)]
        [TestCompiler(0ul, uint.MaxValue)]
        public static unsafe IntPtr PointerMathSubPNTypesUInt(UInt64 p,uint a)
        {
            var pointer = (uint*)p;
            return new IntPtr(pointer - a);   // Pointer LHS (no RHS since not legal in C#)
        }

        [TestCompiler(0ul, long.MinValue)]
        [TestCompiler(0ul, long.MaxValue)]
        public static unsafe IntPtr PolongerMathAddPNTypesLong(UInt64 p,long a)
        {
            var polonger = (long*)p;
            return new IntPtr(polonger + a);   // Polonger LHS
        }

        [TestCompiler(0ul, long.MinValue)]
        [TestCompiler(0ul, long.MaxValue)]
        public static unsafe IntPtr PolongerMathAddNPTypesLong(UInt64 p,long a)
        {
            var polonger = (long*)p;
            return new IntPtr(a + polonger);   // Polonger RHS
        }

        [TestCompiler(0ul, long.MinValue)]
        [TestCompiler(0ul, long.MaxValue)]
        public static unsafe IntPtr PolongerMathSubPNTypesLong(UInt64 p,long a)
        {
            var polonger = (long*)p;
            return new IntPtr(polonger - a);   // Polonger LHS (no RHS since not legal in C#)
        }

        [TestCompiler(0ul, ulong.MinValue)]
        [TestCompiler(0ul, ulong.MaxValue)]
        public static unsafe IntPtr PolongerMathAddPNTypesULong(UInt64 p,ulong a)
        {
            var polonger = (ulong*)p;
            return new IntPtr(polonger + a);   // Polonger LHS
        }

        [TestCompiler(0ul, ulong.MinValue)]
        [TestCompiler(0ul, ulong.MaxValue)]
        public static unsafe IntPtr PolongerMathAddNPTypesULong(UInt64 p,ulong a)
        {
            var polonger = (ulong*)p;
            return new IntPtr(a + polonger);   // Polonger RHS
        }

        [TestCompiler(0ul, ulong.MinValue)]
        [TestCompiler(0ul, ulong.MaxValue)]
        public static unsafe IntPtr PolongerMathSubPNTypesULong(UInt64 p,ulong a)
        {
            var polonger = (ulong*)p;
            return new IntPtr(polonger - a);   // Polonger LHS (no RHS since not legal in C#)
        }
    }
}
