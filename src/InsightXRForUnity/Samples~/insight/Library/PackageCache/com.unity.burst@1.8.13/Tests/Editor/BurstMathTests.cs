using NUnit.Framework;
using Unity.Burst.Editor;

public class BurstMathTests
{
    [Test]
    [TestCase(1f, 3f, 3f, true)]
    [TestCase(1f, 3f, 2f, true)]
    [TestCase(1f, 3f, 3.00001f, false)]
    public void WithinRangeTest(float start, float end, float num, bool res)
    {
        Assert.That(BurstMath.WithinRange(start, end, num), Is.EqualTo(res));
    }
}
