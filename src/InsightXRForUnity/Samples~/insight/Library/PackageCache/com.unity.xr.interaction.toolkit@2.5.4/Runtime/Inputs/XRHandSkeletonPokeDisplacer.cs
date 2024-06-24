using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;
#if XR_HANDS_1_3_OR_NEWER
using Unity.Mathematics;
using Unity.XR.CoreUtils.Bindings;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;
#endif

namespace UnityEngine.XR.Interaction.Toolkit.Inputs
{
    /// <summary>
    /// Class used to displace the root pose of a hand skeleton based on a poke interaction to enable freezing
    /// the poke pose in place when pressing poke buttons or UI elements. It will help prevent the hand mesh visual
    /// from moving through buttons and UI that can be poked.
    /// </summary>
    /// <remarks>
    /// This component requires the XR Hands (com.unity.xr.hands) package version 1.3.0 or newer to be installed in your project.
    /// <br />
    /// This component is typically added to the Left and Right Hand Interaction Visual GameObjects that has the
    /// XR Hand Skeleton Driver component, and references the XR Poke Interactor for that hand.
    /// </remarks>
#if XR_HANDS_1_3_OR_NEWER
    [RequireComponent(typeof(XRHandSkeletonDriver))]
#endif
    [AddComponentMenu("XR/XR Hand Skeleton Poke Displacer", 11)]
    [HelpURL(XRHelpURLConstants.k_XRHandSkeletonPokeDisplacer)]
    public class XRHandSkeletonPokeDisplacer : MonoBehaviour
    {
        const float k_MinSmoothingAmount = 0f;
        const float k_MaxSmoothingAmount = 30f;

        [SerializeField]
        [RequireInterface(typeof(IPokeStateDataProvider))]
        [Tooltip("Poke interactor reference used to get poke data.")]
        Object m_PokeInteractorObject;

        IPokeStateDataProvider m_PokeInteractor;

        /// <summary>
        /// Poke interactor reference used to get poke data.
        /// </summary>
        public IPokeStateDataProvider pokeInteractor
        {
            get => m_PokeInteractor;
            set
            {
                m_PokeInteractorObject = value as Object;
                m_PokeInteractor = value;
#if XR_HANDS_1_3_OR_NEWER
                if (Application.isPlaying && isActiveAndEnabled)
                    BindToPokeInteractor();
#endif
            }
        }

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Threshold poke interaction strength must be above to snap the poke pose to the current pose.")]
        float m_PokeStrengthSnapThreshold = 0.01f;

        /// <summary>
        /// Threshold poke interaction strength must be above to snap the poke pose to the current pose.
        /// </summary>
        public float pokeStrengthSnapThreshold
        {
            get => m_PokeStrengthSnapThreshold;
            set => m_PokeStrengthSnapThreshold = Mathf.Clamp01(value);
        }

        [SerializeField]
        [Range(k_MinSmoothingAmount, k_MaxSmoothingAmount)]
        [Tooltip("Smoothing to apply to the offset root. If smoothing amount is 0, no smoothing will be applied.")]
        float m_SmoothingAmount = 16f;

        /// <summary>
        /// Smoothing to apply to the offset root. If smoothing amount is 0, no smoothing will be applied.
        /// </summary>
        public float smoothingAmount
        {
            get => m_SmoothingAmount;
            set => m_SmoothingAmount = Mathf.Clamp(value, k_MinSmoothingAmount, k_MaxSmoothingAmount);
        }

        [SerializeField]
        [Tooltip("Additional offset subtracted along the poke interaction axis to apply to the root pose when poking. Default value accounts for the width of the finger mesh.")]
        float m_FixedOffset = 0.005f;

        /// <summary>
        /// Additional offset subtracted along the poke interaction axis to apply to the root pose when poking. Default value accounts for the width of the finger mesh.
        /// </summary>
        public float fixedOffset
        {
            get => m_FixedOffset;
            set => m_FixedOffset = value;
        }

#if XR_HANDS_1_3_OR_NEWER
        XRHandSkeletonDriver m_SkeletonDriver;
        readonly BindingsGroup m_BindingsGroup = new BindingsGroup();
        readonly Vector3TweenableVariable m_PokeOffset = new Vector3TweenableVariable();
#endif

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            if (m_PokeInteractor == null)
                m_PokeInteractor = m_PokeInteractorObject as IPokeStateDataProvider;

#if XR_HANDS_1_3_OR_NEWER
            m_SkeletonDriver = GetComponent<XRHandSkeletonDriver>();
#else
            Debug.LogWarning("XRHandSkeletonPokeDisplacer requires XR Hands (com.unity.xr.hands) 1.3.0 or newer. Disabling component.", this);
            enabled = false;
#endif
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            if (m_PokeInteractor == null)
                m_PokeInteractor = m_PokeInteractorObject as IPokeStateDataProvider;

#if XR_HANDS_1_3_OR_NEWER
            BindToPokeInteractor();
#endif
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
#if XR_HANDS_1_3_OR_NEWER
            m_BindingsGroup.Clear();
#endif
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Update()
        {
#if XR_HANDS_1_3_OR_NEWER
            if (m_SmoothingAmount > 0f)
                m_PokeOffset.HandleTween(Time.deltaTime * m_SmoothingAmount);
#endif
        }

#if XR_HANDS_1_3_OR_NEWER
        void BindToPokeInteractor()
        {
            m_BindingsGroup.Clear();
            if (m_PokeInteractor == null)
            {
                Debug.LogWarning($"XRHandSkeletonPokeDisplacer requires a poke data provider to be set. Disabling {this} on {gameObject.name}.", this);
                enabled = false;
                return;
            }

            m_BindingsGroup.AddBinding(m_PokeInteractor.pokeStateData.SubscribeAndUpdate(OnPokeDataUpdated));
            m_BindingsGroup.AddBinding(m_PokeOffset.Subscribe(newOffset =>
            {
                if (newOffset.Equals(float3.zero))
                    m_SkeletonDriver.ResetRootPoseOffset();
                else
                    m_SkeletonDriver.ApplyRootPoseOffset(newOffset);
            }));
        }

        void OnPokeDataUpdated(PokeStateData pokeStateData)
        {
            var empty = pokeStateData.Equals(default);
            if (empty || pokeStateData.interactionStrength < m_PokeStrengthSnapThreshold)
            {
                if (m_SmoothingAmount > 0f)
                    m_PokeOffset.target = float3.zero;
                else
                    m_SkeletonDriver.ResetRootPoseOffset();
            }
            else
            {
                var referencePoint = pokeStateData.pokeInteractionPoint;

                // Apply offset along the poke axis to account for the width of the finger mesh
                if (m_FixedOffset > 0f)
                    referencePoint -= m_FixedOffset * pokeStateData.axisNormal;

                var currentPokeSurfacePoint = pokeStateData.axisAlignedPokeInteractionPoint;
                var target = Vector3.Project(currentPokeSurfacePoint - referencePoint, pokeStateData.axisNormal);
                if (m_SmoothingAmount > 0f)
                    m_PokeOffset.target = target;
                else
                    m_SkeletonDriver.ApplyRootPoseOffset(target);
            }
        }
#endif
    }
}
