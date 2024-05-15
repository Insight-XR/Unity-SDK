using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace UnityEditor.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Draws the <c>m_Enabled</c> serialized field of the Target Evaluators in the Inspector.
    /// Changes to this property drawer at runtime invokes OnEnable and OnDisable in the evaluator instance if the
    /// filter is a valid runtime instance.
    /// </summary>
    /// <see cref="XRTargetEvaluator"/>
    /// <see cref="XRTargetEvaluatorEnabledAttribute"/>
    [CustomPropertyDrawer(typeof(XRTargetEvaluatorEnabledAttribute))]
    class XRTargetEvaluatorEnabledDrawer : PropertyDrawer
    {
        const string k_StartPropertyPath = "m_Evaluators.Array.data[";
        const string k_EndPropertyPath = "].m_Enabled";

        /// <summary>
        /// Gets the index of the evaluator in its filter.
        /// </summary>
        /// <param name="propertyPath">The <c>m_Enabled</c> property path of the evaluator.</param>
        /// <returns>The index of the evaluator.</returns>
        static int GetEvaluatorIndex(string propertyPath)
        {
            var startIndex = k_StartPropertyPath.Length;
            var endIndex = propertyPath.IndexOf(k_EndPropertyPath, startIndex, StringComparison.Ordinal);
            var indexAsString = propertyPath.Substring(startIndex, endIndex - startIndex);
            if (int.TryParse(indexAsString, out var index))
                return index;

            return -1;
        }

        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var filter = property.serializedObject.targetObject as XRTargetFilter;
            if (filter == null)
            {
                Debug.LogError($"Couldn't retrieve a valid filter at property {property.propertyPath}.");
                return;
            }

            if (XRTargetEvaluatorEditorUtility.IsRuntimeInstance(filter))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                using (new EditorGUI.PropertyScope(position, label, property))
                {
                    var newValue = EditorGUI.Toggle(position, label, property.boolValue);

                    if (check.changed)
                    {
                        var evaluatorIndex = GetEvaluatorIndex(property.propertyPath);
                        if (evaluatorIndex >= 0)
                        {
                            var evaluators = filter.evaluators;
                            if (evaluatorIndex < evaluators.Count && evaluators[evaluatorIndex] != null)
                                evaluators[evaluatorIndex].enabled = newValue;
                            else
                                Debug.LogError($"Couldn't retrieve a valid evaluator at property {property.propertyPath}.", filter);
                        }
                        else
                        {
                            Debug.LogError($"Couldn't retrieve the evaluator index at property {property.propertyPath}.");
                        }
                    }
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}
