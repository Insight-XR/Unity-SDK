using NUnit.Framework;
using Unity.Burst.Intrinsics;
using static Unity.Burst.Intrinsics.X86.Avx;
using static Unity.Burst.Intrinsics.X86.Sse;
using static Unity.Burst.Intrinsics.X86.Sse4_1;

// These tests exist because of a Mono "bug" in the managed fallback versions of some CPU intrinsics.
// Specifically, the version of Mono used in Unity widens all floats to 64-bit when doing any
// operation on them. This can result in the bit representation of float values not being maintained.
// It doesn't happen for all float values, but it happens for e.g. NaN values. It's more likely
// to happen when the source value was actually a uint that we are "viewing" as a float, which is
// what the tests below do.

namespace Burst.Compiler.IL.Tests
{
    internal unsafe class TestInstrinsicsManagedFallbacks
    {
        [Test]
        public static unsafe void Test_shuffle_ps_Managed()
        {
            uint uintValue = 0x7F80_ABCD;

            v128 x = new v128(uintValue);
            v128 y = new v128(uintValue);

            v128 result = shuffle_ps(x, y, SHUFFLE(1, 2, 1, 2));

            Assert.AreEqual(uintValue, result.UInt0);
            Assert.AreEqual(uintValue, result.UInt1);
            Assert.AreEqual(uintValue, result.UInt2);
            Assert.AreEqual(uintValue, result.UInt3);
        }

        [Test]
        public static unsafe void Test_blend_ps_Managed()
        {
            uint uintValue = 0x7F80_ABCD;

            v128 x = new v128(uintValue);
            v128 y = new v128(uintValue);

            v128 result = blend_ps(x, y, 0b1010);

            Assert.AreEqual(uintValue, result.UInt0);
            Assert.AreEqual(uintValue, result.UInt1);
            Assert.AreEqual(uintValue, result.UInt2);
            Assert.AreEqual(uintValue, result.UInt3);
        }

        [Test]
        public static unsafe void Test_blendv_ps_Managed()
        {
            uint uintValue = 0x7F80_ABCD;

            v128 x = new v128(uintValue);
            v128 y = new v128(uintValue);

            v128 mask = new v128(1, 0, 1, 0);

            v128 result = blendv_ps(x, y, mask);

            Assert.AreEqual(uintValue, result.UInt0);
            Assert.AreEqual(uintValue, result.UInt1);
            Assert.AreEqual(uintValue, result.UInt2);
            Assert.AreEqual(uintValue, result.UInt3);
        }

        // There's no way to make this test pass on Mono, at least that I can find.
        [Test, Ignore("A Mono bug causes this test to fail")]
        public static unsafe void Test_extractf_ps_Managed()
        {
            uint uintValue = 0x7F80_ABCD;

            v128 x = new v128(uintValue);

            float result = extractf_ps(x, 1);

            Assert.AreEqual(uintValue, *(uint*)&result);
        }

        [Test]
        public static unsafe void Test_mm256_broadcast_ss_Managed()
        {
            uint uintValue = 0x7F80_ABCD;

            v256 result = mm256_broadcast_ss(&uintValue);

            Assert.AreEqual(uintValue, result.UInt0);
            Assert.AreEqual(uintValue, result.UInt1);
            Assert.AreEqual(uintValue, result.UInt2);
            Assert.AreEqual(uintValue, result.UInt3);
            Assert.AreEqual(uintValue, result.UInt4);
            Assert.AreEqual(uintValue, result.UInt5);
            Assert.AreEqual(uintValue, result.UInt6);
            Assert.AreEqual(uintValue, result.UInt7);
        }

        [Test]
        public static unsafe void Test_broadcast_ss_Managed()
        {
            uint uintValue = 0x7F80_ABCD;

            v128 result = broadcast_ss(&uintValue);

            Assert.AreEqual(uintValue, result.UInt0);
            Assert.AreEqual(uintValue, result.UInt1);
            Assert.AreEqual(uintValue, result.UInt2);
            Assert.AreEqual(uintValue, result.UInt3);
        }
    }
}