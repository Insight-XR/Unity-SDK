using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace UnityEditor.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Custom editor for an <see cref="XRTargetFilter"/>.
    /// </summary>
    [CustomEditor(typeof(XRTargetFilter))]
    class XRTargetFilterEditor : Editor
    {
        XRTargetEvaluatorList m_EvaluatorList;

#if UNITY_2021_2_OR_NEWER
        XRMissingEvaluatorTypeList m_MissingEvaluatorTypeList;
#endif

        void OnEnable()
        {
            var evaluators = serializedObject.FindProperty("m_Evaluators");
            m_EvaluatorList = new XRTargetEvaluatorList(serializedObject, evaluators);

#if UNITY_2021_2_OR_NEWER
            m_MissingEvaluatorTypeList = XRMissingEvaluatorTypeList.CreateList(target as XRTargetFilter);
#endif
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
#if UNITY_2021_2_OR_NEWER
            if (m_MissingEvaluatorTypeList != null && m_MissingEvaluatorTypeList.count > 0)
            {
                using (new EditorGUI.DisabledScope(true))
                    DrawEvaluatorList();

                DrawMissingEvaluatorTypesMessage();
                DrawMissingEvaluatorList();
                DrawSelectedMissingEvaluatorInspector();
            }
            else
            {
                DrawEvaluatorList();
                DrawSelectedEvaluatorInspector();
            }
#else
            DrawEvaluatorList();
            DrawSelectedEvaluatorInspector();
#endif
        }

        /// <summary>
        /// Draw the evaluator list
        /// </summary>
        void DrawEvaluatorList()
        {
            using (var guiChangeCheck = new EditorGUI.ChangeCheckScope())
            {
                serializedObject.Update();
                m_EvaluatorList.DoLayoutList();
                if (guiChangeCheck.changed)
                    serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Draw the selected evaluator inspector
        /// </summary>
        void DrawSelectedEvaluatorInspector()
        {
            var index = m_EvaluatorList.index;
            if (index >= 0 & index < m_EvaluatorList.count)
                m_EvaluatorList.DrawListElementInspectorGUI(index);
        }

#if UNITY_2021_2_OR_NEWER
        /// <summary>
        /// Draw the missing evaluator types message
        /// </summary>
        void DrawMissingEvaluatorTypesMessage()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Some associated evaluator scripts cannot be loaded." +
                                    "\nPlease fix any compile errors and do the following options:" +
                                    "\n1 - If you renamed the class, namespace or moved the type to another assembly then decorate the class with MovedFromAttribute." +
                                    "\n2 - Remove any deleted scripts from the Missing Evaluator Types list below.",  MessageType.Warning);
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Draw the missing evaluator type list
        /// </summary>
        void DrawMissingEvaluatorList()
        {
            m_MissingEvaluatorTypeList.DoLayoutList();
        }

        /// <summary>
        /// Draw the selected missing evaluator type inspector
        /// </summary>
        void DrawSelectedMissingEvaluatorInspector()
        {
            var index = m_MissingEvaluatorTypeList.index;
            if (index >= 0 & index < m_MissingEvaluatorTypeList.count)
                m_MissingEvaluatorTypeList.DrawListElementInspectorGUI(index);
        }
#endif
    }
}
