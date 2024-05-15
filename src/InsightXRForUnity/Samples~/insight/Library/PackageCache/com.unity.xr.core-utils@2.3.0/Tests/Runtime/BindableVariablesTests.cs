using System;
using NUnit.Framework;
using Unity.XR.CoreUtils.Bindings;
using Unity.XR.CoreUtils.Bindings.Variables;

namespace Unity.XR.CoreUtils.Tests
{
    [TestFixture]
    class BindableVariablesTests
    {
        [Test]
        public void TestEventBinding()
        {
            var callbackVar = 0;

            var intVar = new BindableVariable<int>(callbackVar);
            var eventBinding = intVar.Subscribe(newValue => callbackVar = newValue);

            // Both callback value and bindable variable value are 0
            Assert.AreEqual(0, intVar.Value);
            Assert.AreEqual(0, callbackVar);

            // Test to ensure the callback works and the subscribed value is updated.
            intVar.Value = 5;
            Assert.AreEqual(5, intVar.Value);
            Assert.AreEqual(5, callbackVar);

            // Test Unbind
            eventBinding.Unbind();
            intVar.Value = 10;
            Assert.AreEqual(10, intVar.Value);
            Assert.AreEqual(5, callbackVar);

            // Test Rebind
            eventBinding.Bind();
            intVar.Value = 15;
            Assert.AreEqual(15, intVar.Value);
            Assert.AreEqual(15, callbackVar);

            // Test Clear
            eventBinding.ClearBinding();
            intVar.Value = 20;
            Assert.AreEqual(20, intVar.Value);
            Assert.AreEqual(15, callbackVar);

            // Test subscribe and update
            intVar.SubscribeAndUpdate(newValue => callbackVar = newValue);
            Assert.AreEqual(20, intVar.Value);
            Assert.AreEqual(20, callbackVar);
        }

        [Test]
        public void TestEventBindingCanBeSkipped()
        {
            var callbackVar = 0;

            var intVar = new BindableVariable<int>(callbackVar);
            intVar.Subscribe(newValue => callbackVar = newValue);

            // Both callback value and bindable variable value are synchronized
            Assert.That(intVar.Value, Is.EqualTo(0));
            Assert.That(callbackVar, Is.EqualTo(0));

            // Test to ensure the callback is skipped but the value is still updated.
            var wouldHaveBroadcast = intVar.SetValueWithoutNotify(5);
            Assert.That(intVar.Value, Is.EqualTo(5));
            Assert.That(callbackVar, Is.EqualTo(0));
            Assert.That(wouldHaveBroadcast, Is.True);

            intVar.BroadcastValue();
            Assert.That(intVar.Value, Is.EqualTo(5));
            Assert.That(callbackVar, Is.EqualTo(5));
        }

        [Test]
        public void TestBindingsGroup()
        {
            var callbackVarA = 0;
            var callbackVarB = 0;

            var intVarA = new BindableVariable<int>(callbackVarA);
            var intVarB = new BindableVariable<int>(callbackVarB);

            var bindingsGroup = new BindingsGroup();

            bindingsGroup.AddBinding(intVarA.Subscribe(newValue => callbackVarA = newValue));
            bindingsGroup.AddBinding(intVarB.Subscribe(newValue => callbackVarB = newValue));

            intVarA.Value = 17;
            intVarB.Value = 17;

            // Both callback value and bindable variable value are 17
            Assert.AreEqual(17, callbackVarA);
            Assert.AreEqual(17, callbackVarB);

            // Test unbind
            bindingsGroup.Unbind();
            intVarA.Value = 18;
            intVarB.Value = 18;

            // Both callback value and bindable variable value are still 17
            Assert.AreEqual(17, callbackVarA);
            Assert.AreEqual(17, callbackVarB);

            // Test re-bind
            bindingsGroup.Bind();
            intVarA.Value = 19;
            intVarB.Value = 19;

            // Both callback value and bindable variable value are now 19
            Assert.AreEqual(19, callbackVarA);
            Assert.AreEqual(19, callbackVarB);

            // Test clear
            bindingsGroup.Clear();
            intVarA.Value = 21;
            intVarB.Value = 21;

            // Both callback value and bindable variable value are still 19
            Assert.AreEqual(19, callbackVarA);
            Assert.AreEqual(19, callbackVarB);

            // Test that bind no longer works
            bindingsGroup.Bind();
            intVarA.Value = 22;
            intVarB.Value = 22;

            // Both callback value and bindable variable value are still 19
            Assert.AreEqual(19, callbackVarA);
            Assert.AreEqual(19, callbackVarB);
        }

        [Test]
        public void TestTaskBinding()
        {
            var callbackVar = 0;

            var intVar = new BindableVariable<int>(callbackVar);

            Func<int, bool> callback = newIntVal =>
            {
                callbackVar = newIntVal;
                return true;
            };

            intVar.Value = 5;
            var task = intVar.Task(callback);
            task.Wait();
            Assert.AreEqual(5, task.Result);
            Assert.AreEqual(5, callbackVar);
        }
    }
}
