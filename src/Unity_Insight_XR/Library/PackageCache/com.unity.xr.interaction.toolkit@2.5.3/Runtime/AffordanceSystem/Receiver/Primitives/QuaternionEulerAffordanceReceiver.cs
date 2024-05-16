using Unity.Mathematics;
using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives
{
    /// <summary>
    /// Affordance receiver applying a Vector3 (Float3) affordance theme that is converted to a Quaternion as an euler rotation.
    /// Broadcasts new affordance value with Unity Event.
    /// </summary>
    [AddComponentMenu("Affordance System/Receiver/Primitives/Quaternion Euler Affordance Receiver", 12)]
    [HelpURL(XRHelpURLConstants.k_QuaternionEulerAffordanceReceiver)]
    public class QuaternionEulerAffordanceReceiver : Vector3AffordanceReceiver
    {
        [SerializeField]
        [Tooltip("The event that is called when the current affordance value is updated, expressed as a quaternion " +
                 "generated from euler angles.")]
        QuaternionUnityEvent m_QuaternionValueUpdated;

        /// <summary>
        /// The event that is called when the current affordance value is updated,
        /// expressed as a <see cref="Quaternion"/> generated from euler angles.
        /// </summary>
        /// <seealso cref="Vector3AffordanceReceiver.valueUpdated"/>
        public QuaternionUnityEvent quaternionValueUpdated
        {
            get => m_QuaternionValueUpdated;
            set => m_QuaternionValueUpdated = value;
        }

        /// <inheritdoc/>
        protected override void OnAffordanceValueUpdated(float3 newValue)
        {
            base.OnAffordanceValueUpdated(newValue);
            m_QuaternionValueUpdated?.Invoke(Quaternion.Euler(newValue));
        }
    }
}