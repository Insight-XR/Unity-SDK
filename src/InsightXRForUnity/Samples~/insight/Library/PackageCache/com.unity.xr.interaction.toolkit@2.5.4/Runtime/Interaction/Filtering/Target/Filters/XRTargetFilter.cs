using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Target Filter that uses a list of evaluator objects to filter Interactable targets (candidates)
    /// returned by the Interactor each frame.
    /// You can edit the evaluator list and the evaluators properties in the Inspector.
    /// </summary>
    /// <remarks>
    /// The <see langword="virtual"/> and <see langword="abstract"/> methods on the Target Evaluators are designed to be
    /// called by this Filter rather than being called directly by the user in order to maintain consistency between all
    /// objects involved in the filtering of an interaction.
    /// </remarks>
    /// <seealso cref="XRTargetEvaluator"/>
    /// <seealso cref="XRBaseInteractor.targetFilter"/>
    /// <seealso cref="XRBaseInteractor.startingTargetFilter"/>
    /// <seealso cref="IXRInteractor.GetValidTargets"/>
    [AddComponentMenu("XR/XR Target Filter", 11)]
    [HelpURL(XRHelpURLConstants.k_XRTargetFilter)]
    public sealed class XRTargetFilter : XRBaseTargetFilter, IEnumerable<XRTargetEvaluator>
    {
        /// <summary>
        /// Reusable list of Target Evaluators (used to fire callbacks in the evaluators).
        /// </summary>
        static readonly LinkedPool<List<XRTargetEvaluator>> s_EvaluatorListPool = new LinkedPool<List<XRTargetEvaluator>>
            (() => new List<XRTargetEvaluator>(), actionOnRelease: list => list.Clear(), collectionCheck: false);

        /// <summary>
        /// Reusable mapping of Interactables to their final score (used for sorting).
        /// </summary>
        static readonly Dictionary<IXRInteractable, float> s_InteractableFinalScoreMap =
            new Dictionary<IXRInteractable, float>();

        /// <summary>
        /// Used to avoid GC Alloc that would happen if using <see cref="InteractableScoreDescendingComparison"/> directly
        /// as argument to <see cref="List{T}.Sort(Comparison{T})"/>.
        /// </summary>
        static readonly Comparison<IXRInteractable> s_InteractableScoreComparison = InteractableScoreDescendingComparison;

#if UNITY_EDITOR
        /// <summary>
        /// (Editor Only) Reusable list of scores (caches the unused lists in <see cref="s_InteractableScoreListMap"/>).
        /// </summary>
        static readonly LinkedPool<List<float>> s_ScoreListPool = new LinkedPool<List<float>>
            (() => new List<float>(), actionOnRelease: list => list.Clear(), collectionCheck: false);

        /// <summary>
        /// (Editor Only) Reusable mapping of Interactables to their individual evaluators' scores (used to forward debug data to editors).
        /// The same index in the evaluator's score list (the Dictionary's key value) maps to the the evaluator it represents in the list returned by <see cref="GetEnabledEvaluators"/>.
        /// </summary>
        static readonly Dictionary<IXRInteractable, List<float>> s_InteractableScoreListMap =
            new Dictionary<IXRInteractable, List<float>>();
#endif

        static int InteractableScoreDescendingComparison(IXRInteractable x, IXRInteractable y)
        {
            var xFinalScore = s_InteractableFinalScoreMap[x];
            var yFinalScore = s_InteractableFinalScoreMap[y];
            if (xFinalScore < yFinalScore)
                return 1;
            if (xFinalScore > yFinalScore)
                return -1;

            return 0;
        }

#if UNITY_EDITOR
        internal static readonly List<XRTargetFilter> enabledFilters = new List<XRTargetFilter>();

        internal event Action<IXRInteractor, List<IXRInteractable>, List<IXRInteractable>, Dictionary<IXRInteractable, float>,
            Dictionary<IXRInteractable, List<float>>> processingCompleted;
#endif

        List<IXRInteractor> m_LinkedInteractors = new List<IXRInteractor>();

        /// <summary>
        /// (Read Only) List of linked Interactors.
        /// </summary>
        /// <remarks>
        /// Intended to be used by editors, debuggers and test classes.
        /// </remarks>
        internal List<IXRInteractor> linkedInteractors => m_LinkedInteractors;

        [SerializeReference]
        List<XRTargetEvaluator> m_Evaluators = new List<XRTargetEvaluator>();

        /// <summary>
        /// (Read Only) List of evaluators.
        /// </summary>
        /// <remarks>
        /// Intended to be used by editors, debuggers and test classes.
        /// </remarks>
        internal List<XRTargetEvaluator> evaluators => m_Evaluators;

        /// <summary>
        /// The number of evaluators this filter has.
        /// </summary>
        public int evaluatorCount => m_Evaluators.Count;

        bool m_IsAwake;

        /// <summary>
        /// Whether this filter is currently processing and filtering Interactables.
        /// </summary>
        internal bool isProcessing { get; private set; }

        /// <summary>
        /// Calls the methods in this invocation when this filter is linked to an Interactor.
        /// </summary>
        /// <seealso cref="Link"/>
        public event Action<IXRInteractor> interactorLinked;

        /// <summary>
        /// Calls the methods in this invocation when this filter is unlinked from an Interactor.
        /// </summary>
        /// <seealso cref="Unlink"/>
        public event Action<IXRInteractor> interactorUnlinked;

        /// <inheritdoc />
        public override bool canProcess => !isProcessing && base.canProcess;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void Awake()
        {
            m_IsAwake = true;

            using (s_EvaluatorListPool.Get(out var evaluatorList))
            {
                GetEvaluators(evaluatorList);
                for (var i = 0; i < evaluatorList.Count && m_IsAwake; i++)
                    evaluatorList[i].AwakeInternal();
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void OnEnable()
        {
#if UNITY_EDITOR
            enabledFilters.Add(this);
#endif

            using (s_EvaluatorListPool.Get(out var evaluatorList))
            {
                GetEvaluators(evaluatorList);
                for (var i = 0; i < evaluatorList.Count && isActiveAndEnabled; i++)
                {
                    var evaluator = evaluatorList[i];
                    if (evaluator.enabled)
                        evaluator.EnableInternal();
                }
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void OnDisable()
        {
#if UNITY_EDITOR
            enabledFilters.Remove(this);
#endif

            using (s_EvaluatorListPool.Get(out var enabledEvaluatorList))
            {
                GetEnabledEvaluators(enabledEvaluatorList);
                for (var i = 0; i < enabledEvaluatorList.Count && !isActiveAndEnabled; i++)
                    enabledEvaluatorList[i].DisableInternal();
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void OnDestroy()
        {
            m_IsAwake = false;

            using (s_EvaluatorListPool.Get(out var evaluatorList))
            {
                GetEvaluators(evaluatorList);
                foreach (var evaluator in evaluatorList)
                    evaluator.DisposeInternal();
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void Reset()
        {
            var distanceEvaluator = AddEvaluator<XRDistanceEvaluator>();
            distanceEvaluator.Reset();
        }

        /// <summary>
        /// Register the handlers implemented by the given evaluator to this filter.
        /// </summary>
        /// <param name="evaluator">Evaluator that implements the handlers.</param>
        /// <seealso cref="IXRTargetEvaluatorLinkable"/>>
        internal void RegisterEvaluatorHandlers(XRTargetEvaluator evaluator)
        {
            if (evaluator is IXRTargetEvaluatorLinkable linkableHandler)
            {
                interactorLinked += linkableHandler.OnLink;
                interactorUnlinked += linkableHandler.OnUnlink;

                foreach (var interactor in m_LinkedInteractors)
                    linkableHandler.OnLink(interactor);
            }
        }

        /// <summary>
        /// Unregister the handlers implemented by the given evaluator from this filter.
        /// </summary>
        /// <param name="evaluator">Evaluator that implements the handlers.</param>
        /// <seealso cref="IXRTargetEvaluatorLinkable"/>>
        internal void UnregisterEvaluatorHandlers(XRTargetEvaluator evaluator)
        {
            if (evaluator is IXRTargetEvaluatorLinkable linkableHandler)
            {
                interactorLinked -= linkableHandler.OnLink;
                interactorUnlinked -= linkableHandler.OnUnlink;

                foreach (var interactor in m_LinkedInteractors)
                    linkableHandler.OnUnlink(interactor);
            }
        }

        /// <summary>
        /// Returns the Interactors linked to this filter.
        /// </summary>
        /// <param name="results">List to receive the results.</param>
        /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// Clears <paramref name="results"/> before adding to it.
        /// </remarks>
        public void GetLinkedInteractors(List<IXRInteractor> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            results.Clear();
            results.AddRange(m_LinkedInteractors);
        }

        /// <summary>
        /// Returns the evaluators in this filter.
        /// </summary>
        /// <param name="results">List to receive the results.</param>
        /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// Clears <paramref name="results"/> before adding to it.
        /// </remarks>
        public void GetEvaluators(List<XRTargetEvaluator> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            results.Clear();
            results.AddRange(m_Evaluators);
        }

        /// <summary>
        /// Returns an enumerator to iterate through the evaluators in this Target Filter.
        /// </summary>
        /// <returns>Returns an <c>IEnumerator</c> object that can be used to iterate through the evaluators.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_Evaluators).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator to iterate through the evaluators in this Target Filter.
        /// </summary>
        /// <returns>Returns an <c>IEnumerator</c> object that can be used to iterate through the evaluators.</returns>
        public IEnumerator<XRTargetEvaluator> GetEnumerator()
        {
            return m_Evaluators.GetEnumerator();
        }

        /// <summary>
        /// Gets the evaluator in the given index.
        /// </summary>
        /// <param name="index">Index of the evaluator to return. Must be smaller than <see cref="evaluatorCount"/> and not negative.</param>
        /// <returns>The evaluator in the given index.</returns>
        /// <remarks>
        /// The total number of evaluators can be provided by <see cref="evaluatorCount"/>.
        /// </remarks>
        public XRTargetEvaluator GetEvaluatorAt(int index)
        {
            return m_Evaluators[index];
        }

        /// <summary>
        /// Returns the first Target Evaluator of the specified <paramref name="type"/> if this Target Filter has one.
        /// </summary>
        /// <param name="type">The Type of the Evaluator to retrieve.</param>
        /// <returns>Returns the first Evaluator of the specified <paramref name="type"/>. Returns <see langword="null"/> if this Filter has no Evaluator of the specified Type in it.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        /// <seealso cref="GetEvaluator{T}"/>
        public XRTargetEvaluator GetEvaluator(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            foreach (var evaluator in m_Evaluators)
            {
                if (type.IsInstanceOfType(evaluator))
                    return evaluator;
            }

            return null;
        }

        /// <summary>
        /// Returns the first Target Evaluator of the specified Type if this Target Filter has one.
        /// </summary>
        /// <typeparam name="T">The Type of the Evaluator to retrieve.</typeparam>
        /// <returns>Returns the first Evaluator of the specified Type. Returns <see langword="null"/> if this Filter has no Evaluator of the specified Type in it.</returns>
        /// <remarks>
        /// Generic version of <see cref="GetEvaluator(Type)"/>.
        /// </remarks>
        public T GetEvaluator<T>()
        {
            return (T)(object)GetEvaluator(typeof(T));
        }

        /// <summary>
        /// Returns the enabled evaluators in this filter.
        /// </summary>
        /// <param name="results">List to receive the results.</param>
        /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// Clears <paramref name="results"/> before adding to it.
        /// </remarks>
        public void GetEnabledEvaluators(List<XRTargetEvaluator> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            results.Clear();
            foreach (var evaluator in m_Evaluators)
            {
                if (evaluator.enabled)
                    results.Add(evaluator);
            }
        }

        /// <summary>
        /// Adds an instance of the given evaluator type to this filter.
        /// </summary>
        /// <param name="evaluatorType">Type of the evaluator to be added.</param>
        /// <returns>The added evaluator or null if the instance could not be added.</returns>
        /// <remarks>
        /// If the filter is processing the added evaluator will only participate from the next filtering process.
        /// </remarks>
        public XRTargetEvaluator AddEvaluator(Type evaluatorType)
        {
            if (evaluatorType == null)
                throw new ArgumentNullException(nameof(evaluatorType));

            var evaluator = XRTargetEvaluator.CreateInstance(evaluatorType, this);
            if (evaluator == null)
                return null;

            m_Evaluators.Add(evaluator);
            if (m_IsAwake)
            {
                evaluator.AwakeInternal();
                if (isActiveAndEnabled && evaluator.enabled)
                    evaluator.EnableInternal();
            }
            return evaluator;
        }

        /// <summary>
        /// Adds an instance of the given evaluator type to this filter.
        /// </summary>
        /// <typeparam name="T">Type of the evaluator to be added.</typeparam>
        /// <returns>The added evaluator or null if the instance could not be added.</returns>
        /// <remarks>
        /// If the filter is processing the added evaluator will only participate from the next filtering process.
        /// </remarks>
        public T AddEvaluator<T>() where T : XRTargetEvaluator
        {
            return AddEvaluator(typeof(T)) as T;
        }

        /// <summary>
        /// Removes the evaluator at the given index from this filter.
        /// The evaluator being removed is disabled and disposed.
        /// </summary>
        /// <param name="index">Index of the evaluator to be removed. Must be smaller than <see cref="evaluatorCount"/> and not negative.</param>
        /// <remarks>
        /// The total number of evaluators can be provided by <see cref="evaluatorCount"/>.
        /// You cannot call this method while the filter is processing.
        /// </remarks>
        public void RemoveEvaluatorAt(int index)
        {
            if (isProcessing)
                throw new InvalidOperationException($"Cannot remove evaluators while a filter {name} is processing.");

            var evaluator = m_Evaluators[index];
            if (m_IsAwake && evaluator != null)
            {
                if (isActiveAndEnabled && evaluator.enabled)
                    evaluator.DisableInternal();

                evaluator.DisposeInternal();
            }
            m_Evaluators.RemoveAt(index);
        }

        /// <summary>
        /// Removes the given evaluator from this filter.
        /// The evaluator being removed is disabled and disposed.
        /// </summary>
        /// <param name="evaluator">Evaluator to be removed.</param>
        /// <remarks>
        /// You cannot call this method while the filter is processing.
        /// </remarks>
        public void RemoveEvaluator(XRTargetEvaluator evaluator)
        {
            if (isProcessing)
                throw new InvalidOperationException($"Cannot remove evaluators while a filter {name} is processing.");

            var index = m_Evaluators.IndexOf(evaluator);
            if (index < 0)
                return;

            RemoveEvaluatorAt(index);
        }

        /// <summary>
        /// Moves the given evaluator to the given index.
        /// </summary>
        /// <param name="evaluator">Evaluator to update the index.</param>
        /// <param name="newIndex">New index of the evaluator.</param>
        public void MoveEvaluatorTo(XRTargetEvaluator evaluator, int newIndex)
        {
            var currentIndex = m_Evaluators.IndexOf(evaluator);
            if (currentIndex < 0 || currentIndex == newIndex)
                return;

            m_Evaluators.RemoveAt(currentIndex);
            m_Evaluators.Insert(newIndex, evaluator);
        }

        /// <inheritdoc />
        public override void Link(IXRInteractor interactor)
        {
            if (interactor == null)
                throw new ArgumentNullException(nameof(interactor));

            if (!m_LinkedInteractors.Contains(interactor))
            {
                m_LinkedInteractors.Add(interactor);
                interactorLinked?.Invoke(interactor);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// You cannot call this method while the filter is processing.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Throws when this filter is currently processing and filtering Interactables.</exception>
        public override void Unlink(IXRInteractor interactor)
        {
            if (interactor == null)
                throw new ArgumentNullException(nameof(interactor));
            if (isProcessing)
                throw new InvalidOperationException($"Cannot unlink an interactor {interactor} while the filter {name} is processing.");

            if (m_LinkedInteractors.Remove(interactor))
                interactorUnlinked?.Invoke(interactor);
        }

        /// <inheritdoc />
        /// <remarks>
        /// For each Interactable (in the given targets list), the evaluator list is processed in order.
        /// <br />
        /// Each enabled evaluator calculates an interaction score for the supplied Interactable. A <c>0</c> or negative score
        /// immediately stops the processing for the current Interactable.
        /// <br />
        /// These scores are multiplied together to get the final interaction score for the Interactable. A negative final
        /// score excludes the Interactable as a candidate for interaction (it won't added to the results list).
        /// <br />
        /// This final score is then used to sort (in descending order) the Interactables in the results list.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Throws when this filter is currently processing and filtering Interactables.</exception>
        public override void Process(IXRInteractor interactor, List<IXRInteractable> targets, List<IXRInteractable> results)
        {
#if UNITY_EDITOR
            Debug.Assert(s_InteractableScoreListMap.Count == 0, this);
#endif
            Debug.Assert(s_InteractableFinalScoreMap.Count == 0, this);

            if (isProcessing)
                throw new InvalidOperationException($"Process for filter {name} is already running, cannot start a new one.");
            isProcessing = true;

            try
            {
                results.Clear();

                using (s_EvaluatorListPool.Get(out var enabledEvaluatorList))
                {
                    GetEnabledEvaluators(enabledEvaluatorList);
                    foreach (var interactable in targets)
                    {
#if UNITY_EDITOR
                        // Caches a new list to store the evaluators' score for the interactable being processed
                        var interactableScoreList = s_ScoreListPool.Get();
                        s_InteractableScoreListMap[interactable] = interactableScoreList;
#endif

                        var finalScore = 1f;
                        foreach (var evaluator in enabledEvaluatorList)
                        {
                            var score = evaluator.GetWeightedScore(interactor, interactable);
#if UNITY_EDITOR
                            // Adds the score at the same index the evaluator it represents in the list returned by GetEnabledEvaluators
                            interactableScoreList.Add(score);
#endif
                            finalScore *= score;
                            if (finalScore <= 0f)
                                break;
                        }

                        if (finalScore >= 0f)
                        {
                            results.Add(interactable);
                            s_InteractableFinalScoreMap[interactable] = finalScore;
                        }
                    }
                }
                results.Sort(s_InteractableScoreComparison);

#if UNITY_EDITOR
                processingCompleted?.Invoke(interactor, targets, results, s_InteractableFinalScoreMap, s_InteractableScoreListMap);
#endif
            }
            finally
            {
                isProcessing = false;
                s_InteractableFinalScoreMap.Clear();

#if UNITY_EDITOR
                foreach (var scoreList in s_InteractableScoreListMap.Values)
                    s_ScoreListPool.Release(scoreList);
                s_InteractableScoreListMap.Clear();
#endif
            }
        }
    }
}
