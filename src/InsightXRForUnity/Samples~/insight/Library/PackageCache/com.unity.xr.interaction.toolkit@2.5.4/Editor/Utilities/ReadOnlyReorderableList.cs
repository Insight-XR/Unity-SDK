using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditor.XR.Interaction.Toolkit.Utilities
{
    /// <summary>
    /// ReorderableList that draw readonly items in the Inspector.
    /// Users cannot edit the content of the items but they can change the order of the items in this list.
    /// </summary>
    /// <typeparam name="T">The list element type.</typeparam>
    class ReadOnlyReorderableList<T> : ReorderableList
    {
        readonly List<T> m_Elements;
        readonly GUIContent m_HeaderContent;
        readonly string m_HeaderSessionStateKey;

        public Action<List<T>> updateElements { get; set; }
        public Action<T, int> onListReordered { get; set; }
        public bool isExpanded { get; set; }

        public ReadOnlyReorderableList(List<T> elements, GUIContent headerContent, string headerSessionStateKey)
            : base(elements, typeof(T), true, true, false, false)
        {
            m_Elements = elements;
            m_HeaderContent = headerContent;
            m_HeaderSessionStateKey = headerSessionStateKey;
            drawElementCallback += OnDrawListElement;
            onReorderCallbackWithDetails += OnReorderList;
            elementHeight = EditorGUIUtility.singleLineHeight;
            footerHeight = 0f;
            headerHeight = 1f;
        }

        public new void DoLayoutList()
        {
            updateElements?.Invoke(m_Elements);
            OnDrawHeader();
            if (isExpanded)
                base.DoLayoutList();
        }

        void OnDrawHeader()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                isExpanded = EditorGUILayout.Foldout(isExpanded, m_HeaderContent, true);
                if (check.changed)
                    SessionState.SetBool(m_HeaderSessionStateKey, isExpanded);
            }
        }

        void OnDrawListElement(Rect rect, int elementIndex, bool isActive, bool isFocused)
        {
            var element = list[elementIndex];
            rect.yMin += 1;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.ObjectField(rect, $"Element {elementIndex}", element as Object, typeof(T), true);
            }
        }

        void OnReorderList(ReorderableList reorderableList, int oldIndex, int newIndex)
        {
            // The list has already been reordered when this callback is invoked,
            // so obtain the element that was moved using the new index.
            if (list[newIndex] is T element)
                onListReordered?.Invoke(element, newIndex);
        }
    }
}
