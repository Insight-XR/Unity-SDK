using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Assertions;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Represents the parameters to control the tunneling vignette material and customize its effects.
    /// </summary>
    /// <remarks>
    /// <seealso cref="TunnelingVignetteController"/>,
    /// <seealso cref="ITunnelingVignetteProvider"/>,
    /// <seealso cref="LocomotionVignetteProvider"/>
    /// </remarks>
    [Serializable]
    public sealed class VignetteParameters
    {
        [SerializeField]
        [Range(0f, Defaults.apertureSizeMax)]
        float m_ApertureSize  = Defaults.apertureSizeDefault;

        /// <summary>
        /// The diameter of the inner transparent circle of the tunneling vignette.
        /// </summary>
        /// <remarks>
        /// When multiple providers trigger the tunneling vignette animation, the one with the smallest aperture size
        /// will be used. The range of this value is <c>[0, 1]</c>, where <c>1</c> represents having no vignette effect.
        /// </remarks>
        public float apertureSize
        {
            get => m_ApertureSize;
            set => m_ApertureSize = value;
        }

        [SerializeField]
        [Range(0f, Defaults.featheringEffectMax)]
        float m_FeatheringEffect = Defaults.featheringEffectDefault;

        /// <summary>
        /// The degree of smoothly blending the edges between the aperture and full visual cut-off.
        /// Set this to a non-zero value to add a gradual transition from the transparent aperture to the black vignette edges.
        /// </summary>
        public float featheringEffect
        {
            get => m_FeatheringEffect;
            set => m_FeatheringEffect = value;
        }

        [SerializeField]
        float m_EaseInTime = Defaults.easeInTimeDefault;

        /// <summary>
        /// The transition time (in seconds) of easing in the tunneling vignette.
        /// Set this to a non-zero value to reduce the potential distraction from instantaneously changing the user's
        /// field of view when beginning the vignette.
        /// </summary>
        public float easeInTime
        {
            get => m_EaseInTime;
            set => m_EaseInTime = value;
        }

        [SerializeField]
        float m_EaseOutTime = Defaults.easeOutTimeDefault;

        /// <summary>
        /// The transition time (in seconds) of easing out the tunneling vignette.
        /// Set this to a non-zero value to reduce the potential distraction from instantaneously changing the user's
        /// field of view when ending the vignette.
        /// </summary>
        public float easeOutTime
        {
            get => m_EaseOutTime;
            set => m_EaseOutTime = value;
        }

        [SerializeField]
        bool m_EaseInTimeLock = Defaults.easeInTimeLockDefault;

        /// <summary>
        /// Persists the easing-in transition until it is complete.
        /// Enable this option if you want the easing-in transition to persist until it is complete.
        /// This can be useful for instant changes, such as snap turn and teleportation, to trigger
        /// the full tunneling effect without easing out the vignette partway through the easing in process.
        /// </summary>
        public bool easeInTimeLock
        {
            get => m_EaseInTimeLock;
            set => m_EaseInTimeLock = value;
        }

        [SerializeField]
        float m_EaseOutDelayTime = Defaults.easeOutDelayTimeDefault;

        /// <summary>
        /// The delay time (in seconds) before starting to ease out of the tunneling vignette.
        /// </summary>
        public float easeOutDelayTime
        {
            get => m_EaseOutDelayTime;
            set => m_EaseOutDelayTime = value;
        }

        [SerializeField]
        Color m_VignetteColor = Defaults.vignetteColorDefault;

        /// <summary>
        /// The primary color of the visual cut-off area of the vignette.
        /// </summary>
        public Color vignetteColor
        {
            get => m_VignetteColor;
            set => m_VignetteColor = value;
        }

        [SerializeField]
        Color m_VignetteColorBlend = Defaults.vignetteColorBlendDefault;

        /// <summary>
        /// The optional color to add color blending to the visual cut-off area of the vignette.
        /// </summary>
        public Color vignetteColorBlend
        {
            get => m_VignetteColorBlend;
            set => m_VignetteColorBlend = value;
        }

        [SerializeField]
        [Range(Defaults.apertureVerticalPositionMin, Defaults.apertureVerticalPositionMax)]
        float m_ApertureVerticalPosition = Defaults.apertureVerticalPositionDefault;

        /// <summary>
        /// The vertical position offset of the vignette.
        /// Changing this value will change the local y-position of the GameObject that this script is attached to.
        /// </summary>
        public float apertureVerticalPosition
        {
            get => m_ApertureVerticalPosition;
            set => m_ApertureVerticalPosition = value;
        }

        /// <summary>
        /// Provides default values for <see cref="VignetteParameters"/>.
        /// </summary>
        internal static class Defaults
        {
            /// <summary>
            /// The default maximum value of <see cref="apertureSize"/>.
            /// </summary>
            public const float apertureSizeMax = 1f;

            /// <summary>
            /// The default maximum value of <see cref="featheringEffect"/>.
            /// </summary>
            public const float featheringEffectMax = 1f;

            /// <summary>
            /// The default maximum value of <see cref="apertureVerticalPosition"/>.
            /// </summary>
            public const float apertureVerticalPositionMax = 0.2f;

            /// <summary>
            /// The default minimum value of <see cref="apertureVerticalPosition"/>.
            /// </summary>
            public const float apertureVerticalPositionMin = -apertureVerticalPositionMax;

            /// <summary>
            /// The default value of <see cref="apertureSize"/>.
            /// </summary>
            public const float apertureSizeDefault = 0.7f;

            /// <summary>
            /// The default value of <see cref="featheringEffect"/>.
            /// </summary>
            public const float featheringEffectDefault = 0.2f;

            /// <summary>
            /// The default value of <see cref="easeInTime"/>.
            /// </summary>
            public const float easeInTimeDefault = 0.3f;

            /// <summary>
            /// The default value of <see cref="easeOutTime"/>.
            /// </summary>
            public const float easeOutTimeDefault = 0.3f;

            /// <summary>
            /// The default value of <see cref="easeInTimeLock"/>.
            /// </summary>
            public const bool easeInTimeLockDefault = false;

            /// <summary>
            /// The default value of <see cref="easeOutDelayTime"/>.
            /// </summary>
            public const float easeOutDelayTimeDefault = 0f;

            /// <summary>
            /// (Read Only) The default value of <see cref="vignetteColor"/>.
            /// </summary>
            public static readonly Color vignetteColorDefault = Color.black;

            /// <summary>
            /// (Read Only) The default value of <see cref="vignetteColorBlend"/>.
            /// </summary>
            public static readonly Color vignetteColorBlendDefault = Color.black;

            /// <summary>
            /// The default value of <see cref="apertureVerticalPosition"/>.
            /// </summary>
            public const float apertureVerticalPositionDefault = 0f;

            /// <summary>
            /// (Read Only) The <see cref="VignetteParameters"/> that represents the default effect for the vignette.
            /// </summary>
            public static readonly VignetteParameters defaultEffect = new VignetteParameters
            {
                apertureSize = apertureSizeDefault,
                featheringEffect = featheringEffectDefault,
                easeInTime = easeInTimeDefault,
                easeOutTime = easeOutTimeDefault,
                easeInTimeLock = easeInTimeLockDefault,
                easeOutDelayTime = easeOutDelayTimeDefault,
                vignetteColor = vignetteColorDefault,
                vignetteColorBlend = vignetteColorBlendDefault,
                apertureVerticalPosition = apertureVerticalPositionDefault,
            };

            /// <summary>
            /// (Read Only) The <see cref="VignetteParameters"/> that represents no effect for the vignette.
            /// </summary>
            public static readonly VignetteParameters noEffect = new VignetteParameters
            {
                apertureSize = apertureSizeMax,
                featheringEffect = 0f,
                easeInTime = 0f,
                easeOutTime = 0f,
                easeInTimeLock = false,
                easeOutDelayTime = 0f,
                vignetteColor = vignetteColorDefault,
                vignetteColorBlend = vignetteColorBlendDefault,
                apertureVerticalPosition = apertureVerticalPositionDefault,
            };
        }

        /// <summary>
        /// Copies the parameter values from the given <see cref="VignetteParameters"/>.
        /// </summary>
        /// <param name="parameters">The <see cref="VignetteParameters"/> to copy values from.</param>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="parameters"/> is <see langword="null"/>.</exception>
        public void CopyFrom(VignetteParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            apertureSize = parameters.apertureSize;
            featheringEffect = parameters.featheringEffect;
            easeInTime = parameters.easeInTime;
            easeOutTime = parameters.easeOutTime;
            easeInTimeLock = parameters.easeInTimeLock;
            easeOutDelayTime = parameters.easeOutDelayTime;
            vignetteColor = parameters.vignetteColor;
            vignetteColorBlend = parameters.vignetteColorBlend;
            apertureVerticalPosition = parameters.apertureVerticalPosition;
        }
    }

    /// <summary>
    /// Options for displaying easing transitions of the tunneling vignette effect
    /// to reduce potential distractions from instantaneously changing the user's field of view.
    /// </summary>
    public enum EaseState
    {
        /// <summary>
        /// Display the normal state with no vignette effect (e.g., when the user is idling).
        /// </summary>
        NotEasing,

        /// <summary>
        /// Display the ease-in transition from the normal state to the tunneled field of view,
        /// (e.g., when the user started or is continuously moving).
        /// </summary>
        EasingIn,

        /// <summary>
        /// Continue displaying the ease-in transition after an ease-out transition is triggered
        /// and switch to the ease-out transition state after the ease-in transition is complete.
        /// </summary>
        EasingInHoldBeforeEasingOut,

        /// <summary>
        /// Delay the start of ease-out transition.
        /// </summary>
        EasingOutDelay,

        /// <summary>
        /// Display the ease-out transition from the tunneled field of view to the normal state,
        /// (e.g., when the user completed moving).
        /// </summary>
        EasingOut,
    }

    /// <summary>
    /// An interface that provides <see cref="VignetteParameters"/> needed to control the tunneling vignette effect.
    /// </summary>
    public interface ITunnelingVignetteProvider
    {
        /// <summary>
        /// Represents the parameter values that this provider wants to set for the tunneling vignette effect.
        /// A value of <see langword="null"/> indicates the <see cref="TunnelingVignetteController.defaultParameters"/> should be used.
        /// </summary>
        VignetteParameters vignetteParameters { get; }
    }

    /// <summary>
    /// Represents an <see cref="ITunnelingVignetteProvider"/> with a <see cref="LocomotionProvider"/>.
    /// </summary>
    [Serializable]
    public class LocomotionVignetteProvider : ITunnelingVignetteProvider
    {
        [SerializeField]
        LocomotionProvider m_LocomotionProvider;

        /// <summary>
        /// The <see cref="LocomotionProvider"/> to trigger the tunneling vignette effects based on its <see cref="LocomotionPhase"/>.
        /// </summary>
        public LocomotionProvider locomotionProvider
        {
            get => m_LocomotionProvider;
            set => m_LocomotionProvider = value;
        }

        [SerializeField]
        bool m_Enabled;

        /// <summary>
        /// Whether to enable this <see cref="LocomotionProvider"/> to trigger the tunneling vignette effects.
        /// </summary>
        public bool enabled
        {
            get => m_Enabled;
            set => m_Enabled = value;
        }

        [SerializeField]
        bool m_OverrideDefaultParameters;

        /// <summary>
        /// If enabled, Unity will override the value of <see cref="TunnelingVignetteController.defaultParameters"/>
        /// and instead use the customized <see cref="VignetteParameters"/> defined by this class.
        /// </summary>
        /// <seealso cref="overrideParameters"/>
        public bool overrideDefaultParameters
        {
            get => m_OverrideDefaultParameters;
            set => m_OverrideDefaultParameters = value;
        }

        [SerializeField]
        VignetteParameters m_OverrideParameters = new VignetteParameters();

        /// <summary>
        /// The <see cref="VignetteParameters"/> that this <see cref="LocomotionVignetteProvider"/> uses to control the vignette
        /// when the property to override <see cref="TunnelingVignetteController.defaultParameters"/> is enabled.
        /// </summary>
        /// <seealso cref="overrideDefaultParameters"/>
        public VignetteParameters overrideParameters
        {
            get => m_OverrideParameters;
            set => m_OverrideParameters = value;
        }

        /// <inheritdoc />
        public VignetteParameters vignetteParameters => m_OverrideDefaultParameters ? m_OverrideParameters : null;
    }

    /// <summary>
    /// Provides methods for <see cref="ITunnelingVignetteProvider"/> components to control the tunneling vignette material.
    /// </summary>
    [AddComponentMenu("XR/Locomotion/Tunneling Vignette Controller", 11)]
    [HelpURL((XRHelpURLConstants.k_TunnelingVignetteController))]
    public class TunnelingVignetteController : MonoBehaviour
    {
        const string k_DefaultShader = "VR/TunnelingVignette";

        static class ShaderPropertyLookup
        {
            public static readonly int apertureSize = Shader.PropertyToID("_ApertureSize");
            public static readonly int featheringEffect = Shader.PropertyToID("_FeatheringEffect");
            public static readonly int vignetteColor = Shader.PropertyToID("_VignetteColor");
            public static readonly int vignetteColorBlend = Shader.PropertyToID("_VignetteColorBlend");
        }

        [SerializeField]
        VignetteParameters m_DefaultParameters = new VignetteParameters();

        /// <summary>
        /// The default <see cref="VignetteParameters"/> of this <see cref="TunnelingVignetteController"/>.
        /// </summary>
        public VignetteParameters defaultParameters
        {
            get => m_DefaultParameters;
            set => m_DefaultParameters = value;
        }

        [SerializeField]
        VignetteParameters m_CurrentParameters = new VignetteParameters();

        /// <summary>
        /// (Read Only) The current <see cref="VignetteParameters"/> that is controlling the tunneling vignette material.
        /// </summary>
        public VignetteParameters currentParameters => m_CurrentParameters;

        [SerializeField]
        List<LocomotionVignetteProvider> m_LocomotionVignetteProviders = new List<LocomotionVignetteProvider>();

        /// <summary>
        /// List to store <see cref="LocomotionVignetteProvider"/> instances that trigger the tunneling vignette on their locomotion state changes.
        /// </summary>
        public List<LocomotionVignetteProvider> locomotionVignetteProviders
        {
            get => m_LocomotionVignetteProviders;
            set => m_LocomotionVignetteProviders = value;
        }

        /// <summary>
        /// Represents a record of a <see cref="ITunnelingVignetteProvider"/> and its dynamically updated values.
        /// </summary>
        class ProviderRecord
        {
            public ITunnelingVignetteProvider provider { get; }
            public EaseState easeState { get; set; } = EaseState.NotEasing;
            public float dynamicApertureSize { get; set; } = VignetteParameters.Defaults.apertureSizeMax;
            public bool easeInLockEnded { get; set; }
            public float dynamicEaseOutDelayTime { get; set; }

            public ProviderRecord(ITunnelingVignetteProvider provider)
            {
                this.provider = provider;
            }
        }

        /// <summary>
        /// List to keep the records of all the <see cref="ITunnelingVignetteProvider"/> instances of this controller and their dynamically updated values.
        /// </summary>
        readonly List<ProviderRecord> m_ProviderRecords = new List<ProviderRecord>();

        MeshRenderer m_MeshRender;
        MeshFilter m_MeshFilter;
        Material m_SharedMaterial;
        MaterialPropertyBlock m_VignettePropertyBlock;

        /// <summary>
        /// Queues a <see cref="ITunnelingVignetteProvider"/> to trigger the ease-in vignette effect.
        /// </summary>
        /// <param name="provider">The <see cref="ITunnelingVignetteProvider"/> that contains information of <see cref="VignetteParameters"/>.</param>
        /// <remarks>
        /// Unity will automatically sort all providers by their aperture size to prioritize the control from the one with the smallest aperture size if
        /// multiple providers are calling this method.
        /// </remarks>
        public void BeginTunnelingVignette(ITunnelingVignetteProvider provider)
        {
            foreach (var record in m_ProviderRecords)
            {
                if (record.provider == provider)
                {
                    record.easeState = EaseState.EasingIn;
                    return;
                }
            }

            m_ProviderRecords.Add(new ProviderRecord(provider) {easeState = EaseState.EasingIn});
        }

        /// <summary>
        /// Queues a <see cref="ITunnelingVignetteProvider"/> to trigger the ease-out vignette effect.
        /// </summary>
        /// <param name="provider">The <see cref="ITunnelingVignetteProvider"/> that contains information of <see cref="VignetteParameters"/>.</param>
        /// <remarks>
        /// Unity will automatically sort all providers by their aperture size to prioritize the control from the one with the smallest aperture size if
        /// multiple providers are calling this method.
        /// </remarks>
        public void EndTunnelingVignette(ITunnelingVignetteProvider provider)
        {
            var parameters = provider.vignetteParameters ?? m_DefaultParameters;

            // Check if this provider is already in our record.
            foreach (var record in m_ProviderRecords)
            {
                if (record.provider == provider)
                {
                    // Update the record.
                    record.easeState = parameters.easeInTimeLock && !record.easeInLockEnded
                        ? EaseState.EasingInHoldBeforeEasingOut
                        : parameters.easeOutDelayTime > 0f &&
                        record.dynamicEaseOutDelayTime < parameters.easeOutDelayTime
                            ? EaseState.EasingOutDelay
                            : EaseState.EasingOut;
                    return;
                }
            }

            // Otherwise, add the new provider to the record and use its parameters to determine its EaseState.
            var easeState = parameters.easeInTimeLock
                ? EaseState.EasingInHoldBeforeEasingOut
                : parameters.easeOutDelayTime > 0f
                    ? EaseState.EasingOutDelay
                    : EaseState.EasingOut;

            m_ProviderRecords.Add(new ProviderRecord(provider) { easeState = easeState });
        }

        /// <summary>
        /// (Editor Only) Previews a vignette effect in Editor with the given <see cref="VignetteParameters"/>.
        /// </summary>
        /// <param name="previewParameters">The <see cref="VignetteParameters"/> to preview in Editor.</param>
        [Conditional("UNITY_EDITOR")]
        internal void PreviewInEditor(VignetteParameters previewParameters)
        {
            // Avoid previewing when inspecting the prefab asset, which may cause the editor constantly refreshing.
            // Only preview it when it is in the scene or in the prefab window.
            if (!Application.isPlaying && gameObject.activeInHierarchy)
                UpdateTunnelingVignette(previewParameters);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Awake()
        {
#if UNITY_EDITOR
            UnityEditor.SceneVisibilityManager.instance.DisablePicking(gameObject, false);
#endif
            m_CurrentParameters.CopyFrom(VignetteParameters.Defaults.noEffect);
            UpdateTunnelingVignette(VignetteParameters.Defaults.noEffect);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        protected virtual void Reset()
        {
            m_DefaultParameters.CopyFrom(VignetteParameters.Defaults.defaultEffect);
            m_CurrentParameters.CopyFrom(VignetteParameters.Defaults.noEffect);
            UpdateTunnelingVignette(m_DefaultParameters);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Update()
        {
            // Add (only if not already) providers to the list that keeps track of their aperture sizes and ease-out delay time.
            // Queue their EaseStates to begin/end the easing transitions according to their LocomotionStates.
            if (m_LocomotionVignetteProviders.Count > 0)
            {
                foreach (var provider in m_LocomotionVignetteProviders)
                {
                    var locomotionProvider = provider.locomotionProvider;
                    if (!provider.enabled || locomotionProvider == null)
                        continue;

                    switch (locomotionProvider.locomotionPhase)
                    {
                        case LocomotionPhase.Started:
                        case LocomotionPhase.Moving:
                            BeginTunnelingVignette(provider);
                            break;
                        case LocomotionPhase.Done:
                            EndTunnelingVignette(provider);
                            break;
                    }
                }
            }

            if (m_ProviderRecords.Count == 0)
                return;

            // Max aperture size for no effect
            const float apertureSizeMax = VignetteParameters.Defaults.apertureSizeMax;

            // Compute dynamic parameter values for all providers and update their records.
            foreach (var record in m_ProviderRecords)
            {
                var provider = record.provider;
                var parameters = provider.vignetteParameters ?? m_DefaultParameters;
                var currentSize = record.dynamicApertureSize;

                switch (record.easeState)
                {
                    case EaseState.NotEasing:
                    {
                        record.dynamicApertureSize = apertureSizeMax;
                        record.dynamicEaseOutDelayTime = 0f;
                        record.easeInLockEnded = false;

                        break;
                    }

                    case EaseState.EasingIn:
                    {
                        var desiredEaseInTime = Mathf.Max(parameters.easeInTime, 0f);
                        var desiredEaseInSize = parameters.apertureSize;
                        record.easeInLockEnded = false;

                        if (desiredEaseInTime > 0f && currentSize > desiredEaseInSize)
                        {
                            var updatedSize = currentSize + (desiredEaseInSize - apertureSizeMax) / desiredEaseInTime * Time.unscaledDeltaTime;
                            record.dynamicApertureSize = updatedSize < desiredEaseInSize ? desiredEaseInSize : updatedSize;
                        }
                        else
                        {
                            record.dynamicApertureSize = desiredEaseInSize;
                        }

                        break;
                    }

                    case EaseState.EasingInHoldBeforeEasingOut:
                    {
                        if (!record.easeInLockEnded)
                        {
                            var desiredEaseInTime = Mathf.Max(parameters.easeInTime, 0f);
                            var desiredEaseInSize = parameters.apertureSize;

                            if (desiredEaseInTime > 0f && currentSize > desiredEaseInSize)
                            {
                                var updatedSize = currentSize + (desiredEaseInSize - apertureSizeMax) / desiredEaseInTime * Time.unscaledDeltaTime;
                                record.dynamicApertureSize = updatedSize < desiredEaseInSize ? desiredEaseInSize : updatedSize;
                            }
                            else
                            {
                                record.easeInLockEnded = true;
                                if (parameters.easeOutDelayTime > 0f &&
                                    record.dynamicEaseOutDelayTime < parameters.easeOutDelayTime)
                                {
                                    record.easeState = EaseState.EasingOutDelay;
                                    goto case EaseState.EasingOutDelay;
                                }

                                record.easeState = EaseState.EasingOut;
                                goto case EaseState.EasingOut;
                            }
                        }
                        else
                        {
                            if (parameters.easeOutDelayTime > 0f)
                            {
                                record.easeState = EaseState.EasingOutDelay;
                                goto case EaseState.EasingOutDelay;
                            }

                            record.easeState = EaseState.EasingOutDelay;
                            goto case EaseState.EasingOut;
                        }

                        break;
                    }

                    case EaseState.EasingOutDelay:
                    {
                        var currentDelayTime = record.dynamicEaseOutDelayTime;
                        var desiredEaseOutDelayTime = Mathf.Max(parameters.easeOutDelayTime, 0f);

                        if (desiredEaseOutDelayTime > 0f && currentDelayTime < desiredEaseOutDelayTime)
                        {
                            currentDelayTime += Time.unscaledDeltaTime;

                            record.dynamicEaseOutDelayTime = currentDelayTime > desiredEaseOutDelayTime
                                ? desiredEaseOutDelayTime
                                : currentDelayTime;
                        }

                        if (record.dynamicEaseOutDelayTime >= desiredEaseOutDelayTime)
                        {
                            record.easeState = EaseState.EasingOut;
                            goto case EaseState.EasingOut;
                        }

                        break;
                    }

                    case EaseState.EasingOut:
                    {
                        var desiredEaseOutTime = Mathf.Max(parameters.easeOutTime, 0f);
                        var startSize = parameters.apertureSize;

                        if (desiredEaseOutTime > 0f && currentSize < apertureSizeMax)
                        {
                            var updatedSize = currentSize + (apertureSizeMax - startSize) / desiredEaseOutTime * Time.unscaledDeltaTime;
                            record.dynamicApertureSize = updatedSize > apertureSizeMax ? apertureSizeMax : updatedSize;
                        }
                        else
                        {
                            record.dynamicApertureSize = apertureSizeMax;
                        }

                        if (record.dynamicApertureSize >= apertureSizeMax)
                            record.easeState = EaseState.NotEasing;

                        break;
                    }

                    default:
                        Assert.IsTrue(false, $"Unhandled {nameof(EaseState)}={record.easeState}");
                        break;
                }
            }

            // Find the minimum dynamic aperture size among all providers and update the current parameters with its associated vignette parameters.
            var minDynamicApertureSize = apertureSizeMax;
            ProviderRecord minRecord = null;
            foreach (var record in m_ProviderRecords)
            {
                var apertureSize = record.dynamicApertureSize;
                if (apertureSize < minDynamicApertureSize)
                {
                    minRecord = record;
                    minDynamicApertureSize = apertureSize;
                }
            }

            if (minRecord != null)
                m_CurrentParameters.CopyFrom(minRecord.provider.vignetteParameters ?? m_DefaultParameters);

            m_CurrentParameters.apertureSize = minDynamicApertureSize;

            // Update the visuals of the tunneling vignette.
            UpdateTunnelingVignette(m_CurrentParameters);
        }

        /// <summary>
        /// Updates the tunneling vignette with the vignette parameters.
        /// </summary>
        /// <param name="parameters">The <see cref="VignetteParameters"/> uses to update the material values.</param>
        /// <remarks>
        /// Use this method with caution when other <see cref="ITunnelingVignetteProvider"/> instances are updating the material simultaneously.
        /// Calling this method will automatically try to set up the material and its renderer for the <see cref="TunnelingVignetteController"/> if it is not set up already.
        /// </remarks>
        void UpdateTunnelingVignette(VignetteParameters parameters)
        {
            if (parameters == null)
                parameters = m_DefaultParameters;

            if (TrySetUpMaterial())
            {
                m_MeshRender.GetPropertyBlock(m_VignettePropertyBlock);
                m_VignettePropertyBlock.SetFloat(ShaderPropertyLookup.apertureSize, parameters.apertureSize);
                m_VignettePropertyBlock.SetFloat(ShaderPropertyLookup.featheringEffect, parameters.featheringEffect);
                m_VignettePropertyBlock.SetColor(ShaderPropertyLookup.vignetteColor, parameters.vignetteColor);
                m_VignettePropertyBlock.SetColor(ShaderPropertyLookup.vignetteColorBlend, parameters.vignetteColorBlend);
                m_MeshRender.SetPropertyBlock(m_VignettePropertyBlock);
            }

            // Update the Transform y-position to match apertureVerticalPosition
            var thisTransform = transform;
            var localPosition = thisTransform.localPosition;
            if (!Mathf.Approximately(localPosition.y, parameters.apertureVerticalPosition))
            {
                localPosition.y = parameters.apertureVerticalPosition;
                thisTransform.localPosition = localPosition;
            }
        }

        bool TrySetUpMaterial()
        {
            if (m_MeshRender == null)
                m_MeshRender = GetComponent<MeshRenderer>();
            if (m_MeshRender == null)
                m_MeshRender = gameObject.AddComponent<MeshRenderer>();

            if (m_VignettePropertyBlock == null)
                m_VignettePropertyBlock = new MaterialPropertyBlock();

            if (m_MeshFilter == null)
                m_MeshFilter = GetComponent<MeshFilter>();
            if (m_MeshFilter == null)
                m_MeshFilter = gameObject.AddComponent<MeshFilter>();

            if (m_MeshFilter.sharedMesh == null)
            {
                Debug.LogWarning("The default mesh for the TunnelingVignetteController is not set. " +
                    "Make sure to import it from the Tunneling Vignette Sample of XR Interaction Toolkit.", this);
                return false;
            }

            if (m_MeshRender.sharedMaterial == null)
            {
                var defaultShader = Shader.Find(k_DefaultShader);
                if (defaultShader == null)
                {
                    Debug.LogWarning("The default material for the TunnelingVignetteController is not set, and the default Shader: " + k_DefaultShader
                        + " cannot be found. Make sure they are imported from the Tunneling Vignette Sample of XR Interaction Toolkit.", this);
                    return false;
                }

                Debug.LogWarning("The default material for the TunnelingVignetteController is not set. " +
                    "Make sure it is imported from the Tunneling Vignette Sample of XR Interaction Toolkit. + " +
                    "Try creating a material using the default Shader: " + k_DefaultShader, this);

                m_SharedMaterial = new Material(defaultShader)
                {
                    name = "DefaultTunnelingVignette",
                };
                m_MeshRender.sharedMaterial = m_SharedMaterial;
            }
            else
            {
                m_SharedMaterial = m_MeshRender.sharedMaterial;
            }

            return true;
        }
    }
}
