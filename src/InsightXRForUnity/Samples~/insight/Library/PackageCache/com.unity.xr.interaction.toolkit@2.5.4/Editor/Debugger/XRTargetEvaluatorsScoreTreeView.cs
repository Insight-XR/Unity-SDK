using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

namespace UnityEditor.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Multi-column <see cref="TreeView"/> that shows the Interactables final score and the contribution of each enabled evaluator.
    /// The filter is the root of the tree. The Interactables are displayed as children of the linked Interactor that invoked the filter process.
    /// </summary>
    class XRTargetEvaluatorsScoreTreeView : TreeView
    {
        enum ColumnId
        {
            Name,
            Type,
            FinalScore,
            // Count is used to easily get the number of items defined in this enum. New enum items should be added before Count.
            Count,
        }

        class Item : TreeViewItem
        {
            public object target;
            public string finalScore;
            public List<string> scoreList;
        }

        class ScoreTracker
        {
            static readonly LinkedPool<List<float>> s_ScoreListPool = new LinkedPool<List<float>>
                (() => new List<float>(), actionOnRelease: list => list.Clear(), collectionCheck: false);

            public List<IXRInteractable> sortedInteractables { get; } = new List<IXRInteractable>();

            public Dictionary<IXRInteractable, float> interactableFinalScoreMap { get; } = new Dictionary<IXRInteractable, float>();

            public Dictionary<IXRInteractable, List<float>> interactableScoreListMap { get; } = new Dictionary<IXRInteractable, List<float>>();

            public void OnProcessingCompleted(List<IXRInteractable> results,
                Dictionary<IXRInteractable, float> finalScoreMap,
                Dictionary<IXRInteractable, List<float>> scoreListMap)
            {
                // Clear cached values
                sortedInteractables.Clear();
                interactableFinalScoreMap.Clear();

                foreach (var scoreList in interactableScoreListMap.Values)
                    s_ScoreListPool.Release(scoreList);
                interactableScoreListMap.Clear();

                // Cache new values
                sortedInteractables.AddRange(results);

                foreach (var pair in finalScoreMap)
                    interactableFinalScoreMap.Add(pair.Key, pair.Value);

                foreach (var pair in scoreListMap)
                {
                    var scoreList = s_ScoreListPool.Get();
                    scoreList.AddRange(pair.Value);
                    interactableScoreListMap.Add(pair.Key, scoreList);
                }
            }
        }

        const float k_RowHeight = 20f;

        static readonly List<XRTargetEvaluator> s_Evaluators = new List<XRTargetEvaluator>();

        static bool exitingPlayMode => EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode;

        static MultiColumnHeaderState CreateHeaderState(XRTargetFilter filter)
        {
            if (filter != null)
                filter.GetEnabledEvaluators(s_Evaluators);
            else
                s_Evaluators.Clear();

            var columns = new MultiColumnHeaderState.Column[(int)ColumnId.Count + s_Evaluators.Count];

            columns[(int)ColumnId.Name] = new MultiColumnHeaderState.Column
            {
                width = 180f,
                minWidth = 60f,
                headerContent = EditorGUIUtility.TrTextContent("Name"),
            };

            columns[(int)ColumnId.Type] = new MultiColumnHeaderState.Column
            {
                width = 120f,
                minWidth = 60f,
                headerContent = EditorGUIUtility.TrTextContent("Type"),
            };

            columns[(int)ColumnId.FinalScore] = new MultiColumnHeaderState.Column
            {
                width = 80f,
                minWidth = 40f,
                headerContent = EditorGUIUtility.TrTextContent("Final Score"),
            };

            var columnIndex = (int)ColumnId.Count;
            foreach (var evaluator in s_Evaluators)
            {
                columns[columnIndex] = new MultiColumnHeaderState.Column
                {
                    width = 80f,
                    minWidth = 40f,
                    headerContent = EditorGUIUtility.TrTextContent(XRInteractionDebuggerWindow.GetDisplayType(evaluator)),
                };

                columnIndex++;
            }

            return new MultiColumnHeaderState(columns);
        }

        public static XRTargetEvaluatorsScoreTreeView Create(XRTargetFilter filter,
            ref TreeViewState treeState, ref MultiColumnHeaderState headerState)
        {
            treeState = treeState ?? new TreeViewState();
            var newHeaderState = CreateHeaderState(filter);
            if (headerState != null && MultiColumnHeaderState.CanOverwriteSerializedFields(headerState, newHeaderState))
                MultiColumnHeaderState.OverwriteSerializedFields(headerState, newHeaderState);
            headerState = newHeaderState;

            var header = new MultiColumnHeader(headerState);

            return new XRTargetEvaluatorsScoreTreeView(filter, treeState, header);
        }

        readonly Dictionary<IXRInteractor, ScoreTracker> m_ScoreByInteractorMap = new Dictionary<IXRInteractor, ScoreTracker>();
        readonly List<XRTargetEvaluator> m_EnabledEvaluators = new List<XRTargetEvaluator>();
        readonly XRTargetFilter m_Filter;

        public XRTargetFilter filter => m_Filter;

        XRTargetEvaluatorsScoreTreeView(XRTargetFilter filter, TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
            showBorder = false;
            rowHeight = k_RowHeight;
            m_Filter = filter;
            if (m_Filter != null)
            {
                m_Filter.GetEnabledEvaluators(m_EnabledEvaluators);
                m_Filter.processingCompleted += OnProcessingCompleted;
                m_Filter.interactorLinked += AddInteractor;
                m_Filter.interactorUnlinked += RemoveInteractor;

                foreach (var interactor in m_Filter.linkedInteractors)
                    AddInteractor(interactor);
            }

            Reload();
        }

        /// <summary>
        /// Call this when this tree has no more use.
        /// </summary>
        public void Release()
        {
            if (m_Filter == null)
                return;

            m_Filter.processingCompleted -= OnProcessingCompleted;
            m_Filter.interactorLinked -= AddInteractor;
            m_Filter.interactorUnlinked -= RemoveInteractor;
        }

        void OnProcessingCompleted(IXRInteractor interactor, List<IXRInteractable> interactables, List<IXRInteractable> results,
            Dictionary<IXRInteractable, float> interactableFinalScoreMap,
            Dictionary<IXRInteractable, List<float>> interactableScoreListMap)
        {
            if (m_ScoreByInteractorMap.TryGetValue(interactor, out var scoreTracker))
                scoreTracker.OnProcessingCompleted(results, interactableFinalScoreMap, interactableScoreListMap);
        }

        void AddInteractor(IXRInteractor interactor)
        {
            if (!m_ScoreByInteractorMap.ContainsKey(interactor))
                m_ScoreByInteractorMap[interactor] = new ScoreTracker();
        }

        void RemoveInteractor(IXRInteractor interactor)
        {
            m_ScoreByInteractorMap.Remove(interactor);
        }

        /// <inheritdoc />
        protected override TreeViewItem BuildRoot()
        {
            // Wrap root control in invisible item required by TreeView.
            return new Item
            {
                id = 0,
                children = BuildInteractorTree(),
                depth = -1,
            };
        }

        List<TreeViewItem> BuildInteractorTree()
        {
            var items = new List<TreeViewItem>();

            if (m_Filter == null)
                return items;

            var rootTreeItem = new Item
            {
                id = XRInteractionDebuggerWindow.GetUniqueTreeViewId(m_Filter),
                displayName = XRInteractionDebuggerWindow.GetDisplayName(m_Filter),
                target = m_Filter,
                depth = 0
            };

            var children = new List<TreeViewItem>();
            foreach (var interactor in filter.linkedInteractors)
            {
                var childItem = new Item
                {
                    id = XRInteractionDebuggerWindow.GetUniqueTreeViewId(interactor),
                    displayName = XRInteractionDebuggerWindow.GetDisplayName(interactor),
                    target = interactor,
                    depth = 1,
                    parent = rootTreeItem
                };

                childItem.children = BuildInteractableScoreTree(childItem, interactor);
                children.Add(childItem);
            }

            rootTreeItem.children = children;
            items.Add(rootTreeItem);

            return items;
        }

        List<TreeViewItem> BuildInteractableScoreTree(TreeViewItem parent, IXRInteractor interactor)
        {
            var items = new List<TreeViewItem>();

            if (!m_ScoreByInteractorMap.TryGetValue(interactor, out var scoreTracker))
                return items;

            foreach (var interactable in scoreTracker.sortedInteractables)
            {
                string finalScore;
                List<string> scoreList;
                if (scoreTracker.interactableFinalScoreMap.ContainsKey(interactable) && scoreTracker.interactableScoreListMap.ContainsKey(interactable))
                {
                    finalScore = scoreTracker.interactableFinalScoreMap[interactable].ToString("F3");
                    scoreList = new List<string>();
                    foreach (var score in scoreTracker.interactableScoreListMap[interactable])
                        scoreList.Add(score.ToString("F3"));
                }
                else
                {
                    finalScore = null;
                    scoreList = null;
                }

                var childItem = new Item
                {
                    id = XRInteractionDebuggerWindow.GetUniqueTreeViewId(interactable),
                    displayName = XRInteractionDebuggerWindow.GetDisplayName(interactable),
                    target = interactable,
                    finalScore = finalScore,
                    scoreList = scoreList,
                    depth = 2,
                    parent = parent,
                };

                items.Add(childItem);
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

            switch (column)
            {
                case (int)ColumnId.Type:
                    GUI.Label(cellRect, XRInteractionDebuggerWindow.GetDisplayType(item.target));
                    break;

                case (int)ColumnId.FinalScore:
                    if (item.finalScore != null)
                        GUI.Label(cellRect, item.finalScore);
                    break;

                default:
                    var scoreIndex = column - (int)ColumnId.Count;
                    if (item.scoreList != null && scoreIndex >= 0 && scoreIndex < item.scoreList.Count)
                        GUI.Label(cellRect, item.scoreList[scoreIndex]);
                    break;
            }
        }

        /// <inheritdoc />
        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);

            EditorGUIUtility.PingObject(id);
            Selection.activeInstanceID = id;
        }

        /// <summary>
        /// Checks if the enabled evaluator list has changed.
        /// The list is changed when an evaluator in the filter is moved, enabled or disabled.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the enabled filter list has changed. Otherwise, returns <see langword="false"/>.</returns>
        public bool EnabledEvaluatorListHasChanged()
        {
            if (m_Filter == null)
                return false;

            m_Filter.GetEnabledEvaluators(s_Evaluators);

            // Checks if the lists have different size
            if (s_Evaluators.Count != m_EnabledEvaluators.Count)
                return true;

            // Checks if each filter in both lists are the same and have the same index
            for (var i = 0; i < s_Evaluators.Count; i++)
            {
                if (s_Evaluators[i] != m_EnabledEvaluators[i])
                    return true;
            }

            return false;
        }
    }
}
