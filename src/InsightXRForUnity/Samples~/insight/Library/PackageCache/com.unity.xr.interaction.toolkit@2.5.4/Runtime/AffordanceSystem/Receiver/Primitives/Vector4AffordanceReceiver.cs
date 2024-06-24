using Unity.Jobs;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives
{
    /// <summary>
    /// Affordance receiver applying a Vector4 (Float4) affordance theme.
    /// Broadcasts new affordance value with Unity Event.
    /// </summary>
    [AddComponentMenu("Affordance System/Receiver/Primitives/Vector4 Affordance Receiver", 12)]
    [HelpURL(XRHelpURLConstants.k_Vector4AffordanceReceiver)]
    public class Vector4AffordanceReceiver : BaseAsyncAffordanceStateReceiver<float4>
    {
        [SerializeField]
        [Tooltip("Vector4 Affordance Theme datum property used to map affordance state to a Vector4 affordance value. Can store an asset or a serialized value.")]
        Vector4AffordanceThemeDatumProperty m_AffordanceThemeDatum;

        /// <summary>
        /// Affordance theme datum property used as a template for creating the runtime copy used during initialization.
        /// </summary>
        /// <remarks>
        /// This value can be bypassed by directly setting the <see cref="BaseAffordanceStateReceiver{T}.affordanceTheme"/> at runtime.
        /// </remarks>
        /// <seealso cref="defaultAffordanceTheme"/>
        public Vector4AffordanceThemeDatumProperty affordanceThemeDatum
        {
            get => m_AffordanceThemeDatum;
            set => m_AffordanceThemeDatum = value;
        }

        [SerializeField]
        [Tooltip("The event that is called when the current affordance value is updated.")]
        Vector4UnityEvent m_ValueUpdated;

        /// <summary>
        /// The event that is called when the current affordance value is updated.
        /// </summary>
        public Vector4UnityEvent valueUpdated
        {
            get => m_ValueUpdated;
            set => m_ValueUpdated = value;
        }

        /// <inheritdoc/>
        protected override BaseAffordanceTheme<float4> defaultAffordanceTheme => m_AffordanceThemeDatum != null ? m_AffordanceThemeDatum.Value : null;

        /// <inheritdoc/>
        protected override BindableVariable<float4> affordanceValue { get; } = new BindableVariable<float4>();

        /// <inheritdoc/>
        protected override JobHandle ScheduleTweenJob(ref TweenJobData<float4> jobData)
        {
            var job = new Float4TweenJob { jobData = jobData };
            return job.Schedule();
        }

        /// <inheritdoc/>
        protected override BaseAffordanceTheme<float4> GenerateNewAffordanceThemeInstance()
        {
            return new Vector4AffordanceTheme();
        }

        /// <inheritdoc/>
        protected override void OnAffordanceValueUpdated(float4 newValue)
        {
            m_ValueUpdated?.Invoke(newValue);
        }
    }
}