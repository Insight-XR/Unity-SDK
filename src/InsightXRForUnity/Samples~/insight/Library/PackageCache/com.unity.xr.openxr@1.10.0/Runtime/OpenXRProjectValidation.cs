#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.XR.Management;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace UnityEditor.XR.OpenXR
{
    /// <summary>
    /// OpenXR project validation details.
    /// </summary>
    public static class OpenXRProjectValidation
    {
        private static readonly OpenXRFeature.ValidationRule[] BuiltinValidationRules =
        {
            new OpenXRFeature.ValidationRule
            {
                message = "The OpenXR package has been updated and Unity must be restarted to complete the update.",
                checkPredicate = () => OpenXRSettings.Instance == null || (!OpenXRSettings.Instance.versionChanged),
                fixIt = RequireRestart,
                error = true,
                errorEnteringPlaymode = true,
                buildTargetGroup = BuildTargetGroup.Standalone,
            },
            new OpenXRFeature.ValidationRule
            {
                message = "The OpenXR Package Settings asset has duplicate settings and must be regenerated.",
                checkPredicate = AssetHasNoDuplicates,
                fixIt = RegenerateXRPackageSettingsAsset,
                error = false,
                errorEnteringPlaymode = false
            },
            new OpenXRFeature.ValidationRule()
            {
                message = "Gamma Color Space is not supported when using OpenGLES.",
                checkPredicate = () =>
                {
                    if (PlayerSettings.colorSpace == ColorSpace.Linear)
                        return true;

                    return !Enum.GetValues(typeof(BuildTarget))
                        .Cast<BuildTarget>()
                        .Where(t =>
                        {
                            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(t);
                            if (!BuildPipeline.IsBuildTargetSupported(buildTargetGroup, t))
                                return false;

                            var settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTargetGroup);
                            if (null == settings)
                                return false;

                            var manager = settings.Manager;
                            if (null == manager)
                                return false;

                            return manager.activeLoaders.OfType<OpenXRLoader>().Any();
                        })
#if UNITY_2023_1_OR_NEWER
                        .Any(buildTarget => PlayerSettings.GetGraphicsAPIs(buildTarget).Any(g => g == GraphicsDeviceType.OpenGLES3));
#else
                        // Keeping OpenGL ES 2 support for 2022 and older versions.
                        .Any(buildTarget => PlayerSettings.GetGraphicsAPIs(buildTarget).Any(g => g == GraphicsDeviceType.OpenGLES2 || g == GraphicsDeviceType.OpenGLES3));
#endif
                },
                fixIt = () => PlayerSettings.colorSpace = ColorSpace.Linear,
                fixItMessage = "Set PlayerSettings.colorSpace to ColorSpace.Linear",
                error = true,
                errorEnteringPlaymode = true,
                buildTargetGroup = BuildTargetGroup.Android,
            },
            new OpenXRFeature.ValidationRule()
            {
                message = "At least one interaction profile must be added.  Please select which controllers you will be testing against in the Features menu.",
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                    return settings == null || settings.GetFeatures<OpenXRInteractionFeature>().Any(f => f.enabled);
                },
                fixIt = OpenProjectSettings,
                fixItAutomatic = false,
                fixItMessage = "Open Project Settings to select one or more interaction profiles."
            },
            new OpenXRFeature.ValidationRule()
            {
                message = "Only arm64 or x86_x64 is supported on Android with OpenXR.  Other architectures are not supported.",
                checkPredicate = () => (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android) || ((PlayerSettings.Android.targetArchitectures & (~(AndroidArchitecture.ARM64 | AndroidArchitecture.X86_64))) == 0) && (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.None),
                fixIt = () =>
                {
#if UNITY_2021_3_OR_NEWER
                    PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
#else
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
#endif
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                },
                fixItMessage = "Change android build to arm64 or x86_64 and enable il2cpp.",
                error = true,
                buildTargetGroup = BuildTargetGroup.Android,
            },
            new OpenXRFeature.ValidationRule()
            {
                message = "The only standalone targets supported are Windows x64 and OSX with OpenXR.  Other architectures and operating systems are not supported at this time.",
                checkPredicate = () => (BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget) != BuildTargetGroup.Standalone) || (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64) || (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX),
                fixIt = () => EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64),
                fixItMessage = "Switch active build target to StandaloneWindows64.",
                error = true,
                errorEnteringPlaymode = true,
                buildTargetGroup = BuildTargetGroup.Standalone,
            },
            new OpenXRFeature.ValidationRule()
            {
                message = "Lock Input to Game View in order for tracked pose driver to work in editor playmode.",
                checkPredicate = () =>
                {
                    var cls = typeof(UnityEngine.InputSystem.InputDevice).Assembly.GetType("UnityEngine.InputSystem.Editor.InputEditorUserSettings");
                    if (cls == null) return true;
                    var prop = cls.GetProperty("lockInputToGameView", BindingFlags.Static | BindingFlags.Public);
                    if (prop == null) return true;
                    return (bool)prop.GetValue(null);
                },
                fixItMessage =  "Enables the 'Lock Input to Game View' setting in Window -> Analysis -> Input Debugger -> Options",
                fixIt = () =>
                {
                    var cls = typeof(UnityEngine.InputSystem.InputDevice).Assembly.GetType("UnityEngine.InputSystem.Editor.InputEditorUserSettings");
                    if (cls == null) return;
                    var prop = cls.GetProperty("lockInputToGameView", BindingFlags.Static | BindingFlags.Public);
                    if (prop == null) return;
                    prop.SetValue(null, true);
                },
                errorEnteringPlaymode = true,
                buildTargetGroup = BuildTargetGroup.Standalone,
            },
            new OpenXRFeature.ValidationRule()
            {
                message = "Active Input Handling must be set to Input System Package (New) for OpenXR.",
                checkPredicate = () =>
                {
                    // There is no public way to check if the input handling backend is set correctly .. so resorting to non-public way for now.
                    var ps = (SerializedObject)typeof(PlayerSettings).GetMethod("GetSerializedObject", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
                    var newInputEnabledProp = ps?.FindProperty("activeInputHandler");
                    return newInputEnabledProp?.intValue != 0;
                },
                fixIt = () =>
                {
                    var ps = (SerializedObject)typeof(PlayerSettings).GetMethod("GetSerializedObject", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
                    if (ps == null)
                        return;

                    ps.Update();
                    var newInputEnabledProp = ps.FindProperty("activeInputHandler");
                    if (newInputEnabledProp != null)
                        newInputEnabledProp.intValue = 1;
                    ps.ApplyModifiedProperties();

                    RequireRestart();
                },
                error = true,
                errorEnteringPlaymode = true,
            },
            new OpenXRFeature.ValidationRule()
            {
                message = "[Optional] Switch to use InputSystem.XR.PoseControl instead of OpenXR.Input.PoseControl, which will be deprecated in a future release.",
                checkPredicate = () =>
                {
#if !USE_INPUT_SYSTEM_POSE_CONTROL && INPUT_SYSTEM_POSE_VALID
                    return false;
#else
                    return true;
#endif
                },
                fixIt = EnableInputSystemPoseControlDefine,
                error = false,
                errorEnteringPlaymode = false,
            },
            new OpenXRFeature.ValidationRule()
            {
                message = "[Optional] Switch to use StickControl thumbsticks instead of Vector2Control, but may break existing projects that have code dependencies to the Vector2Control type. StickControl allows more input options for thumbstick-based control, such as acting as both a combined 2D vector, two independent axes or a four-way Dpad with 4 independent buttons.",
                checkPredicate = () =>
                {
#if USE_STICK_CONTROL_THUMBSTICKS
                    return true;
#else
                    return false;
#endif
                },
                fixIt = EnableStickControlThumbsticksDefine,
                error = false,
                errorEnteringPlaymode = false,
            }
        };

        private static readonly List<OpenXRFeature.ValidationRule> CachedValidationList = new List<OpenXRFeature.ValidationRule>(BuiltinValidationRules.Length);

        internal static void EnableInputSystemPoseControlDefine()
        {
            AddDefineToBuildTarget("USE_INPUT_SYSTEM_POSE_CONTROL");
        }

        internal static void EnableStickControlThumbsticksDefine()
        {
            AddDefineToBuildTarget("USE_STICK_CONTROL_THUMBSTICKS");
        }

        private static void AddDefineToBuildTarget(string defineName)
        {
#if UNITY_2021_3_OR_NEWER
            NamedBuildTarget[] targets = { NamedBuildTarget.Android, NamedBuildTarget.Standalone, NamedBuildTarget.WindowsStoreApps };
            for (var index = 0; index < targets.Length; index++)
            {
                var defines = PlayerSettings.GetScriptingDefineSymbols(targets[index]);
                defines += $";{defineName}";
                PlayerSettings.SetScriptingDefineSymbols(targets[index], defines);
            }

#else
            BuildTargetGroup[] buildTargets = {BuildTargetGroup.Android, BuildTargetGroup.Standalone, BuildTargetGroup.WSA};
            for (var index = 0; index < buildTargets.Length; index++)
            {
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargets[index]);
                defines += $";{defineName}";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargets[index], defines);
            }
