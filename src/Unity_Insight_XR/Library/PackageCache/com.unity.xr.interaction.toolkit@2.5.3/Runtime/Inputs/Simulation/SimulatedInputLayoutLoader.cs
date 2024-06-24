#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER) || PACKAGE_DOCS_GENERATION
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Scripting;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation
{
    /// <summary>
    /// This class automatically registers control layouts used by the <see cref="XRDeviceSimulator"/>.
    /// </summary>
    /// <seealso cref="XRSimulatedHMD"/>
    /// <seealso cref="XRSimulatedController"/>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [Preserve]
    public static class SimulatedInputLayoutLoader
    {
        [Preserve]
        static SimulatedInputLayoutLoader()
        {
            RegisterInputLayouts();
        }

        /// <summary>
        /// See <see cref="RuntimeInitializeLoadType.BeforeSceneLoad"/>.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad), Preserve]
        public static void Initialize()
        {
            // Will execute the static constructor as a side effect.
        }

        static void RegisterInputLayouts()
        {
            // See XRDeviceSimulator.AddDevices for product pattern
            InputSystem.InputSystem.RegisterLayout<XRSimulatedHMD>(
                matches: new InputDeviceMatcher()
                    .WithProduct(nameof(XRSimulatedHMD)));
            InputSystem.InputSystem.RegisterLayout<XRSimulatedController>(
                matches: new InputDeviceMatcher()
                    .WithProduct(nameof(XRSimulatedController)));
        }
    }
}
#endif
