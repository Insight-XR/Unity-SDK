namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Hands
{
    /// <summary>
    /// Stores the joints for a hand expression for both a left and right hand. The pose data is used to simulate a specific hand expression
    /// in the XR Device Simulator.
    /// </summary>
    class HandExpressionCapture : ScriptableObject
    {
        [SerializeField]
        [Tooltip("An icon to represent the hand expression.")]
        Sprite m_Icon;

        /// <summary>
        /// The icon to represent the hand expression.
        /// </summary>
        public Sprite icon
        {
            get => m_Icon;
            set => m_Icon = value;
        }

        [SerializeField]
        [Tooltip("The captured left hand joint poses.")]
        Pose[] m_LeftCapturedPoses;

        /// <summary>
        /// The captured poses of the left hand that is serialized to the <see cref="ScriptableObject"/> asset.
        /// </summary>
        public Pose[] leftHandCapturedPoses
        {
            get => m_LeftCapturedPoses;
            set => m_LeftCapturedPoses = value;
        }

        [SerializeField]
        [Tooltip("The captured right hand joint poses.")]
        Pose[] m_RightCapturedPoses;

        /// <summary>
        /// The captured poses of the right hand that is serialized to the <see cref="ScriptableObject"/> asset.
        /// </summary>
        public Pose[] rightHandCapturedPoses
        {
            get => m_RightCapturedPoses;
            set => m_RightCapturedPoses = value;
        }
    }
}
