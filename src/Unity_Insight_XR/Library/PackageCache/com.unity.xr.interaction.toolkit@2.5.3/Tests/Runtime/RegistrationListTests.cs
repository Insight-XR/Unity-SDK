using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    class RegistrationListTests
    {
        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllSceneObjects();
        }

        public static IEnumerable<BaseRegistrationList<string>> GetRegistrationList()
        {
            yield return new RegistrationList<string>();
            yield return new SmallRegistrationList<string>();
            yield return new ExposedRegistrationList<string>();
        }

        [Test, TestCaseSource(nameof(GetRegistrationList))]
        public void RegistrationListRegisterReturnsStatusChange(BaseRegistrationList<string> registrationList)
        {
            Assert.That(registrationList.Register("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.True);

            Assert.That(registrationList.Register("A"), Is.False);
            Assert.That(registrationList.IsRegistered("A"), Is.True);

            Assert.That(registrationList.Unregister("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.False);

            Assert.That(registrationList.Register("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.True);

            registrationList.Flush();

            Assert.That(registrationList.Unregister("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.False);

            Assert.That(registrationList.Register("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.True);

            registrationList.Flush();

            Assert.That(registrationList.Register("A"), Is.False);
            Assert.That(registrationList.IsRegistered("A"), Is.True);
        }

        [Test, TestCaseSource(nameof(GetRegistrationList))]
        public void RegistrationListUnregisterReturnsStatusChange(BaseRegistrationList<string> registrationList)
        {
            Assert.That(registrationList.Unregister("A"), Is.False);
            Assert.That(registrationList.IsRegistered("A"), Is.False);

            Assert.That(registrationList.Register("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.True);

            Assert.That(registrationList.Unregister("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.False);

            registrationList.Flush();

            Assert.That(registrationList.IsRegistered("A"), Is.False);
            Assert.That(registrationList.Unregister("A"), Is.False);

            Assert.That(registrationList.Register("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.True);

            registrationList.Flush();

            Assert.That(registrationList.IsRegistered("A"), Is.True);
            Assert.That(registrationList.Unregister("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.False);
            Assert.That(registrationList.Register("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.True);
            Assert.That(registrationList.Unregister("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.False);

            registrationList.Flush();

            Assert.That(registrationList.IsRegistered("A"), Is.False);
        }

        [Test, TestCaseSource(nameof(GetRegistrationList))]
        public void RegistrationListMoveItemImmediatelyReturnsStatusChange(BaseRegistrationList<string> registrationList)
        {
            Assert.That(registrationList.MoveItemImmediately("A", 0), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.True);

            Assert.That(registrationList.MoveItemImmediately("A", 0), Is.False);
            Assert.That(registrationList.IsRegistered("A"), Is.True);

            Assert.That(registrationList.Unregister("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.False);

            registrationList.Flush();

            Assert.That(registrationList.MoveItemImmediately("A", 0), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.True);

            Assert.That(registrationList.MoveItemImmediately("B", 0), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.True);
            Assert.That(registrationList.IsRegistered("B"), Is.True);

            Assert.That(registrationList.MoveItemImmediately("A", 0), Is.False);
            Assert.That(registrationList.IsRegistered("A"), Is.True);
            Assert.That(registrationList.IsRegistered("B"), Is.True);

            Assert.That(registrationList.MoveItemImmediately("B", 1), Is.False);
            Assert.That(registrationList.IsRegistered("A"), Is.True);
            Assert.That(registrationList.IsRegistered("B"), Is.True);
        }

        [Test, TestCaseSource(nameof(GetRegistrationList))]
        public void RegistrationListMoveItemImmediatelyAffectsSnapshotImmediately(BaseRegistrationList<string> registrationList)
        {
            Assert.That(registrationList.MoveItemImmediately("A", 0), Is.True);
            Assert.That(registrationList.registeredSnapshot, Is.EqualTo(new[] { "A" }));

            Assert.That(registrationList.MoveItemImmediately("A", 0), Is.False);
            Assert.That(registrationList.registeredSnapshot, Is.EqualTo(new[] { "A" }));

            Assert.That(registrationList.Unregister("A"), Is.True);
            registrationList.Flush();
            Assert.That(registrationList.registeredSnapshot, Is.Empty);

            Assert.That(registrationList.MoveItemImmediately("A", 0), Is.True);
            Assert.That(registrationList.registeredSnapshot, Is.EqualTo(new[] { "A" }));

            Assert.That(registrationList.MoveItemImmediately("B", 0), Is.True);
            Assert.That(registrationList.registeredSnapshot, Is.EqualTo(new[] { "B", "A" }));

            Assert.That(registrationList.MoveItemImmediately("A", 0), Is.False);
            Assert.That(registrationList.registeredSnapshot, Is.EqualTo(new[] { "A", "B" }));

            Assert.That(registrationList.MoveItemImmediately("B", 1), Is.False);
            Assert.That(registrationList.registeredSnapshot, Is.EqualTo(new[] { "A", "B" }));
        }

        [Test, TestCaseSource(nameof(GetRegistrationList))]
        public void RegistrationListSnapshotUnaffectedUntilFlush(BaseRegistrationList<string> registrationList)
        {
            Assert.That(registrationList.Register("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.True);
            Assert.That(registrationList.registeredSnapshot, Is.Empty);

            registrationList.Flush();

            Assert.That(registrationList.IsRegistered("A"), Is.True);
            Assert.That(registrationList.registeredSnapshot, Is.EqualTo(new[] { "A" }));

            Assert.That(registrationList.Unregister("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.False);
            Assert.That(registrationList.Register("B"), Is.True);
            Assert.That(registrationList.IsRegistered("B"), Is.True);
            Assert.That(registrationList.registeredSnapshot, Is.EqualTo(new[] { "A" }));

            registrationList.Flush();

            Assert.That(registrationList.IsRegistered("A"), Is.False);
            Assert.That(registrationList.IsRegistered("B"), Is.True);
            Assert.That(registrationList.registeredSnapshot, Is.EqualTo(new[] { "B" }));
        }

        [Test, TestCaseSource(nameof(GetRegistrationList))]
        public void RegistrationListGetRegisteredItemsIncludesAll(BaseRegistrationList<string> registrationList)
        {
            var registeredItems = new List<string>();
            registrationList.GetRegisteredItems(registeredItems);
            Assert.That(registeredItems, Is.Empty);

            // Should include pending adds
            Assert.That(registrationList.Register("A"), Is.True);
            registrationList.GetRegisteredItems(registeredItems);
            Assert.That(registeredItems, Is.EqualTo(new[] { "A" }));
            Assert.That(registrationList.GetRegisteredItemAt(0), Is.EqualTo("A"));

            registrationList.Flush();

            // Should still be equal after flush
            registrationList.GetRegisteredItems(registeredItems);
            Assert.That(registeredItems, Is.EqualTo(new[] { "A" }));
            Assert.That(registrationList.GetRegisteredItemAt(0), Is.EqualTo("A"));

            // Removing and adding the same item should have no net change
            Assert.That(registrationList.Unregister("A"), Is.True);
            Assert.That(registrationList.Register("A"), Is.True);
            registrationList.GetRegisteredItems(registeredItems);
            Assert.That(registeredItems, Is.EqualTo(new[] { "A" }));
            Assert.That(registrationList.GetRegisteredItemAt(0), Is.EqualTo("A"));

            // Should include all in the order they were registered
            Assert.That(registrationList.Register("B"), Is.True);
            registrationList.GetRegisteredItems(registeredItems);
            Assert.That(registeredItems, Is.EqualTo(new[] { "A", "B" }));
            Assert.That(registrationList.GetRegisteredItemAt(0), Is.EqualTo("A"));
            Assert.That(registrationList.GetRegisteredItemAt(1), Is.EqualTo("B"));

            // Should filter out pending removes from the snapshot
            Assert.That(registrationList.Unregister("A"), Is.True);
            registrationList.GetRegisteredItems(registeredItems);
            Assert.That(registeredItems, Is.EqualTo(new[] { "B" }));
            Assert.That(registrationList.GetRegisteredItemAt(0), Is.EqualTo("B"));
        }

        [Test, TestCaseSource(nameof(GetRegistrationList))]
        public void RegistrationListFastPathMatches(BaseRegistrationList<string> registrationList)
        {
            Assert.That(registrationList.Register("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.True);

            registrationList.Flush();

            Assert.That(registrationList.IsRegistered("A"), Is.True);
            Assert.That(registrationList.IsStillRegistered("A"), Is.True);

            Assert.That(registrationList.Unregister("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.False);
            Assert.That(registrationList.IsStillRegistered("A"), Is.False);
        }

        [Test, TestCaseSource(nameof(GetRegistrationList))]
        public void RegistrationListCanUnregisterAll(BaseRegistrationList<string> registrationList)
        {
            var registeredItems = new List<string>();
            registrationList.GetRegisteredItems(registeredItems);
            Assert.That(registeredItems, Is.Empty);

            // Register A and B and Flush
            Assert.That(registrationList.Register("A"), Is.True);
            Assert.That(registrationList.Register("B"), Is.True);
            registrationList.Flush();
            registrationList.GetRegisteredItems(registeredItems);
            Assert.That(registeredItems, Is.EqualTo(new[] { "A", "B" }));
            Assert.That(registrationList.registeredSnapshot, Is.EqualTo(new[] { "A", "B" }));

            // Unregister B
            Assert.That(registrationList.Unregister("B"), Is.True);
            registrationList.GetRegisteredItems(registeredItems);
            Assert.That(registeredItems, Is.EqualTo(new[] { "A" }));

            // Register C
            Assert.That(registrationList.Register("C"), Is.True);
            registrationList.GetRegisteredItems(registeredItems);
            Assert.That(registeredItems, Is.EqualTo(new[] { "A", "C" }));

            // Unregister all items
            registrationList.UnregisterAll();
            registrationList.GetRegisteredItems(registeredItems);
            Assert.That(registeredItems, Is.Empty);
            Assert.That(registrationList.registeredSnapshot, Is.EqualTo(new[] { "A", "B" }));
            Assert.That(registrationList.IsRegistered("A"), Is.False);
            Assert.That(registrationList.IsStillRegistered("A"), Is.False);
            Assert.That(registrationList.IsRegistered("B"), Is.False);
            Assert.That(registrationList.IsStillRegistered("B"), Is.False);
            Assert.That(registrationList.IsRegistered("C"), Is.False);
            // Not calling IsStillRegistered("C") since it was not part of the registered snapshot,
            // so it is not valid to call that method. The IsStillRegistered method is used while
            // iterating the registered snapshot, and it was already verified that "C" is not in it.

            // Flush and retest
            registrationList.Flush();
            registrationList.GetRegisteredItems(registeredItems);
            Assert.That(registeredItems, Is.Empty);
            Assert.That(registrationList.registeredSnapshot, Is.Empty);
            Assert.That(registrationList.IsRegistered("A"), Is.False);
            Assert.That(registrationList.IsRegistered("B"), Is.False);
            Assert.That(registrationList.IsRegistered("C"), Is.False);
        }

        [Test, TestCaseSource(nameof(GetRegistrationList))]
        public void RegistrationListRemoveBufferedRemove(BaseRegistrationList<string> registrationList)
        {
            // Register A and Flush
            Assert.That(registrationList.Register("A"), Is.True);
            registrationList.Flush();
            Assert.That(registrationList.IsRegistered("A"), Is.True);
            Assert.That(registrationList.IsRegistered("B"), Is.False);
            Assert.That(registrationList.registeredSnapshot, Is.EqualTo(new[] { "A" }));
            Assert.That(registrationList.flushedCount, Is.EqualTo(1));
            Assert.That(registrationList.GetRegisteredItemAt(0), Is.EqualTo("A"));
            var registeredItems = new List<string>();
            registrationList.GetRegisteredItems(registeredItems);
            Assert.That(registeredItems, Is.EqualTo(new[] { "A" }));

            // Register B (as a buffered change)
            Assert.That(registrationList.Register("B"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.True);
            Assert.That(registrationList.IsRegistered("B"), Is.True);
            Assert.That(registrationList.registeredSnapshot, Is.EqualTo(new[] { "A" }));
            Assert.That(registrationList.flushedCount, Is.EqualTo(2));
            Assert.That(registrationList.GetRegisteredItemAt(0), Is.EqualTo("A"));
            Assert.That(registrationList.GetRegisteredItemAt(1), Is.EqualTo("B"));
            registrationList.GetRegisteredItems(registeredItems);
            Assert.That(registeredItems, Is.EqualTo(new[] { "A", "B" }));

            // Unregister B (without first calling Flush) to remove buffered add
            Assert.That(registrationList.Unregister("B"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.True);
            Assert.That(registrationList.IsRegistered("B"), Is.False);
            Assert.That(registrationList.registeredSnapshot, Is.EqualTo(new[] { "A" }));
            Assert.That(registrationList.flushedCount, Is.EqualTo(1));
            Assert.That(registrationList.GetRegisteredItemAt(0), Is.EqualTo("A"));
            registrationList.GetRegisteredItems(registeredItems);
            Assert.That(registeredItems, Is.EqualTo(new[] { "A" }));

            // Unregister A
            Assert.That(registrationList.Unregister("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.False);
            Assert.That(registrationList.IsRegistered("B"), Is.False);
            Assert.That(registrationList.registeredSnapshot, Is.EqualTo(new[] { "A" }));
            Assert.That(registrationList.flushedCount, Is.EqualTo(0));
            registrationList.GetRegisteredItems(registeredItems);
            Assert.That(registeredItems, Is.Empty);
        }

        [Test]
        public void SmallRegistrationListCanDisableBufferChanges()
        {
            var registrationList = new SmallRegistrationList<string>();

            // Register A (as a buffered change)
            Assert.That(registrationList.Register("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.True);
            Assert.That(registrationList.registeredSnapshot.Contains("A"), Is.False);

            // Disable buffer changes (automatically calls Flush)
            registrationList.bufferChanges = false;
            Assert.That(registrationList.bufferChanges, Is.False);
            Assert.That(registrationList.registeredSnapshot.Contains("A"), Is.True);
            Assert.That(registrationList.IsRegistered("A"), Is.True);

            // Register B immediately
            Assert.That(registrationList.Register("B"), Is.True);
            Assert.That(registrationList.registeredSnapshot.Contains("B"), Is.True);
            Assert.That(registrationList.IsRegistered("B"), Is.True);

            // Unregister A immediately
            Assert.That(registrationList.Unregister("A"), Is.True);
            Assert.That(registrationList.registeredSnapshot.Contains("A"), Is.False);
            Assert.That(registrationList.IsRegistered("A"), Is.False);
        }

        [Test]
        public void ExposedRegistrationListCanRegisterReferences()
        {
            TestUtilities.CreateInteractionManager();
            var interactable1 = TestUtilities.CreateSimpleInteractable();
            var interactable2 = TestUtilities.CreateSimpleInteractable();
            var interactable3 = TestUtilities.CreateSimpleInteractable();
            var registrationList = new ExposedRegistrationList<IXRInteractable>();

            var references = new List<XRSimpleInteractable> { interactable1, interactable2, interactable3 };
            var registeredItems = new List<IXRInteractable>();

            registrationList.RegisterReferences(references);
            registrationList.GetAll(registeredItems);
            Assert.That(registeredItems, Is.EqualTo(references));
        }
    }
}
