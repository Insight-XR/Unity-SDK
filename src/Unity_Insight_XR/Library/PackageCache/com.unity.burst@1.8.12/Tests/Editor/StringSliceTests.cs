using System;
using NUnit.Framework;
using Unity.Burst.Editor;

public class StringSliceTests
{
    private const string _someText = "This is some text we are going to take StringSlice from.";
    private const string _target = "StringSlice";
    private readonly StringSlice _ssTarget = new StringSlice(_someText, _someText.IndexOf(_target, StringComparison.InvariantCulture), _target.Length);

    [Test]
    public void StringSliceStringRepresentationTest()
    {
        Assert.AreEqual(_target, _ssTarget.ToString());
        Assert.AreEqual('S', _ssTarget[0]);
        Assert.IsTrue(_ssTarget == new StringSlice(_target));
    }

    [Test]
    public void StartsWithTest()
    {
        Assert.IsFalse(_ssTarget.StartsWith("This"));
        Assert.IsTrue(_ssTarget.StartsWith("S"));
        Assert.IsTrue(_ssTarget.StartsWith(_target));
    }

    [Test]
    public void ContainsTest()
    {
        Assert.IsFalse(_ssTarget.Contains(' '));
        Assert.IsFalse(_ssTarget.Contains('T'));
        Assert.IsFalse(_ssTarget.Contains('s'));
        Assert.IsTrue(_ssTarget.Contains('S'));
        Assert.IsTrue(_ssTarget.Contains('g'));
        Assert.IsTrue(_ssTarget.Contains('e'));
    }
}
