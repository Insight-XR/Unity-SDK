#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER) || PACKAGE_DOCS_GENERATION
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation
{
    /// <summary>
    /// This class instantiates the <see cref="XRDeviceSimulator"/> in the scene depending on
    /// project settings.
    /// </summary>
    [Preserve]
    public static class XRDeviceSimulatorLoader
    {
        /// <summary>
        /// See <see cref="RuntimeInitializeLoadType.AfterSceneLoad"/>.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad), Preserve]
        public static void Initialize()
        {
            // Will execute the static constructor as a side effect.
        }

        [Preserve]
        static XRDeviceSimulatorLoader()
        {
            if (!XRDeviceSimulatorSettings.Instance.automaticallyInstantiateSimulatorPrefab ||
                (XRDeviceSimulatorSettings.Instance.automaticallyInstantiateInEditorOnly && !Application.isEditor))
                return;

#if UNITY_INCLUDE_TESTS
            // For a consistent test environment, do not instantiate the simulator when running tests.
            // The XR Device Simulator will need to be explicitly added during a test if it is used for testing.
            // Additionally, as of Input System 1.4.4, the InputState.Change call in XRDeviceSimulator.Update causes
            // a NullReferenceException deep in the stack trace if running during tests.
            // The test runner will create a scene named "InitTestScene{DateTime.Now.Ticks}.unity".
            var scene = SceneManager.GetActiveScene();
            var isUnityTest = scene.IsValid() && scene.name.StartsWith("InitTestScene");
            if (isUnityTest)
            {
                Debug.Log("Skipping automatic instantiation of XR Device Simulator prefab since tests are running.");
                return;
            }
#endif

            if (XRDeviceSimulator.instance == null)
            {
                var simulatorPrefab = XRDeviceSimulatorSettings.Instance.simulatorPrefab;
                if (simulatorPrefab == null)
                {
                    Debug.LogWarning("XR Device Simulator prefab was missing, cannot automatically instantiate." +
                        " Open Window > Package Manager, select XR Interaction Toolkit, and Reimport the XR Device Simulator sample," +
                        " and then toggle the setting in Edit > Project Settings > XR Plug-in Management > XR Interaction Toolkit to try to resolve this issue.");
                    return;
                }

                var simulatorInstance = Object.Instantiate(simulatorPrefab);
                // Strip off (Clone) from the name
                simulatorInstance.name = simulatorPrefab.name;
                Object.DontDestroyOnLoad(simulatorInstance);
            }
            else
            {
                Object.DontDestroyOnLoad(XRDeviceSimulator.instance);
            }
        }
    }
}
#endif
