using System.Collections;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    class TweenableVariableTests
    {
        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllSceneObjects();
        }

        [UnityTest]
        public IEnumerator FloatTweenBehaviorTest()
        {
            var floatTweenableVariable = new FloatTweenableVariable
            {
                Value = 0f,
                target = 1f,
            };

            Assert.That(floatTweenableVariable.Value, Is.EqualTo(0f));

            // Simulate many iterations of interpolation
            for (var i = 0; i < 30; ++i)
            {
                floatTweenableVariable.HandleTween(1 / 2f);
            }

            Assert.That(floatTweenableVariable.Value, Is.EqualTo(1f));

            yield return null;

            floatTweenableVariable.Dispose();
        }

        [UnityTest]
        public IEnumerator Vector3TweenBehaviorTest()
        {
            var vector3TweenableVariable = new Vector3TweenableVariable
            {
                Value = new float3(0f, 0f, 0f),
                target = new float3(1f, 1f, 1f),
            };

            Assert.That(vector3TweenableVariable.Value, Is.EqualTo(new float3(0f, 0f, 0f)));

            // Simulate many iterations of interpolation
            for (var i = 0; i < 30; ++i)
            {
                vector3TweenableVariable.HandleTween(1 / 2f);
            }

            Assert.That(vector3TweenableVariable.Value, Is.EqualTo(new float3(1f, 1f, 1f)));

            yield return null;

            vector3TweenableVariable.Dispose();
        }

        [UnityTest]
        public IEnumerator FloatSequenceTweenTest()
        {
            var floatTweenableVariable = new FloatTweenableVariable
            {
                Value = 0f,
            };

            Assert.That(floatTweenableVariable.Value, Is.EqualTo(0f));

            var completedActionCalled = false;
            yield return floatTweenableVariable.PlaySequence(0f, 1f, 0.1f, () => completedActionCalled = true);

            Assert.That(floatTweenableVariable.Value, Is.EqualTo(1f));
            Assert.That(completedActionCalled, Is.True);

            floatTweenableVariable.Dispose();
        }

        [UnityTest]
        public IEnumerator Vector3SequenceTweenTest()
        {
            var vector3TweenableVariable = new Vector3TweenableVariable
            {
                Value = new float3(0f, 0f, 0f),
                target = new float3(1f, 1f, 1f),
            };

            Assert.That(vector3TweenableVariable.Value, Is.EqualTo(new float3(0f, 0f, 0f)));

            var completedActionCalled = false;
            yield return vector3TweenableVariable.PlaySequence(0f, 1f, 0.1f, () => completedActionCalled = true);

            Assert.That(vector3TweenableVariable.Value, Is.EqualTo(new float3(1f, 1f, 1f)));
            Assert.That(completedActionCalled, Is.True);

            vector3TweenableVariable.Dispose();
        }
    }
}