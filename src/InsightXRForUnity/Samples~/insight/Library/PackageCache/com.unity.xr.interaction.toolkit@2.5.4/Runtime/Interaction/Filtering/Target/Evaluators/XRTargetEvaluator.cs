using System;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Abstract base class from which all Target Evaluators derive.
    /// This class evaluates the intention to interact with an Interactable by calculating a score (a <c>float</c> value).
    /// Used by the <see cref="XRTargetFilter"/> behavior which allows the list of valid targets returned by an Interactor
    /// to be customized as determined by these evaluators.
    /// </summary>
    /// <remarks>
    /// All <see langword="virtual"/> and <see langword="abstract"/> methods in this class
    /// (<see cref="Awake"/>, <see cref="OnDispose"/>, <see cref="OnEnable"/>, <see cref="OnDisable"/>,
    /// <see cref="Reset"/>, and <see cref="CalculateNormalizedScore"/>)
    /// are designed to be called at specific moments and you shouldn't manually call them.
    /// </remarks>
    /// <seealso cref="XRAngleGazeEvaluator"/>
    /// <seealso cref="XRDistanceEvaluator"/>
    /// <seealso cref="XRLastSelectedEvaluator"/>
    /// <seealso cref="IXRTargetEvaluatorLinkable"/>
    /// <seealso cref="XRTargetFilter"/>
    [Serializable]
    public abstract class XRTargetEvaluator : IDisposable
    {
        /// <summary>
        /// Evaluates whether the given type is a valid Target Evaluator instance and can be instantiated.
        /// </summary>
        /// <param name="evaluatorType">The type to check.</param>
        /// <returns>Returns <see langword="true"/> when the type is a valid evaluator instance. Otherwise, returns <see langword="false"/>.</returns>
        internal static bool IsInstanceType(Type evaluatorType)
        {
            return evaluatorType != null && !evaluatorType.IsInterface && !evaluatorType.IsAbstract && !evaluatorType.IsGenericType
                   && typeof(XRTargetEvaluator).IsAssignableFrom(evaluatorType);
        }

        /// <summary>
        /// Creates a new instance of the given Target Evaluator type and associates it with the given Target Filter.
        /// </summary>
        /// <param name="evaluatorType">The type of the new Target Evaluator to create.</param>
        /// <param name="filter">The filter whose the new Target Evaluator instance will be associated to.</param>
        /// <returns>The created Target Evaluator or null io it couldn't be created.</returns>
        /// <seealso cref="XRTargetFilter.AddEvaluator"/>
        /// <seealso cref="XRTargetFilter.AddEvaluator{T}"/>
        internal static XRTargetEvaluator CreateInstance(Type evaluatorType, XRTargetFilter filter)
        {
            if (!IsInstanceType(evaluatorType) || !(Activator.CreateInstance(evaluatorType) is XRTargetEvaluator instance))
                return null;

            instance.m_Filter = filter;
            instance.m_Weight = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            return instance;
        }

        [HideInInspector, SerializeField]
        XRTargetFilter m_Filter;

        /// <summary>
        /// (Read Only) The <see cref="XRTargetFilter"/> that owns this Target Evaluator.
        /// </summary>
        public XRTargetFilter filter => m_Filter;

        [HideInInspector, SerializeField, XRTargetEvaluatorEnabled]
        bool m_Enabled = true;

        /// <summary>
        /// Whether this Target Evaluator is enabled.
        /// Enabled evaluators are processed by its <see cref="filter"/>, disabled evaluators are not.
        /// </summary>
        /// <remarks>
        /// You cannot disable an evaluator while its filter is processing.
        /// If the evaluator becomes enabled while the <see cref="filter"/> is processing, it'll only participate from the next filtering process.
        /// </remarks>
        public bool enabled
        {
            get => m_Enabled;
            set
            {
                Debug.Assert(!disposed, $"Trying to change the {nameof(enabled)} property value of a disposed evaluator {GetType().Name}.", filter);

                if (m_Enabled == value || disposed)
                    return;

                if (m_Filter.isProcessing && !value)
                    throw new InvalidOperationException($"Cannot disable an evaluator {GetType().Name} while its filter {m_Filter.name} is processing.");

                m_Enabled = value;

                if (!m_IsAwake || !m_Filter.isActiveAndEnabled)
                    return;

                if (value)
                    EnableInternal();
                else
                    DisableInternal();
            }
        }

        [Tooltip("The weight curve of this evaluator. Use this curve to configure the returned score." +
                 "\n\nThe x-axis is the normalized score (calculated in CalculateNormalizedScore) and the y-axis is the returned score of this evaluator.")]
        [SerializeField]
        AnimationCurve m_Weight;

        /// <summary>
        /// The weight curve of this evaluator. Use this curve to configure the returned value of the <see cref="GetWeightedScore"/> method.
        /// </summary>
        /// <remarks>
        /// The x-axis is the normalized score (calculated in <see cref="CalculateNormalizedScore"/>) and the y-axis is the
        /// returned weighted score of this evaluator (in <see cref="GetWeightedScore"/>).
        /// </remarks>
        /// <seealso cref="GetWeightedScore"/>
        public AnimationCurve weight
        {
            get => m_Weight;
            set => m_Weight = value;
        }

        /// <summary>
        /// (Read Only) Whether this evaluator was disposed.
        /// A disposed Target Evaluator has no use and you should not keep a reference to it.
        /// </summary>
        /// <seealso cref="Dispose"/>
        public bool disposed => m_Filter == null;

        bool m_IsAwake;
        bool m_IsEnabled;
        bool m_IsRegistered;

        void RegisterHandlers()
        {
            if (m_IsRegistered || disposed)
                return;

            m_IsRegistered = true;
            m_Filter.RegisterEvaluatorHandlers(this);
        }

        void UnregisterHandlers()
        {
            if (!m_IsRegistered || disposed)
                return;

            m_IsRegistered = false;
            m_Filter.UnregisterEvaluatorHandlers(this);
        }

        internal void AwakeInternal()
        {
            if (m_IsAwake || disposed)
                return;

            m_IsAwake = true;
            Awake();
            RegisterHandlers();
        }

        internal void DisposeInternal()
        {
            if (!m_IsAwake)
                return;

            m_IsAwake = false;
            UnregisterHandlers();
            OnDispose();
            m_Filter = null;
        }

        internal void EnableInternal()
        {
            if (m_IsEnabled || disposed)
                return;

            m_IsEnabled = true;
            OnEnable();
        }

        internal void DisableInternal()
        {
            if (!m_IsEnabled)
                return;

            m_IsEnabled = false;
            OnDisable();
        }

        /// <summary>
        /// Unity calls this automatically when the evaluator instance is being loaded.
        /// </summary>
        /// <seealso cref="XRTargetFilter.AddEvaluator"/>
        /// <seealso cref="XRTargetFilter.AddEvaluator{T}"/>
        protected virtual void Awake()
        {
        }

        /// <summary>
        /// Unity calls this automatically when the evaluator is being disposed.
        /// This is also called when the evaluator <see cref="filter"/> is destroyed.
        /// </summary>
        /// <remarks>
        /// <c>OnDispose</c> will only be called if a call to <see cref="Awake"/> happened before.
        /// </remarks>
        /// <seealso cref="Dispose"/>
        /// <seealso cref="XRTargetFilter.RemoveEvaluator"/>
        /// <seealso cref="XRTargetFilter.RemoveEvaluatorAt"/>
        protected virtual void OnDispose()
        {
        }

        /// <summary>
        /// Unity calls this automatically when the evaluator becomes enabled and active.
        /// </summary>
        /// <seealso cref="enabled"/>
        protected virtual void OnEnable()
        {
        }

        /// <summary>
        /// Unity calls this automatically when the evaluator becomes disabled. Use this method for any code cleanup.
        /// This is also called when the evaluator is disposed or when its <see cref="filter"/> is destroyed or disabled.
        /// </summary>
        /// <remarks>
        /// When scripts are reloaded after compilation has finished, <c>OnDisable</c> will be called, followed by <c>OnEnable</c>
        /// after the evaluator has been loaded.
        /// <br />
        /// <c>OnDisable</c> will only be called if a call to <see cref="OnEnable"/> happened before.
        /// </remarks>
        /// <seealso cref="enabled"/>
        protected virtual void OnDisable()
        {
        }

        /// <summary>
        /// (Editor Only) Unity calls this automatically when adding the evaluator to the Filter Target the first time.
        /// This function is only called in the Unity editor. Reset is most commonly used to give good default values in the Inspector.
        /// </summary>
        public virtual void Reset()
        {
        }

        /// <summary>
        /// The <see cref="filter"/> calls this method to get a value that represents the intention to select the given target Interactable.
        /// Gets the weighted interaction score, the y-axis value in the <see cref="weight"/> curve for an x-axis value returned by
        /// <see cref="CalculateNormalizedScore"/>.
        /// </summary>
        /// <param name="interactor">The Interactor whose Interactable targets (candidates) are being evaluated.</param>
        /// <param name="target">The Interactable to evaluate the weighted score.</param>
        /// <returns>
        /// Returns the weighted interaction score of the given target. Usually a normalized value but it can be negative and more than <c>1</c>.
        /// <br />
        /// You can configure the returned value of this method by editing the <see cref="weight"/> curve in the Inspector.
        /// <br />
        /// Returns <c>1</c> if the intention is to select the given target.
        /// <br />
        /// Returns <c>0</c> if the intention is to not select the given target. This will stop the evaluation process
        /// for the given target.
        /// <br />
        /// Returns a negative value to exclude the given target from the list of targets for interaction.
        /// </returns>
        /// <seealso cref="XRTargetFilter"/>
        /// <seealso cref="IXRInteractor.GetValidTargets"/>
        public float GetWeightedScore(IXRInteractor interactor, IXRInteractable target)
        {
            return m_Weight.Evaluate(CalculateNormalizedScore(interactor, target));
        }

        /// <summary>
        /// Calculates and returns a normalized value that represents the intention to select the given target Interactable.
        /// <br />
        /// The highest score of <c>1</c> represents the intention to select the given target and the lowest score of <c>0</c>
        /// the intention to not select it, any value in between is valid.
        /// </summary>
        /// <param name="interactor">The Interactor whose Interactable targets (candidates) are being evaluated.</param>
        /// <param name="target">The target Interactable to evaluate the normalized score.</param>
        /// <returns>
        /// Returns the normalized interaction score of the given target.
        /// <br />
        /// It's a good practice to return a value between <c>0</c> and <c>1</c> inclusive and edit the <see cref="weight"/> curve
        /// to evaluate the target weighted interaction score.
        /// </returns>
        /// <remarks>
        /// The returned normalized score is evaluated by the <see cref="weight"/> curve (in <see cref="GetWeightedScore"/>) and then
        /// (in <see cref="XRTargetFilter.Process"/>) it's multiplied by the other evaluator weighted score in the same <see cref="filter"/>
        /// to get the target final score.
        /// <br />
        /// You cannot disable, remove, or dispose evaluators from the <see cref="filter"/> in this method.
        /// You also cannot unlink Interactors from the <see cref="filter"/> in this method.
        /// </remarks>
        /// <seealso cref="XRTargetFilter"/>
        /// <seealso cref="IXRInteractor.GetValidTargets"/>
        protected abstract float CalculateNormalizedScore(IXRInteractor interactor, IXRInteractable target);

        /// <summary>
        /// Call this to dispose this Target Evaluator. This removes the evaluator from its filter.
        /// After this call the evaluator has no use and you should not keep a reference to it.
        /// </summary>
        /// <seealso cref="XRTargetFilter.Process"/>
        public void Dispose()
        {
            Debug.Assert(!disposed, $"Trying to dispose an already disposed evaluator {GetType().Name}", filter);

            if (m_Filter != null)
                m_Filter.RemoveEvaluator(this);
        }
    }
}
