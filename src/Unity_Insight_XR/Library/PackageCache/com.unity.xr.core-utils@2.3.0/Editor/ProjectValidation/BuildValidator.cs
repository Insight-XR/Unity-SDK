using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_2021_2_OR_NEWER
using PrefabStageUtility = UnityEditor.SceneManagement.PrefabStageUtility;
#else
using PrefabStageUtility = UnityEditor.Experimental.SceneManagement.PrefabStageUtility;
#endif

#if ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
using Unity.XR.CoreUtils.Editor.Analytics;
#endif

namespace Unity.XR.CoreUtils.Editor
{
    /// <summary>
    /// Manages <see cref="BuildValidationRule"/> rules for verifying that project settings are compatible
    /// with the features of an installed XR package.
    /// </summary>
    /// <remarks>
    /// XR packages can implement a set of <see cref="BuildValidationRule"/> objects and call
    /// <see cref="AddRules(BuildTargetGroup, IEnumerable{BuildValidationRule})"/>
    /// to add them to the UNity project validation system. The rules are displayed to
    /// Unity developers in the **XR Plug-in Management** section of the **Project Settings** window.
    ///
    /// See [Project Validation](xref:xr-core-utils-project-validation) for more information.
    /// </remarks>
    [InitializeOnLoad]
    public static class BuildValidator
    {
        const string k_FixIssuesProgressBarTitle = "Fix Project Issues";
        const string k_FixIssuesProgressBarInfo = "{0} ({1}/{2})";

#if ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
        static Dictionary<string, ProjectValidationUsageEvent.IssuesStatus> s_IssuesStatusByCategory =
            new Dictionary<string, ProjectValidationUsageEvent.IssuesStatus>();

        static ProjectValidationUsageEvent.IssuesStatus GetCategoryIssuesStatus(string category)
        {
            if (string.IsNullOrEmpty(category))
                category = ProjectValidationUsageEvent.NoneCategoryName;

            return s_IssuesStatusByCategory.TryGetValue(category, out var status) ?
                status :
                new ProjectValidationUsageEvent.IssuesStatus {Category = category};
        }
#endif

        static Dictionary<BuildTargetGroup, List<BuildValidationRule>> s_PlatformRules =
            new Dictionary<BuildTargetGroup, List<BuildValidationRule>>();

        internal static Dictionary<BuildTargetGroup, List<BuildValidationRule>> PlatformRules => s_PlatformRules;

        static BuildValidator()
        {
            // Used implicitly. Called when Unity launches the Editor / Player or recompiles scripts.
        }

        /// <summary>
        /// Adds a set of <see cref="BuildValidationRule"/> for a given platform (<see cref="BuildTargetGroup"/>).
        /// </summary>
        /// <param name="group">The platform to which to add these rules.</param>
        /// <param name="rules">The rules to add to the platform.</param>
        public static void AddRules(BuildTargetGroup group, IEnumerable<BuildValidationRule> rules)
        {
            if (s_PlatformRules.TryGetValue(group, out var groupRules))
                groupRules.AddRange(rules);
            else
            {
                groupRules = new List<BuildValidationRule>(rules);
                s_PlatformRules.Add(group, groupRules);
            }
        }

        internal static void GetCurrentValidationIssues(HashSet<BuildValidationRule> failures,
            BuildTargetGroup buildTargetGroup)
        {
            failures.Clear();
            if (!s_PlatformRules.TryGetValue(buildTargetGroup, out var rules))
                return;

            var inPrefabStage = PrefabStageUtility.GetCurrentPrefabStage() != null;
            foreach (var validation in rules)
            {
                // If current scene is prefab isolation do not run scene validation
                if (inPrefabStage && validation.SceneOnlyValidation)
                    continue;

                if (validation.IsRuleEnabled.Invoke() && (validation.CheckPredicate == null || !validation.CheckPredicate.Invoke()))
                    failures.Add(validation);
            }
        }

        /// <summary>
        /// Checks if any member of a given set of types are present in the currently open scenes.
        /// </summary>
        /// <param name="subscribers">The set of types to check on scenes.</param>
        /// <returns><see langword="true"/> if any of the types have been found in the currently open scenes.</returns>
        public static bool HasTypesInSceneSetup(IEnumerable<Type> subscribers)
        {
            if (Application.isPlaying)
                return false;

            foreach (var sceneSetup in EditorSceneManager.GetSceneManagerSetup())
            {
                if (!sceneSetup.isLoaded)
                    continue;

                var scene = SceneManager.GetSceneByPath(sceneSetup.path);

                foreach (var go in scene.GetRootGameObjects())
                {
                    if (subscribers.Any(subscriber => go.GetComponentInChildren(subscriber, true)))
                        return true;
                }
            }

            return false;
        }

        internal static bool HasRulesForPlatform(BuildTargetGroup buildTarget)
        {
            return s_PlatformRules.TryGetValue(buildTarget, out var rules) && rules.Count > 0;
        }

        /// <summary>
        /// Fix all issues in the given <paramref name="issues"/> list.
        /// </summary>
        /// <param name="issues">The list of issues to fix.</param>
        /// <param name="progressBarTitle">The progress bar title.</param>
        public static void FixIssues(IList<BuildValidationRule> issues, string progressBarTitle = k_FixIssuesProgressBarTitle)
        {
#if ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
            s_IssuesStatusByCategory.Clear();
#endif
            var issuesFixed = 0;
            BuildValidationRule targetIssue = null;
            try
            {
                foreach (var issue in issues)
                {
                    targetIssue = issue;
                    var progressBarInfo = string.Format(k_FixIssuesProgressBarInfo, issue.GetDisplayString(),
                        issuesFixed + 1, issues.Count);
                    EditorUtility.DisplayProgressBar(progressBarTitle, progressBarInfo,
                        issuesFixed / (float)(issues.Count - 1));

                    if (issue.IsRuleEnabled.Invoke() &&
                        (issue.CheckPredicate == null || !issue.CheckPredicate.Invoke()))
                    {
                        issue.FixIt?.Invoke();
                        issuesFixed++;
                    }

#if ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
                    var issuesStatus = GetCategoryIssuesStatus(targetIssue.Category);
                    issuesStatus.SuccessfullyFixed++;
                    s_IssuesStatusByCategory[targetIssue.Category] = issuesStatus;
#endif
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);

#if ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
                if (targetIssue != null)
                {
                    var issuesStatus = GetCategoryIssuesStatus(targetIssue.Category);
                    issuesStatus.FailedToFix++;
                    s_IssuesStatusByCategory[targetIssue.Category] = issuesStatus;
                }
#endif
            }
            finally
            {
                if (issues.Count > 0)
                    EditorUtility.ClearProgressBar();

#if ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
                if (s_IssuesStatusByCategory.Count > 0)
                    CoreUtilsAnalytics.ProjectValidationUsageEvent.SendFixIssues(s_IssuesStatusByCategory.Values.ToArray());
#endif
            }
        }

        /// <summary>
        /// If your issue has a Unity object associated with it, you can use this method to select the object when users
        /// click in the issue in the validator window.
        /// </summary>
        /// <param name="instanceID"></param>
        /// <seealso cref="BuildValidationRule.OnClick"/>
        public static void SelectObject(int instanceID)
        {
            var objToSelect = EditorUtility.InstanceIDToObject(instanceID);
            if (objToSelect != null)
                Selection.activeObject = objToSelect;
        }
    }
}
