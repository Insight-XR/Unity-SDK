#if UNITY_2022_1_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.XR.CoreUtils.Capabilities.Editor;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

#if ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
using Unity.XR.CoreUtils.Editor.Analytics;
#endif

namespace Unity.XR.CoreUtils.Editor.BuildingBlocks
{
    /// <summary>
    /// This overlay present the Building Block elements in the Scene View.
    /// The different sections are presented as dropdowns (toolbar mode) or foldouts (panel mode) containing all
    /// the associated Building Blocks options. The unsectioned (blocks that dont belong to any section) Building Blocks
    /// are presented separately.
    /// </summary>
    [Overlay(typeof(SceneView), "XR Building Blocks")]
    internal class BuildingBlocksOverlay : Overlay, ICreateToolbar
    {
        internal static readonly string k_OverlayStyleSheet = "BuildingBlockOverlay";
        static readonly string k_OverlayClass = "building-block-overlay";
        static readonly string k_NoBuildingBlockMessage = L10n.Tr("No building blocks are defined.");

        const int k_MaxButtonTextLength = 21;

        VisualElement m_Root;

        public override VisualElement CreatePanelContent()
        {
            m_Root = new VisualElement();

            // We do this on a delay call since there could be PrefabCreator Building blocks and when projects are opened
            // for the first time the assets will not be found
            EditorApplication.delayCall += () =>
            {
                var styleSheet = Resources.Load(k_OverlayStyleSheet) as StyleSheet;
                m_Root.styleSheets.Add(styleSheet);

                m_Root.AddToClassList(k_OverlayClass);

                var list = new ScrollView(ScrollViewMode.Vertical);
                list.verticalScrollerVisibility = ScrollerVisibility.Hidden;

                var hasElement = CreateSections(list);
                if (hasElement)
                {
                    m_Root.Add(list);
                }
                else
                {
                    m_Root.Add(new Label(k_NoBuildingBlockMessage));
                }
            };

            return m_Root;
        }

        /// <inheritdoc />
        public override void OnCreated()
        {
            EditorApplication.delayCall += RefreshBuildingBlocksEnabledState;
        }

        void RefreshBuildingBlocksEnabledState()
        {
            // No need to refresh the building blocks state if the overlay is docked in a toolbar or not created yet
            if (isInToolbar || m_Root == null)
                return;

            var stack = new Stack<VisualElement>();
            stack.Push(m_Root);

            // Go through all the elements under the root overlay and refresh the enabled state of the Building Block buttons
            while (stack.Count > 0)
            {
                VisualElement currentElement = stack.Pop();

                if (currentElement is BuildingBlockButton)
                    ((BuildingBlockButton) currentElement).RefreshEnabled();

                foreach (VisualElement child in currentElement.Children())
                    stack.Push(child);
            }
        }

        bool CreateSections(VisualElement root)
        {
            //Build unsectioned Building Blocks first
            var unsectionedBblocks = new List<IBuildingBlock>();
            BuildingBlockManager.GetUnsectionedBuildingBlocks(unsectionedBblocks);
            foreach (var bblock in unsectionedBblocks)
                CreateBuildingBlockButton(root, bblock, "");

            var sections = new List<IBuildingBlockSection>();
            BuildingBlockManager.GetSections(sections);

            if (sections.Count == 0)
                return unsectionedBblocks.Count > 0;

            var sectionsWithSameIDDict = new Dictionary<string, List<IBuildingBlockSection>>();
            foreach (var section in sections)
            {
                if (!sectionsWithSameIDDict.ContainsKey(section.SectionId))
                    sectionsWithSameIDDict.Add(section.SectionId, new List<IBuildingBlockSection>());

                sectionsWithSameIDDict[section.SectionId].Add(section);
            }

            foreach (var keyValuePair in sectionsWithSameIDDict)
            {
                List<IBuildingBlockSection> sectionsWithSameID = keyValuePair.Value;

                var iconTex = AssetDatabase.LoadAssetAtPath<Texture2D>(sectionsWithSameID[0].SectionIconPath);
                var foldout = new BuildingBlockSectionFoldout(sectionsWithSameID[0].SectionId, iconTex);
                root.Add(foldout);

                IEnumerable<IBuildingBlock> blocks = Enumerable.Empty<IBuildingBlock>();
                foreach (var section in sectionsWithSameID)
                {
                    blocks = blocks.Concat(section.GetBuildingBlocks());
                }

                foreach (var bblock in blocks)
                    CreateBuildingBlockButton(foldout, bblock, keyValuePair.Key);
            }

            return true;
        }

