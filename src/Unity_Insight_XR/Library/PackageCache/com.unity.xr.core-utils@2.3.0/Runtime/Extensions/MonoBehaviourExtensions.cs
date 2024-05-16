using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for <see cref="MonoBehaviour"/> objects.
    /// </summary>
    public static class MonoBehaviourExtensions
    {
#if UNITY_EDITOR
        /// <summary>
        /// Starts running this <see cref="MonoBehaviour"/> while in edit mode.
        /// </summary>
        /// <remarks>
        /// This function sets <see cref="MonoBehaviour.runInEditMode"/> to <see langword="true"/>, which, if the behaviour is
        /// currently enabled, calls [OnDisable](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnDisable.html)
        /// and then [OnEnable](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnEnable.html).
        /// </remarks>
        /// <param name="behaviour">The behaviour</param>
        public static void StartRunInEditMode(this MonoBehaviour behaviour)
        {
            behaviour.runInEditMode = true;
        }

        /// <summary>
        /// Stops this <see cref="MonoBehaviour"/> from running in edit mode.
        /// </summary>
        /// <remarks>
        /// If this <see cref="MonoBehaviour"/> is currently enabled, this function disables it,
        /// sets <see cref="MonoBehaviour.runInEditMode"/> to <see langword="false"/>, and the re-enables it.
        /// [OnDisable](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnDisable.html) and
        /// [OnEnable](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnEnable.html) are called.
        ///
        /// If this <see cref="MonoBehaviour"/> is currently disabled, this function only sets  <see cref="MonoBehaviour.runInEditMode"/> to <see langword="false"/>.
        /// </remarks>
        /// <param name="behaviour">The behaviour</param>
        public static void StopRunInEditMode(this MonoBehaviour behaviour)
        {
            var wasEnabled = behaviour.enabled;
            if (wasEnabled)
                behaviour.enabled = false;

            behaviour.runInEditMode = false;

            if (wasEnabled)
                behaviour.enabled = true;
        }
#endif
    }
}
