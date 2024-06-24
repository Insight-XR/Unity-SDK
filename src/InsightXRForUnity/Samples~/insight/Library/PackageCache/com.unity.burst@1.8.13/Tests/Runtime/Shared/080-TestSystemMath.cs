using System;
using Burst.Compiler.IL.Tests.Helpers;

namespace Burst.Compiler.IL.Tests
{
    /// <summary>
    /// Tests of the <see cref="System.Math"/> functions.
    /// </summary>
    internal partial class TestSystemMath
    {
        [TestCompiler(DataRange.Standard)]
        public static double TestCos(float value)
        {
            return Math.Cos(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static double TestSin(float value)
        {
            return Math.Sin(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static float TestTan(float value)
        {
            return (float) Math.Tan(value);
        }

        [TestCompiler(DataRange.Standard11)]
        public static double TestAcos(float value)
        {
            return Math.Acos(value);
        }

        [TestCompiler(DataRange.Standard11)]
        public static double TestAsin(float value)
        {
            return Math.Asin(value);
        }

        [TestCompiler(DataRange.Standard11)]
        public static float TestAtan(float value)
        {
            return (float)Math.Atan(value);
        }

        [TestCompiler(DataRange.ZeroExclusiveToOneInclusive, DataRange.ZeroExclusiveToOneInclusive)]
        public static float TestAtan2(float y, float x)
        {
            return (float)Math.Atan2(y, x);
        }

        [TestCompiler(DataRange.Standard)]
        public static double TestCosh(float value)
        {
            return Math.Cosh(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static double TestSinh(float value)
        {
            return Math.Sinh(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static float TestTanh(float value)
        {
            return (float)Math.Tanh(value);
        }

        [TestCompiler(DataRange.StandardPositive)]
        public static double TestSqrt(float value)
        {
            return Math.Sqrt(value);
        }

        [TestCompiler(DataRange.StandardPositive & ~DataRange.Zero)]
        public static double TestLog(float value)
        {
            return Math.Log(value);
        }

        [TestCompiler(DataRange.StandardPositive & ~DataRange.Zero)]
        public static double TestLog10(float value)
        {
            return Math.Log10(value);
        }

        [TestCompiler(DataRange.StandardPositive)]
        public static double TestExp(float value)
        {
            return Math.Exp(value);
        }

        [TestCompiler(DataRange.Standard & ~(DataRange.Zero|DataRange.NaN), DataRange.Standard)]
        [TestCompiler(DataRange.Standard & ~DataRange.Zero, DataRange.Standard & ~DataRange.Zero)]
        public static double TestPow(float value, float power)
        {
            return Math.Pow(value, power);
        }

        [TestCompiler(DataRange.Standard)]
        public static sbyte TestAbsSByte(sbyte value)
        {
            return Math.Abs(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static short TestAbsShort(short value)
        {
            return Math.Abs(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static int TestAbsInt(int value)
        {
            return Math.Abs(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static long TestAbsLong(long value)
        {
            return Math.Abs(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static float TestAbsFloat(float value)
        {
            return Math.Abs(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static double TestAbsDouble(double value)
        {
            return Math.Abs(value);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int TestMaxInt(int left, int right)
        {
            return Math.Max(left, right);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int TestMinInt(int left, int right)
        {
            return Math.Min(left, right);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static double TestMaxDouble(double left, double right)
        {
            return Math.Max(left, right);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static double TestMinDouble(double left, double right)
        {
            return Math.Min(left, right);
        }

        [TestCompiler(DataRange.Standard)]
        public static int TestSignInt(int value)
        {
            return Math.Sign(value);
        }

        [TestCompiler(DataRange.Standard & ~DataRange.NaN)]
        public static int TestSignFloat(float value)
        {
            return Math.Sign(value);
        }

        [TestCompiler(float.NaN, ExpectedException = typeof(ArithmeticException))]
        [MonoOnly(".NET CLR does not support burst.abort correctly")]
        public static int TestSignException(float value)
        {
            return Math.Sign(value);
        }

        [TestCompiler(DataRange.Standard & ~DataRange.NaN)]
        public static int TestSignDouble(double value)
        {
            return Math.Sign(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static double TestCeilingDouble(double value)
        {
            return Math.Ceiling(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static double TestFloorDouble(double value)
        {
            return Math.Floor(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static double TestRoundDouble(double value)
        {
            return Math.Round(value);
        }

        [TestCompiler(DataRange.Standard, DataRange.ZeroInclusiveToFifteenInclusive, SkipForILInterpreter = true)] // https://jira.unity3d.com/browse/BUR-2376
        public static double TestRoundDoubleDigits(double value, int digits)
        {
            return Math.Round(value, digits);
        }

        [TestCompiler(DataRange.Standard, MidpointRounding.ToEven, SkipForILInterpreter = true)]        // https://jira.unity3d.com/browse/BUR-2376
        [TestCompiler(DataRange.Standard, MidpointRounding.AwayFromZero, SkipForILInterpreter = true)]  // https://jira.unity3d.com/browse/BUR-2376
        public static double TestRoundDoubleMidpoint(double value, MidpointRounding mode)
        {
            return Math.Round(value, mode);
        }

        [TestCompiler(DataRange.Standard, DataRange.ZeroInclusiveToFifteenInclusive, MidpointRounding.ToEven, SkipForILInterpreter = true)] // https://jira.unity3d.com/browse/BUR-2376
        [TestCompiler(DataRange.Standard, DataRange.ZeroInclusiveToFifteenInclusive, MidpointRounding.AwayFromZero, SkipForILInterpreter = true)] // https://jira.unity3d.com/browse/BUR-2376
        public static double TestRoundDoubleDigitsMidpoint(double value, int digits, MidpointRounding mode)
        {
            return Math.Round(value, digits, mode);
        }

        [TestCompiler(DataRange.Standard)]
        public static double TestTruncateDouble(double value)
        {
            return Math.Truncate(value);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard & ~DataRange.Zero)]
        public static int TestDivRemInt(int a, int b)
        {
            int remResult;
            var divResult = Math.DivRem(a, b, out remResult);
            return divResult + remResult * 7;
        }

        [TestCompiler(DataRange.Standard, DataRange.StandardPositiveExclusiveZero)]
        public static long TestDivRemLong(long a, long b)
        {
            var divResult = Math.DivRem(a, b, out var remResult);
            return divResult + remResult * 7;
        }

#if NET6_0_OR_GREATER
        [TestCompiler(0xdead, 0xbeef)]
        [TestCompiler(0x0, 0xbeef)]
        public static nint TestDivRemIntPtr(nint a, nint b)
        {
            var quotrem = Math.DivRem(a, b);
            return quotrem.Quotient + quotrem.Remainder * 7;
        }

        [TestCompiler(0xdeadu, 0xbeefu)]
        [TestCompiler(0x0u, 0xbeefu)]
        public static nuint TestDivRemUIntPtr(nuint a, nuint b)
        {
            var quotRem = Math.DivRem(a, b);
            return quotRem.Quotient + quotRem.Remainder * 7;
        }

        [TestCompiler(DataRange.StandardPositive, DataRange.StandardPositiveExclusiveZero)]
        public static ulong TestDivRemUlong(ulong a, ulong b)
        {
            var quotRem = Math.DivRem(a, b);
            return quotRem.Quotient + quotRem.Remainder * 7;
        }

        [TestCompiler(DataRange.StandardPositive, DataRange.StandardPositiveExclusiveZero)]
        public static uint TestDivRemUint(uint a, uint b)
        {
            var quotRem = Math.DivRem(a, b);
            return quotRem.Quotient + quotRem.Remainder * 7;
        }

        [TestCompiler(DataRange.Standard11, DataRange.StandardPositiveExclusiveZero)]
        public static sbyte TestDivRemSByte(sbyte a, sbyte b)
        {
            var quotRem = Math.DivRem(a, b);
            return (sbyte)(quotRem.Quotient + quotRem.Remainder * 7);
        }

        [TestCompiler(DataRange.StandardPositive, DataRange.StandardPositiveExclusiveZero)]
        public static byte TestDivRemByte(byte a, byte b)
        {
            var quotRem = Math.DivRem(a, b);
            return (byte)(quotRem.Quotient + quotRem.Remainder * 7);
        }

        [TestCompiler(DataRange.Standard & ~DataRange.HighIntRange, DataRange.StandardPositiveExclusiveZero)]
        public static short TestDivRemInt16(short a, short b)
        {
            var quotRem = Math.DivRem(a, b);
            return (short)(quotRem.Quotient + quotRem.Remainder * 7);
        }

        [TestCompiler(DataRange.StandardPositive, DataRange.StandardPositiveExclusiveZero)]
        public static ushort TestDivRemUint16(ushort a, ushort b)
        {
            var quotRem = Math.DivRem(a, b);
            return (ushort)(quotRem.Quotient + quotRem.Remainder * 7);
        }

        // TODO: Commented out due to missing intrinsic implementation.
        // TODO: Can be reenabled when the intrinsic PR lands.
        //[TestCompiler(DataRange.Standard11)]
        public static double TestBitDecrement(double a)
        {
            return Math.BitDecrement(a);
        }

        // TODO: Commented out due to missing intrinsic implementation.
        // TODO: Can be reenabled when the intrinsic PR lands.
        //[TestCompiler(DataRange.Standard11)]
        public static double TestBitIncrement(double a)
        {
            return Math.BitIncrement(a);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static double TestCopySign(double a, double b)
        {
            return Math.CopySign(a, b);
        }

        [TestCompiler(DataRange.Standard)]
        public static int TestILogB(double a)
        {
            return Math.ILogB(a);
        }

        [TestCompiler(DataRange.Standard)]
        public static double TestLog2(double a)
        {
            return Math.Log2(a);
        }

        // TODO: Commented out due to missing intrinsic implementation.
        // TODO: Can be reenabled when the intrinsic PR lands.
        //[TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static double TestMaxMagnitude(double a, double b)
        {
            return Math.MaxMagnitude(a, b);
        }

        // TODO: Commented out due to missing intrinsic implementation.
        // TODO: Can be reenabled when the intrinsic PR lands.
        //[TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static double TestMinMagnitude(double a, double b)
        {
            return Math.MinMagnitude(a, b);
        }

        // TODO: Commented out due to missing intrinsic implementation.
        // TODO: Can be reenabled when the intrinsic PR lands.
        //[TestCompiler(DataRange.Standard)]
        public static double TestReciprocalEstimate(double a)
        {
            return Math.ReciprocalEstimate(a);
        }

        // TODO: Commented out due to missing intrinsic implementation.
        // TODO: Can be reenabled when the intrinsic PR lands.
        //[TestCompiler(DataRange.Standard)]
        public static double TestReciprocalSqrtEstimate(double a)
        {
            return Math.ReciprocalSqrtEstimate(a);
        }

        // TODO: Commented out due to missing intrinsic implementation.
        // TODO: Can be reenabled when the intrinsic PR lands.
        //[TestCompiler(DataRange.Standard, DataRange.Standard)]
        //[TestCompiler(int.MaxValue, DataRange.Standard)]
        public static ulong TestBigMulUlong(ulong a, ulong b)
        {
            var high = Math.BigMul(a, b, out ulong low);
            return high + low;
        }

        // TODO: Commented out due to missing intrinsic implementation.
        // TODO: Can be reenabled when the intrinsic PR lands.
        //[TestCompiler(DataRange.Standard, DataRange.Standard)]
        //[TestCompiler(int.MaxValue, DataRange.Standard)]
        public static long TestBigMulLong(long a, long b)
        {
            var high = Math.BigMul(a, b, out long low);
            return high + low;
        }
#endif

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        [TestCompiler(int.MaxValue, DataRange.Standard)]
        public static long TestBigMulInt(int a, int b)
        {
            return Math.BigMul(a, b);
        }

        [TestCompiler(DataRange.Standard & ~DataRange.Zero, DataRange.Standard & ~DataRange.Zero)]
        public static double TestLogWithBaseDouble(double a, double newBase)
        {
            return Math.Log(a, newBase);
        }

#if NET5_0_OR_GREATER || NETSTANDARD2_1
        [TestCompiler(DataRange.Standard, (byte)1, (byte)50)]
        public static byte TestClampByte(byte a, byte min, byte max)
        {
            return Math.Clamp(a, min, max);
        }

        [TestCompiler(DataRange.Standard, 1.0, 50.0)]
        public static double TestClampDouble(double a, double min, double max)
        {
            return Math.Clamp(a, min, max);
        }

        [TestCompiler(DataRange.Standard, (short)1, (short)50)]
        public static short TestClampShort(short a, short min, short max)
        {
            return Math.Clamp(a, min, max);
        }

        [TestCompiler(DataRange.Standard, 1, 50)]
        public static int TestClampInt(int a, int min, int max)
        {
            return Math.Clamp(a, min, max);
        }

        [TestCompiler(DataRange.Standard, 1L, 50L)]
        public static long TestClampLong(long a, long min, long max)
        {
            return Math.Clamp(a, min, max);
        }

        [TestCompiler(DataRange.Standard, (sbyte)1, (sbyte)50)]
        public static sbyte TestClampSByte(sbyte a, sbyte min, sbyte max)
        {
            return Math.Clamp(a, min, max);
        }

        [TestCompiler(DataRange.Standard, 1.0f, 50.0f)]
        public static float TestClampFloat(float a, float min, float max)
        {
            return Math.Clamp(a, min, max);
        }

        [TestCompiler(DataRange.Standard, (ushort)1, (ushort)50)]
        public static ushort TestClampUShort(ushort a, ushort min, ushort max)
        {
            return Math.Clamp(a, min, max);
        }

        [TestCompiler(DataRange.Standard, (uint)1, (uint)50)]
        public static uint TestClampUInt(uint a, uint min, uint max)
        {
            return Math.Clamp(a, min, max);
        }

        [TestCompiler(DataRange.Standard, (ulong)1, (ulong)50)]
        public static ulong TestClampULong(ulong a, ulong min, ulong max)
        {
            return Math.Clamp(a, min, max);
        }

        // TODO: Commented out due to missing intrinsic implementation.
        // TODO: Can be reenabled when the intrinsic PR lands.
        //[TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static double TestIEEERemainder(double a, double newBase)
        {
            return Math.IEEERemainder(a, newBase);
        }

        [TestCompiler(DataRange.Standard11)]
        public static double TestAcosh(double a)
        {
            return Math.Acosh(a);
        }

        [TestCompiler(DataRange.Standard11)]
        public static double TestAsinh(double a)
        {
            return Math.Asinh(a);
        }

        [TestCompiler(DataRange.Standard11)]
        public static double TestAtanh(double a)
        {
            return Math.Atanh(a);
        }

        [TestCompiler(DataRange.Standard11)]
        public static double TestCbrt(double a)
        {
            return Math.Cbrt(a);
        }
#endif

        [TestCompiler(DataRange.Standard)]
        public static bool TestIsNanDouble(double a)
        {
            return double.IsNaN(a);
        }

        [TestCompiler(DataRange.Standard)]
        public static bool TestIsNanFloat(float a)
        {
            return float.IsNaN(a);
        }

        [TestCompiler(DataRange.Standard, IgnoreOnNetCore = true)]  // Disabled due to BitConverter doubletoint which pulls in System.Runtime.Intrinsics, which pulls in a typeof
        public static bool TestIsInfinityDouble(double a)
        {
            return double.IsInfinity(a);
        }

        [TestCompiler(DataRange.Standard, IgnoreOnNetCore = true)]  // Disabled due to BitConverter doubletoint which pulls in System.Runtime.Intrinsics, which pulls in a typeof
        public static bool TestIsInfinityFloat(float a)
        {
            return float.IsInfinity(a);
        }
    }
}
