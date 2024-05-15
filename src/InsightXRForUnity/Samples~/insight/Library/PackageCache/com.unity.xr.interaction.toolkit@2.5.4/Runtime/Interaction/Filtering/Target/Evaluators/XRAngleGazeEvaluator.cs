using System;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Evaluates the angle between the Gaze Transform's forward direction and the vector to the target Interactable.
    /// The target Interactable with the smallest angle will receive the highest normalized score.
    /// </summary>
    [Serializable]
    public class XRAngleGazeEvaluator : XRTargetEvaluator
    {
        static Camera GetXROriginCamera()
        {
            return ComponentLocatorUtility<XROrigin>.TryFindComponent(out var xrOrigin) ? xrOrigin.Camera : null;
        }

        [Tooltip("The Transform whose forward direction is used to evaluate the target Interactable angle. If none is specified, during OnEnable this property is initialized with the XROrigin Camera.")]
        [SerializeField]
        Transform m_GazeTransform;

        /// <summary>
        /// The Transform whose forward direction is used to evaluate the target Interactable angle. If none is specified, during OnEnable this property is initialized with the XROrigin Camera.
        /// </summary>
        public Transform gazeTransform
        {
            get => m_GazeTransform;
            set => m_GazeTransform = value;
        }

        [Tooltip("The maximum value an angle can be evaluated as before the Interactor receives a normalized score of 0. Think of it as a field-of-view angle.")]
        [SerializeField, Range(0f, 180f)]
        float m_MaxAngle = 60f;

        /// <summary>
        /// The maximum value an angle can be evaluated as before the Interactor receives a normalized score of 0. Think of it as a field-of-view angle.
        /// </summary>
        public float maxAngle
        {
            get => m_MaxAngle;
            set => m_MaxAngle = Mathf.Clamp(value, 0f, 180f);
        }

        void InitializeGazeTransform()
        {
            var xrOriginCamera = GetXROriginCamera();
            if (xrOriginCamera != null && xrOriginCamera.enabled && xrOriginCamera.gameObject.activeInHierarchy)
                m_GazeTransform = xrOriginCamera.transform;
            else
                Debug.LogWarning($"Couldn't find an active XROrigin Camera for {nameof(XRAngleGazeEvaluator)}", filter);
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            if (m_GazeTransform == null)
                InitializeGazeTransform();
        }

        /// <inheritdoc />
        public override void Reset()
        {
            base.Reset();
            InitializeGazeTransform();
        }

        /// <inheritdoc />
        protected override float CalculateNormalizedScore(IXRInteractor interactor, IXRInteractable target)
        {
            var cachedGazeTransform = gazeTransform;
            if (cachedGazeTransform == null || m_MaxAngle <= 0f)
                return 0f;

            // Gets the target position
            Vector3 targetPosition;
            if (target is XRBaseInteractable targetAsBaseInteractable)
            {
                var distanceInfo = targetAsBaseInteractable.GetDistance(cachedGazeTransform.position);
                targetPosition = distanceInfo.point;
            }
            else
            {
                var targetTransform = target.transform;
                if (targetTransform == null)
                    return 0f;

                targetPosition = targetTransform.position;
            }

            // Calculates the angle between the gaze forward direction and the direction from
            // the gaze to the target Interactable
            var directionToTarget = targetPosition - cachedGazeTransform.position;
            var targetAngleFromCamera = Vector3.Angle(directionToTarget, cachedGazeTransform.forward);

            // Calculates the percentage of the target Interactable angle compared to the max angle
            // The target angle is multiplied by 2 to transform it into a field of view angle
            var targetAnglePercentage = targetAngleFromCamera * 2f / m_MaxAngle;
            return 1f - targetAnglePercentage;
        }
    }
}
