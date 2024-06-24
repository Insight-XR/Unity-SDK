using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    class InteractableTests
    {
        public enum InteractorPositionOption
        {
            InteractorInsideCollider,
            InteractorOutsideCollider,
        }

        public enum InteractionOption
        {
            HoverOnly,
            SelectOnly,
            HoverAndSelect,
        }

        static readonly InteractionOption[] s_InteractionOptions =
        {
            InteractionOption.HoverOnly,
            InteractionOption.SelectOnly,
            InteractionOption.HoverAndSelect,
        };

        static readonly InteractableSelectMode[] s_SelectModes =
        {
            InteractableSelectMode.Single,
            InteractableSelectMode.Multiple,
        };

        static readonly InteractableFocusMode[] s_FocusModes =
        {
            InteractableFocusMode.Single,
            InteractableFocusMode.Multiple,
        };

        static readonly XRBaseInteractable.DistanceCalculationMode[] s_DistanceCalculationMode =
        {
            XRBaseInteractable.DistanceCalculationMode.TransformPosition,
            XRBaseInteractable.DistanceCalculationMode.ColliderPosition,
            XRBaseInteractable.DistanceCalculationMode.ColliderVolume,
        };

        static readonly InteractorPositionOption[] s_InteractorPositionOption =
        {
            InteractorPositionOption.InteractorInsideCollider,
            InteractorPositionOption.InteractorOutsideCollider,
        };

        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllSceneObjects();
        }

        [UnityTest]
        public IEnumerator InteractableIsHoveredWhileAnyInteractorHovering()
        {
            TestUtilities.CreateInteractionManager();
            var interactor1 = TestUtilities.CreateMockInteractor();
            var interactor2 = TestUtilities.CreateMockInteractor();
            var interactable = TestUtilities.CreateGrabInteractable();

            Assert.That(interactable.isHovered, Is.False);
            Assert.That(interactable.interactorsHovering, Is.Empty);

            interactor1.validTargets.Add(interactable);
            interactor2.validTargets.Add(interactable);

            yield return null;

            Assert.That(interactable.isHovered, Is.True);
            Assert.That(interactable.interactorsHovering, Is.EquivalentTo(new[] { interactor1, interactor2 }));

            interactor2.validTargets.Clear();

            yield return null;

            Assert.That(interactable.isHovered, Is.True);
            Assert.That(interactable.interactorsHovering, Is.EquivalentTo(new[] { interactor1 }));

            interactor1.validTargets.Clear();

            yield return null;

            Assert.That(interactable.isHovered, Is.False);
            Assert.That(interactable.interactorsHovering, Is.Empty);
        }

        [UnityTest]
        public IEnumerator InteractableSelectModeSelect([ValueSource(nameof(s_SelectModes))] InteractableSelectMode selectMode)
        {
            TestUtilities.CreateInteractionManager();
            var interactor1 = TestUtilities.CreateMockInteractor();
            var interactor2 = TestUtilities.CreateMockInteractor();
            var interactable = TestUtilities.CreateGrabInteractable();
            interactable.selectMode = selectMode;

            interactor1.validTargets.Add(interactable);

            yield return null;

            Assert.That(interactable.isSelected, Is.True);
            Assert.That(interactable.interactorsSelecting, Is.EqualTo(new[] { interactor1 }));

            interactor2.validTargets.Add(interactable);

            yield return null;

            Assert.That(interactable.isSelected, Is.True);
            switch (selectMode)
            {
                case InteractableSelectMode.Single:
                    Assert.That(interactable.interactorsSelecting, Is.EqualTo(new[] { interactor2 }));
                    break;
                case InteractableSelectMode.Multiple:
                    Assert.That(interactable.interactorsSelecting, Is.EqualTo(new[] { interactor1, interactor2 }));
                    break;
                default:
                    Assert.Fail($"Unhandled {nameof(InteractableSelectMode)}={selectMode}");
                    break;
            }
        }

        [UnityTest]
        public IEnumerator InteractableFocusModeSelect([ValueSource(nameof(s_FocusModes))] InteractableFocusMode focusMode)
        {
            TestUtilities.CreateInteractionManager();
            var group1 = TestUtilities.CreateInteractionGroup();
            var group2 = TestUtilities.CreateInteractionGroup();
            var interactor1 = TestUtilities.CreateMockInteractor();
            var interactor2 = TestUtilities.CreateMockInteractor();

            group1.AddGroupMember(interactor1);
            group2.AddGroupMember(interactor2);

            var interactable = TestUtilities.CreateGrabInteractable();
            interactable.focusMode = focusMode;

            interactor1.validTargets.Add(interactable);

            yield return null;

            Assert.That(interactable.isFocused, Is.True);
            Assert.That(interactable.interactionGroupsFocusing, Is.EqualTo(new[] { group1 }));

            interactor2.validTargets.Add(interactable);

            yield return null;

            Assert.That(interactable.isFocused, Is.True);
            switch (focusMode)
            {
                case InteractableFocusMode.Single:
                    Assert.That(interactable.interactionGroupsFocusing, Is.EqualTo(new[] { group2 }));
                    break;
                case InteractableFocusMode.Multiple:
                    Assert.That(interactable.interactionGroupsFocusing, Is.EqualTo(new[] { group1, group2 }));
                    break;
                default:
                    Assert.Fail($"Unhandled {nameof(InteractableFocusMode)}={focusMode}");
                    break;
            }
        }

        [Test]
        public void InteractableDistanceCalculationModeWithInteractorInsideColliders([ValueSource(nameof(s_DistanceCalculationMode))]
            XRBaseInteractable.DistanceCalculationMode distanceCalculationMode)
        {
            TestUtilities.CreateInteractionManager();
            IXRInteractor interactor = TestUtilities.CreateMockInteractor();
            var interactable = TestUtilities.CreateSimpleInteractableWithColliders();

            interactor.transform.position = new Vector3(0.5f, 0f, 0f);
            interactable.distanceCalculationMode = distanceCalculationMode;

            Assert.That(interactable.distanceCalculationMode, Is.EqualTo(distanceCalculationMode));
            Assert.That(interactable.colliders.Count, Is.GreaterThan(1));

            var distanceSqr = interactable.GetDistanceSqrToInteractor(interactor);
            switch (distanceCalculationMode)
            {
                case XRBaseInteractable.DistanceCalculationMode.TransformPosition:
                    Assert.That(distanceSqr, Is.EqualTo(0.5f * 0.5f));
                    break;
                case XRBaseInteractable.DistanceCalculationMode.ColliderPosition:
                    Assert.That(distanceSqr, Is.EqualTo(0.5f * 0.5f));
                    break;
                case XRBaseInteractable.DistanceCalculationMode.ColliderVolume:
                    Assert.That(distanceSqr, Is.EqualTo(0f));
                    break;
                default:
                    Assert.Fail($"Unhandled {nameof(InteractableSelectMode)}={distanceCalculationMode}");
                    break;
            }
        }

        [Test]
        public void InteractableDistanceCalculationMode(
            [ValueSource(nameof(s_DistanceCalculationMode))] XRBaseInteractable.DistanceCalculationMode distanceCalculationMode,
            [ValueSource(nameof(s_InteractorPositionOption))] InteractorPositionOption interactorPositionOption)
        {
            TestUtilities.CreateInteractionManager();
            IXRInteractor interactor = TestUtilities.CreateMockInteractor();
            var interactable = TestUtilities.CreateSimpleInteractableWithColliders();

            // The sphere or box colliders have size of 1f and they are translated (below) to a random position inside a sphere of radius 0.25f
            var interactorPosition = interactorPositionOption == InteractorPositionOption.InteractorInsideCollider
                ? Random.insideUnitSphere * 0.2f
                : Random.onUnitSphere * 5f;

            interactor.transform.position = interactorPosition;
            interactable.distanceCalculationMode = distanceCalculationMode;

            Assert.That(interactable.distanceCalculationMode, Is.EqualTo(distanceCalculationMode));
            Assert.That(interactable.colliders.Count, Is.GreaterThan(1));

            // Move the colliders to random positions not far from the interactable
            foreach (var col in interactable.colliders)
                col.transform.position = Random.insideUnitSphere * 0.25f;

            var distanceSqr = interactable.GetDistanceSqrToInteractor(interactor);
            switch (distanceCalculationMode)
            {
                case XRBaseInteractable.DistanceCalculationMode.TransformPosition:
                    var offset = interactable.transform.position - interactorPosition;
                    Assert.That(distanceSqr, Is.EqualTo(offset.sqrMagnitude));
                    break;
                case XRBaseInteractable.DistanceCalculationMode.ColliderPosition:
                    var minColDistanceSqr = float.MaxValue;
                    foreach (var col in interactable.colliders)
                    {
                        offset = col.transform.position - interactorPosition;
                        minColDistanceSqr = Mathf.Min(minColDistanceSqr, offset.sqrMagnitude);
                    }
                    Assert.That(distanceSqr, Is.EqualTo(minColDistanceSqr));
                    break;
                case XRBaseInteractable.DistanceCalculationMode.ColliderVolume:
                    minColDistanceSqr = float.MaxValue;
                    foreach (var col in interactable.colliders)
                    {
                        offset = col.ClosestPoint(interactorPosition) - interactorPosition;
                        minColDistanceSqr = Mathf.Min(minColDistanceSqr, offset.sqrMagnitude);
                    }
                    Assert.That(distanceSqr, Is.EqualTo(minColDistanceSqr));
                    break;
                default:
                    Assert.Fail($"Unhandled {nameof(InteractableSelectMode)}={distanceCalculationMode}");
                    break;
            }
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
            interactable.hoverFilters.Add(filter1);

            var filter2WasProcessed = false;
            var filter2 = new XRHoverFilterDelegate((x, y) =>
            {
                filter2WasProcessed = true;
                return true;
            });
            interactable.hoverFilters.Add(filter2);

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
            interactable.hoverFilters.Add(filter3);

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
            interactable.selectFilters.Add(filter1);

            var filter2WasProcessed = false;
            var filter2 = new XRSelectFilterDelegate((x, y) =>
            {
                filter2WasProcessed = true;
                return true;
            });
            interactable.selectFilters.Add(filter2);

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
            interactable.selectFilters.Add(filter3);

            yield return null;

            Assert.That(filter3WasProcessed, Is.True);
            Assert.That(interactable.interactorsSelecting, Is.Empty);
        }

        [UnityTest]
        public IEnumerator InteractableCanProcessInteractionStrengthFilters([ValueSource(nameof(s_InteractionOptions))] InteractionOption interactionOption)
        {
            TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateMockInteractor();
            var interactable = TestUtilities.CreateSimpleInteractable();
            interactor.validTargets.Add(interactable);

            switch (interactionOption)
            {
                case InteractionOption.HoverOnly:
                    interactor.allowSelect = false;
                    break;
                case InteractionOption.SelectOnly:
                    interactor.allowHover = false;
                    break;
                case InteractionOption.HoverAndSelect:
                    break;
                default:
                    Assert.Fail($"Unhandled {nameof(InteractionOption)}={interactionOption}");
                    break;
            }

            var filter1ProcessedCount = 0;
            var filter1InputStrength = -1f;
            const float filter1Strength = 0.5f;
            var filter1 = new XRInteractionStrengthFilterDelegate((_, __, strength) =>
            {
                filter1ProcessedCount++;
                filter1InputStrength = strength;
                return filter1Strength;
            });
            interactable.interactionStrengthFilters.Add(filter1);

            var filter2ProcessedCount = 0;
            var filter2InputStrength = -1f;
            const float filter2Strength = 0.75f;
            var filter2 = new XRInteractionStrengthFilterDelegate((_, __, strength) =>
            {
                filter2ProcessedCount++;
                filter2InputStrength = strength;
                return filter2Strength;
            });
            interactable.interactionStrengthFilters.Add(filter2);

            yield return null;

            var expectedInitialInputStrength = 0f;
            switch (interactionOption)
            {
                case InteractionOption.HoverOnly:
                    Assert.That(interactable.interactorsHovering, Is.EquivalentTo(new[] { interactor }));
                    Assert.That(interactable.interactorsSelecting, Is.Empty);
                    expectedInitialInputStrength = 0f;
                    break;
                case InteractionOption.SelectOnly:
                    Assert.That(interactable.interactorsSelecting, Is.EquivalentTo(new[] { interactor }));
                    Assert.That(interactable.interactorsHovering, Is.Empty);
                    expectedInitialInputStrength = 1f;
                    break;
                case InteractionOption.HoverAndSelect:
                    Assert.That(interactable.interactorsHovering, Is.EquivalentTo(new[] { interactor }));
                    Assert.That(interactable.interactorsSelecting, Is.EquivalentTo(new[] { interactor }));
                    expectedInitialInputStrength = 1f;
                    break;
                default:
                    Assert.Fail($"Unhandled {nameof(InteractionOption)}={interactionOption}");
                    break;
            }

            Assert.That(filter1ProcessedCount, Is.EqualTo(1));
            Assert.That(filter2ProcessedCount, Is.EqualTo(1));
            Assert.That(filter1InputStrength, Is.EqualTo(expectedInitialInputStrength));
            Assert.That(filter2InputStrength, Is.EqualTo(filter1Strength));

            Assert.That(interactable.largestInteractionStrength.Value, Is.EqualTo(filter2Strength));
            Assert.That(interactor.largestInteractionStrength.Value, Is.EqualTo(filter2Strength));
            Assert.That(interactable.GetInteractionStrength(interactor), Is.EqualTo(filter2Strength));
            Assert.That(interactor.GetInteractionStrength(interactable), Is.EqualTo(filter2Strength));
        }

        [UnityTest]
        public IEnumerator InteractableLosesFocusOnSelectOfOtherInteractable()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateInteractionGroup();
            var directInteractor = TestUtilities.CreateDirectInteractor();
            group.AddGroupMember(directInteractor);

            var interactable = TestUtilities.CreateGrabInteractable();
            var interactable2 = TestUtilities.CreateGrabInteractable();
            interactable2.transform.position = new Vector3(10f, 0, 0);

            var controller = directInteractor.GetComponent<XRController>();
            var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
            {
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.0f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    false, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.1f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.2f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    false, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.3f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    false, false, false));
            });
            controllerRecorder.isPlaying = true;
            controllerRecorder.visitEachFrame = true;
            while (controllerRecorder.isPlaying)
            {
                yield return new WaitForSeconds(0.1f);
            }

            // Selection has come and gone - resulting in a focus of the grab interactable
            Assert.That(group.focusInteractable, Is.EqualTo( interactable ));
            Assert.That(interactable.isFocused, Is.EqualTo(true));
            Assert.That(interactable2.isFocused, Is.EqualTo(false));
            
            controllerRecorder.isPlaying = false;
            GameObject.Destroy(controllerRecorder);
            yield return null;

            var offset = new Vector3(10.0f, 0.0f, 0.0f);
            controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
            {
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.0f, offset, Quaternion.identity, InputTrackingState.All, true,
                    false, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.1f, offset, Quaternion.identity, InputTrackingState.All, true,
                    false, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.2f, offset, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.3f, offset, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
            });

            controllerRecorder.isPlaying = true;
            controllerRecorder.visitEachFrame = true;
            while (controllerRecorder.isPlaying)
            {
                yield return new WaitForSeconds(0.1f);
            }

            // Selection attempted again with 2nd object in focus.
            Assert.That(group.focusInteractable, Is.EqualTo(interactable2));
            Assert.That(interactable.isFocused, Is.EqualTo(false));
            Assert.That(interactable2.isFocused, Is.EqualTo(true));
        }
    }
}
