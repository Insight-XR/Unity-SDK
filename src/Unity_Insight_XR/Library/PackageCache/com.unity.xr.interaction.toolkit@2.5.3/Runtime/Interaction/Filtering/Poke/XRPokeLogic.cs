using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings.Variables;
using Unity.XR.CoreUtils.Collections;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Class that encapsulates the logic for evaluating whether an interactable has been poked or not
    /// through enter and exit thresholds.
    /// </summary>
    class XRPokeLogic : IDisposable
    {
        /// <summary>
        /// Length of interaction axis computed from the attached collider bounds and configured interaction direction.
        /// </summary>
        float interactionAxisLength { get; set; } = 1f;

        readonly BindableVariable<PokeStateData> m_PokeStateData = new BindableVariable<PokeStateData>();

        /// <summary>
        /// Bindable variable that updates whenever the poke logic state is evaluated.
        /// </summary>
        public IReadOnlyBindableVariable<PokeStateData> pokeStateData => m_PokeStateData;

        Transform m_InitialTransform;
        PokeThresholdData m_PokeThresholdData;
        float m_SelectEntranceVectorDotThreshold;

        readonly Dictionary<object, Vector3> m_LastHoverEnterLocalPosition = new Dictionary<object, Vector3>();
        readonly Dictionary<object, Transform> m_LastHoveredTransform = new Dictionary<object, Transform>();
        readonly Dictionary<object, bool> m_HoldingHoverCheck = new Dictionary<object, bool>();
        readonly Dictionary<Transform, HashSetList<object>> m_HoveredInteractorsOnThisTransform = new Dictionary<Transform, HashSetList<object>>();
        readonly Dictionary<object, float> m_LastInteractorPressDepth = new Dictionary<object, float>();

        /// <summary>
        /// Threshold value where the poke interaction is considered to be selecting the interactable.
        /// We normally checked 1% as the activation point, but setting it to 2.5 % makes things feel a bit more responsive.
        /// </summary>
        const float k_DepthPercentActivationThreshold = 0.025f;//0.05f;
        
        /// <summary>
        /// We require a minimum velocity for poke hover conditions to be met, and avoid the noise of tracking jitter.
        /// </summary>
        const float k_SquareVelocityHoverThreshold = 0.0001f;

        /// <summary>
        /// Initializes <see cref="XRPokeLogic"/> with properties calculated from the collider of the associated interactable.
        /// </summary>
        /// <param name="associatedTransform"><see cref="Transform"/> object used for poke calculations.</param>
        /// <param name="pokeThresholdData"><see cref="PokeThresholdData"/> object containing the specific poke parameters used for calculating
        /// whether or not the current interaction meets the requirements for poke hover or select.</param>
        /// <param name="collider"><see cref="Collider"/> for computing the interaction axis length used to detect if poke depth requirements are met.</param>
        public void Initialize(Transform associatedTransform, PokeThresholdData pokeThresholdData, Collider collider)
        {
            m_InitialTransform = associatedTransform;
            m_PokeThresholdData = pokeThresholdData;
            m_SelectEntranceVectorDotThreshold = pokeThresholdData.GetSelectEntranceVectorDotThreshold();

            if (collider != null)
            {             
                interactionAxisLength = ComputeInteractionAxisLength(ComputeBounds(collider));
            }
            ResetPokeStateData(m_InitialTransform);
        }

        /// <summary>
        /// This method will reset the underlying interaction length used to determine if the current poke depth has been reached. This is typically
        /// used on UI objects, or objects where poke depth is not appropriately defined by the collider bounds of the object.
        /// </summary>
        /// <param name="pokeDepth">A value representing the poke depth required to meet requirements for select.</param>
        public void SetPokeDepth(float pokeDepth)
        {
            interactionAxisLength = pokeDepth;
        }

        /// <summary>
        /// Clears cached data hover enter pose data.
        /// </summary>
        public void Dispose()
        {
            m_LastHoverEnterLocalPosition.Clear();
        }

        /// <summary>
        /// Logic to check if the attempted poke interaction meets the requirements for a select action.
        /// </summary>
        /// <param name="interactor">The interactor that is a candidate for selection.</param>
        /// <param name="pokableAttachPosition">The attach transform position of the pokable object, typically an interactable object.</param>
        /// <param name="pokerAttachPosition">The attach transform position for the interactor.</param>
        /// <param name="pokeInteractionOffset">An additional offset that will be applied to the calculation for the depth required to meet requirements for selection.</param>
        /// <param name="pokedTransform">The target Transform that is being poked.</param>
        /// <returns>
        /// Returns <see langword="true"/> if interaction meets requirements for select action.
        /// Otherwise, returns <see langword="false"/>.
        /// </returns>
        public bool MeetsRequirementsForSelectAction(object interactor, Vector3 pokableAttachPosition, Vector3 pokerAttachPosition, float pokeInteractionOffset, Transform pokedTransform)
        {
            if (m_PokeThresholdData == null || pokedTransform == null)
                return false;

            Vector3 axisNormal = ComputeRotatedDepthEvaluationAxis(pokedTransform);

            // Move interaction point towards the target, along the interaction normal, by the determined offset.
            float combinedOffset = pokeInteractionOffset + m_PokeThresholdData.interactionDepthOffset;
            Vector3 toleranceOffset = axisNormal * combinedOffset;
            Vector3 interactionPoint = pokerAttachPosition - toleranceOffset;

            Vector3 interactionPointOffset = interactionPoint - pokableAttachPosition;
            Vector3 axisAlignedInteractionPointOffset = Vector3.Project(interactionPointOffset, axisNormal);
            float interactionDepth = axisAlignedInteractionPointOffset.magnitude;


            float entranceVectorDot = Vector3.Dot(axisNormal, interactionPointOffset.normalized);
            float entranceVectorDotSign = Mathf.Sign(entranceVectorDot);
            bool isOverObject = entranceVectorDot > 0f;
            
            float depthPercent = entranceVectorDotSign * interactionDepth / interactionAxisLength;
            float clampedDepthPercent = Mathf.Clamp01(depthPercent);
            
            // Compare with hover pose, to ensure interaction started on the right side of the interaction bounds
            bool meetsHoverRequirements = true;
            if (m_PokeThresholdData.enablePokeAngleThreshold)
            {
                // If we previously passed the hover check for this select, then we hold it.
                // This allows us to hold a button without moving.
                if (!m_HoldingHoverCheck.ContainsKey(interactor))
                    m_HoldingHoverCheck[interactor] = false;
                
                if (!m_HoldingHoverCheck[interactor])
                {
                    if (isOverObject)
                    {
                        // Ensure the object's hover started from the right side of the object
                        if(m_LastHoverEnterLocalPosition.TryGetValue(interactor, out var hoverLocalPosition))
                        {
                            // Restore the world space pos of the hover enter point relative to the hovered transform.
                            var hoverWorldPos = m_LastHoveredTransform[interactor].TransformPoint(hoverLocalPosition);
                            Vector3 hoverInteractionPointOffset = (hoverWorldPos - pokableAttachPosition).normalized;
                            meetsHoverRequirements = Vector3.Dot(hoverInteractionPointOffset, axisNormal) > 0;
                        }
                        
                        // If we've met the first hover check of starting from the right side, we then check to ensure our velocity delta approach meets our threshold.
                        if (meetsHoverRequirements && interactor is XRBaseInteractor baseInteractor && baseInteractor.useAttachPointVelocity)
                        {
                            var interactorVelocity = baseInteractor.GetAttachPointVelocity();

                            bool isVelocitySufficient = Vector3.SqrMagnitude(interactorVelocity) > k_SquareVelocityHoverThreshold;

                            // If interactor velocity is sufficiently large, check approach vector
                            if (isVelocitySufficient)
                            {
                                // Start with hover check based on velocity
                                float velocityAxisDotProduct = Vector3.Dot(-interactorVelocity.normalized, axisNormal);
                                meetsHoverRequirements = velocityAxisDotProduct > m_SelectEntranceVectorDotThreshold;
                            }
                            else
                            {
                                meetsHoverRequirements = false;
                            }
                        }
                    }
                    else
                    {
                        meetsHoverRequirements = false;
                    }
                }
            }

            // Reset depth percent if hover requirements are not met.
            if (!meetsHoverRequirements)
            {
                clampedDepthPercent = 1f;
            }
            
            // Either depth lines up, or we've moved passed the goal post after passing the hover check
            bool meetsRequirements = meetsHoverRequirements && clampedDepthPercent < k_DepthPercentActivationThreshold;
            
            // Store holding hover check for this interactor
            m_HoldingHoverCheck[interactor] = meetsHoverRequirements;
            
            m_LastInteractorPressDepth[interactor] = clampedDepthPercent;

            // If multiple interactors are poking this transform, we only want to allow the one that is deepest to select.
            if (!meetsRequirements && m_HoveredInteractorsOnThisTransform.TryGetValue(pokedTransform, out var hoveringInteractors))
            {
                var hoveringInteractorsCount = hoveringInteractors.Count;
                if (hoveringInteractorsCount > 1)
                {
                    var hoveringInteractorsList = hoveringInteractors.AsList();
                    for (int i = 0; i < hoveringInteractorsCount; i++)
                    {
                        var hoveringInteractor = hoveringInteractorsList[i];
                        if (hoveringInteractor == interactor)
                            continue;
                        
                        // If something else deeper, we don't allow this interactor to broadcast it's press depth.
                        var otherInteractorPressDepth = m_LastInteractorPressDepth[hoveringInteractor];
                        if (otherInteractorPressDepth < clampedDepthPercent)
                            return false;
                    }
                }
            }
            
            // Remove offset from visual callback to better match the actual poke position.
            var offsetRemoval = depthPercent < 1f && !meetsRequirements ? combinedOffset : 0f;

            // Update poke state data for affordances
            var axisDepth = meetsRequirements ? 0f : meetsHoverRequirements ? clampedDepthPercent : 1f;
            var clampedPokeDepth = Mathf.Clamp(axisDepth * interactionAxisLength + offsetRemoval, 0f, interactionAxisLength);
            
            m_PokeStateData.Value = new PokeStateData
            {
                meetsRequirements = meetsRequirements,
                pokeInteractionPoint = pokerAttachPosition,
                axisAlignedPokeInteractionPoint = pokableAttachPosition + clampedPokeDepth * axisNormal,
                interactionStrength = 1f - clampedDepthPercent,
                axisNormal = axisNormal,
                target = pokedTransform,
            };

            return meetsRequirements;
        }

        /// <summary>
        /// Computes the direction of the interaction axis, as configured with the poke threshold data.
        /// </summary>
        /// <param name="associatedTransform">This represents the Transform used to determine the evaluation axis along the specified poke axis.</param>
        /// <param name="isWorldSpace">World space uses the current interactable rotation, local space takes basic vector directions.</param>
        /// <returns>Normalized vector along the axis of interaction.</returns>
        Vector3 ComputeRotatedDepthEvaluationAxis(Transform associatedTransform, bool isWorldSpace = true)
        {
            if (m_PokeThresholdData == null || associatedTransform == null)
                return Vector3.zero;

            Vector3 rotatedDepthEvaluationAxis = Vector3.zero;
            switch (m_PokeThresholdData.pokeDirection)
            {
                case PokeAxis.X:
                case PokeAxis.NegativeX:
                    rotatedDepthEvaluationAxis = isWorldSpace ? associatedTransform.right : Vector3.right;
                    break;
                case PokeAxis.Y:
                case PokeAxis.NegativeY:
                    rotatedDepthEvaluationAxis = isWorldSpace ? associatedTransform.up : Vector3.up;
                    break;
                case PokeAxis.Z:
                case PokeAxis.NegativeZ:
                    rotatedDepthEvaluationAxis = isWorldSpace ? associatedTransform.forward : Vector3.forward;
                    break;
            }

            switch (m_PokeThresholdData.pokeDirection)
            {
                case PokeAxis.X:
                case PokeAxis.Y:
                case PokeAxis.Z:
                    rotatedDepthEvaluationAxis = -rotatedDepthEvaluationAxis;
                    break;
            }

            return rotatedDepthEvaluationAxis;
        }

        float ComputeInteractionAxisLength(Bounds bounds)
        {
            if (m_PokeThresholdData == null || m_InitialTransform == null)
                return 0f;

            Vector3 boundsSize = bounds.size;

            Vector3 center = m_InitialTransform.position;

            float lengthOfInteractionAxis = 0f;
            float centerOffsetLength;

            switch (m_PokeThresholdData.pokeDirection)
            {
                case PokeAxis.X:
                case PokeAxis.NegativeX:
                    centerOffsetLength = bounds.center.x - center.x;
                    lengthOfInteractionAxis = boundsSize.x / 2f + centerOffsetLength;
                    break;
                case PokeAxis.Y:
                case PokeAxis.NegativeY:
                    centerOffsetLength = bounds.center.y - center.y;
                    lengthOfInteractionAxis = boundsSize.y / 2f + centerOffsetLength;
                    break;
                case PokeAxis.Z:
                case PokeAxis.NegativeZ:
                    centerOffsetLength = bounds.center.z - center.z;
                    lengthOfInteractionAxis = boundsSize.z / 2f + centerOffsetLength;
                    break;
            }

            return lengthOfInteractionAxis;
        }

        /// <summary>
        /// Logic for caching pose when an <see cref="IXRInteractor"/> enters a hover state.
        /// </summary>
        /// <param name="interactor">The XR Interactor associated with the hover enter event interaction.</param>
        /// <param name="updatedPose">The pose of the interactor's attach transform, in world space.</param>
        /// <param name="pokedTransform">The transform of the poked object. Mainly considered for UGUI work where poke logic is shared between multiple transforms.</param>
        public void OnHoverEntered(object interactor, Pose updatedPose, Transform pokedTransform)
        {
            m_LastHoveredTransform[interactor] = pokedTransform;
            
            // Store hovered point in local space relative to the poked transform in case the transform moves.
            m_LastHoverEnterLocalPosition[interactor] = pokedTransform.InverseTransformPoint(updatedPose.position);
            
            m_LastInteractorPressDepth[interactor] = 1f;
            m_HoldingHoverCheck[interactor] = false;

            if (!m_HoveredInteractorsOnThisTransform.TryGetValue(pokedTransform, out var hoveringInteractors))
            {
                hoveringInteractors = new HashSetList<object>();
                m_HoveredInteractorsOnThisTransform[pokedTransform] = hoveringInteractors;
            }

            hoveringInteractors.Add(interactor);
        }

        /// <summary>
        /// Logic to update poke state data when interaction terminates.
        /// </summary>
        /// <param name="interactor">The XR Interactor associated with the hover exit event interaction.</param>
        public void OnHoverExited(object interactor)
        {
            m_LastHoverEnterLocalPosition.Remove(interactor);
            m_HoldingHoverCheck[interactor] = false;
            m_LastInteractorPressDepth[interactor] = 1f;

            if (m_LastHoveredTransform.TryGetValue(interactor, out var lastTransform))
            {
                if (m_HoveredInteractorsOnThisTransform.TryGetValue(lastTransform, out var hoveringInteractors))
                    hoveringInteractors.Remove(interactor);
                
                ResetPokeStateData(lastTransform);
                m_LastHoveredTransform.Remove(interactor);
            }
            if (m_LastHoverEnterLocalPosition.Count == 0)
            {
                ResetPokeStateData(m_InitialTransform);
            }
        }

        void ResetPokeStateData(Transform transform)
        {
            if (transform == null)
                return;

            var startPos = transform.position;
            var axisNormal = ComputeRotatedDepthEvaluationAxis(transform);
            var axisExtent = startPos + axisNormal * interactionAxisLength;

            m_PokeStateData.Value = new PokeStateData
            {
                meetsRequirements = false,
                pokeInteractionPoint = axisExtent,
                axisAlignedPokeInteractionPoint = axisExtent,
                interactionStrength = 0f,
                axisNormal = Vector3.zero,
                target = null,
            };
        }

        static Bounds ComputeBounds(Collider targetCollider, bool rotateBoundsScale = false, Space targetSpace = Space.World)
        {
            Bounds newBounds = default;
            if (targetCollider is BoxCollider boxCollider)
            {
                newBounds = new Bounds(boxCollider.center, boxCollider.size);
            }
            else if (targetCollider is SphereCollider sphereCollider)
            {
                newBounds = new Bounds(sphereCollider.center, Vector3.one * (sphereCollider.radius * 2));
            }
            else if (targetCollider is CapsuleCollider capsuleCollider)
            {
                Vector3 targetSize = Vector3.zero;
                float diameter = capsuleCollider.radius * 2f;
                float fullHeight = capsuleCollider.height;
                switch (capsuleCollider.direction)
                {
                    // X
                    case 0:
                        targetSize = new Vector3(fullHeight, diameter, diameter);
                        break;
                    // Y
                    case 1:
                        targetSize = new Vector3(diameter, fullHeight, diameter);
                        break;
                    // Z
                    case 2:
                        targetSize = new Vector3(diameter, diameter, fullHeight);
                        break;
                }

                newBounds = new Bounds(capsuleCollider.center, targetSize);
            }

            if (targetSpace == Space.Self)
                return newBounds;

            return BoundsLocalToWorld(newBounds, targetCollider.transform, rotateBoundsScale);
        }

        static Bounds BoundsLocalToWorld(Bounds targetBounds, Transform targetTransform, bool rotateBoundsScale = false)
        {
            Vector3 localScale = targetTransform.localScale;
            Vector3 adjustedSize = localScale.Multiply(targetBounds.size);
            Vector3 rotatedSize = rotateBoundsScale ? targetTransform.rotation * adjustedSize : adjustedSize;
            return new Bounds(targetTransform.position + localScale.Multiply(targetBounds.center), rotatedSize);
        }

        /// <summary>
        /// Logic for drawing gizmos in the editor that visualize the collider bounds and vector through which a poke
        /// interaction will be evaluated for interactables that support poke.
        /// </summary>
        public void DrawGizmos()
        {
            if (m_PokeThresholdData == null || m_InitialTransform == null)
                return;

            Vector3 interactionOrigin = m_InitialTransform.position;
            var interactionNormal = ComputeRotatedDepthEvaluationAxis(m_InitialTransform);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(interactionOrigin, interactionOrigin + interactionNormal * interactionAxisLength);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(interactionOrigin, interactionOrigin + interactionNormal * m_PokeThresholdData.interactionDepthOffset);

            if (m_PokeStateData != null && m_PokeStateData.Value.interactionStrength > 0f)
            {
                Gizmos.color = m_PokeStateData.Value.meetsRequirements ? Color.green : Color.yellow;
                Gizmos.DrawWireSphere(m_PokeStateData.Value.pokeInteractionPoint, 0.01f);
                Gizmos.DrawWireSphere(m_PokeStateData.Value.axisAlignedPokeInteractionPoint, 0.01f);
            }
        }
    }
}