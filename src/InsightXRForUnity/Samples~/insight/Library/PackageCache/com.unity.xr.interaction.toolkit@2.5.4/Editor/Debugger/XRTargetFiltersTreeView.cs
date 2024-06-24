using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace UnityEditor.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Multi-column <see cref="TreeView"/> that shows Target Filters.
    /// </summary>
    class XRTargetFiltersTreeView : TreeView
    {
        enum ColumnId
        {
            Name,
            LinkedInteractors,
            // Count is used to easily get the number of items defined in this enum. New enum items should be added before Count.
            Count,
        }

        class Item : TreeViewItem
        {
            public XRTargetFilter filter;
        }

        const float k_RowHeight = 20f;

        static bool exitingPlayMode => EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode;

        public static XRTargetFiltersTreeView Create(List<XRTargetFilter> enabledFilters, ref TreeViewState treeState, ref MultiColumnHeaderState headerState)
        {
            treeState = treeState ?? new TreeViewState();
            var newHeaderState = CreateHeaderState();
            if (headerState != null && MultiColumnHeaderState.CanOverwriteSerializedFields(headerState, newHeaderState))
                MultiColumnHeaderState.OverwriteSerializedFields(headerState, newHeaderState);
            headerState = newHeaderState;

            var header = new MultiColumnHeader(headerState);
            return new XRTargetFiltersTreeView(enabledFilters, treeState, header);
        }

        static MultiColumnHeaderState CreateHeaderState()
        {
            var columns = new MultiColumnHeaderState.Column[(int)ColumnId.Count];

            columns[(int)ColumnId.Name] = new MultiColumnHeaderState.Column
            {
                width = 180f,
                minWidth = 60f,
                headerContent = EditorGUIUtility.TrTextContent("Name"),
            };
            columns[(int)ColumnId.LinkedInteractors] = new MultiColumnHeaderState.Column
            {
                width = 120f,
                minWidth = 60f,
                headerContent = EditorGUIUtility.TrTextContent("Linked Interactors"),
            };

            return new MultiColumnHeaderState(columns);
        }

        readonly List<XRTargetFilter> m_Filters = new List<XRTargetFilter>();

        XRBaseTargetFilter m_SelectedFilter;

        internal XRBaseTargetFilter selectedFilter => m_SelectedFilter;

        public XRTargetFiltersTreeView(List<XRTargetFilter> enabledFilters, TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
            foreach(var filter in enabledFilters)
                AddFilter(filter);
            showBorder = false;
            rowHeight = k_RowHeight;
            Reload();
        }

        void AddFilter(XRTargetFilter filter)
        {
            if (m_Filters.Contains(filter))
                return;

            filter.interactorLinked += OnInteractorLinked;
            filter.interactorUnlinked += OnInteractorUnlinked;

            m_Filters.Add(filter);
        }

        void RemoveFilter(XRTargetFilter filter)
        {
            if (!m_Filters.Contains(filter))
                return;

            if (filter != null)
            {
                filter.interactorLinked -= OnInteractorLinked;
                filter.interactorUnlinked -= OnInteractorUnlinked;
            }

            m_Filters.Remove(filter);
        }

        void OnInteractorLinked(IXRInteractor eventArgs)
        {
            Reload();
        }

        void OnInteractorUnlinked(IXRInteractor eventArgs)
        {
            // Skip reloading as each interactor is being destroyed when exiting Play mode
            if (!exitingPlayMode)
                Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            // Wrap root control in invisible item required by TreeView.
            return new Item
            {
                id = 0,
                children = BuildFilterTree(),
                depth = -1,
            };
        }

        List<TreeViewItem> BuildFilterTree()
        {
            var items = new List<TreeViewItem>();

            foreach (var filter in m_Filters)
            {
                if (filter == null)
                    continue;

                var rootTreeItem = new Item
                {
                    id = XRInteractionDebuggerWindow.GetUniqueTreeViewId(filter),
                    displayName = XRInteractionDebuggerWindow.GetDisplayName(filter),
                    filter = filter,
                    depth = 0,
                };

                items.Add(rootTreeItem);
            }

            return items;
        }

        /// <inheritdoc />
        protected override void RowGUI(RowGUIArgs args)
        {
            if (!Application.isPlaying || exitingPlayMode)
                return;

            var item = (Item)args.item;

            var columnCount = args.GetNumVisibleColumns();
            for (var i = 0; i < columnCount; ++i)
                ColumnGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
        }

        void ColumnGUI(Rect cellRect, Item item, int column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            if (column == (int)ColumnId.Name)
            {
                args.rowRect = cellRect;
                base.RowGUI(args);
            }

            if (item.filter == null)
                return;

            switch (column)
            {
                case (int)ColumnId.LinkedInteractors:
                    var linkedInteractors = item.filter.linkedInteractors;
                    if (linkedInteractors.Count > 0)
                        GUI.Label(cellRect, XRInteractionDebuggerWindow.JoinNames(",", linkedInteractors));
                    break;
            }
        }

        /// <inheritdoc />
        protected override void SingleClickedItem(int id)
        {
            base.SingleClickedItem(id);

            var filter = EditorUtility.InstanceIDToObject(id) as XRTargetFilter;
            if (filter == null)
                return;

            m_SelectedFilter = filter;
        }

        /// <inheritdoc />
        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);

            EditorGUIUtility.PingObject(id);
            Selection.activeInstanceID = id;
        }

        public void UpdateFilterList(List<XRTargetFilter> currentEnabledFilters)
        {
            var filterListChanged = false;

            // Check for Removal
            for (var i = 0; i < m_Filters.Count; i++)
            {
                var filter = m_Filters[i];
                if (!currentEnabledFilters.Contains(filter))
                {
                    RemoveFilter(filter);
                    filterListChanged = true;
                    --i;
                }
            }

            // Check for Add
            foreach (var filter in currentEnabledFilters)
            {
                if (!m_Filters.Contains(filter))
                {
                    AddFilter(filter);
                    filterListChanged = true;
                }
            }

            if (filterListChanged)
                Reload();
        }
    }
}
