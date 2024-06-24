namespace UnityEngine.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Attribute used to mark the <c>m_Enabled</c> serialized field of XR Target Evaluators to allow its
    /// <see cref="XRTargetEvaluator.OnEnable"/> and <see cref="XRTargetEvaluator.OnDisable"/> methods to be invoked at runtime
    /// when toggled in the Inspector window.
    /// </summary>
    /// <seealso cref="XRTargetEvaluator"/>
    class XRTargetEvaluatorEnabledAttribute : PropertyAttribute
    {
    }
}
