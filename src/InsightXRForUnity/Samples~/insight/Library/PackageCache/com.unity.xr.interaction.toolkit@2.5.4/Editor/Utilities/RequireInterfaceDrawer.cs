using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;
using Object = UnityEngine.Object;

namespace UnityEditor.XR.Interaction.Toolkit.Utilities.Internal
{
    /// <summary>
    /// Draws the fields marked with the <see cref="RequireInterfaceAttribute"/> and validates if the referenced value
    /// implements the interface supplied in the attribute.
    /// If the referenced object is not valid, a warning message will be displayed below the gui control in the Inspector.
    /// This class does not validate the interface implementation when dragging objects into the foldout array (or list)
    /// in the Inspector.
    /// </summary>
    /// <remarks>
    /// This is accomplished by listening to some ImGUI events and changing the Object Field behavior:
    /// <list type="bullet">
    /// <item>
    /// <description>The Object Field's button click check is overriden to call the Object Selector window with a search
    /// filter containing the minimum set of valid field types that implement the given interface (<see cref="OnGUI"/>).</description>
    /// </item>
    /// <item>
    /// <description>The Object Field's Repaint event has a different object type to correctly display the interface type
    /// name in the Inspector (<see cref="GetObjectFieldType"/>).</description>
    /// </item>
    /// <item>
    /// <description>The Object Field's Drag Update and Perform events have a different object type to correctly discard
    /// dragged references that are not valid (<see cref="GetObjectFieldType"/>).</description>
    ///</item>
    /// </list>
    /// </remarks>
    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
    class RequireInterfaceDrawer : PropertyDrawer
    {
        static class Contents
        {
            public const float objectFieldMiniThumbnailHeight = 18f;
            public const float objectFieldMiniThumbnailWidth = 32f;
            public const float mismatchImplementationMessageHeight = 20f;

            public static GUIContent invalidTypeMessage { get; } = EditorGUIUtility.TrTextContent($"Use {nameof(RequireInterfaceAttribute)} with Object reference fields.");
            public static GUIContent invalidAttributeMessage { get; } = EditorGUIUtility.TrTextContent($"The attribute is not a {nameof(RequireInterfaceAttribute)}.");
            public static GUIContent invalidInterfaceMessage { get; } = EditorGUIUtility.TrTextContent("The required type is not an interface.");
            public static GUIContent mismatchImplementationMessage { get; } = EditorGUIUtility.TrTextContent("The referenced object does not implement {0}.");
        }

        const string k_ObjectSelectorUpdateCommand = "ObjectSelectorUpdated";

        /// <summary>
        /// Map that caches the search filter of a field and interface type pair.
        /// </summary>
        static readonly Dictionary<Type, Dictionary<Type, string>> s_FilterMapByFieldType = new Dictionary<Type, Dictionary<Type, string>>();

        /// <summary>
        /// Reusable string builder to create search filters.
        /// </summary>
        static readonly StringBuilder s_SearchFilterBuilder = new StringBuilder();

        /// <summary>
        /// Reusable list used to store the minimum assignable field types that implement the given interface.
        /// </summary>
        static readonly List<Type> s_MinimumAssignableImplementations = new List<Type>();

        #region Object Field

        /// <summary>
        /// Copied from the internal EditorGUI.ObjectFieldVisualType
        /// </summary>
        enum ObjectFieldVisualType
        {
            IconAndText,
            LargePreview,
            MiniPreview,
        }