#endif
        }

        internal static bool AssetHasNoDuplicates()
        {
            var packageSettings = OpenXRSettings.GetPackageSettings();
            if (packageSettings == null)
            {
                return true;
            }

            string path = packageSettings.PackageSettingsAssetPath();
            var loadedAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            if (loadedAssets == null)
            {
                return true;
            }

            // Check for duplicate "full" feature name (Feature class type name + Feature name (contains BuildTargetGroup))
            HashSet<string> fullFeatureNames = new HashSet<string>();
            foreach (var loadedAsset in loadedAssets)
            {
                OpenXRFeature individualFeature = loadedAsset as OpenXRFeature;
                if (individualFeature != null)
                {
                    Type type = individualFeature.GetType();
                    string featureName = individualFeature.name;
                    string fullFeatureName = type.FullName + "\\" + featureName;

                    if (fullFeatureNames.Contains(fullFeatureName))
                        return false;
                    fullFeatureNames.Add(fullFeatureName);
                }
            }

            return true;
        }

        internal static void RegenerateXRPackageSettingsAsset()
        {
            // Deleting the OpenXR PackageSettings asset also destroys the OpenXRPackageSettings object.
            // Need to get the static method to create the new asset and object before deleting the asset.
            var packageSettings = OpenXRSettings.GetPackageSettings();
            if (packageSettings == null)
            {
                return;
            }

            string path = packageSettings.PackageSettingsAssetPath();

            Action createAssetCallback = packageSettings.RefreshFeatureSets;
            AssetDatabase.DeleteAsset(path);

            EditorBuildSettings.RemoveConfigObject(Constants.k_SettingsKey);

            createAssetCallback();
        }

        /// <summary>
        /// Open the OpenXR project settings
        /// </summary>
        internal static void OpenProjectSettings() => SettingsService.OpenProjectSettings("Project/XR Plug-in Management/OpenXR");

        internal static void GetAllValidationIssues(List<OpenXRFeature.ValidationRule> issues, BuildTargetGroup buildTargetGroup)
        {
            issues.Clear();
            issues.AddRange(BuiltinValidationRules.Where(s => s.buildTargetGroup == buildTargetGroup || s.buildTargetGroup == BuildTargetGroup.Unknown));
            OpenXRFeature.GetFullValidationList(issues, buildTargetGroup);
        }

        /// <summary>
        /// Gathers and evaluates validation issues and adds them to a list.
        /// </summary>
        /// <param name="issues">List of validation issues to populate. List is cleared before populating.</param>
        /// <param name="buildTarget">Build target group to check for validation issues</param>
        public static void GetCurrentValidationIssues(List<OpenXRFeature.ValidationRule> issues, BuildTargetGroup buildTargetGroup)
        {
            CachedValidationList.Clear();
            CachedValidationList.AddRange(BuiltinValidationRules.Where(s => s.buildTargetGroup == buildTargetGroup || s.buildTargetGroup == BuildTargetGroup.Unknown));
            OpenXRFeature.GetValidationList(CachedValidationList, buildTargetGroup);

            issues.Clear();
            foreach (var validation in CachedValidationList)
            {
                if (!validation.checkPredicate?.Invoke() ?? false)
                {
                    issues.Add(validation);
                }
            }
        }

        /// <summary>
        /// Logs validation issues to console.
        /// </summary>
        /// <param name="targetGroup"></param>
        /// <returns>true if there were any errors that should stop the build</returns>
        internal static bool LogBuildValidationIssues(BuildTargetGroup targetGroup)
        {
            var failures = new List<OpenXRFeature.ValidationRule>();
            GetCurrentValidationIssues(failures, targetGroup);

            if (failures.Count <= 0) return false;

            bool anyErrors = false;
            foreach (var result in failures)
            {
                if (result.error)
                    Debug.LogError(result.message);
                else
                    Debug.LogWarning(result.message);
                anyErrors |= result.error;
            }

            if (anyErrors)
            {
                Debug.LogError("Double click to fix OpenXR Project Validation Issues.");
            }

            return anyErrors;
        }

        /// <summary>
        /// Logs playmode validation issues (anything rule that fails with errorEnteringPlaymode set to true).
        /// </summary>
        /// <returns>true if there were any errors that should prevent openxr starting in editor playmode</returns>
        internal static bool LogPlaymodeValidationIssues()
        {
            var failures = new List<OpenXRFeature.ValidationRule>();
            GetCurrentValidationIssues(failures, BuildTargetGroup.Standalone);

            if (failures.Count <= 0) return false;

            bool playmodeErrors = false;
            foreach (var result in failures)
            {
                if (result.errorEnteringPlaymode)
                    Debug.LogError(result.message);
                playmodeErrors |= result.errorEnteringPlaymode;
            }

            return playmodeErrors;
        }

        private static void RequireRestart()
        {
            // There is no public way to change the input handling backend .. so resorting to non-public way for now.
            if (!EditorUtility.DisplayDialog("Unity editor restart required", "The Unity editor must be restarted for this change to take effect.  Cancel to revert changes.", "Apply", "Cancel"))
                return;

            typeof(EditorApplication).GetMethod("RestartEditorAndRecompileScripts", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
        }
    }
}
#endif
