// Object.FindFirstObjectByType<T> API added in:

// 2022.3.0f1 or newer
#if UNITY_2022_3_OR_NEWER
#define HAS_FIND_FIRST_OBJECT_BY_TYPE
#endif

// 2022.2.5f1 or newer
#if UNITY_2022_2 && !(UNITY_2022_2_0 || UNITY_2022_2_1 || UNITY_2022_2_2 || UNITY_2022_2_3 || UNITY_2022_2_4)
#define HAS_FIND_FIRST_OBJECT_BY_TYPE
#endif

// 2021.3.18f1 or newer
#if UNITY_2021_3 && !(UNITY_2021_3_0 || UNITY_2021_3_1 || UNITY_2021_3_2 || UNITY_2021_3_3 || UNITY_2021_3_4 || UNITY_2021_3_5 || UNITY_2021_3_6 || UNITY_2021_3_7 || UNITY_2021_3_8 || UNITY_2021_3_9 || UNITY_2021_3_10 || UNITY_2021_3_11 || UNITY_2021_3_12 || UNITY_2021_3_13 || UNITY_2021_3_14 || UNITY_2021_3_15 || UNITY_2021_3_16 || UNITY_2021_3_17)
#define HAS_FIND_FIRST_OBJECT_BY_TYPE
#endif

// 2020.3.45f1 or newer (48 is the final 2020.3 patch version)
#if UNITY_2020_3 && (UNITY_2020_3_45 || UNITY_2020_3_46 || UNITY_2020_3_47 || UNITY_2020_3_48)
#define HAS_FIND_FIRST_OBJECT_BY_TYPE
#endif

namespace UnityEngine.XR.Interaction.Toolkit.Utilities
{
    /// <summary>
    /// Utility methods for locating component instances.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    static class ComponentLocatorUtility<T> where T : Component
    {
        /// <summary>
        /// Cached reference to a found component of type <see cref="T"/>.
        /// </summary>
        static T s_ComponentCache;

        /// <summary>
        /// Cached reference to a found component of type <see cref="T"/>.
        /// </summary>
        internal static T componentCache => s_ComponentCache;

        /// <summary>
        /// Find or create a new GameObject with component <typeparamref name="T"/>.
        /// </summary>
        /// <returns>Returns the found or created component.</returns>
        /// <remarks>
        /// Does not include inactive GameObjects when finding the component, but if a component was previously created
        /// as a direct result of this class, it will return that component even if the GameObject is now inactive.
        /// </remarks>
        public static T FindOrCreateComponent()
        {
            if (s_ComponentCache == null)
            {
                s_ComponentCache = Find();

                if (s_ComponentCache == null)
                    s_ComponentCache = new GameObject(typeof(T).Name, typeof(T)).GetComponent<T>();
            }

            return s_ComponentCache;
        }

        /// <summary>
        /// Find a component <typeparamref name="T"/>.
        /// </summary>
        /// <returns>Returns the found component, or <see langword="null"/> if one could not be found.</returns>
        /// <remarks>
        /// Does not include inactive GameObjects when finding the component, but if a component was previously created
        /// as a direct result of this class, it will return that component even if the GameObject is now inactive.
        /// </remarks>
        public static T FindComponent()
        {
            TryFindComponent(out var component);
            return component;
        }

        /// <summary>
        /// Find a component <typeparamref name="T"/>.
        /// </summary>
        /// <param name="component">When this method returns, contains the found component, or <see langword="null"/> if one could not be found.</param>
        /// <returns>Returns <see langword="true"/> if the component exists, otherwise returns <see langword="false"/>.</returns>
        /// <remarks>
        /// Does not include inactive GameObjects when finding the component, but if a component was previously created
        /// as a direct result of this class, it will return that component even if the GameObject is now inactive.
        /// </remarks>
        public static bool TryFindComponent(out T component)
        {
            if (s_ComponentCache != null)
            {
                component = s_ComponentCache;
                return true;
            }

            s_ComponentCache = Find();
            component = s_ComponentCache;
            return component != null;
        }

        static T Find()
        {
#if HAS_FIND_FIRST_OBJECT_BY_TYPE
            // Preferred API
            return Object.FindFirstObjectByType<T>();
#else
            // Fallback API
            return Object.FindObjectOfType<T>();
#endif
        }
    }
}