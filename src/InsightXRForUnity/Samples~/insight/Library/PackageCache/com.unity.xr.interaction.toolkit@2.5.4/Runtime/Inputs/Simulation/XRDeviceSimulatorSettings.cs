using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation
{
    /// <summary>
    /// Configuration class for XR Device Simulator which
    /// stores settings related to automatic instantiation.
    /// </summary>
    [ScriptableSettingsPath(ProjectPath.k_XRInteractionSettingsFolder)]
    class XRDeviceSimulatorSettings : ScriptableSettings<XRDeviceSimulatorSettings>
    {
        [SerializeField]
        bool m_AutomaticallyInstantiateSimulatorPrefab;

        /// <summary>
        /// Setting this value to <see langword="true"/> will tell the <see cref="XRDeviceSimulatorLoader"/> to look for and automatically
        /// add the <see cref="simulatorPrefab"/> to the current scene if it does not already exist.
        /// </summary>
        internal bool automaticallyInstantiateSimulatorPrefab
        {
            get => m_AutomaticallyInstantiateSimulatorPrefab;
            set => m_AutomaticallyInstantiateSimulatorPrefab = value;
        }

        [SerializeField]
        bool m_AutomaticallyInstantiateInEditorOnly = true;

        /// <summary>
        /// Enable to only automatically instantiate the <see cref="simulatorPrefab"/> if the application is running inside the Unity Editor,
        /// preventing it from automatically appearing in standalone builds. Disable to allow the simulator to be created in standalone builds.
        /// </summary>
        /// <remarks>
        /// Setting this value to <see langword="true"/> will limit the <see cref="XRDeviceSimulatorLoader"/> to
        /// only automatically instantiate the <see cref="simulatorPrefab"/> if the application is running inside the Unity Editor.
        /// This property is only used if <see cref="automaticallyInstantiateSimulatorPrefab"/> is enabled.
        /// </remarks>
        internal bool automaticallyInstantiateInEditorOnly
        {
            get => m_AutomaticallyInstantiateInEditorOnly;
            set => m_AutomaticallyInstantiateInEditorOnly = value;
        }

        [SerializeField]
        GameObject m_SimulatorPrefab;

        /// <summary>
        /// This is the prefab to instantiate when <see cref="automaticallyInstantiateSimulatorPrefab"/> is set to <see langword="true"/>.
        /// </summary>
        internal GameObject simulatorPrefab
        {
            get => m_SimulatorPrefab;
            set => m_SimulatorPrefab = value;
        }

    }
}
