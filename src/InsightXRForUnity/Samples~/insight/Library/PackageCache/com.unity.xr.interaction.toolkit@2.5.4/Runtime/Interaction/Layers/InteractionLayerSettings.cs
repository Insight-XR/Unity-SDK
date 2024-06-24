using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Configuration class for interaction layers.
    /// Stores all interaction layers.
    /// </summary>
    [ScriptableSettingsPath(ProjectPath.k_XRInteractionSettingsFolder)]
    class InteractionLayerSettings : ScriptableSettings<InteractionLayerSettings>, ISerializationCallbackReceiver
    {
        const string k_DefaultLayerName = "Default";

        internal const int layerSize = 32;
        internal const int builtInLayerSize = 1;

        [SerializeField]
        string[] m_LayerNames;

        /// <summary>
        /// Check if the interaction layer name at the supplied index is empty.
        /// </summary>
        /// <param name="index">The index of the target interaction layer.</param>
        /// <returns>Returns <see langword="true"/> if the target interaction layer is empty.</returns>
        internal bool IsLayerEmpty(int index)
        {
            return string.IsNullOrEmpty(m_LayerNames[index]);
        }

        /// <summary>
        /// Sets the interaction layer name at the supplied index.
        /// </summary>
        /// <param name="index">The index of the target interaction layer.</param>
        /// <param name="layerName">The name of the target interaction layer.</param>
        internal void SetLayerNameAt(int index, string layerName)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Interaction Layer");
#endif
            m_LayerNames[index] = layerName;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Gets the interaction layer name at the supplied index.
        /// </summary>
        /// <param name="index">The index of the target interaction layer.</param>
        /// <returns>Returns the target interaction layer name.</returns>
        internal string GetLayerNameAt(int index)
        {
            return m_LayerNames[index];
        }

        /// <summary>
        /// Gets the value (or bit index) of the supplied interaction layer name.
        /// </summary>
        /// <param name="layerName">The name of the interaction layer to search for its value.</param>
        /// <returns>Returns the interaction layer value.</returns>
        internal int GetLayer(string layerName)
        {
            for (var i = 0; i < m_LayerNames.Length; i++)
            {
                if (string.Equals(layerName, m_LayerNames[i]))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Fills in the supplied lists with the interaction layer name and its correspondent value in the same index.
        /// </summary>
        /// <param name="names">The list to fill in with interaction layer names.</param>
        /// <param name="values">The list to fill in with interaction layer values.</param>
        internal void GetLayerNamesAndValues(List<string> names, List<int> values)
        {
            for (var i = 0; i < m_LayerNames.Length; i++)
            {
                var layerName = m_LayerNames[i];
                if (string.IsNullOrEmpty(layerName))
                    continue;

                names.Add(layerName);
                values.Add(i);
            }
        }

        /// <inheritdoc />
        public void OnBeforeSerialize()
        {
            if (m_LayerNames == null)
                m_LayerNames = new string[layerSize];

            if (m_LayerNames.Length != layerSize)
                Array.Resize(ref m_LayerNames, layerSize);

            if (!string.Equals(m_LayerNames[0], k_DefaultLayerName))
                m_LayerNames[0] = k_DefaultLayerName;
        }

        /// <inheritdoc />
        public void OnAfterDeserialize()
        {
        }
    }
}
