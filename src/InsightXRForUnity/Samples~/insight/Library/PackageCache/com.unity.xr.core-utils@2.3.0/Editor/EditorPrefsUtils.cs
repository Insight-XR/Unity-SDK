using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CoreUtils.Editor
{
    /// <summary>
    /// Utilities for getting and setting values stored in <see cref="EditorPrefs"/>.
    /// </summary>
    /// <remarks>The `EditorPrefUtils` class caches any preference values retrieved or set using its methods.
    /// Avoid accessing the same preference values using the <see cref="EditorPrefs"/> class directly.</remarks>
    public static class EditorPrefsUtils
    {
        static readonly Dictionary<string, object> k_EditorPrefsValueSessionCache = new Dictionary<string, object>();

        /// <summary>
        /// Gets a standardized Editor preference key name.
        /// </summary>
        /// <remarks>Constructs the key name by combining the parent object type's full name and the property name.</remarks>
        /// <param name="typeName">The name of the type that declares the property.</param>
        /// <param name="propertyName">Name of property.</param>
        /// <returns>Editor preference key for property.</returns>
        public static string GetPrefKey(string typeName, string propertyName)
        {
            return $"{typeName}.{propertyName}";
        }

        /// <summary>
        /// Gets the bool value stored in the Editor preferences for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type that declares the property.</param>
        /// <param name="defaultValue">Value to be used as default if a preference value has not been stored.</param>
        /// <param name="propertyName">Name of calling method. When invoking this function from a property getter
        /// or setter, you can leave this parameter blank and it will be filled in by the property name.</param>
        /// <returns>The bool value stored in the Editor preferences for the calling property.</returns>
        public static bool GetBool(string typeName, bool defaultValue = false,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            return GetEditorPrefsValueOrDefault(prefsKey, defaultValue);
        }

        /// <summary>
        /// Stores the bool value in the Editor preferences for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type which declares the property.</param>
        /// <param name="value">Value to set in Editor preferences.</param>
        /// <param name="propertyName">Name of calling property. When invoking this function from a property getter
        /// or setter, you can leave this parameter blank and it will be filled in by the property name.</param>
        public static void SetBool(string typeName, bool value,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            SetEditorPrefsValue(prefsKey, value);
        }

        /// <summary>
        /// Gets the float value stored in the Editor preferences for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type that declares the property</param>
        /// <param name="defaultValue">Value to be used as default if a preference value has not been stored.</param>
        /// <param name="propertyName">Name of calling property. When invoking this function from a property getter
        /// or setter, you can leave this parameter blank and it will be filled in by the property name.</param>
        /// <returns>The float value stored in the Editor preferences for the calling property.</returns>
        public static float GetFloat(string typeName, float defaultValue = 0f,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            return GetEditorPrefsValueOrDefault(prefsKey, defaultValue);
        }

        /// <summary>
        /// Stores the float value in the Editor preferences for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type that declares the property.</param>
        /// <param name="value">Value to set in Editor preferences.</param>
        /// <param name="propertyName">Name of calling property. When invoking this function from a property getter
        /// or setter, you can leave this parameter blank and it will be filled in by the property name.</param>
        public static void SetFloat(string typeName, float value,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            SetEditorPrefsValue(prefsKey, value);
        }

        /// <summary>
        /// Gets the int value stored in the Editor preferences for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type that declares the property.</param>
        /// <param name="defaultValue">Value to be used as default if a preference value has not been stored.</param>
        /// <param name="propertyName">Name of calling property. When invoking this function from a property getter
        /// or setter, you can leave this parameter blank and it will be filled in by the property name.</param>
        /// <returns>The int value stored in the Editor preferences for the calling property.</returns>
        public static int GetInt(string typeName, int defaultValue = 0,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            return GetEditorPrefsValueOrDefault(prefsKey, defaultValue);
        }

        /// <summary>
        /// Stores the int value in the Editor preferences for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type which declares the property.</param>
        /// <param name="value">Value to set in Editor preferences.</param>
        /// <param name="propertyName">Name of calling property. When invoking this function from a property getter
        /// or setter, you can leave this parameter blank and it will be filled in by the property name.</param>
        public static void SetInt(string typeName, int value,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            SetEditorPrefsValue(prefsKey, value);
        }

        /// <summary>
        /// Gets the string value stored in the Editor preferences for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type that declares the property.</param>
        /// <param name="defaultValue">Value to be used as default if a preference value has not been stored.</param>
        /// <param name="propertyName">Name of calling property. When invoking this function from a property getter
        /// or setter, you can leave this parameter blank and it will be filled in by the property name.</param>
        /// <returns>The string value stored in the Editor preferences for the calling property.</returns>
        public static string GetString(string typeName, string defaultValue = "",
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            return GetEditorPrefsValueOrDefault(prefsKey, defaultValue);
        }

        /// <summary>
        /// Stores the string value in the Editor preferences for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type that declares the property.</param>
        /// <param name="value">Value to set in Editor preferences.</param>
        /// <param name="propertyName">Name of calling property. When invoking this function from a property getter
        /// or setter, you can leave this parameter blank and it will be filled in by the property name.</param>
        public static void SetString(string typeName, string value,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            SetEditorPrefsValue(prefsKey, value);
        }

        /// <summary>
        /// Gets the color value stored in the Editor preferences for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type that declares the property.</param>
        /// <param name="defaultValue">Value to be used as default if a preference value has not been stored.</param>
        /// <param name="propertyName">Name of calling property. When invoking this function from a property getter
        /// or setter, you can leave this parameter blank and it will be filled in by the property name.</param>
        /// <returns>The color value stored in the Editor preferences for the calling property.</returns>
        public static Color GetColor(string typeName, Color defaultValue,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            return GetEditorPrefsValueOrDefault(prefsKey, defaultValue);
        }

        /// <summary>
        /// Stores the color value in the Editor preferences for the calling property.
        /// </summary>
        /// <param name="typeName">The name of the type that declares the property.</param>
        /// <param name="value">Value to set in Editor preferences.</param>
        /// <param name="propertyName">Name of calling property. When invoking this function from a property getter
        /// or setter, you can leave this parameter blank and it will be filled in by the property name.</param>
        public static void SetColor(string typeName, Color value,
            [CallerMemberName] string propertyName = null)
        {
            var prefsKey = GetPrefKey(typeName, propertyName);
            SetEditorPrefsValue(prefsKey, value);
        }

        /// <summary>
        /// Resets the cached values stored by this `EditorPrefsUtils` object.
        /// </summary>
        internal static void ResetEditorPrefsValueSessionCache()
        {
            k_EditorPrefsValueSessionCache.Clear();
        }

        static void SetEditorPrefsValue<T>(string prefsKey, T value)
        {
            if (TryGetCachedEditorPrefsValue(prefsKey, out T cachedValue) && cachedValue.Equals(value))
                return;

            var type = typeof(T);

            if (type == typeof(bool))
            {
                EditorPrefs.SetBool(prefsKey, (bool)(object)value);
            }
            else if (type == typeof(int) && value is int)
            {
                EditorPrefs.SetInt(prefsKey, (int)(object)value);
            }
            else if (type == typeof(float) && value is float)
            {
                EditorPrefs.SetFloat(prefsKey, (float)(object)value);
            }
            else if (type == typeof(string) && value is string)
            {
                EditorPrefs.SetString(prefsKey, (string)(object)value);
            }
            else if (type.IsAssignableFromOrSubclassOf(typeof(Enum))
                && value.GetType().IsAssignableFromOrSubclassOf(typeof(Enum)))
            {
                EditorPrefs.SetInt(prefsKey, (int)(object)value);
            }
            else if (type == typeof(Color) && value is Color)
            {
                EditorPrefs.SetString(prefsKey, ColorToColorPref(prefsKey, (Color)(object)value));
            }
            else
            {
                Debug.LogError($"Could not set Editor Preference Value of type : {type} with value {value} !");
                return;
            }

            if (k_EditorPrefsValueSessionCache.ContainsKey(prefsKey))
                k_EditorPrefsValueSessionCache[prefsKey] = value;
            else
                k_EditorPrefsValueSessionCache.Add(prefsKey, value);
        }

        static void GetEditorPrefsValue<T>(string prefsKey, out T prefValue)
        {
            if (TryGetCachedEditorPrefsValue(prefsKey, out prefValue))
                return;

            var type = typeof(T);
            var prefsSet = false;
            if (type == typeof(bool))
            {
                prefValue = (T)(object)EditorPrefs.GetBool(prefsKey);
                prefsSet = true;
            }
            else if (type == typeof(int))
            {
                prefValue = (T)(object)EditorPrefs.GetInt(prefsKey);
                prefsSet = true;
            }
            else if (type == typeof(float))
            {
                prefValue = (T)(object)EditorPrefs.GetFloat(prefsKey);
                prefsSet = true;
            }
            else if (type == typeof(string))
            {
                prefValue = (T)(object)EditorPrefs.GetString(prefsKey);
                prefsSet = true;
            }
            else if (type.IsAssignableFromOrSubclassOf(typeof(Enum)))
            {
                prefValue = (T)(object)EditorPrefs.GetInt(prefsKey);
                prefsSet = true;
            }
            else if (type == typeof(Color))
            {
                prefValue = (T)(object)PrefToColor(EditorPrefs.GetString(prefsKey));
                prefsSet = true;
            }
            else
            {
                Debug.LogError($"Could not get Editor Preference Default of type : {type} Type is not supported!");
            }

            if (prefsSet && prefValue != null)
            {
                SetEditorPrefsValue(prefsKey, prefValue);
                return;
            }

            SetEditorPrefsValue(prefsKey, default(T));
            prefValue = default;
        }

        static bool TryGetCachedEditorPrefsValue<T>(string prefsKey, out T prefValue)
        {
            if (k_EditorPrefsValueSessionCache.TryGetValue(prefsKey, out var cachedObj))
            {
                if (cachedObj is T || cachedObj.GetType().IsAssignableFromOrSubclassOf(typeof(T)))
                {
                    prefValue = (T)cachedObj;
                    return true;
                }
            }

            prefValue = default;
            return false;
        }

        static T GetEditorPrefsValueOrDefault<T>(string prefsKey, T defaultValue = default)
        {
            var value = defaultValue;
            if (!EditorPrefs.HasKey(prefsKey))
                SetEditorPrefsValue(prefsKey, value);
            else
                GetEditorPrefsValue(prefsKey, out value);

            return value;
        }

        /// <summary>
        /// Creates a <see cref="Color"/> object from a specially formatted string.
        /// </summary>
        /// <remarks>
        /// `PrefToColor` decodes a string encoded by <see cref="ColorToColorPref(string, Color)"/>.
        /// 
        /// This function is used by <see cref="GetColor(string, Color, string)"/> to read a preference
        /// value stored by <see cref="SetColor(string, Color, string)"/>. 
        /// </remarks>
        /// <param name="pref">A color preference value encoded as a string by <see cref="ColorToColorPref(string, Color)"/>.</param>
        /// <returns>A decoded <see cref="Color"/> object for a string stored in the Editor preferences. If the string cannot be decoded,
        /// the <see langword="default"/> `Color` (all 0 components) is returned.</returns>
        public static Color PrefToColor(string pref)
        {
            var split = pref.Split(';');
            if (split.Length != 5)
            {
                Debug.LogWarningFormat("Parsing PrefColor failed on {0}", pref);
                return default;
            }

            split[1] = split[1].Replace(',', '.');
            split[2] = split[2].Replace(',', '.');
            split[3] = split[3].Replace(',', '.');
            split[4] = split[4].Replace(',', '.');
            var success = float.TryParse(split[1], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out var r);
            success &= float.TryParse(split[2], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out var g);
            success &= float.TryParse(split[3], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out var b);
            success &= float.TryParse(split[4], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out var a);

            if (success)
                return new Color(r, g, b, a);

            Debug.LogWarningFormat("Parsing PrefColor failed on {0}", pref);
            return default;
        }

        /// <summary>
        /// Encodes a <see cref="Color"/> object as a string.
        /// </summary>
        /// <remarks>
        /// `ColorToColorPref` encodes a string that can be decoded by <see cref="PrefToColor(string)"/>.
        /// The function prepends the color information with the string specified in <paramref name="path"/>.
        /// 
        /// This function is used by <see cref="SetColor(string, Color, string)"/> to store a preference
        /// that can be retrieved with <see cref="GetColor(string, Color, string)"/>. 
        /// </remarks>
        /// <param name="path">The preference key/path prepended to the color string.</param>
        /// <param name="value">The color value to encode.</param>
        /// <returns>A formatted string representing the color value.</returns>
        public static string ColorToColorPref(string path, Color value)
        {
            var colorString = $"{value.r:0.000};{value.g:0.000};{value.b:0.000};{value.a:0.000}".Replace('.', ',');
            return $"{path};{colorString}";
        }
    }
}
