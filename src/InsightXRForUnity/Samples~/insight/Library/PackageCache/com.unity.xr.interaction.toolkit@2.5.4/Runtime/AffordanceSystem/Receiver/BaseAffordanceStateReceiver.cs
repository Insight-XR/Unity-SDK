using System;
using System.Text;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver
{
    /// <summary>
    /// Abstract base class for affordance state receivers used to expose the typed functions and properties necessary
    /// for an affordance state receiver to work.
    /// </summary>
    /// <typeparam name="T">The type of the value struct.</typeparam>
    public abstract class BaseAffordanceStateReceiver<T> : MonoBehaviour, IAffordanceStateReceiver<T> where T : struct, IEquatable<T>
    {
        [SerializeField]
        [Tooltip("Affordance state provider component to subscribe to.")]
        BaseAffordanceStateProvider m_AffordanceStateProvider;

        /// <summary>
        /// Affordance state provider reference used to drive the receiver's affordance state.
        /// </summary>
        public BaseAffordanceStateProvider affordanceStateProvider
        {
            get => m_AffordanceStateProvider;
            set => m_AffordanceStateProvider = value;
        }
        
        [SerializeField]
        [Tooltip("If true, the initial captured state value for the receiver will replace the idle value in the affordance theme.")]
        bool m_ReplaceIdleStateValueWithInitialValue;

        /// <summary>
        /// If true, the initial captured state value for the receiver will replace the idle value in the affordance theme.
        /// </summary>
        public bool replaceIdleStateValueWithInitialValue
        {
            get => m_ReplaceIdleStateValueWithInitialValue;
            set => m_ReplaceIdleStateValueWithInitialValue = value;
        }

        BaseAffordanceTheme<T> m_AffordanceTheme;

        /// <inheritdoc />
        public BaseAffordanceTheme<T> affordanceTheme
        {
            get => m_AffordanceTheme;
            set
            {
                m_AffordanceTheme = value;
                OnAffordanceThemeChanged(value);
            }
        }

        /// <summary>
        /// The default affordance theme that is cloned and assigned to <see cref="affordanceTheme"/>
        /// during initialization if that property is not set.
        /// </summary>
        /// <remarks>
        /// This should typically be the theme contained in a serialized ScriptableObject asset reference.
        /// </remarks>
        /// <seealso cref="affordanceTheme"/>
        protected abstract BaseAffordanceTheme<T> defaultAffordanceTheme { get; }

        /// <summary>
        /// Bindable variable for current typed affordance value. Updated as scheduled tween jobs complete.
        /// </summary>
        /// <seealso cref="currentAffordanceValue"/>
        protected abstract BindableVariable<T> affordanceValue { get; }

        /// <inheritdoc />
        public IReadOnlyBindableVariable<T> currentAffordanceValue => affordanceValue;

        readonly BindableVariable<AffordanceStateData> m_AffordanceStateData = new BindableVariable<AffordanceStateData>();

        /// <inheritdoc />
        public IReadOnlyBindableVariable<AffordanceStateData> currentAffordanceStateData => m_AffordanceStateData;

        bool m_Initialized;

        /// <summary>
        /// Initial affordance value captured from the derived receiver class.
        /// Used in certain tween jobs, and can also be used to replace the idle state of a theme.
        /// </summary>
        protected T initialValue { get; set; }
        
        /// <summary>
        /// Flag informing whether the initial value has been captured yet.
        /// </summary>
        protected bool initialValueCaptured { get; set; }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Awake()
        {
            if (m_AffordanceStateProvider == null)
                m_AffordanceStateProvider = GetComponentInParent<BaseAffordanceStateProvider>();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnEnable()
        {
            Initialize();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (m_AffordanceStateProvider != null)
                m_AffordanceStateProvider.UnregisterAffordanceReceiver(this);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Start()
        {
            // Try initializing on Start to continue to work if a receiver was programmatically configured after OnEnable
            Initialize();

            if (m_AffordanceStateProvider == null)
                XRLoggingUtils.LogError($"Missing Affordance State Provider reference. Please set one on {this}.", this);
        }

        void Initialize()
        {
            if (!m_Initialized)
            {
                if (m_AffordanceStateProvider == null)
                {
                    return;
                }

                if (affordanceTheme == null)
                {
                    if (defaultAffordanceTheme == null)
                        return;
                    
                    // Ensure the default theme is valid before cloning it
                    defaultAffordanceTheme.ValidateTheme();
                    
                    var copiedTheme = GenerateNewAffordanceThemeInstance();
                    copiedTheme.CopyFrom(defaultAffordanceTheme);
                    affordanceTheme = copiedTheme;
                }

                m_Initialized = true;
            }

            m_AffordanceStateProvider.RegisterAffordanceReceiver(this);
        }

        /// <summary>
        /// Automatically called by Unity during initialization to create a new instance of the affordance theme used for this receiver.
        /// </summary>
        /// <returns>Returns a new instance of the affordance theme.</returns>
        protected abstract BaseAffordanceTheme<T> GenerateNewAffordanceThemeInstance();

        /// <summary>
        /// Called when the affordance theme changes for this receiver, allowing proper setup to take place as a result.
        /// </summary>
        /// <param name="newValue">New affordance theme set for this receiver.</param>
        /// <seealso cref="affordanceTheme"/>
        protected virtual void OnAffordanceThemeChanged(BaseAffordanceTheme<T> newValue)
        {
            LogIfMissingAffordanceStates(newValue);
        }

        void LogIfMissingAffordanceStates(BaseAffordanceTheme<T> theme)
        {
            if (theme.GetAffordanceThemeDataForIndex((byte)(AffordanceStateShortcuts.stateCount - 1)) == null)
            {
                var sb = new StringBuilder();
                var actualCount = 0;
                for (byte index = 0; index < AffordanceStateShortcuts.stateCount; ++index)
                {
                    var themeData = theme.GetAffordanceThemeDataForIndex(index);
                    sb.Append($"Expected: {index} \"{AffordanceStateShortcuts.GetNameForIndex(index)}\",\tActual: ");
                    sb.AppendLine(themeData != null ? $"{index} \"{themeData.stateName}\"" : "<b>(None)</b>");

                    if (themeData != null)
                        ++actualCount;
                }

                Debug.LogWarning("Affordance Theme on affordance receiver is missing a potential affordance state. Expected states:" +
                    $"\nExpected Count: {AffordanceStateShortcuts.stateCount}, Actual Count: {actualCount}" +
                    $"\n{sb}", this);
            }
        }

        /// <inheritdoc/>
        public virtual void OnAffordanceStateUpdated(AffordanceStateData previousState, AffordanceStateData newState)
        {
            m_AffordanceStateData.Value = newState;
        }

        /// <summary>
        /// Update the affordance state with the new affordance value.
        /// Automatically called by Unity after the affordance tween output is calculated.
        /// </summary>
        /// <param name="newValue">The new typed affordance value.</param>
        /// <seealso cref="OnAffordanceValueUpdated"/>
        protected virtual void ConsumeAffordance(T newValue)
        {
            affordanceValue.Value = newValue;
            OnAffordanceValueUpdated(newValue);
        }

        /// <summary>
        /// Method that is called when the typed affordance value is updated.
        /// Implement this method in a derived class to apply the current affordance value,
        /// such as setting a material property or raising an event.
        /// </summary>
        /// <param name="newValue">New typed affordance value.</param>
        protected abstract void OnAffordanceValueUpdated(T newValue);

        /// <summary>
        /// One time call that captures the initial value used for certain processing operations.
        /// </summary>
        protected virtual void CaptureInitialValue()
        {
            if (initialValueCaptured)
                return;
            
            initialValue = GetCurrentValueForCapture();
            affordanceValue.Value = initialValue;
            initialValueCaptured = true;
        }

        /// <summary>
        /// Function used to get the current value of the receiver's target property.
        /// Is overriden for material properties or other targets where the initial state exists outside the receiver. 
        /// </summary>
        /// <returns>Initial value.</returns>
        protected virtual T GetCurrentValueForCapture()
        {
            return affordanceValue.Value;
        }

        /// <summary>
        /// Overrideable function that allows a receiver to modify the target affordance value before the tween is calculated.
        /// </summary>
        /// <param name="newValue">Target tween value used if no processing is applied.</param>
        /// <returns>Processed target tween value.</returns>
        protected virtual T ProcessTargetAffordanceValue(T newValue)
        {
            return newValue;
        }
    }
}