using System.Collections.Generic;
using UnityEditor.XR.Interaction.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace UnityEditor.XR.Interaction.Toolkit
{
    /// <summary>
    /// Custom editor for an <see cref="XRInteractionManager"/>.
    /// </summary>
    [CustomEditor(typeof(XRInteractionManager), true), CanEditMultipleObjects]
    public class XRInteractionManagerEditor : BaseInteractionEditor
    {
        const string k_FiltersExpandedKey = "XRI." + nameof(XRInteractionManagerEditor) + ".FiltersExpanded";
        const string k_HoverFiltersExpandedKey = "XRI." + nameof(XRInteractionManagerEditor) + ".HoverFiltersExpanded";
        const string k_SelectFiltersExpandedKey = "XRI." + nameof(XRInteractionManagerEditor) + ".SelectFiltersExpanded";

        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractionManager.startingHoverFilters"/>.</summary>
        protected SerializedProperty m_StartingHoverFilters;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractionManager.startingSelectFilters"/>.</summary>
        protected SerializedProperty m_StartingSelectFilters;

        bool m_FiltersExpanded;
        ReadOnlyReorderableList<IXRHoverFilter> m_HoverFilters;
        ReadOnlyReorderableList<IXRSelectFilter> m_SelectFilters;

        /// <summary>
        /// Contents of GUI elements used by this editor.
        /// </summary>
        protected static class Contents
        {
            /// <summary>The global filters foldout.</summary>
            public static readonly GUIContent globalFilters = EditorGUIUtility.TrTextContent("Global Filters", "Add filters to extend this object without needing to create a derived behavior.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractionManager.startingSelectFilters"/>.</summary>
            public static readonly GUIContent startingHoverFilters = EditorGUIUtility.TrTextContent("Starting Hover Filters", "The hover filters that this manager will automatically link at startup (optional, may be empty). Used as additional global hover validations for this manager.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractionManager.startingSelectFilters"/>.</summary>
            public static readonly GUIContent startingSelectFilters = EditorGUIUtility.TrTextContent("Starting Select Filters", "The select filters that this manager will automatically link at startup (optional, may be empty). Used as additional global select validations for this manager.");
            /// <summary>The list of runtime hover filters.</summary>
            public static readonly GUIContent hoverFiltersHeader = EditorGUIUtility.TrTextContent("Hover Filters", "The list is populated in Awake, during Play mode.");
            /// <summary>The list of runtime select filters.</summary>
            public static readonly GUIContent selectFiltersHeader = EditorGUIUtility.TrTextContent("Select Filters", "The list is populated in Awake, during Play mode.");
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        /// <seealso cref="MonoBehaviour"/>
        protected virtual void OnEnable()
        {
            m_FiltersExpanded = SessionState.GetBool(k_FiltersExpandedKey, false);

            m_StartingHoverFilters = serializedObject.FindProperty("m_StartingHoverFilters");
            m_StartingSelectFilters = serializedObject.FindProperty("m_StartingSelectFilters");

            var manager = (XRInteractionManager)target;
            m_HoverFilters = new ReadOnlyReorderableList<IXRHoverFilter>(new List<IXRHoverFilter>(), Contents.hoverFiltersHeader, k_HoverFiltersExpandedKey)
            {
                isExpanded = SessionState.GetBool(k_HoverFiltersExpandedKey, true),
                updateElements = list => manager.hoverFilters.GetAll(list),
                onListReordered = (element, newIndex) => manager.hoverFilters.MoveTo(element, newIndex),
            };

            m_SelectFilters = new ReadOnlyReorderableList<IXRSelectFilter>(new List<IXRSelectFilter>(), Contents.selectFiltersHeader, k_SelectFiltersExpandedKey)
            {
                isExpanded = SessionState.GetBool(k_SelectFiltersExpandedKey, true),
                updateElements = list => manager.selectFilters.GetAll(list),
                onListReordered = (element, newIndex) => manager.selectFilters.MoveTo(element, newIndex),
            };
        }

        /// <summary>
        /// This method is automatically called by <see cref="BaseInteractionEditor.OnInspectorGUI"/> to
        /// draw the custom inspector. Override this method to customize the
        /// inspector as a whole.
        /// </summary>
        protected override void DrawInspector()
        {
            DrawBeforeProperties();
            DrawProperties();
            DrawDerivedProperties();
            DrawFilters();
        }

        /// <summary>
        /// This method is automatically called by <see cref="DrawInspector"/> to
        /// draw the section of the custom inspector before <see cref="DrawProperties"/>.
        /// By default, this draws the read-only Script property.
        /// </summary>
        protected virtual void DrawBeforeProperties()
        {
            DrawScript();
        }

        /// <summary>
        /// This method is automatically called by <see cref="DrawInspector"/> to
        /// draw the property fields. Override this method to customize the
        /// properties shown in the Inspector. This is typically the method overridden
        /// when a derived behavior adds additional serialized properties
        /// that should be displayed in the Inspector.
        /// </summary>
        protected virtual void DrawProperties()
        {
        }

        /// <summary>
        /// Draw the Global Filter foldout.
        /// </summary>
        protected virtual void DrawFilters()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_FiltersExpanded = EditorGUILayout.Foldout(m_FiltersExpanded, Contents.globalFilters, true);
                if (check.changed)
                    SessionState.SetBool(k_FiltersExpandedKey, m_FiltersExpanded);
            }

            if (!m_FiltersExpanded)
                return;

            using (new EditorGUI.IndentLevelScope())
            {
                DrawFiltersNested();
            }
        }

        /// <summary>
        /// Draw the property fields related to the hover and select filters.
        /// </summary>
        protected virtual void DrawFiltersNested()
        {
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                EditorGUILayout.PropertyField(m_StartingHoverFilters, Contents.startingHoverFilters);
                EditorGUILayout.PropertyField(m_StartingSelectFilters, Contents.startingSelectFilters);
            }

            if (!Application.isPlaying)
                return;

            if (serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.HelpBox("Filters cannot be multi-edited.", MessageType.None);
                return;
            }

            m_HoverFilters.DoLayoutList();
            m_SelectFilters.DoLayoutList();
        }
    }
}
