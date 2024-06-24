using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CoreUtils.Editor
{
    class ProjectValidationDrawer
    {
        class Styles
        {
            static readonly RectOffset k_LineBorder = new RectOffset(0, 0, 0, 0);
            static readonly RectOffset k_LinePadding = new RectOffset(5, 5, 5, 5);
            static readonly RectOffset k_LineMargin = new RectOffset(0, 0, 0, 0);


            internal static readonly Vector2 IconSize = new Vector2(16.0f, 16.0f);
            internal static readonly float DisabledRulePadding = IconSize.x + 5;

            internal static readonly float MessagePadding = Styles.IconSize.x + styles.IconStyle.padding.horizontal;

            internal const float NoButtonPadding = 5;
            internal const float ErrorIconPadding = 3;

            internal const float Space = 15.0f;
            internal const float FixButtonWidth = 80.0f;
            internal const float ShowAllChecksWidth = 96f;
            internal const float IgnoreBuildErrorsWidth = 140f;

            internal const string DisabledRuleTooltip = "This rule is disabled and won't be checked until certain conditions are met.";

            internal readonly GUIStyle Wrap;
            internal readonly GUIContent FixButton;
            internal readonly GUIContent EditButton;
            internal readonly GUIContent HelpButton;
            internal readonly GUIContent PlayMode;

            internal readonly GUIContent IgnoreBuildErrorsContent;

            internal readonly GUIContent WarningIcon;
            internal readonly GUIContent ErrorIcon;
            internal readonly GUIContent TestPassedIcon;

            internal GUIStyle IssuesBackground;
            internal GUIStyle ListLine;
            internal GUIStyle ListLineBackgroundOdd;
            internal GUIStyle ListLineBackgroundEven;
            internal GUIStyle IssuesTitleLabel;
            internal GUIStyle FixAllStyle;
            internal GUIStyle IconStyle;

            internal Styles()
            {
                FixButton = new GUIContent("Fix");
                EditButton = new GUIContent("Edit");
                HelpButton = new GUIContent(EditorGUIUtility.IconContent("_Help@2x").image);
                PlayMode = new GUIContent("Exit play mode before fixing project validation issues.", EditorGUIUtility.IconContent("console.infoicon").image);

                IgnoreBuildErrorsContent = new GUIContent("Ignore build errors",
                    "Errors from Build Validator Rules will not cause the build to fail.");

                WarningIcon = EditorGUIUtility.IconContent("Warning@2x");
                ErrorIcon = EditorGUIUtility.IconContent("Error@2x");
                TestPassedIcon = EditorGUIUtility.IconContent("TestPassed");

                IssuesBackground = "ScrollViewAlt";

                ListLine = new GUIStyle("TV Line")
                {
                    border = k_LineBorder,
                    padding = k_LinePadding,
                    margin = k_LineMargin
                };

                ListLineBackgroundOdd = new GUIStyle("CN EntryBackOdd")
                {
                    border = k_LineBorder,
                    padding = k_LinePadding,
                    margin = k_LineMargin
                };

                ListLineBackgroundEven = new GUIStyle("CN EntryBackEven")
                {
                    border = k_LineBorder,
                    padding = k_LinePadding,
                    margin = k_LineMargin
                };

                IssuesTitleLabel = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(10, 10, 0, 0)
                };

                Wrap = new GUIStyle(EditorStyles.label)
                {
                    wordWrap = true,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(0, 5, 1, 1),
                    richText = true
                };

                IconStyle = new GUIStyle(EditorStyles.label)
                {
                    margin = new RectOffset(5, 5, 4, 0),
                    fixedWidth = IconSize.x * 2
                };

                FixAllStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    stretchWidth = false,
                    fixedWidth = 80,
                    margin = new RectOffset(0, 5, 2, 2)
                };
            }
        }

        const string k_PrefPrefix = "XRProjectValidation";
        const string k_BuildValidationShowAllPref = k_PrefPrefix + ".BuildValidationShowAll";
        const string k_BuildValidationIgnoreBuildErrorsPref = k_PrefPrefix + ".BuildValidationIgnoreBuildErrors";

        /// <summary>
        /// Interval between issue updates
        /// </summary>
        const double k_UpdateInterval = 1.0d;

        /// <summary>
        /// Interval between issue updates when the window does not have focus
        /// </summary>
        const double k_BackgroundUpdateInterval = 3.0d;

        /// <summary>
        /// Highlight animation Duration in Seconds
        /// Start time of the highlight animation
        /// </summary>
        const float k_HighlightDuration = 3f;
        static float s_HighlightStartTime;

        static Styles s_Styles;
        // Delay creation of Styles till first access
        static Styles styles => s_Styles ?? (s_Styles = new Styles());

        static bool s_BuildValidationShowAll;
        static bool BuildValidationShowAll
        {
            get { return s_BuildValidationShowAll; }
            set
            {
                s_BuildValidationShowAll = value;
                EditorPrefs.SetBool(k_BuildValidationShowAllPref, value);
            }
        }

        static bool s_BuildValidationIgnoreBuildErrors;
        static bool BuildValidationIgnoreBuildErrors
        {
            get { return s_BuildValidationIgnoreBuildErrors; }
            set
            {
                if (s_BuildValidationIgnoreBuildErrors != value)
                {
                    EditorPrefs.SetBool(k_BuildValidationIgnoreBuildErrorsPref, value);
                    s_BuildValidationIgnoreBuildErrors = value;
                }
            }
        }

        static void DrawListLineBox(Rect position, GUIContent content, bool selected, GUIStyle lineStyle, GUIStyle lineSelectionStyle)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (selected)
                lineSelectionStyle.Draw(position, content, false, false, true, true);
            else
                lineStyle.Draw(position, content, position.Contains(Event.current.mousePosition), false, false, false);
        }

        static string GetIssueDisplayString(BuildValidationRule issue)
        {
            return string.IsNullOrEmpty(issue.Category) ? issue.Message : $"[{issue.Category}] {issue.Message}";
        }

        static void HighlightIssues(string windowTitle, string searchText)
        {
            s_HighlightStartTime = (float)EditorApplication.timeSinceStartup;
            EditorApplication.update += HandleHighlightUpdate;
            Highlighter.Highlight(windowTitle, searchText);
        }

        static void HandleHighlightUpdate()
        {
            if (EditorApplication.timeSinceStartup - s_HighlightStartTime > k_HighlightDuration)
            {
                Highlighter.Stop();
                EditorApplication.update -= HandleHighlightUpdate;
            }
        }

        /// <summary>
        /// Last time the issues in the window were updated
        /// </summary>
        double m_LastUpdate;

        Vector2 m_ScrollViewPos = Vector2.zero;
        bool m_CheckedInPlayMode;

        List<BuildValidationRule> m_BuildRules;

        // Fix all state
        List<BuildValidationRule> m_FixAllList = new List<BuildValidationRule>();

        HashSet<BuildValidationRule> m_RuleFailures = new HashSet<BuildValidationRule>();

        BuildTargetGroup m_SelectedBuildTargetGroup;
        BuildValidationRule m_SelectedRule;

        bool CheckInPlayMode
        {
            get
            {
                if (Application.isPlaying)
                {
                    if (!m_CheckedInPlayMode)
                    {
                        m_CheckedInPlayMode = true;
                        return true;
                    }

                    return false;
                }

                m_CheckedInPlayMode = false;
                return false;
            }
        }

        internal ProjectValidationDrawer(BuildTargetGroup targetGroup)
        {
            s_BuildValidationShowAll = EditorPrefs.GetBool(k_BuildValidationShowAllPref);
            s_BuildValidationIgnoreBuildErrors = EditorPrefs.GetBool(k_BuildValidationIgnoreBuildErrorsPref);

            m_SelectedBuildTargetGroup = targetGroup;

            BuildValidator.GetCurrentValidationIssues(m_RuleFailures, m_SelectedBuildTargetGroup);
        }

        internal void OnGUI()
        {
            EditorGUIUtility.SetIconSize(Styles.IconSize);

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                m_SelectedBuildTargetGroup = EditorGUILayout.BeginBuildTargetSelectionGrouping();
                if (m_SelectedBuildTargetGroup == BuildTargetGroup.Unknown)
                {
                    m_SelectedBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                }

                if (!BuildValidator.PlatformRules.TryGetValue(m_SelectedBuildTargetGroup, out m_BuildRules))
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField($"'{m_SelectedBuildTargetGroup}' does not have any associated build rules.",
                        styles.IssuesTitleLabel);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndBuildTargetSelectionGrouping();
                    return;
                }

                if (change.changed)
                {
                    BuildValidator.GetCurrentValidationIssues(m_RuleFailures, m_SelectedBuildTargetGroup);
                }
            }

            EditorGUILayout.BeginVertical();

            if (EditorApplication.isPlaying && m_RuleFailures.Count > 0)
            {
                GUILayout.Space(Styles.Space);
                GUILayout.Label(styles.PlayMode);
            }

            if (m_BuildRules != null)
            {
                EditorGUILayout.Space();
                DrawIssuesList();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndBuildTargetSelectionGrouping();
        }

        void DrawIssuesList()
        {
            var hasFix = m_RuleFailures.Any(f => f.FixIt != null);
            var hasAutoFix = hasFix && m_RuleFailures.Any(f => f.FixIt != null && f.FixItAutomatic);

            using (new EditorGUI.DisabledGroupScope(EditorApplication.isPlaying))
            {
                // Header
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"Issues ({m_RuleFailures.Count}) of Checks ({m_BuildRules.Count(rule => rule.IsRuleEnabled())})",
                        styles.IssuesTitleLabel);

                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        var showAllChecks = EditorGUILayout.ToggleLeft("Show all",
                            BuildValidationShowAll, GUILayout.Width(Styles.ShowAllChecksWidth));

                        if (change.changed)
                            BuildValidationShowAll = showAllChecks;
                    }

                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        var ignoreBuildErrorsCheck = EditorGUILayout.ToggleLeft(styles.IgnoreBuildErrorsContent,
                            BuildValidationIgnoreBuildErrors, GUILayout.Width(Styles.IgnoreBuildErrorsWidth));

                        if (change.changed)
                            BuildValidationIgnoreBuildErrors = ignoreBuildErrorsCheck;
                    }

                    // FixAll button
                    if (hasAutoFix)
                    {
                        using (new EditorGUI.DisabledScope(m_FixAllList.Count > 0))
                        {
                            if (GUILayout.Button("Fix All", styles.FixAllStyle, GUILayout.Width(Styles.FixButtonWidth)))
                            {
                                foreach (var ruleFailure in m_RuleFailures)
                                {
                                    if (ruleFailure.FixIt != null && ruleFailure.FixItAutomatic)
                                        m_FixAllList.Add(ruleFailure);
                                }
                            }
                        }
                    }
                }

                m_ScrollViewPos = EditorGUILayout.BeginScrollView(m_ScrollViewPos, styles.IssuesBackground,
                    GUILayout.ExpandHeight(true));

                var index = 0;
                m_BuildRules = SortRulesByEnabledCondition(m_BuildRules);
                foreach (var result in m_BuildRules)
                {
                    var rulePassed = !m_RuleFailures.Contains(result);
                    if (BuildValidationShowAll || !rulePassed)
                    {
                        DrawIssue(result, rulePassed, hasFix, index);
                        index++;
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        void DrawIssue(BuildValidationRule result, bool rulePassed, bool hasFix, int index)
        {
            bool isRuleEnabled = result.IsRuleEnabled();
            using (new EditorGUI.DisabledScope(!isRuleEnabled))
            {
                var lineBackgroundGUIStyle = index % 2 == 0 ? styles.ListLineBackgroundOdd : styles.ListLineBackgroundEven;
                var listItemRect = EditorGUILayout.BeginHorizontal(lineBackgroundGUIStyle);
                DrawListLineBox(listItemRect, GUIContent.none, m_SelectedRule == result, styles.ListLine, lineBackgroundGUIStyle);

                if (isRuleEnabled)
                {
                    if (!rulePassed && result.Error)
                        GUILayout.Space(Styles.ErrorIconPadding);

                    GUILayout.Label(rulePassed ? styles.TestPassedIcon
                        : result.Error ? styles.ErrorIcon
                        : styles.WarningIcon, styles.IconStyle,
                        GUILayout.Width(Styles.IconSize.x));
                }
                else
                {
                    GUILayout.Space(Styles.DisabledRulePadding);
                }

                var message = GetIssueDisplayString(result);

                GUILayout.Label(new GUIContent(message, isRuleEnabled ? string.Empty : Styles.DisabledRuleTooltip), styles.Wrap);

                if (!string.IsNullOrEmpty(result.HelpText) || !string.IsNullOrEmpty(result.HelpLink))
                {
                    styles.HelpButton.tooltip = result.HelpText;
                    if (GUILayout.Button(styles.HelpButton, styles.IconStyle, GUILayout.Width(Styles.IconSize.x
                            + styles.IconStyle.padding.horizontal +
                            (result.FixIt != null ? 0 : Styles.NoButtonPadding))))
                    {
                        if (!string.IsNullOrEmpty(result.HelpLink))
                            Application.OpenURL(result.HelpLink);
                    }
                }
                else
                {
                    GUILayout.Space(Styles.MessagePadding);
                }

                using (new EditorGUI.DisabledScope(!m_RuleFailures.Contains(result)))
                {
                    if (result.FixIt != null)
                    {
                        using (new EditorGUI.DisabledScope(m_FixAllList.Count != 0))
                        {
                            var button = result.FixItAutomatic ? styles.FixButton : styles.EditButton;
                            button.tooltip = result.FixItMessage;
                            if (GUILayout.Button(button, GUILayout.Width(Styles.FixButtonWidth)))
                            {
                                if (result.FixItAutomatic)
                                    m_FixAllList.Add(result);
                                else
                                {
                                    result.FixIt();
                                    if (result.HighlighterFocus.WindowTitle != null & result.HighlighterFocus.SearchText != null)
                                    {
                                        EditorApplication.delayCall += () =>
                                        {
                                            HighlightIssues(result.HighlighterFocus.WindowTitle, result.HighlighterFocus.SearchText);
                                        };
                                    }
                                }
                            }
                        }
                    }
                    else if (hasFix)
                    {
                        GUILayout.Space(Styles.FixButtonWidth);
                    }
                }

                EditorGUILayout.EndHorizontal();

                var currentEvt = Event.current;
                if (currentEvt.type == EventType.MouseDown && currentEvt.button == 0 && listItemRect.Contains(currentEvt.mousePosition))
                {
                    currentEvt.Use();
                    m_SelectedRule = result;
                    result.OnClick?.Invoke();
                }
            }
        }

        internal bool UpdateIssues(bool focused, bool force)
        {
            if (CheckInPlayMode)
                force = true;
            else if (Application.isPlaying)
                return false;

            var interval = focused ? k_UpdateInterval : k_BackgroundUpdateInterval;
            if (!force && EditorApplication.timeSinceStartup - m_LastUpdate < interval)
                return false;

            if (m_FixAllList.Count > 0)
            {
                BuildValidator.FixIssues(m_FixAllList);
                m_FixAllList.Clear();
            }

            var activeBuildTargetGroup = m_SelectedBuildTargetGroup;
            if (activeBuildTargetGroup == BuildTargetGroup.Unknown)
                activeBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

            if (!BuildValidator.HasRulesForPlatform(activeBuildTargetGroup))
                return false;

            BuildValidator.GetCurrentValidationIssues(m_RuleFailures, activeBuildTargetGroup);

            // Always repaint the window if there are rules
            var needsRepaint = m_BuildRules != null && m_BuildRules.Count > 0;

            m_LastUpdate = EditorApplication.timeSinceStartup;
            return needsRepaint;
        }

        static List<BuildValidationRule> SortRulesByEnabledCondition(List<BuildValidationRule> rulesToSort)
        {
            var sortedRules = new List<BuildValidationRule>(rulesToSort.Count);
            var enabledInsertIndex = 0;
            foreach (var rule in rulesToSort)
            {
                if (rule.IsRuleEnabled.Invoke())
                    sortedRules.Insert(enabledInsertIndex++, rule);
                else
                    sortedRules.Add(rule);
            }

            return sortedRules;
        }
    }
}
