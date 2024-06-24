using Unity.Mathematics;
using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives
{
    /// <summary>
    /// Affordance receiver applying a Vector4 (Float4) affordance theme as Quaternion.
    /// Broadcasts new affordance value with Unity Event.
    /// </summary>
    [AddComponentMenu("Affordance System/Receiver/Primitives/Quaternion Affordance Receiver", 12)]
    [HelpURL(XRHelpURLConstants.k_QuaternionAffordanceReceiver)]
    public class QuaternionAffordanceReceiver : Vector4AffordanceReceiver
    {
        [SerializeField]
        [Tooltip("The event that is called when the current affordance value is updated, expressed as a quaternion.")]
        QuaternionUnityEvent m_QuaternionValueUpdated;

        /// <summary>
        /// The event that is called when the current affordance value is updated,
        /// expressed as a <see cref="Quaternion"/>.
        /// </summary>
        /// <seealso cref="Vector4AffordanceReceiver.valueUpdated"/>
        public QuaternionUnityEvent quaternionValueUpdated
        {
            get => m_QuaternionValueUpdated;
            set => m_QuaternionValueUpdated = value;
        }

        /// <inheritdoc/>
        protected override void OnAffordanceValueUpdated(float4 newValue)
        {
            base.OnAffordanceValueUpdated(newValue);
            m_QuaternionValueUpdated?.Invoke(new Quaternion(newValue.x, newValue.y, newValue.z, newValue.w));
        }
    }
}