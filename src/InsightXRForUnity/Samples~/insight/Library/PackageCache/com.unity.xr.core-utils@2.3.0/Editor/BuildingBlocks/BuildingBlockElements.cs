using System;
using System.Collections.Generic;

namespace Unity.XR.CoreUtils.Editor.BuildingBlocks
{
    /// <summary>
    /// A Building Block defines a reusable block developers will be using often in their applications.
    /// To accelerate development and improve the user experience, these Building Blocks are regrouped in a common overlay
    /// in the Scene View allowing the user to easily access and drop these elements in the current scene.
    /// Implement this interface to define a Building Block. In order to add this block to the UI, this Building Block
    /// should either be returned by an <see cref="IBuildingBlockSection.GetBuildingBlocks"/> enumerable (common case),
    /// or else identified as an unsectioned Building Block by directly using the <see cref="BuildingBlockItemAttribute"/>
    /// on the Building Block class.
    /// </summary>
    public interface IBuildingBlock
    {
        /// <summary>
        /// The name of this Building Block. This id will be used to show this Building Block in the UI.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The path to the icon of this Building Block in the UI. This icon should be placed in a Resources folder.
        /// </summary>
        string IconPath { get; }

        /// <summary>
        /// Whether the user know if this Building Block is enabled or disabled. If disabled, the Building Block will be grayed out in the UI.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Description of the Building Block. This description will be displayed as a tooltip in the UI.
        /// </summary>
        string Tooltip { get; }

        /// <summary>
        /// Implement this method to execute the actions associated to this Building Block.
        /// </summary>
        void ExecuteBuildingBlock();
    }

    /// <summary>
    /// A Building Block Section defines a group of Building Block from the same area. The building Blocks of that
    /// section are grouped together in the UI to improve discoverability and ease the user experience.
    /// A section must also use the <see cref="BuildingBlockItemAttribute"/> attribute to be registered by
    /// the <see cref="BuildingBlockManager"/>.
    /// Note : If the <see cref="IBuildingBlockSection.GetBuildingBlocks"/> returns an empty set of Building Blocks,
    /// the section will not be displayed in the overlay. Only non-empty sections will be displayed.
    /// </summary>
    public interface IBuildingBlockSection
    {
        /// <summary>
        /// The name of this Building Block Section. This id will be used to show this Section in the UI.
        /// </summary>
        string SectionId { get; }

        /// <summary>
        /// The path to the icon of this Building Block in the UI. This icon should be placed in a Resources folder.
        /// </summary>
        string SectionIconPath { get; }

        /// <summary>
        /// Implement this method to return the collection of Building Blocks composing this section.
        /// </summary>
        /// <returns>The list of Building Blocks instances composing this section.</returns>
        IEnumerable<IBuildingBlock> GetBuildingBlocks();
    }

    /// <summary>
    /// Attribute used to register a class as a Building Block Section or a Building Block.
    /// Using this attribute directly on a Building Block will register the class in the manager as an unsectioned
    /// Building Block.
    /// Unsectioned building blocks which will display blocks outside sections in the interface. This is not recommended since
    /// it makes it prone to confusion with other packages.
    /// The target class must derive from either <see cref="IBuildingBlockSection"/> or <see cref="IBuildingBlock"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BuildingBlockItemAttribute : Attribute
    {
        /// <summary>
        /// The priority of this item used to order elements in the overlay.
        /// Unsectioned Building Blocks will be ordered and displayed together first, then all the Building Block Sections
        /// will follow and be ordered between them.
        /// </summary>
        public int Priority = 0;
    }
}
