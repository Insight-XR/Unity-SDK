using UnityEditor;
using UnityEngine;

namespace Unity.XR.CoreUtils.Editor.BuildingBlocks
{
    /// <summary>
    /// This Building Block can be used in a Building Block Section to instantiate a prefab.
    /// The Building Block Section is in charge of setting the prefab to instantiate using that Building Block as well
    /// as setting a unique and recognizable name and icon for this Building Block.
    /// </summary>
    public class PrefabCreatorBuildingBlock : IBuildingBlock
    {
        string m_Id;
        string m_IconPath;
        bool m_IsEnabled;
        string m_Tooltip;
        GameObject m_Prefab = null;
        string m_PrefabPath;

        /// <inheritdoc />
        public string Id => m_Id;

        /// <inheritdoc />
        public string IconPath => m_IconPath;

        /// <inheritdoc />
        public string Tooltip => m_Tooltip;

        /// <inheritdoc />
        public bool IsEnabled => m_IsEnabled;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="prefabPath">The path for the prefab to be created.</param>
        /// <param name="buildingBlockId">The name of this Building Block. This id will be used to show this Building Block in the UI.</param>
        /// <param name="buildingBlockIconPath">The path to the icon of this Building Block in the UI. This icon should be placed in a Resources folder.</param>
        /// <param name="isEnabled">Whether the user know if this Building Block is enabled or disabled. If disabled, the Building Block will be grayed out in the UI.</param>
        /// <param name="tooltip">Description of the Building Block. This description will be displayed as a tooltip in the UI.</param>
        public PrefabCreatorBuildingBlock(string prefabPath, string buildingBlockId = "Prefab Creator", string buildingBlockIconPath = null, bool isEnabled = true, string tooltip = "")
        {
            m_PrefabPath = prefabPath;
            m_Id = buildingBlockId;
            m_IconPath = buildingBlockIconPath;
            m_IsEnabled = isEnabled;
            m_Tooltip = tooltip;
        }

        /// <inheritdoc />
        public void ExecuteBuildingBlock()
        {
            // Do lazy loading of the asset since AssetDatabase is non deterministic and can cause false positives when starting a new project
            if (m_Prefab == null)
            {
                m_Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
                if (m_Prefab == null)
                {
                    Debug.LogError("Building block cannot find prefab at path: " + m_PrefabPath + "\nDid it get moved?");
                    return;
                }
            }

            var objName = GameObjectUtility.GetUniqueNameForSibling(null,m_Prefab.name);
            var createdObj = Object.Instantiate(m_Prefab);
            createdObj.name = objName;

            Undo.RegisterCreatedObjectUndo(createdObj, $"Create {objName}");
            Selection.activeGameObject = createdObj;
        }
    }
}
