using Unity.Jobs;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives
{
    /// <summary>
    /// Affordance receiver applying a Color affordance theme.
    /// Broadcasts new affordance value with Unity Event.
    /// </summary>
    [AddComponentMenu("Affordance System/Receiver/Primitives/Color Affordance Receiver", 12)]
    [HelpURL(XRHelpURLConstants.k_ColorAffordanceReceiver)]
    public class ColorAffordanceReceiver : BaseAsyncAffordanceStateReceiver<Color>
    {
        [SerializeField]
        [Tooltip("Color Affordance Theme datum property used to map affordance state to a color affordance value. Can store an asset or a serialized value.")]
        ColorAffordanceThemeDatumProperty m_AffordanceThemeDatum;

        /// <summary>
        /// Affordance theme datum property used as a template for creating the runtime copy used during initialization.
        /// </summary>
        /// <remarks>
        /// This value can be bypassed by directly setting the <see cref="BaseAffordanceStateReceiver{T}.affordanceTheme"/> at runtime.
        /// </remarks>
        /// <seealso cref="defaultAffordanceTheme"/>
        public ColorAffordanceThemeDatumProperty affordanceThemeDatum
        {
            get => m_AffordanceThemeDatum;
            set => m_AffordanceThemeDatum = value;
        }

        [SerializeField]
        [Tooltip("The event that is called when the current affordance value is updated.")]
        ColorUnityEvent m_ValueUpdated;

        /// <summary>
        /// The event that is called when the current affordance value is updated.
        /// </summary>
        public ColorUnityEvent valueUpdated
        {
            get => m_ValueUpdated;
            set => m_ValueUpdated = value;
        }

        /// <inheritdoc/>
        protected override BaseAffordanceTheme<Color> defaultAffordanceTheme => m_AffordanceThemeDatum != null ? m_AffordanceThemeDatum.Value : null;

        /// <inheritdoc/>
        protected override BindableVariable<Color> affordanceValue { get; } = new BindableVariable<Color>();

        /// <inheritdoc/>
        protected override JobHandle ScheduleTweenJob(ref TweenJobData<Color> jobData)
        {
            var theme = (ColorAffordanceTheme)affordanceTheme;
            var job = new ColorTweenJob
            {
                jobData = jobData,
                colorBlendAmount = theme.blendAmount,
                colorBlendMode = (byte)theme.colorBlendMode
            };
            return job.Schedule();
        }

        /// <inheritdoc/>
        protected override BaseAffordanceTheme<Color> GenerateNewAffordanceThemeInstance()
        {
            return new ColorAffordanceTheme();
        }

        /// <inheritdoc/>
        protected override void OnAffordanceValueUpdated(Color newValue)
        {
            m_ValueUpdated?.Invoke(newValue);
        }
    }
}