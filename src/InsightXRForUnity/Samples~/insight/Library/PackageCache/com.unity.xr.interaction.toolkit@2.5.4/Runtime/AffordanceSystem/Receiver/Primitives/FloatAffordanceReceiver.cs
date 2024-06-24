using Unity.Jobs;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives
{
    /// <summary>
    /// Affordance receiver applying a Float affordance theme.
    /// Broadcasts new affordance value with Unity Event.
    /// </summary>
    [AddComponentMenu("Affordance System/Receiver/Primitives/Float Affordance Receiver", 12)]
    [HelpURL(XRHelpURLConstants.k_FloatAffordanceReceiver)]
    public class FloatAffordanceReceiver : BaseAsyncAffordanceStateReceiver<float>
    {
        [SerializeField]
        [Tooltip("Float Affordance Theme datum property used to map affordance state to a float affordance value. Can store an asset or a serialized value.")]
        FloatAffordanceThemeDatumProperty m_AffordanceThemeDatum;

        /// <summary>
        /// Affordance theme datum property used as a template for creating the runtime copy used during initialization.
        /// </summary>
        /// <remarks>
        /// This value can be bypassed by directly setting the <see cref="BaseAffordanceStateReceiver{T}.affordanceTheme"/> at runtime.
        /// </remarks>
        /// <seealso cref="defaultAffordanceTheme"/>
        public FloatAffordanceThemeDatumProperty affordanceThemeDatum
        {
            get => m_AffordanceThemeDatum;
            set => m_AffordanceThemeDatum = value;
        }

        [SerializeField]
        [Tooltip("The event that is called when the current affordance value is updated.")]
        FloatUnityEvent m_ValueUpdated;

        /// <summary>
        /// The event that is called when the current affordance value is updated.
        /// </summary>
        public FloatUnityEvent valueUpdated
        {
            get => m_ValueUpdated;
            set => m_ValueUpdated = value;
        }

        /// <inheritdoc/>
        protected override BaseAffordanceTheme<float> defaultAffordanceTheme => m_AffordanceThemeDatum != null ? m_AffordanceThemeDatum.Value : null;

        /// <inheritdoc/>
        protected override BindableVariable<float> affordanceValue { get; } = new BindableVariable<float>();

        /// <inheritdoc/>
        protected override JobHandle ScheduleTweenJob(ref TweenJobData<float> jobData)
        {
            var job = new FloatTweenJob { jobData = jobData };
            return job.Schedule();
        }

        /// <inheritdoc/>
        protected override BaseAffordanceTheme<float> GenerateNewAffordanceThemeInstance()
        {
            return new FloatAffordanceTheme();
        }

        /// <inheritdoc/>
        protected override void OnAffordanceValueUpdated(float newValue)
        {
            m_ValueUpdated?.Invoke(newValue);
        }
    }
}