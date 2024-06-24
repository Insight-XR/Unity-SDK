using System;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace UnityEditor.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Editor for the list of evaluators and the evaluator properties.
    /// Used to draw the evaluator list of a Target Filter and the properties of the selected element in the Inspector.
    /// </summary>
    class XRTargetEvaluatorList : ReorderableList
    {
        const float k_EnabledWidth = 20f;
        const float k_WeightWidth = 60f;

        readonly SerializedObject m_SerializedObject;
        readonly GenericMenu m_AddEvaluatorMenu;

        void OnDrawListHeader(Rect rect)
        {
            var width = rect.width - 2f;

            rect.width = width - k_WeightWidth - 4f;
            GUI.Label(rect, serializedProperty.displayName);
            rect.x += rect.width + 4f;

            rect.width = k_WeightWidth;
            GUI.Label(rect, "Weight");
            rect.x += rect.width;
        }

        void OnDrawListElement(Rect rect, int elementIndex, bool isActive, bool isFocused)
        {
            var element = serializedProperty.GetArrayElementAtIndex(elementIndex);
            if (element == null)
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "<null>");
                return;
            }

            var width = rect.width - 2f;
            rect.y += 1f;
            rect.height = EditorGUIUtility.singleLineHeight;

            // Draw the enabled property
            rect.width = k_EnabledWidth;
            rect.x += 4f;
            var enabledProperty = element.FindPropertyRelative("m_Enabled");
            if (enabledProperty != null)
                EditorGUI.PropertyField(rect, enabledProperty, GUIContent.none);
            rect.x += rect.width + 2f;

            // Draw the type name
            rect.width = width - k_EnabledWidth - k_WeightWidth - 10f;
            var evaluatorFullTypeName = element.managedReferenceFullTypename;
            var evaluatorTypeName = string.IsNullOrEmpty(evaluatorFullTypeName)
                ? "<null>"
                : XRTargetEvaluatorEditorUtility.GetTypeName(evaluatorFullTypeName);
            EditorGUI.LabelField(rect, evaluatorTypeName);
            rect.x += rect.width + 4f;

            // Draw the weight curve property
            rect.width = k_WeightWidth;
            var weightProperty = element.FindPropertyRelative("m_Weight");
            if (weightProperty != null)
                EditorGUI.PropertyField(rect, weightProperty, GUIContent.none);
        }

        void OnRemoveListElement(ReorderableList reorderableList)
        {
            if (index < 0 || index >= count)
                return;

            var listCount = count;
            if (XRTargetEvaluatorEditorUtility.RemoveEvaluatorAt(serializedProperty, index))
            {
                // Update the selected evaluator in the list
                --listCount;
                if (listCount <= 0)
                    index = -1;
                else
                    index = Mathf.Clamp(index, 0, listCount - 1);
            }
        }

        void OnAddListElement(ReorderableList reorderableList)
        {
            m_AddEvaluatorMenu.ShowAsContext();
        }

        void AddEvaluator(object userData)
        {
            var evaluatorType = userData as Type;
            if (evaluatorType == null)
                return;

            var listCount = count;
            if (XRTargetEvaluatorEditorUtility.AddEvaluator(serializedProperty, evaluatorType))
            {
                // Select the added evaluator in the list
                index = listCount;
            }
        }

        /// <summary>
        /// Call this to draw the properties of the <see cref="XRTargetEvaluator"/> at the given index.
        /// </summary>
        /// <param name="elementIndex">Index of the evaluator to draw.</param>
        public void DrawListElementInspectorGUI(int elementIndex)
        {
            // Does the element at the given index exist?
            var iterator = serializedProperty.GetArrayElementAtIndex(elementIndex);
            if (iterator == null)
                return;

            // The evaluator has a valid type? An invalid type means that the evaluator is null since it can't be deserialized
            var evaluatorFullTypeName = iterator.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(evaluatorFullTypeName))
                return;

            using (var guiChangeCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.Space();

                // Draw the type name
                var typeName = XRTargetEvaluatorEditorUtility.GetTypeName(evaluatorFullTypeName);
                EditorGUILayout.LabelField(typeName, EditorStyles.boldLabel);

                // Draw the evaluator properties
                var end = iterator.GetEndProperty();
                while (iterator.NextVisible(true) && !SerializedProperty.EqualContents(iterator, end))
                    EditorGUILayout.PropertyField(iterator, true);

                if (guiChangeCheck.changed)
                    m_SerializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Initializes and returns an instance of <see cref="XRTargetEvaluatorList"/>.
        /// Call this to create a new list and then display it in the Inspector.
        /// </summary>
        /// <param name="serializedObject">The <see cref="XRTargetFilter"/> serialized object.</param>
        /// <param name="elements">The serialized property of the evaluators list in the given filter serialized object.</param>
        public XRTargetEvaluatorList(SerializedObject serializedObject, SerializedProperty elements)
            : base(serializedObject, elements, true, true, true, true)
        {
            m_SerializedObject = serializedObject;
            drawHeaderCallback += OnDrawListHeader;
            drawElementCallback += OnDrawListElement;
            onAddCallback += OnAddListElement;
            onRemoveCallback += OnRemoveListElement;

            m_AddEvaluatorMenu = new GenericMenu();
            foreach (var type in XRTargetEvaluatorEditorUtility.GetEvaluatorInstanceTypes())
                m_AddEvaluatorMenu.AddItem(new GUIContent(type.Name), false, AddEvaluator, type);
        }
    }
}