        static void CreateBuildingBlockButton(VisualElement parent, IBuildingBlock buildingBlock, string sectionId)
        {
            string normalizedBlockID = buildingBlock.Id.Length > k_MaxButtonTextLength
                ? buildingBlock.Id.Substring(0, k_MaxButtonTextLength) + "..." :
                buildingBlock.Id;

            var button = new BuildingBlockButton(normalizedBlockID,
                AssetDatabase.LoadAssetAtPath<Texture2D>(buildingBlock.IconPath),
                buildingBlock);

            button.SetEnabled(buildingBlock.IsEnabled);
            button.tooltip = buildingBlock.Tooltip;
#if ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
            button.clicked += () => CoreUtilsAnalytics.BuildingBlocksUsageEvent.SendOverlayButtonClicked(sectionId, buildingBlock.Id);
#endif

            parent.Add(button);
        }

        //This is used when the overlay is docked in a toolbar, otherwise the CreatePanelContent is creating the Panel
        string[] m_Elements = new[] { BuildingBlocksToolbar.id };
        public IEnumerable<string> toolbarElements => m_Elements;
    }

    /// <summary>
    /// Building blocks section foldout
    /// </summary>
    class BuildingBlockSectionFoldout : Foldout
    {
        static readonly string k_ImageClassName = "building-block-foldout_image";

        const int k_IconWidth = 16;
        const int k_IconHeight = 16;
        internal BuildingBlockSectionFoldout(string foldoutText, Texture2D icon)
        {
            name = foldoutText;
            text = foldoutText;

            var prefKey = foldoutText + "_overlayFoldout";
            var open = SessionState.GetBool(prefKey, true);
            SetValueWithoutNotify(open);
            RegisterCallback<ChangeEvent<bool>>(evt => SessionState.SetBool(prefKey, evt.newValue));
            //If icon is not null, adding the icon element to the foldout
            if (icon != null)
            {
                var image = new Image() { image = icon };
                image.style.width = new StyleLength(k_IconWidth);
                image.style.height = new StyleLength(k_IconHeight);
                image.AddToClassList(k_ImageClassName);
                this.Q("unity-checkmark").parent.Insert(1, image);
                this.Q("unity-checkmark").parent.parent.name = "Toggle "+foldoutText;
            }
        }
    }

    /// <summary>
    /// Building blocks button
    /// </summary>
    class BuildingBlockButton : Button
    {
        /// <summary>
        /// USS class name of elements of this type.
        /// </summary>
        static readonly string k_ClassName = "building-block-button";
        static readonly string k_ImageClass = k_ClassName + "_image";
        static readonly string k_TextClass = k_ClassName + "_text";

        IBuildingBlock m_Block;

        internal BuildingBlockButton(string text, Texture2D icon, IBuildingBlock bblock)
        {
            m_Block = bblock;
            // Update the enabled state depending on the capability profile selected.
            CapabilityProfileSelection.SelectionSaved += RefreshEnabled;

            clicked += m_Block.ExecuteBuildingBlock;

            AddToClassList(k_ClassName);

            style.flexDirection = FlexDirection.Row;

            if (icon != null)
            {
                var image = new Image() { image = icon };
                image.AddToClassList(k_ImageClass);
                Add(image);
            }

            var textElem = new TextElement() { text = text };
            textElem.AddToClassList(k_TextClass);

            Add(textElem);
            name = text;
        }

        internal void RefreshEnabled() => SetEnabled(m_Block.IsEnabled);
    }

    /// <summary>
    /// From here to the end are the 3 elements used to display the overlay in toolbar mode :
    /// The BuildingBlocksToolbar build the different elements and arrange the flex Direction to fit the parent.
    /// First the unsectioned Building Blocks (with no sections) are displayed as a button strip.
    /// Then the sections are displayed, each section is in a dropdown.
    /// </summary>
    [EditorToolbarElement(id)]
    class BuildingBlocksToolbar : VisualElement
    {
        public const string id = "BuildingBlocksToolbar";

        static readonly string k_ClassName = "building-block-toolbar-text";
        static readonly string k_ClassNameToRemove ="unity-editor-toolbar-element__label";
        static readonly string k_LabelClassName = "building-block-label";

        static readonly string k_NoBuildingBlockMessageShort = "None";

        public BuildingBlocksToolbar()
        {
            name = id;
            var styleSheet = Resources.Load(BuildingBlocksOverlay.k_OverlayStyleSheet) as StyleSheet;
            styleSheets.Add(styleSheet);

            var unsectionedBblocks = new List<IBuildingBlock>();
            BuildingBlockManager.GetUnsectionedBuildingBlocks(unsectionedBblocks);
            if (unsectionedBblocks.Count > 0)
                Add(new UnsectionedBuildingBlocksToolbar(unsectionedBblocks));

            var sections = new List<IBuildingBlockSection>();
            BuildingBlockManager.GetSections(sections);

            if (sections.Count == 0)
            {
                if (unsectionedBblocks.Count == 0)
                {
                    var message = new Label(k_NoBuildingBlockMessageShort);
                    message.AddToClassList(k_LabelClassName);
                    Add(message);
                }

                return;
            }

            var sectionsWithSameIDDict = new Dictionary<string, List<IBuildingBlockSection>>();
            foreach (var section in sections)
            {
                if(!sectionsWithSameIDDict.ContainsKey(section.SectionId))
                    sectionsWithSameIDDict.Add(section.SectionId, new List<IBuildingBlockSection>());

                sectionsWithSameIDDict[section.SectionId].Add(section);
            }

            foreach (var keyValuePair in sectionsWithSameIDDict)
            {
                Add(new BuildingBlockSectionDropdown(keyValuePair.Value));
            }

            RegisterCallback<GeometryChangedEvent>(OnGeometryChangedCallback);
        }

