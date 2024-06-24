using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Unity.XR.CoreUtils.Editor
{
    /// <summary>
    /// Utility methods for use in Editor code.
    /// </summary>
    public static class EditorUtils
    {
        /// <summary>
        /// Gets the attributes of a <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="property">The property with attributes to enumerate.</param>
        /// <returns>An array of attributes.</returns>
        public static Attribute[] GetMemberAttributes(SerializedProperty property)
        {
            var fi = GetFieldInfoFromProperty(property);
            return fi.GetCustomAttributes(false).Cast<Attribute>().ToArray();
        }

        /// <summary>
        /// Gets the <see cref="FieldInfo"/> of a <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="property">The property to get information about.</param>
        /// <returns>Attributes and metadata about the field.</returns>
        public static FieldInfo GetFieldInfoFromProperty(SerializedProperty property)
        {
            var memberInfo = GetMemberInfoFromPropertyPath(property.serializedObject.targetObject.GetType(), property.propertyPath, out _);
            if (memberInfo.MemberType != MemberTypes.Field)
                return null;

            return memberInfo as FieldInfo;
        }

        /// <summary>
        /// Gets <see cref="MemberInfo"/> for a property by path.
        /// </summary>
        /// <param name="host">The declaring type.</param>
        /// <param name="path">The property path relative to the declaring type. See
        /// <see cref="SerializedProperty.propertyPath"/>.</param>
        /// <param name="type">Assigned the Type of the property.</param>
        /// <returns>Attributes and metadata about the member identified by <paramref name="host"/>
        /// and <paramref name="path"/>.</returns>
        public static MemberInfo GetMemberInfoFromPropertyPath(Type host, string path, out Type type)
        {
            type = host;
            if (host == null)
                return null;
            MemberInfo memberInfo = null;

            var parts = path.Split ('.');
            for (var i = 0; i < parts.Length; i++)
            {
                var member = parts[i];

                // Special handling of array elements.
                // The "Array" and "data[x]" parts of the propertyPath don't correspond to any types,
                // so they should be skipped by the code that drills down into the types.
                // However, we want to change the type from the type of the array to the type of the array
                // element before we do the skipping.
                if (i < parts.Length - 1 && member == "Array" && parts[i + 1].StartsWith ("data["))
                {
                    Type listType = null;
                    // ReSharper disable once PossibleNullReferenceException would have returned if host was null
                    if (type.IsArray)
                    {
                        listType = type.GetElementType();
                    }
                    else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        listType = type.GetGenericArguments()[0];
                    }
                    if (listType != null)
                        type = listType;

                    // Skip rest of handling for this part ("Array") and the next part ("data[x]").
                    i++;
                    continue;
                }

                // GetField on class A will not find private fields in base classes to A,
                // so we have to iterate through the base classes and look there too.
                // Private fields are relevant because they can still be shown in the Inspector,
                // and that applies to private fields in base classes too.
                MemberInfo foundMember = null;
                for (var currentType = type; foundMember == null && currentType != null; currentType =
                    currentType.BaseType)
                {
                    var foundMembers = currentType.GetMember(member, BindingFlags.Instance | BindingFlags.Public |
                        BindingFlags.NonPublic);
                    if (foundMembers.Length > 0)
                    {
                        foundMember = foundMembers[0];
                    }
                }

                if (foundMember == null)
                {
                    type = null;
                    return null;
                }

                memberInfo = foundMember;
                switch (memberInfo.MemberType) {
                    case MemberTypes.Field:
                        var info = memberInfo as FieldInfo;
                        if (info != null)
                            type = info.FieldType;
                        break;
                    case MemberTypes.Property:
                        var propertyInfo = memberInfo as PropertyInfo;
                        if (propertyInfo != null)
                            type = propertyInfo.PropertyType;
                        break;
                    default:
                        type = memberInfo.DeclaringType;
                        break;
                }
            }

            return memberInfo;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> of a <see cref="SerializedProperty"/> object, if possible.
        /// </summary>
        /// <remarks>
        /// Guesses the type of a <c>SerializedProperty</c> and returns a <c>System.Type</c>, if one exists.
        /// This function checks the type of the target object by iterating through its fields looking for
        /// one that matches the property name. This may return null if <paramref name="property"/> is a
        /// <c>SerializedProperty</c> that represents a native type with no managed equivalent.
        /// </remarks>
        /// <param name="property">The <c>SerializedProperty</c> to examine.</param>
        /// <returns>The best guess type.</returns>
        public static Type SerializedPropertyToType(SerializedProperty property)
        {
            var field = SerializedPropertyToField(property);
            return field != null ? field.FieldType : null;
        }

        /// <summary>
        /// Gets the <see cref="FieldInfo"/> for a given property.
        /// </summary>
        /// <param name="property">The property to get information about.</param>
        /// <returns>The <see cref="FieldInfo"/>.</returns>
        public static FieldInfo SerializedPropertyToField(SerializedProperty property)
        {
            var parts = property.propertyPath.Split('.');
            if (parts.Length == 0)
                return null;

            var currentType = property.serializedObject.targetObject.GetType();
            FieldInfo field = null;
            foreach (var part in parts)
            {
                if (part == "Array")
                {
                    currentType = field.FieldType.GetElementType();
                    continue;
                }

                field = currentType.GetFieldInTypeOrBaseType(part);
                if (field == null)
                    continue;

                currentType = field.FieldType;
            }

            return field;
        }

        /// <summary>
        /// Makes an Editor GUI control for mask properties.
        /// </summary>
        /// <remarks>
        /// This function is similar to <see cref="EditorGUI"/>.<see cref="EditorGUI.MaskField(Rect, string, int, string[])"/>,
        /// but ensures that only the chosen bits are set. We need this version of the
        /// function to check explicitly whether only a single bit was set.
        /// </remarks>
        /// <param name="position">Rectangle on the screen to use for this control.</param>
        /// <param name="label">Label for the field.</param>
        /// <param name="mask">The current mask to display.</param>
        /// <param name="displayedOptions">A string array containing the labels for each flag.</param>
        /// <param name="propertyType">The type of the property</param>
        /// <returns>The value modified by the user.</returns>
        public static int MaskField(Rect position, GUIContent label, int mask, string[] displayedOptions, Type propertyType)
        {
            mask = EditorGUI.MaskField(position, label, mask, displayedOptions);
            return ActualEnumFlags(mask, propertyType);
        }

        /// <summary>
        /// Normalizes the value of a flag using the specified Enum type.
        /// </summary>
        /// <remarks>
        /// This function masks the flag so that only the bits corresponding to values declared in the specified Enum are set.
        /// </remarks>
        /// <param name="value">The flag value.</param>
        /// <param name="t">The <see cref="Type"/> of the Enum.</param>
        /// <returns>The masked flag value.</returns>
        static int ActualEnumFlags(int value, Type t)
        {
            if (value < 0)
            {
                var mask = 0;
                foreach (var enumValue in Enum.GetValues(t))
                {
                    mask |= (int)enumValue;
                }

                value &= mask;
            }

            return value;
        }

        /// <summary>
        /// Cleans up a string received from <see cref="SerializedProperty"/>.<see cref="SerializedProperty.type"/>.
        /// </summary>
        /// <remarks>
        /// Strips `PPtr&lt;&gt;` and `$` from a string. Use this function when getting a `System.Type` using `SerializedProperty.type`.
        /// </remarks>
        /// <param name="type">Type string.</param>
        /// <returns>Nicified type string.</returns>
        public static string NicifySerializedPropertyType(string type)
        {
            return type.Replace("PPtr<", "").Replace(">", "").Replace("$", "");
        }

        /// <summary>
        /// Gets the <see cref="Type"/> corresponding to the specified name string.
        /// </summary>
        /// <remarks>
        /// Searches through all assemblies in the current AppDomain for a class that is assignable to UnityObject
        /// and matches the given weak name.
        /// </remarks>
        /// <param name="name">Weak type name</param>
        /// <returns>The best guess for the `System.Type` of <paramref name="name"/>.</returns>
        // TODO: expose internal SerializedProperty.ValidateObjectReferenceValue to remove this hack
        public static Type TypeNameToType(string name)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.Name.Equals(name) && typeof(UnityObject).IsAssignableFrom(type))
                            return type;
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip any assemblies that don't load properly
                }
            }

            return typeof(UnityObject);
        }

        /// <summary>
        /// Tries to get an <see cref="AssetPreview"/> for an asset.
        /// </summary>
        /// <remarks>
        /// If a preview is not immediately available, this function waits until
        /// <see cref="AssetPreview.IsLoadingAssetPreview(int)"/> changes to <see langword="false"/>. If the
        /// preview has still not loaded, the function uses <see cref="AssetPreview.GetMiniThumbnail(UnityObject)"/>
        /// instead.
        /// </remarks>
        /// <param name="asset">The asset for which to get a preview.</param>
        /// <param name="callback">Called with the preview texture as an argument, when it becomes available.</param>
        /// <returns>An enumerator used to tick the coroutine.</returns>
        public static IEnumerator GetAssetPreview(UnityObject asset, Action<Texture> callback)
        {
            // GetAssetPreview will start loading the preview, or return one if available
            var texture = AssetPreview.GetAssetPreview(asset);

            // If the preview is not available, IsLoadingAssetPreview will be true until loading has finished
            while (AssetPreview.IsLoadingAssetPreview(asset.GetInstanceID()))
            {
                texture = AssetPreview.GetAssetPreview(asset);
                yield return null;
            }

            // If loading a preview fails, fall back to the MiniThumbnail
            if (!texture)
                texture = AssetPreview.GetMiniThumbnail(asset);

            callback(texture);
        }
    }
}
