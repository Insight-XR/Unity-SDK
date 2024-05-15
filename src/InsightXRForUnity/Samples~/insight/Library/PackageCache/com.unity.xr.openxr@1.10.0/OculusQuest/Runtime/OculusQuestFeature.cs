using System;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;
#endif

namespace UnityEngine.XR.OpenXR.Features.OculusQuestSupport
{
    /// <summary>
    /// Enables the Oculus mobile OpenXR Loader for Android, and modifies the AndroidManifest to be compatible with Quest.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Oculus Quest Support",
        Desc = "Necessary to deploy an Oculus Quest compatible app.",
        Company = "Unity",
        DocumentationLink = "https://developer.oculus.com/downloads/package/oculus-openxr-mobile-sdk/",
        OpenxrExtensionStrings = "XR_OCULUS_android_initialize_loader",
        Version = "1.0.0",
        BuildTargetGroups = new[] {BuildTargetGroup.Android},
        CustomRuntimeLoaderBuildTargets = new[] {BuildTarget.Android},
        FeatureId = featureId,
        Hidden = true
    )]
#endif
    [Obsolete("OpenXR.Features.OculusQuestSupport.OculusQuestFeature is deprecated. Please use OpenXR.Features.MetaQuestSupport.MetaQuestFeature instead.", false)]
    public class OculusQuestFeature : OpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.oculusquest";

        /// <summary>
        /// Adds a Quest entry to the supported devices list in the Android manifest.
        /// </summary>
        public bool targetQuest = true;
        /// <summary>
        /// Adds a Quest 2 entry to the supported devices list in the Android manifest.
        /// </summary>
        public bool targetQuest2 = true;

#if UNITY_EDITOR
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            rules.Add(new ValidationRule(this)
            {
                message = "Oculus Quest Feature for Android platform is deprecated, please enable Meta Quest Feature instead.",
                checkPredicate = () => !this.enabled,
                error = true,
                errorEnteringPlaymode = true,
                fixIt = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (null == settings)
                        return;
                    this.enabled = false;
                    var metaQuestFeature = settings.GetFeature<MetaQuestFeature>();
                    if (metaQuestFeature != null)
                    {
                        metaQuestFeature.enabled = true;
                        if (metaQuestFeature.targetDevices.Count == 0)
                        {
                            MetaQuestFeature.TargetDevice questDevice = new MetaQuestFeature.TargetDevice { manifestName = "quest", visibleName = "Quest", enabled = this.targetQuest, active = true};
                            metaQuestFeature.targetDevices.Add(questDevice);
                            MetaQuestFeature.TargetDevice quest2Device = new MetaQuestFeature.TargetDevice { manifestName = "quest2", visibleName = "Quest 2", enabled = this.targetQuest2, active = true};
                            metaQuestFeature.targetDevices.Add(quest2Device);
                            return;
                        }
                        for (var i = 0; i < metaQuestFeature.targetDevices.Count; i++)
                        {
                            if (metaQuestFeature.targetDevices[i].manifestName == "quest")
                            {
                                metaQuestFeature.targetDevices[i] = new MetaQuestFeature.TargetDevice() {manifestName = "quest", visibleName = "Quest", enabled = this.targetQuest, active = true};
                            }
                            if (metaQuestFeature.targetDevices[i].manifestName == "quest2")
                            {
                                metaQuestFeature.targetDevices[i] = new MetaQuestFeature.TargetDevice() {manifestName = "quest2", visibleName = "Quest 2", enabled = this.targetQuest2, active = true};
                            }
                        }
                    }
                }
            });
        }

        [Obsolete("OpenXR.Features.OculusQuestSupport.OculusQuestFeatureEditorWindow is deprecated.", false)]
        internal class OculusQuestFeatureEditorWindow : EditorWindow
        {
            private Object feature;
            private Editor featureEditor;

            public static EditorWindow Create(Object feature)
            {
                var window = EditorWindow.GetWindow<OculusQuestFeatureEditorWindow>(true, "Oculus Quest Feature Configuration", true);
                window.feature = feature;
                window.featureEditor = Editor.CreateEditor(feature);
                return window;
            }

            private void OnGUI()
            {
                featureEditor.OnInspectorGUI();
            }
        }
#endif
    }
}
