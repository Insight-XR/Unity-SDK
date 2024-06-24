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
    /// Affordance receiver applying a Vector3 (Float3) affordance theme.
    /// Broadcasts new affordance value with Unity Event.
    /// </summary>
    [AddComponentMenu("Affordance System/Receiver/Primitives/Vector3 Affordance Receiver", 12)]
    [HelpURL(XRHelpURLConstants.k_Vector3AffordanceReceiver)]
    public class Vector3AffordanceReceiver : BaseAsyncAffordanceStateReceiver<float3>
    {
        [SerializeField]
        [Tooltip("Vector3 Affordance Theme datum property used to map affordance state to a Vector3 affordance value. Can store an asset or a serialized value.")]
        Vector3AffordanceThemeDatumProperty m_AffordanceThemeDatum;

        /// <summary>
        /// Affordance theme datum property used as a template for creating the runtime copy used during initialization.
        /// </summary>
        /// <remarks>
        /// This value can be bypassed by directly setting the <see cref="BaseAffordanceStateReceiver{T}.affordanceTheme"/> at runtime.
        /// </remarks>
        /// <seealso cref="defaultAffordanceTheme"/>
        public Vector3AffordanceThemeDatumProperty affordanceThemeDatum
        {
            get => m_AffordanceThemeDatum;
            set => m_AffordanceThemeDatum = value;
        }

        [SerializeField]
        [Tooltip("The event that is called when the current affordance value is updated.")]
        Vector3UnityEvent m_ValueUpdated;

        /// <summary>
        /// The event that is called when the current affordance value is updated.
        /// </summary>
        public Vector3UnityEvent valueUpdated
        {
            get => m_ValueUpdated;
            set => m_ValueUpdated = value;
        }

        /// <inheritdoc/>
        protected override BaseAffordanceTheme<float3> defaultAffordanceTheme => m_AffordanceThemeDatum != null ? m_AffordanceThemeDatum.Value : null;

        /// <inheritdoc/>
        protected override BindableVariable<float3> affordanceValue { get; } = new BindableVariable<float3>();

        /// <inheritdoc/>
        protected override JobHandle ScheduleTweenJob(ref TweenJobData<float3> jobData)
        {
            var job = new Float3TweenJob { jobData = jobData };
            return job.Schedule();
        }

        /// <inheritdoc/>
        protected override BaseAffordanceTheme<float3> GenerateNewAffordanceThemeInstance()
        {
            return new Vector3AffordanceTheme();
        }

        /// <inheritdoc/>
        protected override void OnAffordanceValueUpdated(float3 newValue)
        {
            m_ValueUpdated?.Invoke(newValue);
        }
    }
}