using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    class InteractorTests
    {
        static readonly Type[] s_ContactInteractors =
        {
            typeof(XRDirectInteractor),
            typeof(XRSocketInteractor),
        };

        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllSceneObjects();
        }

        [UnityTest]
        public IEnumerator ContactInteractorTargetStaysValidWhenTouchingAnyCollider([ValueSource(nameof(s_ContactInteractors))] Type interactorType)
        {
            // This tests that an Interactable will stay as a valid target as long as
            // the Direct and Socket Interactor is touching any Collider associated with the Interactable,
            // and remains so if only some (but not all) of the Interactable colliders leaves.
            var manager = TestUtilities.CreateInteractionManager();
            XRBaseInteractor interactor = null;
            if (interactorType == typeof(XRDirectInteractor))
                interactor = TestUtilities.CreateDirectInteractor();
            else if (interactorType == typeof(XRSocketInteractor))
                interactor = TestUtilities.CreateSocketInteractor();

            Assert.That(interactor, Is.Not.Null);

            var triggerCollider = interactor.GetComponent<SphereCollider>();
            Assert.That(triggerCollider, Is.Not.Null);
            Assert.That(triggerCollider.isTrigger, Is.True);

            var interactable = TestUtilities.CreateGrabInteractable();
            // Prevent the Interactable from being selected to allow the object to be moved freely
            interactable.interactionLayers = 0;
            var sphereCollider = interactable.GetComponent<SphereCollider>();
            sphereCollider.center = Vector3.zero;
            sphereCollider.radius = 0.5f;
            Assert.That(sphereCollider, Is.Not.Null);
            interactable.transform.position = Vector3.forward * 10f;

            // Create another Collider to have as part of the Interactable
            var boxColliderTransform = new GameObject("Box Collider", typeof(BoxCollider)).transform;
            boxColliderTransform.SetParent(interactable.transform);
            boxColliderTransform.localPosition = Vector3.right;
            boxColliderTransform.localRotation = Quaternion.identity;
            var boxCollider = boxColliderTransform.GetComponent<BoxCollider>();
            boxCollider.center = Vector3.zero;
            boxCollider.size = Vector3.one;

            interactable.colliders.Clear();
            interactable.colliders.Add(sphereCollider);
            interactable.colliders.Add(boxCollider);

            interactable.enabled = false;
            interactable.enabled = true;

            Assert.That(manager.TryGetInteractableForCollider(sphereCollider, out var sphereColliderInteractable), Is.True);
            Assert.That(sphereColliderInteractable, Is.EqualTo(interactable));
            Assert.That(manager.TryGetInteractableForCollider(boxCollider, out var boxColliderInteractable), Is.True);
            Assert.That(boxColliderInteractable, Is.EqualTo(interactable));

            yield return new WaitForFixedUpdate();
            yield return null;

            var directOverlaps = Physics.OverlapSphere(triggerCollider.transform.position, triggerCollider.radius, -1, QueryTriggerInteraction.Ignore);
            Assert.That(directOverlaps, Is.Empty);

            var validTargets = new List<IXRInteractable>();
            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.Empty);

            // Move the Interactable to the Direct Interactor so that it overlaps both colliders
            interactable.transform.position = Vector3.left * 0.5f;

            yield return new WaitForFixedUpdate();
            yield return null;

            directOverlaps = Physics.OverlapSphere(triggerCollider.transform.position, triggerCollider.radius, -1, QueryTriggerInteraction.Ignore);
            Assert.That(directOverlaps, Is.EquivalentTo(new Collider[] { sphereCollider, boxCollider }));

            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.EqualTo(new[] { interactable }));

            // Move the Interactable some so one of the colliders leaves
            interactable.transform.position = Vector3.left * 2f;

            yield return new WaitForFixedUpdate();
            yield return null;

            directOverlaps = Physics.OverlapSphere(triggerCollider.transform.position, triggerCollider.radius, -1, QueryTriggerInteraction.Ignore);
            Assert.That(directOverlaps, Is.EquivalentTo(new Collider[] { boxCollider }));

            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.EqualTo(new[] { interactable }));

            // Move the Interactable some so the other collider is the one being hovered
            // to test that colliders can re-enter after previously exiting
            interactable.transform.position = Vector3.right * 1f;

            yield return new WaitForFixedUpdate();
            yield return null;

            directOverlaps = Physics.OverlapSphere(triggerCollider.transform.position, triggerCollider.radius, -1, QueryTriggerInteraction.Ignore);
            Assert.That(directOverlaps, Is.EquivalentTo(new Collider[] { sphereCollider }));

            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.EqualTo(new[] { interactable }));

            // Move the Interactable so all colliders exits the Direct Interactor
            interactable.transform.position = Vector3.forward * 10f;

            yield return new WaitForFixedUpdate();
            yield return null;

            directOverlaps = Physics.OverlapSphere(triggerCollider.transform.position, triggerCollider.radius, -1, QueryTriggerInteraction.Ignore);
            Assert.That(directOverlaps, Is.Empty);

            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.Empty);
        }

        [UnityTest]
        public IEnumerator ContactInteractorCullsValidTargetsWhenInteractableUnregistered([ValueSource(nameof(s_ContactInteractors))] Type interactorType)
        {
            // This will test that the Direct and Socket Interactor will remove an unregistered Interactable
            // from its valid targets list.
            TestUtilities.CreateInteractionManager();
            XRBaseInteractor interactor = null;
            if (interactorType == typeof(XRDirectInteractor))
                interactor = TestUtilities.CreateDirectInteractor();
            else if (interactorType == typeof(XRSocketInteractor))
                interactor = TestUtilities.CreateSocketInteractor();

            Assert.That(interactor, Is.Not.Null);

            var interactable = TestUtilities.CreateGrabInteractable();

            yield return new WaitForFixedUpdate();

            var validTargets = new List<IXRInteractable>();
            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.EqualTo(new[] { interactable }));

            interactable.enabled = false;

            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.Empty);
        }

        [UnityTest]
        public IEnumerator ContactInteractorCullsValidTargetsUponRegistering([ValueSource(nameof(s_ContactInteractors))] Type interactorType)
        {
            // This will test that the Direct and Socket Interactor will update the list of valid targets
            // to exclude those that have been unregistered during the time when the Interactor
            // was not subscribed to the unregister event.
            TestUtilities.CreateInteractionManager();
            XRBaseInteractor interactor = null;
            if (interactorType == typeof(XRDirectInteractor))
                interactor = TestUtilities.CreateDirectInteractor();
            else if (interactorType == typeof(XRSocketInteractor))
                interactor = TestUtilities.CreateSocketInteractor();

            Assert.That(interactor, Is.Not.Null);

            var interactable = TestUtilities.CreateGrabInteractable();

            // Wait both for fixed update and a frame to ensure the Interactor has had a chance to update
            // Direct interactor may update on update or on fixed update
            yield return new WaitForFixedUpdate();
            yield return null;

            var validTargets = new List<IXRInteractable>();
            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.EqualTo(new[] { interactable }));

            interactor.enabled = false;
            interactable.enabled = false;
            interactor.enabled = true;

            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.Empty);
        }

        [UnityTest]
        public IEnumerator ContactInteractorUpdatesValidTargetsForPreviouslyUnassociatedCollidersWhenInteractableRegistered([ValueSource(nameof(s_ContactInteractors))] Type interactorType)
        {
            // This will test that the Direct and Socket Interactor will maintain the list of all entered Colliders
            // so that if any of them later become associated with a registered Interactable,
            // that Interactable will become a valid target.
            TestUtilities.CreateInteractionManager();
            var interactable = TestUtilities.CreateGrabInteractable();
            interactable.transform.position = Vector3.forward * 10f;
            interactable.enabled = false;

            yield return new WaitForFixedUpdate();

            XRBaseInteractor interactor = null;
            if (interactorType == typeof(XRDirectInteractor))
                interactor = TestUtilities.CreateDirectInteractor();
            else if (interactorType == typeof(XRSocketInteractor))
                interactor = TestUtilities.CreateSocketInteractor();

            Assert.That(interactor, Is.Not.Null);

            interactable.transform.position = Vector3.zero;

            // Wait both for fixed update and a frame to ensure the Interactor has had a chance to update
            // Direct interactor may update on update or on fixed update
            yield return new WaitForFixedUpdate();
            yield return null;

            var validTargets = new List<IXRInteractable>();
            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.Empty);

            interactable.enabled = true;

            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.EqualTo(new[] { interactable }));
        }

        [UnityTest]
        public IEnumerator ContactInteractorUpdatesValidTargetsForPreviouslyUnassociatedCollidersUponRegistering([ValueSource(nameof(s_ContactInteractors))] Type interactorType)
        {
            // This will test that the Direct and Socket Interactor will later associate the collider when
            // the Interactable is registered during the time when the Interactor
            // was not subscribed to the register event.
            TestUtilities.CreateInteractionManager();
            var interactable = TestUtilities.CreateGrabInteractable();
            interactable.transform.position = Vector3.forward * 10f;
            interactable.enabled = false;

            yield return new WaitForFixedUpdate();

            XRBaseInteractor interactor = null;
            if (interactorType == typeof(XRDirectInteractor))
                interactor = TestUtilities.CreateDirectInteractor();
            else if (interactorType == typeof(XRSocketInteractor))
                interactor = TestUtilities.CreateSocketInteractor();

            Assert.That(interactor, Is.Not.Null);

            interactor.enabled = false;
            interactable.transform.position = Vector3.zero;

            yield return new WaitForFixedUpdate();

            var validTargets = new List<IXRInteractable>();
            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.Empty);

            interactable.enabled = true;
            interactor.enabled = true;
            yield return new WaitForFixedUpdate();

            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.EqualTo(new[] { interactable }));
        }

        [UnityTest]
        public IEnumerator ContactInteractorIgnoresDisabledCollidersWhenSortingValidTargets([ValueSource(nameof(s_ContactInteractors))] Type interactorType)
        {
            // This will test that the Direct and Socket Interactor will ignore disabled colliders
            // when sorting to find the closest interactable to select.

            // Create Interaction Manager
            TestUtilities.CreateInteractionManager();

            // Interactable 1 has a single sphere collider centered on its local origin.
            // The sphere collider has a radius of 1.
            var interactable1 = TestUtilities.CreateGrabInteractable();
            interactable1.transform.position = new Vector3(-1.1f, 0, 0);
            interactable1.enabled = false;
            interactable1.name = "interactable1";

            // Interactable 1 has a single sphere collider centered on its local origin.
            // The sphere collider has a radius of 1. It is also disabled.
            var interactable2 = TestUtilities.CreateGrabInteractable();
            interactable2.GetComponent<SphereCollider>().enabled = false;
            interactable2.transform.position = new Vector3(1, 0, 0);
            interactable2.enabled = false;
            interactable2.name = "interactable2";

            yield return new WaitForFixedUpdate();

            XRBaseInteractor interactor = null;
            if (interactorType == typeof(XRDirectInteractor))
                interactor = TestUtilities.CreateDirectInteractor();
            else if (interactorType == typeof(XRSocketInteractor))
                interactor = TestUtilities.CreateSocketInteractor();

            Assert.That(interactor, Is.Not.Null);

            interactor.enabled = false;

            yield return new WaitForFixedUpdate();

            var validTargets = new List<IXRInteractable>();
            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.Empty, $"All interactors and interactables are disabled, so there should be no valid targets.");

            interactor.enabled = true;
            interactable1.enabled = true;
            interactable2.enabled = true;

            yield return new WaitForFixedUpdate();

            // Since interactable2's collider is disabled, it should not show up in the list of valid targets.
            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.EqualTo(new[] { interactable1 }));
        }

        [UnityTest]
        public IEnumerator ContactInteractorValidTargetsListEmptyWhenInteractorDisabled([ValueSource(nameof(s_ContactInteractors))] Type interactorType)
        {
            // This will test that the Direct and Socket Interactor will clear valid targets
            // and stayed colliders when the interactor or its GameObject is disabled and
            // the targets will be correctly added back in when the interactor is enabled again.
            var interactionManager = TestUtilities.CreateInteractionManager();
            XRBaseInteractor interactor = null;
            if (interactorType == typeof(XRDirectInteractor))
                interactor = TestUtilities.CreateDirectInteractor();
            else if (interactorType == typeof(XRSocketInteractor))
            {
                interactor = TestUtilities.CreateSocketInteractor();
                ((XRSocketInteractor)interactor).recycleDelayTime = 0f;
            }

            Assert.That(interactor, Is.Not.Null);

            // Create Interactable
            var interactable = TestUtilities.CreateGrabInteractable();
            yield return new WaitForFixedUpdate();
            yield return null;

            // Check that the interactable is a valid target of and can be hovered by the interactor.
            var validTargets = new List<IXRInteractable>();
            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.EqualTo(new[] {interactable}));
            Assert.That(interactor.interactablesHovered, Is.EqualTo(new[] {interactable}));
            Assert.That(interactor.hasHover, Is.True);
            
            // De-activate the interactor GameObject
            interactor.gameObject.SetActive(false);

            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.Empty);
            Assert.That(interactor.hasHover, Is.False);

            // Reactivate the interactor GameObject
            interactor.gameObject.SetActive(true);
            yield return new WaitForFixedUpdate();
            yield return null;

            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.EqualTo(new[] {interactable}));
            Assert.That(interactor.interactablesHovered, Is.EqualTo(new[] {interactable}));
            Assert.That(interactor.hasHover, Is.True);

            // De-activate the interactor component.
            interactor.enabled = false;

            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.Empty);
            Assert.That(interactor.hasHover, Is.False);

            // Reactivate the interactor component
            interactor.enabled = true;
            yield return new WaitForFixedUpdate();
            yield return null;
            
            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.EqualTo(new[] {interactable}));
            Assert.That(interactor.interactablesHovered, Is.EqualTo(new[] {interactable}));
            Assert.That(interactor.hasHover, Is.True);
        }

        [UnityTest]
        public IEnumerator ContactInteractorValidTargetsRemainClearWhenEnabledWithNoContact([ValueSource(nameof(s_ContactInteractors))] Type interactorType)
        {
            // This will test that the Direct and Socket Interactor will clear valid targets
            // and colliders when the interactor is disabled during contact and the valid 
            // targets and colliders will remain clear when the interactor is enabled again
            // while not touching any colliders.
            var interactionManager = TestUtilities.CreateInteractionManager();
            XRBaseInteractor interactor = null;
            if (interactorType == typeof(XRDirectInteractor))
                interactor = TestUtilities.CreateDirectInteractor();
            else if (interactorType == typeof(XRSocketInteractor))
            {
                interactor = TestUtilities.CreateSocketInteractor();
                ((XRSocketInteractor)interactor).recycleDelayTime = 0f;
            }

            Assert.That(interactor, Is.Not.Null);

            // Create Interactable
            var interactable = TestUtilities.CreateGrabInteractable();
            Vector3 interactorInitPosition = interactor.transform.position;
            Vector3 interactorMovedPosition = Vector3.one * 2f;
            yield return new WaitForFixedUpdate();
            yield return null;

            // Check that the interactable is a valid target of and can be hovered by the interactor.
            var validTargets = new List<IXRInteractable>();
            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.EqualTo(new[] {interactable}));
            Assert.That(interactor.interactablesHovered, Is.EqualTo(new[] {interactable}));
            Assert.That(interactor.hasHover, Is.True);

            // De-activate the interactor component.
            interactor.enabled = false;

            // Reposition the interactor away from the interactable and re-enable the interactor via Component
            interactor.transform.position = interactorMovedPosition;
            interactor.enabled = true;

            // Ensure no lingering hovers when interactor is moved away when disabled
            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.Empty);
            Assert.That(interactor.hasHover, Is.False);

            yield return new WaitForFixedUpdate();
            yield return null;

            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.Empty);
            Assert.That(interactor.hasHover, Is.False);

            // Move the interactor to the initial position
            interactor.transform.position = interactorInitPosition;
            yield return new WaitForFixedUpdate();
            yield return null;

            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.EqualTo(new[] { interactable }));
            Assert.That(interactor.interactablesHovered, Is.EqualTo(new[] { interactable }));
            Assert.That(interactor.hasHover, Is.True);
            
            // De-activate the interactor GameObject.
            interactor.gameObject.SetActive(false);

            // Reposition the interactor away from the interactable and re-enable the interactor via GameObject
            interactor.transform.position = interactorMovedPosition;
            interactor.gameObject.SetActive(true);

            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.Empty);
            Assert.That(interactor.hasHover, Is.False);

            yield return new WaitForFixedUpdate();
            yield return null;

            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.Empty);
            Assert.That(interactor.hasHover, Is.False);

            // Move the interactor to the initial position
            interactor.transform.position = interactorInitPosition;
            yield return new WaitForFixedUpdate();
            yield return null;

            interactor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.EqualTo(new[] { interactable }));
            Assert.That(interactor.interactablesHovered, Is.EqualTo(new[] { interactable }));
            Assert.That(interactor.hasHover, Is.True);
        }

        [UnityTest]
        public IEnumerator InteractableCanProcessHoverFilters()
        {
            TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateMockInteractor();
            var interactable = TestUtilities.CreateSimpleInteractable();
            interactor.validTargets.Add(interactable);

            var filter1WasProcessed = false;
            var filter1 = new XRHoverFilterDelegate((x, y) =>
            {
                filter1WasProcessed = true;
                return true;
            });
            interactor.hoverFilters.Add(filter1);

            var filter2WasProcessed = false;
            var filter2 = new XRHoverFilterDelegate((x, y) =>
            {
                filter2WasProcessed = true;
                return true;
            });
            interactor.hoverFilters.Add(filter2);

            yield return null;

            Assert.That(filter1WasProcessed, Is.True);
            Assert.That(filter2WasProcessed, Is.True);
            Assert.That(interactable.interactorsHovering, Is.EquivalentTo(new[] { interactor }));

            // Add filter that returns false
            var filter3WasProcessed = false;
            var filter3 = new XRHoverFilterDelegate((x, y) =>
            {
                filter3WasProcessed = true;
                return false;
            });
            interactor.hoverFilters.Add(filter3);

            yield return null;

            Assert.That(filter3WasProcessed, Is.True);
            Assert.That(interactable.interactorsHovering, Is.Empty);
        }

        [UnityTest]
        public IEnumerator InteractableCanProcessSelectFilters()
        {
            TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateMockInteractor();
            var interactable = TestUtilities.CreateSimpleInteractable();
            interactor.validTargets.Add(interactable);

            var filter1WasProcessed = false;
            var filter1 = new XRSelectFilterDelegate((x, y) =>
            {
                filter1WasProcessed = true;
                return true;
            });
            interactor.selectFilters.Add(filter1);

            var filter2WasProcessed = false;
            var filter2 = new XRSelectFilterDelegate((x, y) =>
            {
                filter2WasProcessed = true;
                return true;
            });
            interactor.selectFilters.Add(filter2);

            yield return null;

            Assert.That(filter1WasProcessed, Is.True);
            Assert.That(filter2WasProcessed, Is.True);
            Assert.That(interactable.interactorsSelecting, Is.EquivalentTo(new[] { interactor }));

            // Add filter that returns false
            var filter3WasProcessed = false;
            var filter3 = new XRSelectFilterDelegate((x, y) =>
            {
                filter3WasProcessed = true;
                return false;
            });
            interactor.selectFilters.Add(filter3);

            yield return null;

            Assert.That(filter3WasProcessed, Is.True);
            Assert.That(interactable.interactorsSelecting, Is.Empty);
        }
    }
}