        /// <summary>
        /// Copied from the internal EditorGUI.GetButtonRect and modified to receive the object type as parameter instead
        /// of the ObjectFieldVisualType.
        /// </summary>
        /// <param name="objectType">The type to get the button rect.</param>
        /// <param name="position">The property rect position.</param>
        /// <returns>The Object Field button picker rect position.</returns>
        static Rect GetObjectFieldButtonRect(Type objectType, Rect position)
        {
            var hasThumbnail = EditorGUIUtility.HasObjectThumbnail(objectType);
            var visualType = ObjectFieldVisualType.IconAndText;

            if (hasThumbnail && position.height <= Contents.objectFieldMiniThumbnailHeight && position.width <= Contents.objectFieldMiniThumbnailWidth)
                visualType = ObjectFieldVisualType.MiniPreview;
            else if (hasThumbnail && position.height > EditorGUIUtility.singleLineHeight)
                visualType = ObjectFieldVisualType.LargePreview;

            switch (visualType)
            {
                case ObjectFieldVisualType.IconAndText:
                    return new Rect(position.xMax - 19, position.y, 19, position.height);
                case ObjectFieldVisualType.MiniPreview:
                    return new Rect(position.xMax - 14, position.y, 14, position.height);
                case ObjectFieldVisualType.LargePreview:
                    return new Rect(position.xMax - 36, position.yMax - 14, 36, 14);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static Type GetObjectFieldType(Rect position, Type fieldType, Type interfaceType, out bool? dragAndDropAssignable)
        {
            dragAndDropAssignable = null;

            // Used to correctly display the interface type name
            if (Event.current.type == EventType.Repaint)
                return interfaceType;

            // Used to correctly update the DragAndDrop.visualMode when dragging references that are not assignable
            if (GUI.enabled &&
                (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform) &&
                DragAndDrop.objectReferences.Length > 0 &&
                position.Contains(Event.current.mousePosition))
            {
                var referencedValue = DragAndDrop.objectReferences[0];
                if (referencedValue != null)
                {
                    dragAndDropAssignable = TryGetAssignableObject(referencedValue, fieldType, interfaceType, out _);
                    if (!dragAndDropAssignable.Value)
                        return interfaceType;
                }
            }

            return fieldType;
        }

        #endregion

        #region Search Filter

        static bool IsDirectImplementation(Type type, Type interfaceType)
        {
            var directImplementedInterfaces = type.BaseType == null ? type.GetInterfaces() : type.GetInterfaces().Except(type.BaseType.GetInterfaces());
            return directImplementedInterfaces.Contains(interfaceType);
        }

        static void GetDirectImplementations(Type fieldType, Type interfaceType, List<Type> resultList)
        {
            if (!interfaceType.IsInterface)
                return;

            ReflectionUtils.ForEachType(t =>
            {
                if (!t.IsInterface && fieldType.IsAssignableFrom(t) && interfaceType.IsAssignableFrom(t) && IsDirectImplementation(t, interfaceType))
                    resultList.Add(t);
            });
        }

        static string GetSearchFilter(Type fieldType, Type interfaceType)
        {
            if (!s_FilterMapByFieldType.TryGetValue(fieldType, out var filterByInterfaceType))
            {
                filterByInterfaceType = new Dictionary<Type, string>();
                s_FilterMapByFieldType.Add(fieldType, filterByInterfaceType);
            }
            else if (filterByInterfaceType.TryGetValue(interfaceType, out var cachedSearchFilter))
            {
                return cachedSearchFilter;
            }

            s_MinimumAssignableImplementations.Clear();
            GetDirectImplementations(fieldType, interfaceType, s_MinimumAssignableImplementations);

            s_SearchFilterBuilder.Clear();
            foreach (var type in s_MinimumAssignableImplementations)
            {
                s_SearchFilterBuilder.Append("t:");
                s_SearchFilterBuilder.Append(type.Name);
                s_SearchFilterBuilder.Append(" ");
            }
            var searchFilter = s_SearchFilterBuilder.ToString();

            filterByInterfaceType.Add(interfaceType, searchFilter);
            return searchFilter;
        }

        #endregion

        #region Helper Methods

        static bool TryGetAssignableObject(Object objectToValidate, Type fieldType, Type interfaceType, out Object assignableObject)
        {
            if (objectToValidate == null)
            {
                assignableObject = null;
                return true;
            }

            var valueType = objectToValidate.GetType();
            if (fieldType.IsAssignableFrom(valueType) && interfaceType.IsAssignableFrom(valueType))
            {
                assignableObject = objectToValidate;
                return true;
            }

            // If the given objectToValidate is a GameObject, search its components as well
            if (objectToValidate is GameObject gameObject)
            {
                assignableObject = gameObject.GetComponent(interfaceType);
                if (assignableObject != null && fieldType.IsInstanceOfType(assignableObject) && interfaceType.IsInstanceOfType(assignableObject))
                    return true;
            }

            assignableObject = null;
            return false;
        }

        static Type GetFieldOrElementType(Type fieldType)
        {
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var types = fieldType.GetGenericArguments();
                return types.Length <= 0 ? null : types[0];
            }

            if (fieldType.IsArray)
                return fieldType.GetElementType();

            return fieldType;
        }

        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var propertyHeight = base.GetPropertyHeight(property, label);
            if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null &&
                attribute is RequireInterfaceAttribute requireInterfaceAttr &&
                requireInterfaceAttr.interfaceType.IsInterface &&
                !requireInterfaceAttr.interfaceType.IsInstanceOfType(property.objectReferenceValue))
            {
                propertyHeight += Contents.mismatchImplementationMessageHeight + 4f;
            }

