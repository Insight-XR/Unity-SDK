using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    class InteractableSnapVolumeTests
    {
        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllSceneObjects();
        }

        [Test]
        public void SnapVolumeUpdatesSnapColliderAssociationWhileRegistered()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var interactable = TestUtilities.CreateSimpleInteractable();
            var snapVolume = TestUtilities.CreateSnapVolume();
            snapVolume.interactable = interactable;

            Assert.That(snapVolume.snapCollider, Is.Not.Null);

            Assert.That(manager.TryGetInteractableForCollider(snapVolume.snapCollider, out var associatedInteractable), Is.True);
            Assert.That(associatedInteractable, Is.SameAs(interactable));

            Assert.That(manager.TryGetInteractableForCollider(snapVolume.snapCollider, out associatedInteractable, out var associatedSnapVolume), Is.True);
            Assert.That(associatedInteractable, Is.SameAs(interactable));
            Assert.That(associatedSnapVolume, Is.SameAs(snapVolume));

            var oldCollider = snapVolume.snapCollider;
            var newCollider = TestUtilities.CreateGOSphereCollider(snapVolume.gameObject, isTrigger: true);

            snapVolume.snapCollider = newCollider;

            Assert.That(oldCollider, Is.Not.SameAs(newCollider));
            Assert.That(snapVolume.snapCollider, Is.SameAs(newCollider));

            Assert.That(manager.TryGetInteractableForCollider(snapVolume.snapCollider, out associatedInteractable), Is.True);
            Assert.That(associatedInteractable, Is.SameAs(interactable));

            Assert.That(manager.TryGetInteractableForCollider(snapVolume.snapCollider, out associatedInteractable, out associatedSnapVolume), Is.True);
            Assert.That(associatedInteractable, Is.SameAs(interactable));
            Assert.That(associatedSnapVolume, Is.SameAs(snapVolume));

            Assert.That(manager.TryGetInteractableForCollider(oldCollider, out associatedInteractable), Is.False);
            Assert.That(associatedInteractable, Is.Null);

            Assert.That(manager.TryGetInteractableForCollider(oldCollider, out associatedInteractable, out associatedSnapVolume), Is.False);
            Assert.That(associatedInteractable, Is.Null);
            Assert.That(associatedSnapVolume, Is.Null);
        }

        [Test]
        public void SnapVolumeReRegistersOnEnable()
        {
            // Use a plain C# object to test that a non-Unity Object is retained
            var manager = TestUtilities.CreateInteractionManager();
            var interactable = TestUtilities.CreateMockClassInteractable();
            var snapVolume = TestUtilities.CreateSnapVolume();
            snapVolume.interactable = interactable;

            manager.RegisterInteractable(interactable);

            Assert.That(interactable.isRegistered, Is.True);
            Assert.That(snapVolume.snapCollider, Is.Not.Null);
            Assert.That(snapVolume.enabled, Is.True);

            Assert.That(manager.TryGetInteractableForCollider(snapVolume.snapCollider, out var associatedInteractable), Is.True);
            Assert.That(associatedInteractable, Is.SameAs(interactable));

            Assert.That(manager.TryGetInteractableForCollider(snapVolume.snapCollider, out associatedInteractable, out var associatedSnapVolume), Is.True);
            Assert.That(associatedInteractable, Is.SameAs(interactable));
            Assert.That(associatedSnapVolume, Is.SameAs(snapVolume));

            // Unregister the snap volume
            snapVolume.enabled = false;

            Assert.That(manager.TryGetInteractableForCollider(snapVolume.snapCollider, out associatedInteractable), Is.False);
            Assert.That(associatedInteractable, Is.Null);

            Assert.That(manager.TryGetInteractableForCollider(snapVolume.snapCollider, out associatedInteractable, out associatedSnapVolume), Is.False);
            Assert.That(associatedInteractable, Is.Null);
            Assert.That(associatedSnapVolume, Is.Null);

            // Re-register the snap volume
            snapVolume.enabled = true;

            Assert.That(manager.TryGetInteractableForCollider(snapVolume.snapCollider, out associatedInteractable), Is.True);
            Assert.That(associatedInteractable, Is.SameAs(interactable));

            Assert.That(manager.TryGetInteractableForCollider(snapVolume.snapCollider, out associatedInteractable, out associatedSnapVolume), Is.True);
            Assert.That(associatedInteractable, Is.SameAs(interactable));
            Assert.That(associatedSnapVolume, Is.SameAs(snapVolume));
        }

        [UnityTest]
        public IEnumerator SnapColliderSetDisabledWhenSelected()
        {
            TestUtilities.CreateInteractionManager();
            var interactable = TestUtilities.CreateSimpleInteractable();
            var snapVolume = TestUtilities.CreateSnapVolume();
            snapVolume.interactable = interactable;

            Assert.That(interactable.isSelected, Is.False);
            Assert.That(snapVolume.snapCollider, Is.Not.Null);
            Assert.That(snapVolume.snapCollider.enabled, Is.True);

            var interactor = TestUtilities.CreateMockInteractor();
            interactor.validTargets.Add(interactable);

            yield return null;

            Assert.That(interactable.isSelected, Is.True);
            Assert.That(snapVolume.snapCollider.enabled, Is.False);

            interactor.allowSelect = false;

            yield return null;

            Assert.That(interactable.isSelected, Is.False);
            Assert.That(snapVolume.snapCollider.enabled, Is.True);
        }

        [UnityTest]
        public IEnumerator SnapColliderKeptEnabledWhenAutoDisableDisabled()
        {
            TestUtilities.CreateInteractionManager();
            var interactable = TestUtilities.CreateSimpleInteractable();
            var snapVolume = TestUtilities.CreateSnapVolume();
            snapVolume.disableSnapColliderWhenSelected = false;
            snapVolume.interactable = interactable;

            Assert.That(interactable.isSelected, Is.False);
            Assert.That(snapVolume.snapCollider, Is.Not.Null);
            Assert.That(snapVolume.snapCollider.enabled, Is.True);

            var interactor = TestUtilities.CreateMockInteractor();
            interactor.validTargets.Add(interactable);

            yield return null;

            Assert.That(interactable.isSelected, Is.True);
            Assert.That(snapVolume.snapCollider.enabled, Is.True);

            interactor.allowSelect = false;

            yield return null;

            Assert.That(interactable.isSelected, Is.False);
            Assert.That(snapVolume.snapCollider.enabled, Is.True);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SnapColliderSetDisabledWhenDisabled(bool disableSnapColliderWhenSelected)
        {
            TestUtilities.CreateInteractionManager();
            var interactable = TestUtilities.CreateSimpleInteractable();
            var snapVolume = TestUtilities.CreateSnapVolume();
            snapVolume.disableSnapColliderWhenSelected = disableSnapColliderWhenSelected;
            snapVolume.interactable = interactable;

            Assert.That(snapVolume.snapCollider, Is.Not.Null);
            Assert.That(snapVolume.snapCollider.enabled, Is.True);

            snapVolume.enabled = false;

            Assert.That(snapVolume.snapCollider.enabled, Is.False);

            snapVolume.enabled = true;

            Assert.That(snapVolume.snapCollider.enabled, Is.True);
        }

        [UnityTest]
        public IEnumerator SnapColliderKeptDisabledWhenEnabledWhileSelected()
        {
            TestUtilities.CreateInteractionManager();
            var interactable = TestUtilities.CreateSimpleInteractable();
            var snapVolume = TestUtilities.CreateSnapVolume();
            snapVolume.interactable = interactable;

            Assert.That(snapVolume.snapCollider, Is.Not.Null);
            Assert.That(snapVolume.snapCollider.enabled, Is.True);

            snapVolume.enabled = false;

            Assert.That(snapVolume.snapCollider.enabled, Is.False);

            var interactor = TestUtilities.CreateMockInteractor();
            interactor.validTargets.Add(interactable);

            yield return null;

            Assert.That(interactable.isSelected, Is.True);
            Assert.That(snapVolume.snapCollider.enabled, Is.False);

            snapVolume.enabled = true;

            Assert.That(snapVolume.snapCollider.enabled, Is.False);
        }
    }
}