        void OnGeometryChangedCallback(GeometryChangedEvent evt)
        {
            style.flexDirection = parent.resolvedStyle.flexDirection;
        }

        /// <summary>
        /// Only displays the two first significant letters of the Building Block or Building Block Section to
        /// have a nice looking display of the options in toolbar mode.
        /// </summary>
        /// <param name="s">The name to analyze.</param>
        /// <returns>The significant letters for the Building Block.</returns>
        internal static string GetSignificantLettersForName(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            var folders = s.Split('/');
            var last = folders[folders.Length - 1];
            if (string.IsNullOrEmpty(last))
                return string.Empty;

            var words = last.Trim().Split(' ');
            if (words.Length == 1)
            {
                var regex = new Regex(@"[A-Z][^A-Z]*", RegexOptions.Compiled);
                var matches = regex.Matches(words[0]);
                if (matches == null || matches.Count == 0)
                    return words[0].Length > 1 ? words[0].Substring(0, 2) : words[0][0].ToString();

                if (matches.Count == 1)
                    return matches[0].Length > 1 ? matches[0].Value.Substring(0, 2) : matches[0].Value[0].ToString();

                return matches[0].Value.Substring(0, 1) + matches[1].Value.Substring(0, 1);
            }

            return words[0].Substring(0, 1) + words[1].Substring(0, 1);
        }

        internal static void UpdateTextElementClasses(VisualElement root)
        {
            var textElements = root.Children().OfType<TextElement>();
            if (textElements.Any())
            {
                var textElement = textElements.First();
                textElement.AddToClassList(k_ClassName);
                textElement.RemoveFromClassList(k_ClassNameToRemove);
            }
        }
    }

    /// <summary>
    /// Displaying the Building Blocks Sections as dropdowns in toolbar mode
    /// </summary>
    class BuildingBlockSectionDropdown : EditorToolbarDropdown
    {
        List<IBuildingBlockSection> m_SectionsWithSameID;

        internal BuildingBlockSectionDropdown(List<IBuildingBlockSection> sectionsWithSameID)
        {
            name = sectionsWithSameID[0].SectionId;
            text = BuildingBlocksToolbar.GetSignificantLettersForName(sectionsWithSameID[0].SectionId);
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>(sectionsWithSameID[0].SectionIconPath);
            tooltip = sectionsWithSameID[0].SectionId;
            m_SectionsWithSameID = sectionsWithSameID;

            clicked += ShowDropdown;
        }

        void ShowDropdown()
        {
            var menu = new GenericMenu();
            IEnumerable<IBuildingBlock> blocks = Enumerable.Empty<IBuildingBlock>();
            foreach (var section in m_SectionsWithSameID)
            {
                blocks = blocks.Concat(section.GetBuildingBlocks());
            }

            foreach (var block in blocks)
            {
                var content = new GUIContent(block.Id);
                if (!block.IsEnabled)
                {
                    menu.AddDisabledItem(content);
                    continue;
                }

                menu.AddItem(content,
                    false,
                    () =>
                    {
                        block.ExecuteBuildingBlock();

#if ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
                        CoreUtilsAnalytics.BuildingBlocksUsageEvent.SendToolbarButtonClicked(
                            m_SectionsWithSameID.First().SectionId, block.Id);
#endif
                    });
            }

            menu.ShowAsContext();
        }
    }

    /// <summary>
    /// Displaying the Unsectioned Building Blocks as a Button strip in toolbar mode
    /// </summary>
    class UnsectionedBuildingBlocksToolbar : OverlayToolbar
    {
        public UnsectionedBuildingBlocksToolbar(List<IBuildingBlock> unsectionedBuildingBlocks)
        {
            foreach (var bblock in unsectionedBuildingBlocks)
            {
                var button = new EditorToolbarButton(BuildingBlocksToolbar.GetSignificantLettersForName(bblock.Id),
                    AssetDatabase.LoadAssetAtPath<Texture2D>(bblock.IconPath),
                    () =>
                    {
                        bblock.ExecuteBuildingBlock();

#if ENABLE_CLOUD_SERVICES_ANALYTICS
                        CoreUtilsAnalytics.BuildingBlocksUsageEvent.SendToolbarButtonClicked("", bblock.Id);
#endif
                    });
                button.tooltip = bblock.Id;

                BuildingBlocksToolbar.UpdateTextElementClasses(button);
                Add(button);
            }

            SetupChildrenAsButtonStrip();
        }
    }
}
#endif
