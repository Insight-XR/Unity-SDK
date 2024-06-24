
using UnityBenchShared;

namespace Burst.Compiler.IL.Tests
{
    internal class TestFixed
    {
        public unsafe struct SomeStruct
        {
            public static readonly int[] Ints = new int[4] { 1, 2, 3, 4 };

            public struct OtherStruct
            {
                public int x;
            }

            public static readonly OtherStruct[] Structs = new OtherStruct[2] { new OtherStruct { x = 42 }, new OtherStruct { x = 13 } };

            public fixed ushort array[42];

            public struct Provider : IArgumentProvider
            {
                public object Value
                {
                    get
                    {
                        var s = new SomeStruct();

                        for (ushort i = 0; i < 42; i++)
                        {
                            s.array[i] = i;
                        }

                        return s;
                    }
                }
            }
        }

        [TestCompiler]
        public static unsafe int ReadInts()
        {
            fixed (int* ptr = SomeStruct.Ints)
            {
                return ptr[2];
            }
        }

        [TestCompiler]
        public static unsafe int ReadIntsElement()
        {
            fixed (int* ptr = &SomeStruct.Ints[1])
            {
                return ptr[0];
            }
        }

        [TestCompiler]
        public static unsafe int ReadStructs()
        {
            fixed (SomeStruct.OtherStruct* ptr = SomeStruct.Structs)
            {
                return ptr[1].x;
            }
        }

        [TestCompiler]
        public static unsafe int ReadStructsElement()
        {
            fixed (SomeStruct.OtherStruct* ptr = &SomeStruct.Structs[1])
            {
                return ptr[0].x;
            }
        }

        [TestCompiler(typeof(SomeStruct.Provider))]
        public static unsafe ushort ReadFromFixedArray(ref SomeStruct s)
        {
            fixed (ushort* ptr = s.array)
            {
                ushort total = 0;

                for (ushort i = 0; i < 42; i++)
                {
                    total += ptr[i];
                }

                return total;
            }
        }

        // The below tests are designed to verify the indexer is treated correctly for various fixed arrays (only the smallest case)
        //(the bug was actually to do with pointer addition, so see 031-Pointer.cs for additional coverage)
        //Its not perfect as if the indexer is treated as signed, then in burst we will read off the beginning of the array
        //which might be into another array or off the beginning of the struct... and the value might accidently be correct.
        public unsafe struct IndexerStructTestSByte
        {
            public fixed sbyte sbyteArray[256];

            public struct Provider : IArgumentProvider
            {
                public object Value
                {
                    get
                    {
                        var s = new IndexerStructTestSByte();

                        for (int a=0;a<256;a++)
                        {
                            s.sbyteArray[a] = sbyte.MinValue;
                        }

                        s.sbyteArray[127] = 127;
                        s.sbyteArray[128] = 63;
                        s.sbyteArray[255] = 23;

                        return s;
                    }
                }
            }
        }
        public unsafe struct IndexerStructTestByte
        {
            public fixed byte byteArray[256];

            public struct Provider : IArgumentProvider
            {
                public object Value
                {
                    get
                    {
                        var s = new IndexerStructTestByte();

                        for (int a=0;a<256;a++)
                        {
                            s.byteArray[a] = byte.MinValue;
                        }

                        s.byteArray[127] = 129;
                        s.byteArray[128] = 212;
                        s.byteArray[255] = 165;

                        return s;
                    }
                }
            }
        }
        // SByte array with different indexer types
        [TestCompiler(typeof(IndexerStructTestSByte.Provider),(byte)0)]
        [TestCompiler(typeof(IndexerStructTestSByte.Provider),(byte)128)]
        [TestCompiler(typeof(IndexerStructTestSByte.Provider),(byte)255)]
        public static unsafe sbyte IndexerReadFromSByteArrayWithByteOffset(ref IndexerStructTestSByte s, byte offset)
        {
            return s.sbyteArray[offset];
        }

        [TestCompiler(typeof(IndexerStructTestSByte.Provider),(sbyte)0)]
        [TestCompiler(typeof(IndexerStructTestSByte.Provider),(sbyte)127)]  // signed offset so limited
        public static unsafe sbyte IndexerReadFromSByteArrayWithSByteOffset(ref IndexerStructTestSByte s, sbyte offset)
        {
            return s.sbyteArray[offset];
        }

        // Byte array with different indexer types
        [TestCompiler(typeof(IndexerStructTestByte.Provider),(byte)0)]
        [TestCompiler(typeof(IndexerStructTestByte.Provider),(byte)128)]
        [TestCompiler(typeof(IndexerStructTestByte.Provider),(byte)255)]
        public static unsafe byte IndexerReadFromByteArrayWithByteOffset(ref IndexerStructTestByte s, byte offset)
        {
            return s.byteArray[offset];
        }

        [TestCompiler(typeof(IndexerStructTestByte.Provider),(sbyte)0)]
        [TestCompiler(typeof(IndexerStructTestByte.Provider),(sbyte)127)]  // signed offset so limited
        public static unsafe byte IndexerReadFromByteArrayWithSByteOffset(ref IndexerStructTestByte s, sbyte offset)
        {
            return s.byteArray[offset];
        }
    }



}
