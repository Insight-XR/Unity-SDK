#if UNITY_2021_2_OR_NEWER
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace UnityEditor.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Editor for the list of missing evaluator types.
    /// Used to draw the missing evaluator list of a Target Filter in the Inspector.
    /// </summary>
    class XRMissingEvaluatorTypeList : ReorderableList
    {
        /// <summary>
        /// Creates and returns a list containing the missing evaluator types for the given filter, returns <see langword="null"/> if the filter has no missing types.
        /// </summary>
        /// <param name="targetFilter">The filter to load the missing types.</param>
        /// <returns>Returns a missing evaluator list editor for the given filter. Returns <see langword="null"/> if the given filter has no missing types.</returns>
        public static XRMissingEvaluatorTypeList CreateList(XRTargetFilter targetFilter)
        {
            if (targetFilter == null || !SerializationUtility.HasManagedReferencesWithMissingTypes(targetFilter))
                return null;

            var missingReferences = SerializationUtility.GetManagedReferencesWithMissingTypes(targetFilter);
            return new XRMissingEvaluatorTypeList(targetFilter, missingReferences);
        }

        readonly XRTargetFilter m_TargetFilter;
        readonly GenericMenu m_RemoveOrReplaceMissingTypeMenu;

        void OnDrawListHeader(Rect rect)
        {
            GUI.Label(rect, "Missing Evaluator Types");
        }

        void OnDrawListElement(Rect rect, int i, bool isactive, bool isfocused)
        {
            var element = (ManagedReferenceMissingType)list[i];
            EditorGUI.LabelField(rect, element.className);
        }

        void OnRemoveListElement(ReorderableList reorderableList)
        {
            RemoveSelectedElement();
        }

        void RemoveSelectedElement()
        {
            if (index > 0 || index >= list.Count || m_TargetFilter == null)
                return;

            var element = (ManagedReferenceMissingType)list[index];
            SerializationUtility.ClearManagedReferenceWithMissingType(m_TargetFilter, element.referenceId);
            UpdateMissingTypeList();
        }

        void UpdateMissingTypeList()
        {
            if (m_TargetFilter == null || !SerializationUtility.HasManagedReferencesWithMissingTypes(m_TargetFilter))
            {
                list = new ManagedReferenceMissingType[0];
                index = -1;
                return;
            }

            list = SerializationUtility.GetManagedReferencesWithMissingTypes(m_TargetFilter);
        }

        /// <summary>
        /// Call this to draw the class, namespace and assembly name of the missing evaluator type at the given index in the Inspector.
        /// </summary>
        /// <param name="elementIndex">Index of the missing evaluator type to draw.</param>
        public void DrawListElementInspectorGUI(int elementIndex)
        {
            var element = (ManagedReferenceMissingType)list[elementIndex];

            EditorGUILayout.Space();

            // Draw the class name
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Class");
                EditorGUILayout.LabelField(element.className);
            }

            // Draw the namespace name
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Namespace");
                EditorGUILayout.LabelField(element.namespaceName);
            }

            // Draw the assembly name
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Assembly");
                EditorGUILayout.LabelField(element.assemblyName);
            }
        }

        XRMissingEvaluatorTypeList(XRTargetFilter targetFilter, ManagedReferenceMissingType[] elements)
            : base(elements, typeof (ManagedReferenceMissingType), false, true, false, true)
        {
            m_TargetFilter = targetFilter;

            drawHeaderCallback += OnDrawListHeader;
            drawElementCallback += OnDrawListElement;
            onRemoveCallback += OnRemoveListElement;
            index = 0;
        }
    }
}
#endif
