using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.XR.CoreUtils;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    class GazeInteractorTests
    {
        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllSceneObjects();
        }

        [UnityTest]
        public IEnumerator GazeInteractorCanHoverInteractable()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateGazeInteractor();
            interactor.transform.position = Vector3.zero;
            interactor.transform.forward = Vector3.forward;
            var interactable = TestUtilities.CreateSimpleInteractable();
            interactable.transform.position = interactor.transform.position + interactor.transform.forward * 5.0f;

            Assert.That(interactable.allowGazeInteraction, Is.False);

            // Wait for Physics update for hit
            yield return new WaitForFixedUpdate();
            yield return null;

            var validTargets = new List<IXRInteractable>();
            manager.GetValidTargets(interactor, validTargets);

            Assert.That(validTargets, Is.EqualTo(new[] { interactable }));
            Assert.That(interactor.interactablesHovered, Is.Empty);

            interactable.allowGazeInteraction = true;

            yield return null;

            Assert.That(validTargets, Is.EqualTo(new[] { interactable }));
            Assert.That(interactor.interactablesHovered, Is.EqualTo(new[] { interactable }));
        }

        [UnityTest]
        public IEnumerator ManualInteractorSelection()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateGazeInteractor();
            interactor.transform.position = Vector3.zero;
            interactor.transform.forward = Vector3.forward;
            var interactable = TestUtilities.CreateSimpleInteractable();
            interactable.allowGazeSelect = true;
            interactable.transform.position = interactor.transform.position + interactor.transform.right * 5.0f;

            // Wait for Physics update for hit
            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(interactor.interactablesSelected, Is.Empty);

            interactor.StartManualInteraction((IXRSelectInteractable)interactable);

            yield return null;

            Assert.That(interactor.interactablesSelected, Is.Empty);

            interactable.allowGazeInteraction = true;

            interactor.StartManualInteraction((IXRSelectInteractable)interactable);

            yield return null;

            Assert.That(interactor.interactablesSelected, Is.EqualTo(new[] { interactable }));

            yield return null;

            interactor.EndManualInteraction();

            Assert.That(interactor.interactablesSelected, Is.Empty);
        }

        [UnityTest]
        public IEnumerator GazeInteractorCanHoverToSelect()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateGazeInteractor();
            interactor.transform.position = Vector3.zero;
            interactor.transform.forward = Vector3.forward;
            interactor.hoverToSelect = true;
            interactor.hoverTimeToSelect = 0.1f;
            var interactable = TestUtilities.CreateSimpleInteractable();
            interactable.allowGazeInteraction = true;
            interactable.allowGazeSelect = true;
            interactable.overrideGazeTimeToSelect = true;
            interactable.gazeTimeToSelect = 0.2f;
            interactable.transform.position = interactor.transform.position + interactor.transform.forward * 5.0f;

            // Wait for Physics update for hit
            yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds(0.11f);

            // Hasn't met duration threshold yet
            Assert.That(interactable.isHovered, Is.True);
            Assert.That(interactor.interactablesHovered, Is.EqualTo(new[] { interactable }));
            Assert.That(interactable.isSelected, Is.False);
            Assert.That(interactor.isSelectActive, Is.False);

            yield return new WaitForSeconds(0.2f);

            Assert.That(interactable.isHovered, Is.True);
            Assert.That(interactable.isSelected, Is.True);
            Assert.That(interactor.isSelectActive, Is.True);
            Assert.That(interactor.interactablesSelected, Is.EqualTo(new[] { interactable }));
        }
        
        [UnityTest]
        public IEnumerator GazeInteractorCanResetOnInteractableDestroy()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateGazeInteractor();
            interactor.hoverToSelect = true;
            interactor.hoverTimeToSelect = 0.1f;
            interactor.transform.position = Vector3.zero;
            interactor.transform.forward = Vector3.forward;
            var interactable = TestUtilities.CreateSimpleInteractable();
            interactable.allowGazeInteraction = true;
            interactable.allowGazeSelect = true;
            interactable.transform.position = interactor.transform.position + interactor.transform.forward * 5.0f;

            // Wait for Physics update for hit
            yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds(0.2f);

            Assert.That(interactor.interactablesSelected, Is.EqualTo(new[] { interactable }));

            var attachPosition = interactor.attachTransform.position;

            Object.Destroy(interactable.gameObject);

            yield return null;

            Assert.That(interactor.interactablesSelected, Is.Empty);
            Assert.That(interactor.attachTransform.position, Is.EqualTo(attachPosition));
        }

        [UnityTest]
        public IEnumerator GazeInteractorGazeAssistance()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateGazeInteractor();
            interactor.gazeAssistanceCalculation = XRGazeInteractor.GazeAssistanceCalculation.FixedSize;
            interactor.transform.position = Vector3.zero;
            interactor.transform.forward = Vector3.forward;
            interactor.gazeAssistanceColliderFixedSize = 1.2f;
            interactor.gazeAssistanceColliderScale = 1.5f;
            var interactable = TestUtilities.CreateSimpleInteractable();
            interactable.allowGazeInteraction = true;
            interactable.allowGazeAssistance = true;
            interactable.transform.position = interactor.transform.position + interactor.transform.forward * 5.0f;

            var interactableCollider = interactable.GetComponent<SphereCollider>();
            var snapVolume = interactor.gazeAssistanceSnapVolume;
            var assistSize = interactor.gazeAssistanceColliderFixedSize;
            var assistScale = interactor.gazeAssistanceColliderScale;
            var targetExtents = new Vector3(assistSize * assistScale, assistSize * assistScale, assistSize * assistScale);

            Assert.That(snapVolume.transform.position, Is.EqualTo(Vector3.zero));
            Assert.That(interactor.interactablesHovered, Is.Empty);

            // Wait for Physics update for hit
            yield return new WaitForFixedUpdate();
            yield return null;

            // Test fixed size gaze assistance
            Assert.That(interactor.interactablesHovered, Is.EqualTo(new[] { interactable }));
            Assert.That(snapVolume.transform.position, Is.EqualTo(interactable.colliders[0].bounds.center));
            Assert.That(snapVolume.snapCollider.bounds.extents, Is.EqualTo(targetExtents).Using(Vector3ComparerWithEqualsOperator.Instance));

            // Test collider sized gaze assistance
            interactor.gazeAssistanceCalculation = XRGazeInteractor.GazeAssistanceCalculation.ColliderSize;
            assistSize = interactableCollider.bounds.size.MaxComponent();
            targetExtents = new Vector3(assistSize * assistScale, assistSize * assistScale, assistSize * assistScale);

            yield return null;

            Assert.That(interactor.interactablesHovered, Is.EqualTo(new[] { interactable }));
            Assert.That(snapVolume.transform.position, Is.EqualTo(interactable.colliders[0].bounds.center));
            Assert.That(snapVolume.snapCollider.bounds.extents, Is.EqualTo(targetExtents).Using(Vector3ComparerWithEqualsOperator.Instance));

            interactable.allowGazeInteraction = false;

            yield return null;

            Assert.That(snapVolume.transform.position, Is.EqualTo(Vector3.zero));
            Assert.That(interactor.interactablesHovered, Is.Empty);
        }
    }
}