            return propertyHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var objectPickerID = GUIUtility.GetControlID(FocusType.Passive);

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.LabelField(position, label, Contents.invalidTypeMessage);
                return;
            }

            if (!(attribute is RequireInterfaceAttribute requireInterfaceAttr))
            {
                EditorGUI.LabelField(position, label, Contents.invalidAttributeMessage);
                return;
            }

            if (requireInterfaceAttr.interfaceType == null || !requireInterfaceAttr.interfaceType.IsInterface)
            {
                EditorGUI.LabelField(position, label, Contents.invalidInterfaceMessage);
                return;
            }

            if (property.objectReferenceValue != null && !requireInterfaceAttr.interfaceType.IsInstanceOfType(property.objectReferenceValue))
            {
                var messagePosition = position;
                position.height -= Contents.mismatchImplementationMessageHeight + 4f;
                messagePosition.y = position.yMax + 2f;
                messagePosition.height = Contents.mismatchImplementationMessageHeight;
                EditorGUI.HelpBox(messagePosition, string.Format(Contents.mismatchImplementationMessage.text, requireInterfaceAttr.interfaceType.Name), MessageType.Warning);
            }

            var fieldType = GetFieldOrElementType(fieldInfo.FieldType);

            using (var scope = new EditorGUI.PropertyScope(position, label, property))
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var allowSceneObjs = !EditorUtility.IsPersistent(property.serializedObject.targetObject);
                var objectFieldType = GetObjectFieldType(position, fieldType, requireInterfaceAttr.interfaceType, out var dragAndDropAssignable);

                // Override the Object Field button to call the Object Selector window with a filter containing the minimum set of assignable field types that implement the required interface
                if (GUI.enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0 && position.Contains(Event.current.mousePosition))
                {
                    var buttonRect = GetObjectFieldButtonRect(objectFieldType, position);
                    if (buttonRect.Contains(Event.current.mousePosition))
                    {
                        EditorGUIUtility.editingTextField = false;

                        var searchFilter = GetSearchFilter(fieldType, requireInterfaceAttr.interfaceType);
                        EditorGUIUtility.ShowObjectPicker<Object>(property.objectReferenceValue, allowSceneObjs, searchFilter, objectPickerID);

                        Event.current.Use();
                        GUIUtility.ExitGUI();
                    }
                }

                if (dragAndDropAssignable.HasValue && !dragAndDropAssignable.Value)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    Event.current.Use();
                }

                var value = EditorGUI.ObjectField(position, scope.content, property.objectReferenceValue, objectFieldType, allowSceneObjs);

                // Get the value of the selected Object in the Object Selector window
                if (Event.current.commandName == k_ObjectSelectorUpdateCommand && EditorGUIUtility.GetObjectPickerControlID() == objectPickerID)
                {
                    GUI.changed = true;
                    value = EditorGUIUtility.GetObjectPickerObject();
                }

                if (check.changed && TryGetAssignableObject(value, fieldType, requireInterfaceAttr.interfaceType, out var assignableValue))
                    property.objectReferenceValue =  assignableValue;
            }
        }
    }
}
