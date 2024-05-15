using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEditor.XR.Interaction.Toolkit
{
    /// <summary>
    /// Custom property drawer for a <see cref="ClimbSettings"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(ClimbSettings))]
    public class ClimbSettingsPropertyDrawer : PropertyDrawer
    {
        const string k_AllowFreeXMovementPropertyPath = "m_AllowFreeXMovement";
        const string k_AllowFreeYMovementPropertyPath = "m_AllowFreeYMovement";
        const string k_AllowFreeZMovementPropertyPath = "m_AllowFreeZMovement";

        const float k_HelpBoxHeight = 30f;
        const string k_AllMovementRestrictedHelpMessage =
            "Allow movement along at least one axis to enable climb movement.";

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, true);
            if (!ShouldShowHelpMessage(property))
                return;

            var helpBoxPosition = position;
            helpBoxPosition.y += EditorGUI.GetPropertyHeight(property, true);
            helpBoxPosition.height = k_HelpBoxHeight;
            EditorGUI.HelpBox(helpBoxPosition, k_AllMovementRestrictedHelpMessage, MessageType.Warning);
        }

        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var basePropertyHeight = EditorGUI.GetPropertyHeight(property, true);
            return ShouldShowHelpMessage(property) ? basePropertyHeight + k_HelpBoxHeight : basePropertyHeight;
        }

        static bool ShouldShowHelpMessage(SerializedProperty property)
        {
            if (!property.isExpanded)
                return false;

            var allowFreeXMovement = property.FindPropertyRelative(k_AllowFreeXMovementPropertyPath);
            var allowFreeYMovement = property.FindPropertyRelative(k_AllowFreeYMovementPropertyPath);
            var allowFreeZMovement = property.FindPropertyRelative(k_AllowFreeZMovementPropertyPath);
            return !allowFreeXMovement.boolValue && !allowFreeYMovement.boolValue && !allowFreeZMovement.boolValue;
        }
    }
}