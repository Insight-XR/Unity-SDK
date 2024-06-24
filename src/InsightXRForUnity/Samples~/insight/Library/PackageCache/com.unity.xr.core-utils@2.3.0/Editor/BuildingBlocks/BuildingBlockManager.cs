using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CoreUtils.Editor.BuildingBlocks
{
    /// <summary>
    /// This static class handles the management if the building blocks defined by the users.
    /// All classes using the <see cref="BuildingBlockItemAttribute"/> are retrieved. Then, all sections are created here
    /// and stored for later use. Unsectioned building blocks (building blocks with no section) are also identified here and
    /// added to a special section.
    /// The internal user can retrieve these 2 sets of building blocks using the following utility methods :
    /// <see cref="BuildingBlockManager.GetUnsectionedBuildingBlocks"/> and <see cref="BuildingBlockManager.GetSections"/>.
    /// These methods are internal as only the Overlay creator needs to access to this information for now.
    /// </summary>
    static class BuildingBlockManager
    {
        /// <summary>
        /// Internal class created for a special Section containing all the Building BLocks without any sections (Unsectioned).
        /// This building blocks are referenced in the user code by using the <see cref="BuildingBlockItemAttribute"/> on
        /// the Building Block class definition.
        /// </summary>
        class UnsectionedBuildingBlocksSection : IBuildingBlockSection
        {
            public string SectionId => null;
            public string SectionIconPath => null;

            internal List<IBuildingBlock> m_UnsectionedBuildingBlocks = new List<IBuildingBlock>();
            public IEnumerable<IBuildingBlock> GetBuildingBlocks() => m_UnsectionedBuildingBlocks;
        }

        static List<IBuildingBlockSection> s_Sections;
        static UnsectionedBuildingBlocksSection s_UnsectionedSection;

        /// <summary>
        /// Constructor; here we create all the data structures and fill them with existing building blocks.
        /// </summary>
        static BuildingBlockManager()
        {
            s_Sections = new List<IBuildingBlockSection>();
            s_UnsectionedSection = null;
            var sectionsIds = new List<string>();
            var attributesToSections = new List<(BuildingBlockItemAttribute attribute, IBuildingBlockSection section)>();

            var buildingBlocksItemTypes = TypeCache.GetTypesWithAttribute<BuildingBlockItemAttribute>();

            var unsectionedTypesAndAttributes = new List<(Type type, BuildingBlockItemAttribute attribute)>();
            for (int i = 0; i < buildingBlocksItemTypes.Count; ++i)
            {
                var itemType = buildingBlocksItemTypes[i];
                // skip the item if the class is abstract or static
                if (itemType.IsAbstract)
                    continue;

                if (typeof(IBuildingBlockSection).IsAssignableFrom(itemType))
                {
                    var section = (IBuildingBlockSection)Activator.CreateInstance(itemType);
                    var id = section.SectionId;
                    if (string.IsNullOrEmpty(id))
                    {
                        // Skipping the unsectioned Section as this is not a regular section
                        if (itemType != typeof(UnsectionedBuildingBlocksSection))
                            Debug.LogWarning(
                                $"Building Blocks Section with null or empty id are not valid. " +
                                $"The section type {itemType} will be skipped.");
                        continue;
                    }

                    var elements = section.GetBuildingBlocks();
                    // Only adding sections containing elements
                    if (elements != null && elements.Any())
                    {
                        sectionsIds.Add(id);
                        attributesToSections.Add((GetAttribute(itemType), section));
                    }
                }
                else if (typeof(IBuildingBlock).IsAssignableFrom(itemType))
                    unsectionedTypesAndAttributes.Add((itemType, GetAttribute(itemType)));
            }

            attributesToSections.Sort((el1, el2) => el1.attribute.Priority.CompareTo(el2.attribute.Priority));
            foreach (var (_, section) in attributesToSections)
                s_Sections.Add(section);

            //Adding building blocks without section
            if (unsectionedTypesAndAttributes.Count > 0)
            {
                unsectionedTypesAndAttributes.Sort((el1, el2) => el1.attribute.Priority.CompareTo(el2.attribute.Priority));

                s_UnsectionedSection = new UnsectionedBuildingBlocksSection();
                foreach (var bblockType in unsectionedTypesAndAttributes)
                {
                    var bblockInstance = (IBuildingBlock)Activator.CreateInstance(bblockType.type);
                    s_UnsectionedSection.m_UnsectionedBuildingBlocks.Add(bblockInstance);
                }
            }
        }

        static BuildingBlockItemAttribute GetAttribute(Type type)
        {
            return (BuildingBlockItemAttribute)type.GetCustomAttributes(typeof(BuildingBlockItemAttribute), false)[0];
        }

        /// <summary>
        /// Method to get the unsectioned building blocks from the internal section <see cref="UnsectionedBuildingBlocksSection"/>.
        /// </summary>
        /// <param name="unsectionedBuildingBlocks">A list of unsectioned building blocks to populate.</param>
        internal static void GetUnsectionedBuildingBlocks(List<IBuildingBlock> unsectionedBuildingBlocks)
        {
            if (unsectionedBuildingBlocks == null)
                return;

            unsectionedBuildingBlocks.Clear();
            if (s_UnsectionedSection == null)
                return;

            var unsectionedBlocks = s_UnsectionedSection.GetBuildingBlocks();
            foreach (var unsectionedBlock in unsectionedBlocks)
                unsectionedBuildingBlocks.Add(unsectionedBlock);
        }

        /// <summary>
        /// Method to get the building block sections from the manager.
        /// </summary>
        /// <param name="sections">A list of building block sections to populate.</param>
        internal static void GetSections(List<IBuildingBlockSection> sections)
        {
            if (sections == null)
                return;

            sections.Clear();
            foreach (var section in s_Sections)
                sections.Add(section);
        }
    }
}
