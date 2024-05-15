using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CoreUtils.Capabilities.Editor
{
    [CustomPropertyDrawer(typeof(CapabilityDictionary.Item))]
    sealed class CapabilityDictionaryItemDrawer : PropertyDrawer
    {
        const float k_ItemHeight = 18;
        const float k_SpaceWidth = 6f;
        const float k_CapabilityValueWidth = 20f;
        const string k_CapabilityUndefinedMessage = "Capability not defined";

        class Styles
        {
            readonly GUIContent m_TempContent;

            internal readonly Texture WarningIcon;

            internal GUIContent TempContent(string text, string tooltip = "", Texture image = null)
            {
                m_TempContent.text = text;
                m_TempContent.tooltip = tooltip;
                m_TempContent.image = image;

                return m_TempContent;
            }

            internal Styles()
            {
                m_TempContent = new GUIContent();
                WarningIcon = EditorGUIUtility.IconContent("console.warnicon.sml@2x").image;
            }
        }

        static Styles s_Styles;

        static Styles styles
        {
            get
            {
                if (s_Styles == null)
                    s_Styles = new Styles();

                return s_Styles;
            }
        }

        static void DrawCapabilityKey(Rect position, SerializedProperty property)
        {
            // Caching the current capability value to avoid casts
            var currentCapability = property.stringValue;

            var buttonContent = CapabilityKeysDefinition.CapabilityKeys.Contains(currentCapability)
                ? styles.TempContent(currentCapability)
                : styles.TempContent(currentCapability, k_CapabilityUndefinedMessage, styles.WarningIcon);

            if (!EditorGUI.DropdownButton(position, buttonContent, FocusType.Keyboard))
                return;

            var menu = new GenericMenu();
            foreach (var capabilityKey in CapabilityKeysDefinition.CapabilityKeys)
            {
                var capability = capabilityKey;
                menu.AddItem(new GUIContent(capability), currentCapability == capability, () =>
                {
                    property.stringValue = capability;
                    property.serializedObject.ApplyModifiedProperties();

                    var capabilityProfile = property.serializedObject.targetObject as CapabilityProfile;
                    if (capabilityProfile != null)
                        capabilityProfile.ReportCapabilityChanged();
                });
            }

            menu.DropDown(position);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var keyRect = new Rect(position.x, position.y, position.width - k_CapabilityValueWidth - k_SpaceWidth, k_ItemHeight);
            var valueRect = new Rect(position.x + keyRect.width + k_SpaceWidth, position.y, k_CapabilityValueWidth, k_ItemHeight);

            DrawCapabilityKey(keyRect, property.FindPropertyRelative("Key"));
            EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("Value"), GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}
