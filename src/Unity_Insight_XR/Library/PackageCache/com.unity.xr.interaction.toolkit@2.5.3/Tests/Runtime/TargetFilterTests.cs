using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    class TargetFilterTests
    {
        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllSceneObjects();
        }

        [UnityTest]
        public IEnumerator FilterComponentCanBeDestroyed()
        {
            TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateMockInteractor();
            var filter = TestUtilities.CreateTargetFilter();

            // Link the filter
            interactor.targetFilter = filter;
            Assert.That(interactor.targetFilter, Is.EqualTo(filter));
            Assert.That(filter.linkedInteractors, Is.EqualTo(new List<IXRInteractor> { interactor }));

            // Destroy the filter
            Object.Destroy(filter);
            // Unity only destroys the object after the current update loop
            yield return null;
            Assert.That(interactor.targetFilter, Is.EqualTo(null));
        }

        [Test]
        public void FilterCanAddEvaluator()
        {
            var filter = TestUtilities.CreateTargetFilter();
            Assert.That(filter.evaluators, Is.Empty);

            // Add an evaluator
            var evaluator = filter.AddEvaluator<MockEvaluator>();
            Assert.That(evaluator, Is.Not.Null);
            Assert.That(evaluator.disposed, Is.False);
            Assert.That(evaluator.filter, Is.EqualTo(filter));
            Assert.That(filter.evaluators, Is.EqualTo(new List<XRTargetEvaluator> { evaluator }));
        }

        [Test]
        public void FilterCanRemoveEvaluator()
        {
            var filter = TestUtilities.CreateTargetFilter();
            var evaluator = filter.AddEvaluator<MockEvaluator>();
            Assert.That(filter.evaluators, Is.EqualTo(new List<XRTargetEvaluator> { evaluator }));

            // Remove the added evaluator
            filter.RemoveEvaluator(evaluator);
            Assert.That(evaluator, Is.Not.Null);
            Assert.That(evaluator.disposed, Is.True);
            Assert.That(filter.evaluators, Is.Empty);
        }

        [Test]
        public void FilterCanRemoveEvaluatorAt()
        {
            var filter = TestUtilities.CreateTargetFilter();
            var head = filter.AddEvaluator<MockEvaluator>();
            var body = filter.AddEvaluator<MockEvaluator>();
            var tail = filter.AddEvaluator<MockEvaluator>();
            Assert.That(head, Is.Not.EqualTo(body));
            Assert.That(body, Is.Not.EqualTo(tail));
            Assert.That(filter.evaluators, Is.EqualTo(new List<XRTargetEvaluator> { head, body, tail }));

            // Remove body evaluator
            filter.RemoveEvaluatorAt(1);
            Assert.That(body.disposed, Is.True);
            Assert.That(filter.evaluators, Is.EqualTo(new List<XRTargetEvaluator> { head, tail }));
        }

        [Test]
        public void FilterCanMoveEvaluatorFromHeadToTail()
        {
            var filter = TestUtilities.CreateTargetFilter();
            var head = filter.AddEvaluator<MockEvaluator>();
            var body = filter.AddEvaluator<MockEvaluator>();
            var tail = filter.AddEvaluator<MockEvaluator>();
            Assert.That(head, Is.Not.EqualTo(body));
            Assert.That(body, Is.Not.EqualTo(tail));
            Assert.That(filter.evaluators, Is.EqualTo(new List<XRTargetEvaluator> { head, body, tail }));

            // Move evaluator
            filter.MoveEvaluatorTo(head, 2);
            Assert.That(filter.evaluators, Is.EqualTo(new List<XRTargetEvaluator> { body, tail, head }));
        }

        [Test]
        public void FilterCanMoveEvaluatorFromTailToHead()
        {
            var filter = TestUtilities.CreateTargetFilter();
            var head = filter.AddEvaluator<MockEvaluator>();
            var body = filter.AddEvaluator<MockEvaluator>();
            var tail = filter.AddEvaluator<MockEvaluator>();
            Assert.That(head, Is.Not.EqualTo(body));
            Assert.That(body, Is.Not.EqualTo(tail));
            Assert.That(filter.evaluators, Is.EqualTo(new List<XRTargetEvaluator> { head, body, tail }));

            // Move evaluator
            filter.MoveEvaluatorTo(tail, 0);
            Assert.That(filter.evaluators, Is.EqualTo(new List<XRTargetEvaluator> { tail, head, body }));
        }

        [Test]
        public void FilterCanMoveEvaluatorFromBodyToHead()
        {
            var filter = TestUtilities.CreateTargetFilter();
            var head = filter.AddEvaluator<MockEvaluator>();
            var body = filter.AddEvaluator<MockEvaluator>();
            var tail = filter.AddEvaluator<MockEvaluator>();
            Assert.That(head, Is.Not.EqualTo(body));
            Assert.That(body, Is.Not.EqualTo(tail));
            Assert.That(filter.evaluators, Is.EqualTo(new List<XRTargetEvaluator> { head, body, tail }));

            // Move evaluator
            filter.MoveEvaluatorTo(body, 0);
            Assert.That(filter.evaluators, Is.EqualTo(new List<XRTargetEvaluator> { body, head, tail }));
        }

        [Test]
        public void FilterCanMoveEvaluatorFromBodyToTail()
        {
            var filter = TestUtilities.CreateTargetFilter();
            var head = filter.AddEvaluator<MockEvaluator>();
            var body = filter.AddEvaluator<MockEvaluator>();
            var tail = filter.AddEvaluator<MockEvaluator>();
            Assert.That(head, Is.Not.EqualTo(body));
            Assert.That(body, Is.Not.EqualTo(tail));
            Assert.That(filter.evaluators, Is.EqualTo(new List<XRTargetEvaluator> { head, body, tail }));

            // Move evaluator
            filter.MoveEvaluatorTo(body, 2);
            Assert.That(filter.evaluators, Is.EqualTo(new List<XRTargetEvaluator> { head, tail, body }));
        }

        [Test]
        public void FilterCanGetEvaluators()
        {
            var filter = TestUtilities.CreateTargetFilter();
            var head = filter.AddEvaluator<MockEvaluator>();
            var body = filter.AddEvaluator<MockEvaluator>();
            var tail = filter.AddEvaluator<MockEvaluator>();
            Assert.That(head, Is.Not.EqualTo(body));
            Assert.That(body, Is.Not.EqualTo(tail));
            Assert.That(filter.evaluatorCount, Is.EqualTo(3));

            // GetEvaluators
            var evaluatorList = new List<XRTargetEvaluator>();
            filter.GetEvaluators(evaluatorList);
            Assert.That(evaluatorList, Is.EqualTo(new List<XRTargetEvaluator> { head, body, tail }));

            // IEnumerator
            var i = 0;
            foreach (var evaluator in (IEnumerable)filter)
            {
                Assert.That(evaluator, Is.SameAs(filter.evaluators[i]));
                i++;
            }

            // IEnumerator<XRTargetEvaluator>
            i = 0;
            foreach (var evaluator in filter)
            {
                Assert.That(evaluator, Is.SameAs(filter.evaluators[i]));
                i++;
            }

            // GetEvaluatorAt
            Assert.That(filter.GetEvaluatorAt(0), Is.SameAs(head));
            Assert.That(filter.GetEvaluatorAt(1), Is.SameAs(body));
            Assert.That(filter.GetEvaluatorAt(2), Is.SameAs(tail));
        }

        [Test]
        public void FilterCanGetEvaluatorsOfSpecifiedType()
        {
            var filter = TestUtilities.CreateTargetFilter();
            var distance = filter.AddEvaluator<XRDistanceEvaluator>();
            var angleGaze = filter.AddEvaluator<XRAngleGazeEvaluator>();
            var lastSelected = filter.AddEvaluator<XRLastSelectedEvaluator>();
            Assert.That(distance, Is.Not.EqualTo(angleGaze));
            Assert.That(angleGaze, Is.Not.EqualTo(lastSelected));
            Assert.That(filter.evaluatorCount, Is.EqualTo(3));

            // GetEvaluator(Type)
            Assert.That(filter.GetEvaluator(typeof(XRDistanceEvaluator)), Is.SameAs(distance));
            Assert.That(filter.GetEvaluator(typeof(XRAngleGazeEvaluator)), Is.SameAs(angleGaze));
            Assert.That(filter.GetEvaluator(typeof(XRLastSelectedEvaluator)), Is.SameAs(lastSelected));

            // GetEvaluator<T>
            Assert.That(filter.GetEvaluator<XRDistanceEvaluator>(), Is.SameAs(distance));
            Assert.That(filter.GetEvaluator<XRAngleGazeEvaluator>(), Is.SameAs(angleGaze));
            Assert.That(filter.GetEvaluator<XRLastSelectedEvaluator>(), Is.SameAs(lastSelected));
        }

        [Test]
        public void FilterCanGetLinkedInteractors()
        {
            TestUtilities.CreateInteractionManager();
            var interactor1 = TestUtilities.CreateMockInteractor();
            var interactor2 = TestUtilities.CreateMockInteractor();
            var interactor3 = TestUtilities.CreateMockInteractor();
            var filter = TestUtilities.CreateTargetFilter();

            // Gets the linked interactors from a newly created filter
            var linkedInteractors = new List<IXRInteractor>();
            filter.GetLinkedInteractors(linkedInteractors);
            Assert.That(linkedInteractors, Is.EqualTo(filter.linkedInteractors));
            Assert.That(linkedInteractors, Is.Empty);

            // Link all interactors with the same filter
            interactor1.targetFilter = filter;
            interactor2.targetFilter = filter;
            interactor3.targetFilter = filter;
            filter.GetLinkedInteractors(linkedInteractors);
            Assert.That(linkedInteractors, Is.EqualTo(filter.linkedInteractors));
            Assert.That(linkedInteractors, Is.EqualTo(new List<IXRInteractor> { interactor1, interactor2, interactor3 }));

            // Unlink interactor2
            interactor2.targetFilter = null;
            filter.GetLinkedInteractors(linkedInteractors);
            Assert.That(linkedInteractors, Is.EqualTo(filter.linkedInteractors));
            Assert.That(linkedInteractors, Is.EqualTo(new List<IXRInteractor> { interactor1, interactor3 }));
        }

        [UnityTest]
        public IEnumerator FilterComponentIsLinkableAtStartup()
        {
            TestUtilities.CreateInteractionManager();
            var interactor = CreateMockInteractor();
            var filter = TestUtilities.CreateTargetFilter();
            interactor.startingTargetFilter = filter;

            Assert.That(interactor.startingTargetFilter, Is.EqualTo(filter));
            Assert.That(interactor.targetFilter, Is.Null);
            Assert.That(filter.linkedInteractors, Is.Empty);

            // Check if the filter is linked after awake
            interactor.gameObject.SetActive(true);
            Assert.That(interactor.targetFilter, Is.EqualTo(filter));
            Assert.That(filter.linkedInteractors, Is.EqualTo(new List<IXRInteractor> { interactor }));

            // Destroys the interactor and checks if the filter has been unlinked
            Object.Destroy(interactor);
            yield return null;

            Assert.That(filter.linkedInteractors, Is.Empty);

            MockInteractor CreateMockInteractor()
            {
                var interactorGO = new GameObject("Mock Interactor");
                interactorGO.transform.localPosition = Vector3.zero;
                interactorGO.transform.localRotation = Quaternion.identity;
                // Deactivate the GameObject before adding the Interactor so Awake is not called yet,
                // which is the method that will set the targetFilter property
                interactorGO.SetActive(false);
                return interactorGO.AddComponent<MockInteractor>();
            }
        }

        [UnityTest]
        public IEnumerator FilterComponentIsLinkableAtStartupFromPrefab()
        {
            // This mock interactor isn't a true Unity Prefab, but it will be instantiated to simulate one
            TestUtilities.CreateInteractionManager();
            var interactorMockPrefab = TestUtilities.CreateMockInteractor();
            var filter = TestUtilities.CreateTargetFilter();
            interactorMockPrefab.startingTargetFilter = filter;

            // Instantiate a new interactor from the mock Prefab and check if the filter is linked
            var interactor = Object.Instantiate(interactorMockPrefab);
            Assert.That(interactor.startingTargetFilter, Is.EqualTo(filter));
            Assert.That(filter.linkedInteractors, Is.EqualTo(new List<IXRInteractor> { interactor }));

            // Destroys the interactor and checks if the filter has been unlinked
            Object.Destroy(interactor);
            yield return null;

            Assert.That(filter.linkedInteractors, Is.Empty);
        }

        [Test]
        public void EvaluatorCanBeEnabled()
        {
            var filter = TestUtilities.CreateTargetFilter();
            var evaluator = filter.AddEvaluator<MockEvaluator>();

            // Disable the evaluator
            evaluator.enabled = false;
            var initialEnabledEvaluatorList = new List<XRTargetEvaluator>();
            filter.GetEnabledEvaluators(initialEnabledEvaluatorList);
            Assert.That(evaluator, Is.Not.Null);
            Assert.That(evaluator.enabled, Is.False);
            Assert.That(initialEnabledEvaluatorList, Is.Empty);

            // Enable the evaluator
            evaluator.enabled = true;
            Assert.That(evaluator.enabled, Is.True);
            Assert.That(evaluator.callbackExecution, Is.EqualTo(new List<EvaluatorCallback>
            {
                EvaluatorCallback.Awake,
                EvaluatorCallback.OnEnable,
                EvaluatorCallback.OnDisable,
                EvaluatorCallback.OnEnable
            }));
            var currentEnabledEvaluatorList = new List<XRTargetEvaluator>();
            filter.GetEnabledEvaluators(currentEnabledEvaluatorList);
            Assert.That(currentEnabledEvaluatorList, Is.EqualTo(new List<XRTargetEvaluator> { evaluator }));
        }

        [Test]
        public void EvaluatorCanBeDisabled()
        {
            var filter = TestUtilities.CreateTargetFilter();
            var evaluator = filter.AddEvaluator<MockEvaluator>();
            Assert.That(evaluator, Is.Not.Null);
            Assert.That(evaluator.enabled, Is.True);
            var initialEnabledEvaluatorList = new List<XRTargetEvaluator>();
            filter.GetEnabledEvaluators(initialEnabledEvaluatorList);
            Assert.That(initialEnabledEvaluatorList, Is.EqualTo(new List<XRTargetEvaluator> { evaluator }));

            // Disable the evaluator
            evaluator.enabled = false;
            Assert.That(evaluator.enabled, Is.False);
            Assert.That(evaluator.callbackExecution, Is.EqualTo(new List<EvaluatorCallback>
            {
                EvaluatorCallback.Awake,
                EvaluatorCallback.OnEnable,
                EvaluatorCallback.OnDisable
            }));
            var currentEnabledEvaluatorList = new List<XRTargetEvaluator>();
            filter.GetEnabledEvaluators(currentEnabledEvaluatorList);
            Assert.That(currentEnabledEvaluatorList, Is.Empty);
        }

        [Test]
        public void EvaluatorCanBeDisposed()
        {
            var filter = TestUtilities.CreateTargetFilter();
            var evaluator = filter.AddEvaluator<MockEvaluator>();
            Assert.That(evaluator, Is.Not.Null);
            Assert.That(evaluator.disposed, Is.False);
            Assert.That(filter.evaluators, Is.EqualTo(new List<XRTargetEvaluator> { evaluator }));

            // Dispose the evaluator
            evaluator.Dispose();
            Assert.That(evaluator, Is.Not.Null);
            Assert.That(evaluator.disposed, Is.True);
            Assert.That(filter.evaluators, Is.Empty);
        }

        [Test]
        public void EvaluatorLifecycleCallbacks()
        {
            TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateMockInteractor();
            var interactable = TestUtilities.CreateSimpleInteractable();
            var filter = TestUtilities.CreateTargetFilter();

            // Add the evaluator
            var evaluator = filter.AddEvaluator<MockEvaluator>();
            Assert.That(evaluator.callbackExecution, Is.EqualTo(new List<EvaluatorCallback>
            {
                EvaluatorCallback.Awake,
                EvaluatorCallback.OnEnable
            }));

            // Link the interactor
            interactor.targetFilter = filter;
            Assert.That(evaluator.callbackExecution, Is.EqualTo(new List<EvaluatorCallback>
            {
                EvaluatorCallback.Awake,
                EvaluatorCallback.OnEnable,
                EvaluatorCallback.OnLink
            }));

            // Call Process in the filter
            var targets = new List<IXRInteractable> { interactable };
            var results = new List<IXRInteractable>();
            filter.Process(interactor, targets, results);
            Assert.That(evaluator.callbackExecution, Is.EqualTo(new List<EvaluatorCallback>
            {
                EvaluatorCallback.Awake,
                EvaluatorCallback.OnEnable,
                EvaluatorCallback.OnLink,
                EvaluatorCallback.CalculateNormalizedScore
            }));

            // Unlink the interactor
            interactor.targetFilter = null;
            Assert.That(evaluator.callbackExecution, Is.EqualTo(new List<EvaluatorCallback>
            {
                EvaluatorCallback.Awake,
                EvaluatorCallback.OnEnable,
                EvaluatorCallback.OnLink,
                EvaluatorCallback.CalculateNormalizedScore,
                EvaluatorCallback.OnUnlink
            }));

            // Dispose the evaluator
            evaluator.Dispose();
            Assert.That(evaluator.callbackExecution, Is.EqualTo(new List<EvaluatorCallback>
            {
                EvaluatorCallback.Awake,
                EvaluatorCallback.OnEnable,
                EvaluatorCallback.OnLink,
                EvaluatorCallback.CalculateNormalizedScore,
                EvaluatorCallback.OnUnlink,
                EvaluatorCallback.OnDisable,
                EvaluatorCallback.OnDispose
            }));
        }

        [Test]
        public void EvaluatorLinkableCallbacks()
        {
            TestUtilities.CreateInteractionManager();
            var interactor1 = TestUtilities.CreateMockInteractor();
            var interactor2 = TestUtilities.CreateMockInteractor();
            var interactor3 = TestUtilities.CreateMockInteractor();
            var filter = TestUtilities.CreateTargetFilter();

            // Create the evaluator and it's callbacks to track linked interactors
            var evaluator = filter.AddEvaluator<MockEvaluator>();
            var linkedInteractors = new List<IXRInteractor>();
            evaluator.onLinkInvoked += x => linkedInteractors.Add(x);
            evaluator.onUnlinkInvoked += x => linkedInteractors.Remove(x);
            Assert.That(linkedInteractors, Is.Empty);
            Assert.That(linkedInteractors, Is.EqualTo(filter.linkedInteractors));

            // Link all interactors with the same filter
            interactor1.targetFilter = filter;
            interactor2.targetFilter = filter;
            interactor3.targetFilter = filter;
            Assert.That(linkedInteractors, Is.EqualTo(new List<MockInteractor> { interactor1, interactor2, interactor3 }));
            Assert.That(linkedInteractors, Is.EqualTo(filter.linkedInteractors));

            // Unlink interactor1
            interactor1.targetFilter = null;
            Assert.That(linkedInteractors, Is.EqualTo(new List<IXRInteractor> { interactor2, interactor3 }));
            Assert.That(evaluator.callbackExecution, Is.EqualTo(new List<EvaluatorCallback>
            {
                EvaluatorCallback.Awake,
                EvaluatorCallback.OnEnable,
                EvaluatorCallback.OnLink,
                EvaluatorCallback.OnLink,
                EvaluatorCallback.OnLink,
                EvaluatorCallback.OnUnlink,
            }));
            Assert.That(linkedInteractors, Is.EqualTo(filter.linkedInteractors));
        }

        [Test]
        public void EvaluatorCanBeDisposedInAwake()
        {
            TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateMockInteractor();
            var filter = TestUtilities.CreateTargetFilter();
            interactor.targetFilter = filter;

            var evaluator = filter.AddEvaluator<DisposeSelfInAwakeEvaluator>();
            Assert.That(evaluator, Is.Not.Null);
            Assert.That(evaluator.disposed, Is.True);
            Assert.That(evaluator.callbackExecution, Is.EqualTo(new List<EvaluatorCallback>
            {
                EvaluatorCallback.Awake,
                EvaluatorCallback.OnDispose
            }));
        }

        [Test]
        public void EvaluatorCanBeDisposedInOnEnable()
        {
            TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateMockInteractor();
            var filter = TestUtilities.CreateTargetFilter();
            interactor.targetFilter = filter;

            var evaluator = filter.AddEvaluator<DisposeSelfInOnEnableEvaluator>();
            Assert.That(evaluator, Is.Not.Null);
            Assert.That(evaluator.disposed, Is.True);
            Assert.That(evaluator.callbackExecution, Is.EqualTo(new List<EvaluatorCallback>
            {
                EvaluatorCallback.Awake,
                EvaluatorCallback.OnLink,
                EvaluatorCallback.OnEnable,
                EvaluatorCallback.OnDisable,
                EvaluatorCallback.OnUnlink,
                EvaluatorCallback.OnDispose
            }));
        }

        [Test]
        public void EvaluatorCanBeDisabledInAwake()
        {
            TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateMockInteractor();
            var filter = TestUtilities.CreateTargetFilter();
            interactor.targetFilter = filter;

            var evaluator = filter.AddEvaluator<DisableSelfInAwakeEvaluator>();
            Assert.That(evaluator, Is.Not.Null);
            Assert.That(evaluator.disposed, Is.False);
            Assert.That(evaluator.callbackExecution, Is.EqualTo(new List<EvaluatorCallback>
            {
                EvaluatorCallback.Awake,
                EvaluatorCallback.OnLink
            }));
        }

        [Test]
        public void EvaluatorCanBeDisabledInOnEnable()
        {
            TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateMockInteractor();
            var filter = TestUtilities.CreateTargetFilter();
            interactor.targetFilter = filter;

            var evaluator = filter.AddEvaluator<DisableSelfInOnEnableEvaluator>();
            Assert.That(evaluator, Is.Not.Null);
            Assert.That(evaluator.disposed, Is.False);
            Assert.That(evaluator.callbackExecution, Is.EqualTo(new List<EvaluatorCallback>
            {
                EvaluatorCallback.Awake,
                EvaluatorCallback.OnLink,
                EvaluatorCallback.OnEnable,
                EvaluatorCallback.OnDisable
            }));
        }

        [Test]
        public void XRDistanceEvaluatorScore()
        {
            TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateMockInteractor();
            var interactable1 = TestUtilities.CreateSimpleInteractable();
            var interactable2 = TestUtilities.CreateTriggerInteractable();
            var interactable3 = TestUtilities.CreateSimpleInteractable();
            var filter = TestUtilities.CreateTargetFilter();
            var distanceEvaluator = filter.AddEvaluator<XRDistanceEvaluator>();
            distanceEvaluator.Reset();

            interactable1.transform.position = new Vector3(0.25f, 0.0f, 0.0f);
            interactable2.transform.position = new Vector3(0.5f, 0.0f, 0.0f);
            interactable3.transform.position = new Vector3(0.75f, 0.0f, 0.0f);

            filter.Link(interactor);
            var targets = new List<IXRInteractable> { interactable3, interactable2, interactable1 };
            var results = new List<IXRInteractable>();
            filter.Process(interactor, targets, results);
            Assert.That(results, Is.EqualTo(new List<IXRInteractable> { interactable1, interactable2, interactable3 }));
        }

        [Test]
        public void XRAngleGazeEvaluatorScore()
        {
            TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateMockInteractor();
            var interactable1 = TestUtilities.CreateSimpleInteractable();
            var interactable2 = TestUtilities.CreateSimpleInteractable();
            var interactable3 = TestUtilities.CreateSimpleInteractable();
            var filter = TestUtilities.CreateTargetFilter();
            var angleGazeEvaluator = filter.AddEvaluator<XRAngleGazeEvaluator>();
            angleGazeEvaluator.Reset();
            angleGazeEvaluator.gazeTransform = filter.transform;

            interactable1.transform.position = new Vector3(0.25f, 0.0f, 10f);
            interactable2.transform.position = new Vector3(0.5f, 0.0f, 10f);
            interactable3.transform.position = new Vector3(0.75f, 0.0f, 10f);

            filter.Link(interactor);
            var targets = new List<IXRInteractable> { interactable3, interactable2, interactable1 };
            var results = new List<IXRInteractable>();
            filter.Process(interactor, targets, results);
            Assert.That(results, Is.EqualTo(new List<IXRInteractable> { interactable1, interactable2, interactable3 }));
        }

        [UnityTest]
        public IEnumerator XRLastSelectedEvaluatorScore()
        {
            TestUtilities.CreateInteractionManager();
            var interactor = TestUtilities.CreateMockInteractor();
            var interactable1 = TestUtilities.CreateSimpleInteractable();
            var interactable2 = TestUtilities.CreateSimpleInteractable();
            var filter = TestUtilities.CreateTargetFilter();
            var lastSelectedEvaluator = filter.AddEvaluator<XRLastSelectedEvaluator>();
            lastSelectedEvaluator.Reset();
            filter.Link(interactor);

            interactor.validTargets.Add(interactable1);

            yield return null;

            Assert.That(interactable1.isSelected, Is.True);
            Assert.That(interactable1.interactorsSelecting, Is.EqualTo(new[] { interactor }));
            Assert.That(interactable2.isSelected, Is.False);

            yield return null;

            var targets = new List<IXRInteractable> { interactable2, interactable1 };
            var results = new List<IXRInteractable>();
            filter.Process(interactor, targets, results);
            Assert.That(results, Is.EqualTo(new List<IXRInteractable> { interactable1, interactable2 }));
        }

        enum EvaluatorCallback
        {
            Awake,
            OnDispose,
            OnEnable,
            OnDisable,
            Reset,
            CalculateNormalizedScore,
            OnLink,
            OnUnlink,
        }

        [Serializable]
        class MockEvaluator : XRTargetEvaluator, IXRTargetEvaluatorLinkable
        {
            public float score { get; set; } = 1f;
            public List<EvaluatorCallback> callbackExecution { get; } = new List<EvaluatorCallback>();

            public event Action<IXRInteractor> onLinkInvoked;
            public event Action<IXRInteractor> onUnlinkInvoked;

            protected override void Awake()
            {
                base.Awake();
                callbackExecution.Add(EvaluatorCallback.Awake);
            }

            protected override void OnDispose()
            {
                base.OnDispose();
                callbackExecution.Add(EvaluatorCallback.OnDispose);
            }

            protected override void OnEnable()
            {
                base.OnEnable();
                callbackExecution.Add(EvaluatorCallback.OnEnable);
            }

            protected override void OnDisable()
            {
                base.OnDisable();
                callbackExecution.Add(EvaluatorCallback.OnDisable);
            }

            public override void Reset()
            {
                base.Reset();
                callbackExecution.Add(EvaluatorCallback.Reset);
            }

            protected override float CalculateNormalizedScore(IXRInteractor interactor, IXRInteractable target)
            {
                callbackExecution.Add(EvaluatorCallback.CalculateNormalizedScore);
                return score;
            }

            public void OnLink(IXRInteractor interactor)
            {
                callbackExecution.Add(EvaluatorCallback.OnLink);
                onLinkInvoked?.Invoke(interactor);
            }

            public void OnUnlink(IXRInteractor interactor)
            {
                callbackExecution.Add(EvaluatorCallback.OnUnlink);
                onUnlinkInvoked?.Invoke(interactor);
            }
        }

        [Serializable]
        class DisposeSelfInAwakeEvaluator : MockEvaluator
        {
            protected override void Awake()
            {
                base.Awake();
                Dispose();
            }
        }

        [Serializable]
        class DisposeSelfInOnEnableEvaluator : MockEvaluator
        {
            protected override void OnEnable()
            {
                base.OnEnable();
                Dispose();
            }
        }

        [Serializable]
        class DisableSelfInAwakeEvaluator : MockEvaluator
        {
            protected override void Awake()
            {
                base.Awake();
                enabled = false;
            }
        }

        [Serializable]
        class DisableSelfInOnEnableEvaluator : MockEvaluator
        {
            protected override void OnEnable()
            {
                base.OnEnable();
                enabled = false;
            }
        }
    }
}
