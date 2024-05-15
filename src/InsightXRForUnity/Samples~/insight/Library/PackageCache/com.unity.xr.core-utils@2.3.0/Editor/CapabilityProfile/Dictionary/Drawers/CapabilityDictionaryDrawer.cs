using UnityEditor;
using UnityEngine;

namespace Unity.XR.CoreUtils.Capabilities.Editor
{
    // Ignores the CapabilityDictionary properties and draws its (SerializableDictionary) m_Items property instead
    [CustomPropertyDrawer(typeof(CapabilityDictionary))]
    sealed class CapabilityDictionaryDrawer : PropertyDrawer
    {
        const string k_ItemsPropertyPath = "m_Items";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative(k_ItemsPropertyPath), true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var itemsProperty = property.FindPropertyRelative(k_ItemsPropertyPath);
            EditorGUI.BeginProperty(position, label, itemsProperty);
            EditorGUI.PropertyField(position, itemsProperty, label);
            EditorGUI.EndProperty();
        }
    }
}
