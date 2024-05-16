#if AR_FOUNDATION_PRESENT || PACKAGE_DOCS_GENERATION
using UnityEditor;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Scripting;

namespace UnityEngine.XR.Interaction.Toolkit.AR.Inputs
{
    /// <summary>
    /// This class automatically registers the control layout used by the <see cref="TouchscreenGestureInputController"/>.
    /// </summary>
    /// <seealso cref="TouchscreenGestureInputController"/>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [Preserve]
    public static class TouchscreenGestureInputLayoutLoader
    {
        /// <summary>
        /// See <see cref="RuntimeInitializeLoadType.BeforeSceneLoad"/>.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad), Preserve]
        public static void Initialize()
        {
            // Will execute the static constructor as a side effect.
        }

        [Preserve]
        static TouchscreenGestureInputLayoutLoader()
        {
            InputSystem.InputSystem.RegisterLayout<TouchscreenGestureInputController>(
                matches: new InputDeviceMatcher()
                    .WithProduct(nameof(TouchscreenGestureInputController)));
        }
    }
}
#endif