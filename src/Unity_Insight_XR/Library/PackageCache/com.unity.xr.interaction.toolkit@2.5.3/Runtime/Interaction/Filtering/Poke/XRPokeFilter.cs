using System.Diagnostics;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Filter component that allows for basic poke functionality
    /// and to define constraints for when the interactable will be selected.
    /// </summary>
    [AddComponentMenu("XR/XR Poke Filter", 11)]
    [HelpURL(XRHelpURLConstants.k_XRPokeFilter)]
    public class XRPokeFilter : MonoBehaviour, IXRPokeFilter, IPokeStateDataProvider
    {
        [SerializeField]
        [Tooltip("The interactable associated with this poke filter.")]
        XRBaseInteractable m_Interactable;

        /// <summary>
        /// The <see cref="XRBaseInteractable"/> associated with this poke filter.
        /// </summary>
        public XRBaseInteractable pokeInteractable
        {
            get => m_Interactable;
            set
            {
                m_Interactable = value;
                Setup();
            }
        }

        [SerializeField]
        [Tooltip("The collider used to compute bounds of the poke interaction.")]
        Collider m_PokeCollider;

        /// <summary>
        /// The <see cref="Collider"/> used to compute bounds of the poke interaction.
        /// </summary>
        public Collider pokeCollider
        {
            get => m_PokeCollider;
            set
            {
                m_PokeCollider = value;
                Setup();
            }
        }

        [SerializeField]
        [Tooltip("The settings used to fine tune the vector and offsets which dictate how the poke interaction will be evaluated.")]
        PokeThresholdDatumProperty m_PokeConfiguration = new PokeThresholdDatumProperty(new PokeThresholdData());

        /// <summary>
        /// The settings used to fine tune the vector and offsets which dictate how the poke interaction will be evaluated.
        /// </summary>
        public PokeThresholdDatumProperty pokeConfiguration
        {
            get => m_PokeConfiguration;
            set
            {
                m_PokeConfiguration = value;
                Setup();
            }
        }

        /// <inheritdoc />
        public IReadOnlyBindableVariable<PokeStateData> pokeStateData => m_PokeLogic?.pokeStateData;

        /// <summary>
        /// Whether this poke filter can process interactions.
        /// </summary>
        /// <seealso cref="IXRSelectFilter.canProcess"/>
        /// <seealso cref="IXRInteractionStrengthFilter.canProcess"/>
        public virtual bool canProcess => isActiveAndEnabled && m_PokeLogic != null;

        XRPokeLogic m_PokeLogic = new XRPokeLogic();

        readonly BindingsGroup m_BindingsGroup = new BindingsGroup();

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        protected virtual void Reset()
        {
#if UNITY_EDITOR
            m_Interactable = FindPokeInteractable();
            m_PokeCollider = FindPokeCollider();
#endif
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        protected void OnValidate()
        {
#if UNITY_EDITOR
            Setup();
#endif
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            if (m_Interactable == null)
            {
                m_Interactable = FindPokeInteractable();
                if (m_Interactable == null)
                {
                    Debug.LogWarning($"Could not find associated {nameof(XRBaseInteractable)} in scene." +
                        $"This {nameof(XRPokeFilter)} will be disabled.", this);
                    enabled = false;
                }
            }

            if (m_PokeCollider == null)
            {
                m_PokeCollider = FindPokeCollider();
                if (m_PokeCollider == null)
                {
                    Debug.LogWarning($"Could not find a {nameof(Collider)} associated with this filter in the scene." +
                        $"This {nameof(XRPokeFilter)} will be disabled.", this);
                    enabled = false;
                }
            }

            if (m_PokeConfiguration.Value == null)
            {
                Debug.LogWarning("Poke Data property has been improperly configured. Please assign a Poke Threshold Datum asset if configured to Use Asset.", this);
                enabled = false;
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Start()
        {
            Setup();

            if (m_Interactable != null)
            {
                m_Interactable.selectFilters.Add(this);
                m_Interactable.interactionStrengthFilters.Add(this);
                SetupBindings();
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDestroy()
        {
            if (m_Interactable != null)
            {
                m_Interactable.selectFilters.Remove(this);
                m_Interactable.interactionStrengthFilters.Remove(this);
            }

            ClearBindings();
            m_PokeLogic?.Dispose();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        protected void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (!enabled)
                return;

            if (m_PokeLogic == null)
                Setup();

            m_PokeLogic?.DrawGizmos();
#endif
        }

        void SetupBindings()
        {
            if (m_Interactable != null)
            {
                m_Interactable.hoverEntered.AddListener(OnHoverEntered);
                m_Interactable.hoverExited.AddListener(OnHoverExited);
            }
        }

        void ClearBindings()
        {
            m_BindingsGroup.Clear();

            if (m_Interactable != null)
            {
                m_Interactable.hoverEntered.RemoveListener(OnHoverEntered);
                m_Interactable.hoverExited.RemoveListener(OnHoverExited);
            }
        }

        /// <inheritdoc />
        public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            if (interactor is XRPokeInteractor pokeInteractor)
            {
                var pokeTransform = interactable.GetAttachTransform(interactor);
                return m_PokeLogic.MeetsRequirementsForSelectAction(
                    pokeInteractor,
                    pokeTransform.position,
                    interactor.GetAttachTransform(interactable).position,
                    pokeInteractor.pokeInteractionOffset,
                    pokeTransform);
            }

            return true;
        }

        /// <inheritdoc />
        public float Process(IXRInteractor interactor, IXRInteractable interactable, float interactionStrength)
        {
            var pokeAmount = 0f;
            if (interactor is XRPokeInteractor)
            {
                pokeAmount = pokeStateData?.Value.interactionStrength ?? 0f;
            }

            return Mathf.Max(interactionStrength, pokeAmount);
        }

        void OnHoverEntered(HoverEnterEventArgs args)
        {
            if (m_PokeLogic == null)
                return;

            var interactor = (IXRInteractor)args.interactorObject;
            var interactorAttachTransform = interactor.GetAttachTransform(m_Interactable);
            var interactableAttachTransform = m_Interactable.GetAttachTransform(interactor);
            m_PokeLogic.OnHoverEntered(interactor, interactorAttachTransform.GetWorldPose(), interactableAttachTransform);
        }

        void OnHoverExited(HoverExitEventArgs args)
        {
            if (m_PokeLogic == null)
                return;

            m_PokeLogic.OnHoverExited(args.interactorObject);
        }

        XRBaseInteractable FindPokeInteractable()
        {
            return m_Interactable != null ? m_Interactable : GetComponentInParent<XRBaseInteractable>();
        }

        Collider FindPokeCollider()
        {
            return m_PokeCollider != null ? m_PokeCollider : GetComponentInChildren<Collider>();
        }

        /// <summary>
        /// Method that sets up this filter by initializing the <see cref="XRPokeLogic"/> object, which handles
        /// the poking logic.
        /// </summary>
        void Setup()
        {
            if (m_PokeLogic == null)
                m_PokeLogic = new XRPokeLogic();

            // This method can be called when not in play mode, but it should not modify serialized fields.
            var interactableValue = FindPokeInteractable();
            var colliderValue = FindPokeCollider();
            var thresholdValue = m_PokeConfiguration.Value;
            if (interactableValue != null && colliderValue != null && thresholdValue != null)
            {
                m_PokeLogic.Initialize(interactableValue.GetAttachTransform(null), thresholdValue, colliderValue);
            }
        }
    }
}