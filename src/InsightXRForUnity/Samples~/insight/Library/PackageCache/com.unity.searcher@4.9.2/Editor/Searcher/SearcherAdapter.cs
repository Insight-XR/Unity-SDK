using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Searcher
{
    public enum ItemExpanderState
    {
        Hidden,
        Collapsed,
        Expanded
    }

    [PublicAPI]
    public interface ISearcherAdapter
    {
        VisualElement MakeItem();
        VisualElement Bind(VisualElement target, SearcherItem item, ItemExpanderState expanderState, string text);

        string Title { get; }
        bool HasDetailsPanel { get; }
        bool AddAllChildResults { get; }
        bool MultiSelectEnabled { get; }
        float InitialSplitterDetailRatio { get; }
        void OnSelectionChanged(IEnumerable<SearcherItem> items);
        SearcherItem OnSearchResultsFilter(IEnumerable<SearcherItem> searchResults, string searchQuery);
        void InitDetailsPanel(VisualElement detailsPanel);
    }

    [PublicAPI]
    public class SearcherAdapter : ISearcherAdapter
    {
        const string k_EntryName = "smartSearchItem";
        const int k_IndentDepthFactor = 15;

        readonly VisualTreeAsset m_DefaultItemTemplate;
        public virtual string Title { get; }
        public virtual bool HasDetailsPanel => true;
        public virtual bool AddAllChildResults => true;
        public virtual bool MultiSelectEnabled => false;

        Label m_DetailsLabel;
        public virtual float InitialSplitterDetailRatio => 1.0f;

        public SearcherAdapter(string title)
        {
            Title = title;
            m_DefaultItemTemplate = Resources.Load<VisualTreeAsset>("SearcherItem");
        }

        public virtual VisualElement MakeItem()
        {
            // Create a visual element hierarchy for this search result.
            var item = m_DefaultItemTemplate.CloneTree();
            return item;
        }

        public virtual VisualElement Bind(VisualElement element, SearcherItem item, ItemExpanderState expanderState, string query)
        {
            var indent = element.Q<VisualElement>("itemIndent");
            indent.style.width = item.Depth * k_IndentDepthFactor;

            var expander = element.Q<VisualElement>("itemChildExpander");

            var icon = expander.Query("expanderIcon").First();
            icon.ClearClassList();

            switch (expanderState)
            {
                case ItemExpanderState.Expanded:
                    icon.AddToClassList("Expanded");
                    break;

                case ItemExpanderState.Collapsed:
                    icon.AddToClassList("Collapsed");
                    break;
            }

            var nameLabelsContainer = element.Q<VisualElement>("labelsContainer");
            nameLabelsContainer.Clear();

            var iconElement = element.Q<VisualElement>("itemIconVisualElement");
            iconElement.style.backgroundImage = item.Icon;
            if (item.Icon == null && item.CollapseEmptyIcon)
            {
                iconElement.style.display = DisplayStyle.None;
            } else
            {
                iconElement.style.display = DisplayStyle.Flex;
            }

            if (string.IsNullOrWhiteSpace(query))
                nameLabelsContainer.Add(new Label(item.Name));
            else
                SearcherHighlighter.HighlightTextBasedOnQuery(nameLabelsContainer, item.Name, query);

            element.userData = item;
            element.name = k_EntryName;

            return expander;
        }

        public virtual void InitDetailsPanel(VisualElement detailsPanel)
        {
            m_DetailsLabel = new Label();
            detailsPanel.Add(m_DetailsLabel);
        }

        public virtual void OnSelectionChanged(IEnumerable<SearcherItem> items)
        {
            if (m_DetailsLabel != null)
            {
                var itemsList = items.ToList();
                m_DetailsLabel.text = itemsList.Any() ? itemsList[0].Help : "No results";
            }
        }
        
        // How to handle filtering and prioritization of search results is specific to clients of the searcher window
        // This callback is meant to be implemented by child classes of SearcherAdapter as they need
        public virtual SearcherItem OnSearchResultsFilter(IEnumerable<SearcherItem> searchResults, string searchQuery)
        {
            return new SearcherItem("");
        }
    }
}
