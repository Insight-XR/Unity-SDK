#region building_blocks_sample
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils.Editor.BuildingBlocks;
using UnityEditor;
using UnityEngine;

class ScriptedBuildingBlockSample : IBuildingBlock
{
    const string k_Id = "Scripted Building Block";
    const string k_BuildingBlockPath = "GameObject/MySection/"+k_Id;
    const string k_IconPath = "buildingblockIcon";
    const string k_Tooltip = "My Scripted Building Block tooltip";
    const int k_SectionPriority = 10;

    public string Id => k_Id;
    public string IconPath => k_IconPath;
    public bool IsEnabled => true;
    public string Tooltip => k_Tooltip;

    static void DoInterestingStuff()
    {
        var createdInstance = new GameObject("Empty Object");
        // Do more interesting stuff her
    }

    public void ExecuteBuildingBlock() => DoInterestingStuff();

    // Each building block should have an accompanying MenuItem as a good practice, we add them here.
    [MenuItem(k_BuildingBlockPath, false, k_SectionPriority)]
    public static void ExecuteMenuItem(MenuCommand command) => DoInterestingStuff();
}

[BuildingBlockItem(Priority = k_SectionPriority)]
class BuildingBlockSection1 : IBuildingBlockSection
{
    const string k_SectionId = "My Block Section";
    public string SectionId => k_SectionId;

    const string k_SectionIconPath = "Building/Block/Section/Icon/Path";
    public string SectionIconPath => k_SectionIconPath;
    const int k_SectionPriority = 1;


    string m_PrefabAssetPath = "Assets/Prefabs/SmallCube.prefab";
    GameObject m_Prefab1;

    static PrefabCreatorBuildingBlock s_Prefab1BuildingBlock;
    const int k_Prefab1BuildingBlockPriority = 10;
    const string k_Prefab1BuildingBlockPath = "GameObject/MySection/" + k_SectionId;

    // We add this Menu Item to the prefab building block here.
    [MenuItem(k_Prefab1BuildingBlockPath, false, k_Prefab1BuildingBlockPriority)]
    public static void ExecuteMenuItem(MenuCommand command) => s_Prefab1BuildingBlock.ExecuteBuildingBlock();

    readonly IBuildingBlock[] m_BBlocksElementIds = new IBuildingBlock[]
    {
        new ScriptedBuildingBlockSample()
    };

    public IEnumerable<IBuildingBlock> GetBuildingBlocks()
    {
        if (string.IsNullOrEmpty(m_PrefabAssetPath))
            return m_BBlocksElementIds;

        //Using the already defined Building Block `PrefabCreatorBuildingBlock` and creating an instance of it with a prefab
        s_Prefab1BuildingBlock = new PrefabCreatorBuildingBlock(m_PrefabAssetPath, "Prefab Creator Block", "an/Icon/Path");

        var elements = m_BBlocksElementIds.ToList();
        elements.Add(s_Prefab1BuildingBlock);
        return  elements;
    }
}
#endregion
