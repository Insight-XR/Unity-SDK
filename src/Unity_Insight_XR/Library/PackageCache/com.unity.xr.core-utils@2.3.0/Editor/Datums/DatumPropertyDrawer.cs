using UnityEditor;
using UnityEngine;

namespace Unity.XR.CoreUtils.Datums.Editor
{
    /// <summary>
    /// Base <see cref="DatumProperty{TValue,TDatum}"/> drawer used to better present the options between constants and <see cref="ScriptableObject"/> references.
    /// </summary>
    /// <seealso cref="PropertyDrawer"/>
    /// <seealso cref="DatumProperty{TValue,TDatum}"/>
    public abstract class DatumPropertyDrawer : PropertyDrawer
    {
        readonly string[] m_PopupOptions = { "Use Asset", "Use Value" };

        GUIStyle m_PopupStyle;

        static readonly GUIContent s_TempContent = new GUIContent();

        /// <summary>
        /// Calculates the height of the property based on the number of children of the property.
        /// </summary>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        /// <returns>The height in pixels.</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var selectedValue = GetSelectedProperty(property);
            if (selectedValue.hasVisibleChildren)
            {
                return EditorGUI.GetPropertyHeight(selectedValue, true); 
            }

            return base.GetPropertyHeight(property, label);
        }

        /// <summary>
        /// Draws the property using [immediate mode](xref:GUIScriptingGuide).
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (m_PopupStyle == null)
                m_PopupStyle = new GUIStyle("PaneOptions") { imagePosition = ImagePosition.ImageOnly };

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            label = EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            var useConstant = property.FindPropertyRelative("m_UseConstant");
            var selectedValue = GetSelectedProperty(property);

            // Calculate rect for configuration button
            var buttonRect = position;
            buttonRect.yMin += m_PopupStyle.margin.top + 1f;
            buttonRect.width = m_PopupStyle.fixedWidth + m_PopupStyle.margin.right;
            buttonRect.height = EditorGUIUtility.singleLineHeight;
            position.xMin = buttonRect.xMax;

            // Nudge foldout arrow to right a little more
            if (selectedValue.hasVisibleChildren)
                position.xMin += 12;

            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Using BeginProperty / EndProperty on the popup button allows the user to
            // revert prefab overrides to Use Reference by right-clicking the configuration button.
            EditorGUI.BeginProperty(buttonRect, GUIContent.none, useConstant);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var result = EditorGUI.Popup(buttonRect, useConstant.boolValue ? 1 : 0, m_PopupOptions, m_PopupStyle);
                if (check.changed)
                    useConstant.boolValue = result == 1;
            }
            EditorGUI.EndProperty();

            // When there are multiple serialized values in the constant value, it will be drawn under a foldout.
            // Create a label for it to make it easier to click and to give context about the type, similar to an asset reference.
            var valueLabel = selectedValue.hasVisibleChildren ? GetFoldoutLabel(selectedValue.type) : GUIContent.none;
            EditorGUI.PropertyField(position, selectedValue, valueLabel, true);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Gets the property that represents the correct variable reference value.
        /// </summary>
        /// <param name="property">The SerializedProperty that contains the variable reference values.</param>
        /// <returns>If the VariableReference is set to use a constant value, the constant value will be returned,
        /// otherwise the variable value will be returned.</returns>
        /// <seealso cref="DatumProperty{TValue,TDatum}"/>
        protected SerializedProperty GetSelectedProperty(SerializedProperty property)
        {
            var useConstant = property.FindPropertyRelative("m_UseConstant");
            var constantValue = property.FindPropertyRelative("m_ConstantValue");
            var variable = property.FindPropertyRelative("m_Variable");
            return useConstant.boolValue ? constantValue : variable;
        }

        static GUIContent GetFoldoutLabel(string type)
        {
            s_TempContent.text = "Value (" + ObjectNames.NicifyVariableName(type) + ")";
            return s_TempContent;
        }
    }
}
