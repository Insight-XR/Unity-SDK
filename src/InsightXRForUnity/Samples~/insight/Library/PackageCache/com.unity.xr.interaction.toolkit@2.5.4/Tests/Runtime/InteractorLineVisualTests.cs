using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    class InteractorLineVisualTests
    {
        static readonly XRRayInteractor.LineType[] s_LineTypes =
        {
            XRRayInteractor.LineType.StraightLine,
            XRRayInteractor.LineType.ProjectileCurve,
            XRRayInteractor.LineType.BezierCurve,
        };
        
        static readonly Quaternion[] k_ValidSnapRotations =
        {
            Quaternion.Euler(0f, 5f, 0f),
            Quaternion.Euler(5f, -5f, 0f),
            Quaternion.Euler(0f, 0f, 0f),
        };
        
        static readonly Quaternion k_InvalidSnapRotation = Quaternion.Euler(0f, 90f, 0f);
        
        static readonly Gradient k_InvalidColorGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.red, 1f) },
            alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) },
        };
        static readonly Gradient k_ValidColorGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) },
        };
        static readonly Gradient k_BlockedColorGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(Color.yellow, 0f), new GradientColorKey(Color.yellow, 1f) },
            alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) },
        };

        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllSceneObjects();
        }

        [UnityTest]
        public IEnumerator LineVisualUsesCorrectStateVisualOptions()
        {
            TestUtilities.CreateInteractionManager();

            var interactor = TestUtilities.CreateRayInteractor();
            interactor.transform.position = Vector3.zero;
            interactor.transform.forward = Vector3.forward;
            var lineVisual = interactor.GetComponent<XRInteractorLineVisual>();
            var lineRenderer = lineVisual.GetComponent<LineRenderer>();
            lineVisual.invalidColorGradient = k_InvalidColorGradient;
            lineVisual.validColorGradient = k_ValidColorGradient;
            lineVisual.blockedColorGradient = k_BlockedColorGradient;
            var validReticle = new GameObject("valid reticle");
            lineVisual.reticle = validReticle;
            var blockedReticle = new GameObject("blocked reticle");
            lineVisual.blockedReticle = blockedReticle;

            // No valid target
            yield return new WaitForSeconds(0.1f);
            lineVisual.UpdateLineVisual();
            Assert.That(lineRenderer.colorGradient.Evaluate(0f), Is.EqualTo(k_InvalidColorGradient.Evaluate(0f)).Using(ColorEqualityComparer.Instance));
            Assert.That(lineRenderer.colorGradient.Evaluate(1f), Is.EqualTo(k_InvalidColorGradient.Evaluate(1f)).Using(ColorEqualityComparer.Instance));
            Assert.That(validReticle.activeSelf, Is.False);
            Assert.That(blockedReticle.activeSelf, Is.False);

            // Valid target exists
            var interactable = TestUtilities.CreateGrabInteractable();
            interactable.transform.position = interactor.transform.position + interactor.transform.forward * 5.0f;
            yield return new WaitForSeconds(0.1f);
            lineVisual.UpdateLineVisual();
            Assert.That(lineRenderer.colorGradient.Evaluate(0f), Is.EqualTo(k_ValidColorGradient.Evaluate(0f)).Using(ColorEqualityComparer.Instance));
            Assert.That(lineRenderer.colorGradient.Evaluate(1f), Is.EqualTo(k_ValidColorGradient.Evaluate(1f)).Using(ColorEqualityComparer.Instance));
            Assert.That(validReticle.activeSelf, Is.True);
            Assert.That(blockedReticle.activeSelf, Is.False);

            // Valid target exists but is not selectable
            var blockedFilter = new XRSelectFilterDelegate((x, y) => false);
            interactable.selectFilters.Add(blockedFilter);
            yield return new WaitForSeconds(0.1f);
            lineVisual.UpdateLineVisual();
            Assert.That(lineRenderer.colorGradient.Evaluate(0f), Is.EqualTo(k_BlockedColorGradient.Evaluate(0f)).Using(ColorEqualityComparer.Instance));
            Assert.That(lineRenderer.colorGradient.Evaluate(1f), Is.EqualTo(k_BlockedColorGradient.Evaluate(1f)).Using(ColorEqualityComparer.Instance));
            Assert.That(validReticle.activeSelf, Is.False);
            Assert.That(blockedReticle.activeSelf, Is.True);
        }
        
        [UnityTest]
        public IEnumerator LineVisualSnapsToSnapVolume([ValueSource(nameof(s_LineTypes))] XRRayInteractor.LineType lineType)
        {
            const int numBendyLineRenderPoints = 20;
            var manager = TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateRayInteractor();
            interactor.xrController.enabled = false;
            interactor.transform.position = Vector3.zero;
            const int sampleFrequency = 5;
            interactor.sampleFrequency = sampleFrequency;
            interactor.maxRaycastDistance = 20f;
            interactor.lineType = lineType;
            var lineVisual = interactor.GetComponent<XRInteractorLineVisual>();
            var lineRenderer = lineVisual.GetComponent<LineRenderer>();
            yield return null;

            Vector3[] samplePoints = null;
            var isValid = interactor.GetLinePoints(ref samplePoints, out var samplePointsCount);
            Assert.That(isValid, Is.True);
            Assert.That(samplePoints, Is.Not.Null);
            Assert.That(samplePointsCount, Is.EqualTo(lineType == XRRayInteractor.LineType.StraightLine ? 2 : sampleFrequency));
            yield return null;

            // Setup interactable and snap volume
            var interactable = TestUtilities.CreateSimpleInteractable();
            interactable.transform.position = interactor.transform.position + interactor.transform.forward * 5.0f;
            var snapVolume = TestUtilities.CreateSnapVolume();
            snapVolume.interactable = interactable;
            snapVolume.snapToCollider = interactable.colliders[0];
            snapVolume.transform.position = interactable.transform.position;
            
            // Turn interactor away from interactable
            interactor.transform.rotation = k_InvalidSnapRotation;
            yield return null;

            // Confirm we're not hitting anything
            var isHitInfoValid =  interactor.TryGetHitInfo(out var hp, out _, out var hitPositionInLine, out var isValidTarget);
            Assert.That(isHitInfoValid, Is.False);
            Assert.That(isValidTarget, Is.False);
            Assert.That(hitPositionInLine, Is.EqualTo(0));
            Assert.That(interactor.interactablesHovered, Is.Empty);

            // Enable trigger interaction and snap end point on visuals
            interactor.raycastTriggerInteraction = QueryTriggerInteraction.Collide;
            lineVisual.snapEndpointIfAvailable = true;

            // LineRenderer.GetPositions always returns 0 when running with the -batchmode flag
            if (!Application.isBatchMode)
            {
                // Test various interactor rotations and ensure line render endpoint is updated to the snapped endpoint
                Vector3[] renderedPoints = new Vector3[numBendyLineRenderPoints];
                for (var i = 0; i < k_ValidSnapRotations.Length; i++)
                {
                    interactor.transform.rotation = k_ValidSnapRotations[i];
                    yield return new WaitForSeconds(0.1f);
                    isHitInfoValid = interactor.TryGetHitInfo(out var hitPosition, out _, out _, out isValidTarget);
                    Assert.That(isHitInfoValid, Is.True);
                    Assert.That(isValidTarget, Is.True);
                    var closestPoint = snapVolume.GetClosestPoint(hitPosition);
                    yield return null;
                
                    lineRenderer.GetPositions(renderedPoints);
                    Assert.That(closestPoint, Is.EqualTo(renderedPoints.Last()).Using(Vector3ComparerWithEqualsOperator.Instance));
                }
            }
        }
    }
}