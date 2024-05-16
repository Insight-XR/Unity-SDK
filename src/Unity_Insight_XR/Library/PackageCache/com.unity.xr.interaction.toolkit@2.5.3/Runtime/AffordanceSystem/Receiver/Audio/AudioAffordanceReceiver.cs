using System.Text;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Audio;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Audio
{
    /// <summary>
    /// Audio affordance receiver. Requires an Audio Source and plays audio clips stored in the Audio Affordance Theme Datum.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("Affordance System/Receiver/Audio/Audio Affordance Receiver", 12)]
    [HelpURL(XRHelpURLConstants.k_AudioAffordanceReceiver)]
    public class AudioAffordanceReceiver : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Affordance state provider component to subscribe to.")]
        BaseAffordanceStateProvider m_AffordanceStateProvider;

        /// <summary>
        /// Affordance state provider component to subscribe to.
        /// </summary>
        public BaseAffordanceStateProvider affordanceStateProvider
        {
            get => m_AffordanceStateProvider;
            set => m_AffordanceStateProvider = value;
        }

        [SerializeField]
        [Tooltip("Audio Affordance Theme datum property used to map affordance state to a Audio affordance value. Can store an asset or a serialized value.")]
        AudioAffordanceThemeDatumProperty m_AffordanceThemeDatum;

        /// <summary>
        /// Affordance theme datum property used as a template for creating the runtime copy used during initialization.
        /// </summary>
        public AudioAffordanceThemeDatumProperty affordanceThemeDatum
        {
            get => m_AffordanceThemeDatum;
            set => m_AffordanceThemeDatum = value;
        }

        [SerializeField]
        [Tooltip("Audio Source where the audio clip will be played.")]
        AudioSource m_AudioSource;

        /// <summary>
        /// Audio Source where the audio clip will be played.
        /// </summary>
        public AudioSource audioSource
        {
            get => m_AudioSource;
            set => m_AudioSource = value;
        }

        readonly BindingsGroup m_BindingsGroup = new BindingsGroup();

        byte m_LastAffordanceStateIndex;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnValidate()
        {
            if (m_AudioSource == null)
                m_AudioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            if (m_AudioSource == null)
                m_AudioSource = GetComponent<AudioSource>();

            if (m_AffordanceThemeDatum != null && m_AffordanceThemeDatum.Value != null)
            {
                m_AffordanceThemeDatum.Value.ValidateTheme();
                LogIfMissingAffordanceStates(m_AffordanceThemeDatum.Value);
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            if (m_AffordanceStateProvider == null)
            {
                XRLoggingUtils.LogError($"Missing Affordance State Provider reference. Please set one on {this}.", this);
                enabled = false;
                return;
            }

            if (m_AffordanceThemeDatum == null || m_AffordanceThemeDatum.Value == null)
            {
                XRLoggingUtils.LogError($"Missing Audio Affordance Theme Datum on {this}.", this);
                enabled = false;
                return;
            }

            m_BindingsGroup.AddBinding(m_AffordanceStateProvider.currentAffordanceStateData.Subscribe(OnAffordanceStateUpdated));
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            m_BindingsGroup.Clear();
        }

        void LogIfMissingAffordanceStates(AudioAffordanceTheme theme)
        {
            if (theme.GetAffordanceThemeDataForIndex((byte)(AffordanceStateShortcuts.stateCount - 1)) == null)
            {
                var sb = new StringBuilder();
                var actualCount = 0;
                for (byte index = 0; index < AffordanceStateShortcuts.stateCount; ++index)
                {
                    var themeData = theme.GetAffordanceThemeDataForIndex(index);
                    sb.Append($"Expected: {index} \"{AffordanceStateShortcuts.GetNameForIndex(index)}\",\tActual: ");
                    sb.AppendLine(themeData != null ? $"{index} \"{themeData.stateName}\"" : "<b>(None)</b>");

                    if (themeData != null)
                        ++actualCount;
                }

                Debug.LogWarning("Affordance Theme on affordance receiver is missing a potential affordance state. Expected states:" +
                    $"\nExpected Count: {AffordanceStateShortcuts.stateCount}, Actual Count: {actualCount}" +
                    $"\n{sb}", this);
            }
        }

        void OnAffordanceStateUpdated(AffordanceStateData affordanceStateData)
        {
            var newIndex = affordanceStateData.stateIndex;
            if (newIndex != m_LastAffordanceStateIndex)
            {
                bool newStateIsActivate = newIndex == AffordanceStateShortcuts.activated;
                bool newStateIsHover = newIndex == AffordanceStateShortcuts.hovered;
                bool newStateIsSelect = newIndex == AffordanceStateShortcuts.selected;

                bool lastStateIsSelect = m_LastAffordanceStateIndex == AffordanceStateShortcuts.selected;
                bool lastStateIsActivate = m_LastAffordanceStateIndex == AffordanceStateShortcuts.activated;
                
                bool selectToActivate = newStateIsActivate && lastStateIsSelect;
                bool activateToSelect = newStateIsSelect && lastStateIsActivate;
                bool hoverToSelect = newStateIsHover && lastStateIsSelect;
                bool selectToHover = newStateIsHover && lastStateIsSelect;

                // Do not play select exit if going to activated state because it is a modifier state.
                // Likewise, do not play hover exit if going to selected state because it is a modifier state.
                if (!selectToActivate && !hoverToSelect)
                {
                    var exitData = m_AffordanceThemeDatum.Value?.GetAffordanceThemeDataForIndex(m_LastAffordanceStateIndex);
                    if (exitData != null)
                    {
                        PlayAudioClip(exitData.stateExited);
                    }
                }
                
                // Do not play select enter if coming from activated state because it is a modifier state.
                // Likewise, do not play hover enter if coming from selected state because it is a modifier state.
                if (!activateToSelect && !selectToHover)
                {
                    var enterData = m_AffordanceThemeDatum.Value?.GetAffordanceThemeDataForIndex(newIndex);
                    if (enterData != null)
                    {
                        PlayAudioClip(enterData.stateEntered);
                    }
                    else
                    {
                        var stateName = AffordanceStateShortcuts.GetNameForIndex(newIndex);
                        XRLoggingUtils.LogError($"Missing theme data for affordance state index {newIndex} \"{stateName}\" with {this}.", this);
                    }
                }
                
                m_LastAffordanceStateIndex = newIndex;
            }
        }

        void PlayAudioClip(AudioClip clipToPlay)
        {
            if (clipToPlay == null)
                return;
            m_AudioSource.PlayOneShot(clipToPlay);
        }
    }
}