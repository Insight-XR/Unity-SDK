using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    class InteractionGroupTests
    {
        // ReSharper disable once InconsistentNaming -- Treat this like const
        static readonly Regex k_AnyString = new Regex("");
        static readonly List<IXRGroupMember> s_GroupMembers = new List<IXRGroupMember>();
        static readonly HashSet<IXRGroupMember> s_OverrideGroupMembers = new HashSet<IXRGroupMember>();

        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllSceneObjects();
        }

        [Test]
        public void StartingMembersAreAddedToGroup()
        {
            TestUtilities.CreateInteractionManager();

            // Start inactive so we can set starting members before Awake
            var groupObj = new GameObject("Interaction Group");
            groupObj.SetActive(false);
            var group = groupObj.AddComponent<XRInteractionGroup>();

            // Member interactors are active and enabled so they should register with interaction manager as soon as they are created
            var memberInteractor1 = TestUtilities.CreateMockInteractor();
            var memberInteractor2 = TestUtilities.CreateMockInteractor();
            var memberInteractor3 = TestUtilities.CreateMockInteractor();
            group.startingGroupMembers = new List<Object>
            {
                memberInteractor1,
                memberInteractor2,
                memberInteractor3
            };

            // Interactors should be re-registered with manager after being added to group
            IXRInteractor registeredMemberInteractor1 = null;
            IXRInteractor registeredMemberInteractor2 = null;
            IXRInteractor registeredMemberInteractor3 = null;
            IXRInteractionGroup registeredMember1Group = null;
            IXRInteractionGroup registeredMember2Group = null;
            IXRInteractionGroup registeredMember3Group = null;

            memberInteractor1.registered += args =>
            {
                registeredMemberInteractor1 = args.interactorObject;
                registeredMember1Group = args.containingGroupObject;
            };

            memberInteractor2.registered += args =>
            {
                registeredMemberInteractor2 = args.interactorObject;
                registeredMember2Group = args.containingGroupObject;
            };

            memberInteractor3.registered += args =>
            {
                registeredMemberInteractor3 = args.interactorObject;
                registeredMember3Group = args.containingGroupObject;
            };

            groupObj.SetActive(true);

            Assert.That(memberInteractor1.containingGroup, Is.EqualTo(group));
            Assert.That(memberInteractor2.containingGroup, Is.EqualTo(group));
            Assert.That(memberInteractor3.containingGroup, Is.EqualTo(group));
            Assert.That(group.ContainsGroupMember(memberInteractor1), Is.True);
            Assert.That(group.ContainsGroupMember(memberInteractor2), Is.True);
            Assert.That(group.ContainsGroupMember(memberInteractor3), Is.True);
            group.GetGroupMembers(s_GroupMembers);
            Assert.That(s_GroupMembers.Count, Is.EqualTo(3));
            Assert.That(s_GroupMembers[0], Is.EqualTo(memberInteractor1));
            Assert.That(s_GroupMembers[1], Is.EqualTo(memberInteractor2));
            Assert.That(s_GroupMembers[2], Is.EqualTo(memberInteractor3));

            Assert.That(registeredMemberInteractor1, Is.EqualTo(memberInteractor1));
            Assert.That(registeredMemberInteractor2, Is.EqualTo(memberInteractor2));
            Assert.That(registeredMemberInteractor3, Is.EqualTo(memberInteractor3));
            Assert.That(registeredMember1Group, Is.EqualTo(group));
            Assert.That(registeredMember2Group, Is.EqualTo(group));
            Assert.That(registeredMember3Group, Is.EqualTo(group));
        }

        [Test]
        public void AddMembersToGroupDynamically()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateInteractionGroup();
            var memberInteractor1 = TestUtilities.CreateMockInteractor();
            var memberInteractor2 = TestUtilities.CreateMockInteractor();
            var memberInteractor3 = TestUtilities.CreateMockInteractor();

            // Add 2 and 3 to the group first, then add 1 as the highest priority
            group.AddGroupMember(memberInteractor2);
            Assert.That(group.ContainsGroupMember(memberInteractor2), Is.True);
            group.GetGroupMembers(s_GroupMembers);
            Assert.That(s_GroupMembers.Count, Is.EqualTo(1));
            Assert.That(s_GroupMembers[0], Is.EqualTo(memberInteractor2));

            group.AddGroupMember(memberInteractor3);
            Assert.That(group.ContainsGroupMember(memberInteractor3), Is.True);
            group.GetGroupMembers(s_GroupMembers);
            Assert.That(s_GroupMembers.Count, Is.EqualTo(2));
            Assert.That(s_GroupMembers[0], Is.EqualTo(memberInteractor2));
            Assert.That(s_GroupMembers[1], Is.EqualTo(memberInteractor3));

            group.MoveGroupMemberTo(memberInteractor1, 0);
            Assert.That(group.ContainsGroupMember(memberInteractor1), Is.True);
            group.GetGroupMembers(s_GroupMembers);
            Assert.That(s_GroupMembers.Count, Is.EqualTo(3));
            Assert.That(s_GroupMembers[0], Is.EqualTo(memberInteractor1));
            Assert.That(s_GroupMembers[1], Is.EqualTo(memberInteractor2));
            Assert.That(s_GroupMembers[2], Is.EqualTo(memberInteractor3));

            Assert.That(memberInteractor1.containingGroup, Is.EqualTo(group));
            Assert.That(memberInteractor2.containingGroup, Is.EqualTo(group));
            Assert.That(memberInteractor3.containingGroup, Is.EqualTo(group));
        }

        [Test]
        public void RemoveMembersFromGroup()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(out var memberInteractor1,
                out var memberInteractor2, out var memberInteractor3);

            Assert.That(group.RemoveGroupMember(memberInteractor1), Is.True);
            Assert.That(memberInteractor1.containingGroup, Is.Null);
            Assert.That(group.ContainsGroupMember(memberInteractor1), Is.False);
            group.GetGroupMembers(s_GroupMembers);
            Assert.That(s_GroupMembers.Count, Is.EqualTo(2));
            Assert.That(s_GroupMembers[0], Is.EqualTo(memberInteractor2));
            Assert.That(s_GroupMembers[1], Is.EqualTo(memberInteractor3));

            Assert.That(group.RemoveGroupMember(memberInteractor1), Is.False);
        }

        [Test]
        public void ClearGroupMembers()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(out var memberInteractor1,
                out var memberInteractor2, out var memberInteractor3);

            group.ClearGroupMembers();
            Assert.That(memberInteractor1.containingGroup, Is.Null);
            Assert.That(memberInteractor2.containingGroup, Is.Null);
            Assert.That(memberInteractor3.containingGroup, Is.Null);
            Assert.That(group.ContainsGroupMember(memberInteractor1), Is.False);
            Assert.That(group.ContainsGroupMember(memberInteractor2), Is.False);
            Assert.That(group.ContainsGroupMember(memberInteractor3), Is.False);
            group.GetGroupMembers(s_GroupMembers);
            Assert.That(s_GroupMembers.Count, Is.Zero);
        }

        [Test]
        public void CannotCreateCircularDependencyOfGroups()
        {
            TestUtilities.CreateInteractionManager();
            var group1 = TestUtilities.CreateInteractionGroup();
            var group2 = TestUtilities.CreateInteractionGroup();
            var group3 = TestUtilities.CreateInteractionGroup();

            LogAssert.Expect(LogType.Error, k_AnyString);
            group1.AddGroupMember(group1);
            Assert.That(group1.containingGroup, Is.Null);
            Assert.That(group1.ContainsGroupMember(group1), Is.False);

            LogAssert.Expect(LogType.Error, k_AnyString);
            group1.MoveGroupMemberTo(group1, 0);
            Assert.That(group1.containingGroup, Is.Null);
            Assert.That(group1.ContainsGroupMember(group1), Is.False);

            group1.AddGroupMember(group2);
            Assert.That(group2.containingGroup, Is.EqualTo(group1));
            Assert.That(group1.ContainsGroupMember(group2), Is.True);

            LogAssert.Expect(LogType.Error, k_AnyString);
            group2.AddGroupMember(group1);
            Assert.That(group1.containingGroup, Is.Null);
            Assert.That(group2.ContainsGroupMember(group1), Is.False);

            LogAssert.Expect(LogType.Error, k_AnyString);
            group2.MoveGroupMemberTo(group1, 0);
            Assert.That(group1.containingGroup, Is.Null);
            Assert.That(group2.ContainsGroupMember(group1), Is.False);

            group3.AddGroupMember(group1);
            Assert.That(group1.containingGroup, Is.EqualTo(group3));
            Assert.That(group3.ContainsGroupMember(group1), Is.True);

            LogAssert.Expect(LogType.Error, k_AnyString);
            group2.AddGroupMember(group3);
            Assert.That(group3.containingGroup, Is.Null);
            Assert.That(group2.ContainsGroupMember(group3), Is.False);

            LogAssert.Expect(LogType.Error, k_AnyString);
            group2.MoveGroupMemberTo(group3, 0);
            Assert.That(group3.containingGroup, Is.Null);
            Assert.That(group2.ContainsGroupMember(group3), Is.False);
        }

        [Test]
        public void CannotAddGroupMemberThatIsNotAnInteractorOrGroup()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateInteractionGroup();
            var invalidGroupMember = new InvalidGroupMember();

            LogAssert.Expect(LogType.Error, k_AnyString);
            group.AddGroupMember(invalidGroupMember);
            Assert.That(group.ContainsGroupMember(invalidGroupMember), Is.False);

            LogAssert.Expect(LogType.Error, k_AnyString);
            group.MoveGroupMemberTo(invalidGroupMember, 0);
            Assert.That(group.ContainsGroupMember(invalidGroupMember), Is.False);
        }

        [Test]
        public void CannotAddGroupMemberThatIsAlreadyPartOfDifferentGroup()
        {
            TestUtilities.CreateInteractionManager();
            var group1 = TestUtilities.CreateInteractionGroup();
            var group2 = TestUtilities.CreateInteractionGroup();
            var groupMember = TestUtilities.CreateMockInteractor();

            group1.AddGroupMember(groupMember);

            group1.AddGroupMember(groupMember);
            Assert.That(groupMember.containingGroup, Is.EqualTo(group1));
            Assert.That(group1.ContainsGroupMember(groupMember), Is.True);

            LogAssert.Expect(LogType.Error, k_AnyString);
            group2.AddGroupMember(groupMember);
            Assert.That(groupMember.containingGroup, Is.EqualTo(group1));
            Assert.That(group1.ContainsGroupMember(groupMember), Is.True);
            Assert.That(group2.ContainsGroupMember(groupMember), Is.False);

            LogAssert.Expect(LogType.Error, k_AnyString);
            group2.MoveGroupMemberTo(groupMember, 0);
            Assert.That(groupMember.containingGroup, Is.EqualTo(group1));
            Assert.That(group1.ContainsGroupMember(groupMember), Is.True);
            Assert.That(group2.ContainsGroupMember(groupMember), Is.False);

            group1.RemoveGroupMember(groupMember);
            group2.AddGroupMember(groupMember);
            Assert.That(groupMember.containingGroup, Is.EqualTo(group2));
            Assert.That(group1.ContainsGroupMember(groupMember), Is.False);
            Assert.That(group2.ContainsGroupMember(groupMember), Is.True);
        }

        [Test]
        public void DisablingGroupReRegistersMemberInteractorsAsRegularInteractors()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(out var memberInteractor1,
                out var memberInteractor2, out var memberInteractor3);

            // Interactors should be re-registered with manager after being removed from group
            IXRInteractor registeredMemberInteractor1 = null;
            IXRInteractor registeredMemberInteractor2 = null;
            IXRInteractor registeredMemberInteractor3 = null;
            IXRInteractionGroup registeredMember1Group = null;
            IXRInteractionGroup registeredMember2Group = null;
            IXRInteractionGroup registeredMember3Group = null;

            memberInteractor1.registered += args =>
            {
                registeredMemberInteractor1 = args.interactorObject;
                registeredMember1Group = args.containingGroupObject;
            };

            memberInteractor2.registered += args =>
            {
                registeredMemberInteractor2 = args.interactorObject;
                registeredMember2Group = args.containingGroupObject;
            };

            memberInteractor3.registered += args =>
            {
                registeredMemberInteractor3 = args.interactorObject;
                registeredMember3Group = args.containingGroupObject;
            };

            group.enabled = false;

            Assert.That(registeredMemberInteractor1, Is.EqualTo(memberInteractor1));
            Assert.That(registeredMemberInteractor2, Is.EqualTo(memberInteractor2));
            Assert.That(registeredMemberInteractor3, Is.EqualTo(memberInteractor3));
            Assert.That(registeredMember1Group, Is.Null);
            Assert.That(registeredMember2Group, Is.Null);
            Assert.That(registeredMember3Group, Is.Null);
            Assert.That(memberInteractor1.containingGroup, Is.Null);
            Assert.That(memberInteractor2.containingGroup, Is.Null);
            Assert.That(memberInteractor3.containingGroup, Is.Null);

            // A disabled group should still reference its members even if they are not functionally part of the group
            Assert.That(group.ContainsGroupMember(memberInteractor1), Is.True);
            Assert.That(group.ContainsGroupMember(memberInteractor2), Is.True);
            Assert.That(group.ContainsGroupMember(memberInteractor3), Is.True);
        }

        [Test]
        public void ReEnablingGroupReRegistersRegularInteractorsAsMemberInteractors()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(out var memberInteractor1,
                out var memberInteractor2, out var memberInteractor3);

            group.enabled = false;

            IXRInteractor registeredMemberInteractor1 = null;
            IXRInteractor registeredMemberInteractor2 = null;
            IXRInteractor registeredMemberInteractor3 = null;
            IXRInteractionGroup registeredMember1Group = null;
            IXRInteractionGroup registeredMember2Group = null;
            IXRInteractionGroup registeredMember3Group = null;

            memberInteractor1.registered += args =>
            {
                registeredMemberInteractor1 = args.interactorObject;
                registeredMember1Group = args.containingGroupObject;
            };

            memberInteractor2.registered += args =>
            {
                registeredMemberInteractor2 = args.interactorObject;
                registeredMember2Group = args.containingGroupObject;
            };

            memberInteractor3.registered += args =>
            {
                registeredMemberInteractor3 = args.interactorObject;
                registeredMember3Group = args.containingGroupObject;
            };

            group.enabled = true;

            Assert.That(registeredMemberInteractor1, Is.EqualTo(memberInteractor1));
            Assert.That(registeredMemberInteractor2, Is.EqualTo(memberInteractor2));
            Assert.That(registeredMemberInteractor3, Is.EqualTo(memberInteractor3));
            Assert.That(registeredMember1Group, Is.EqualTo(group));
            Assert.That(registeredMember2Group, Is.EqualTo(group));
            Assert.That(registeredMember3Group, Is.EqualTo(group));
            Assert.That(memberInteractor1.containingGroup, Is.EqualTo(group));
            Assert.That(memberInteractor2.containingGroup, Is.EqualTo(group));
            Assert.That(memberInteractor3.containingGroup, Is.EqualTo(group));
            Assert.That(group.ContainsGroupMember(memberInteractor1), Is.True);
            Assert.That(group.ContainsGroupMember(memberInteractor2), Is.True);
            Assert.That(group.ContainsGroupMember(memberInteractor3), Is.True);
        }

        [Test]
        public void DisablingGroupReRegistersMemberGroupsAsRegularGroups()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithEmptyGroups(out var memberGroup1,
                out var memberGroup2, out var memberGroup3);

            // Groups should be re-registered with manager after being removed from group
            IXRInteractionGroup registeredMemberGroup1 = null;
            IXRInteractionGroup registeredMemberGroup2 = null;
            IXRInteractionGroup registeredMemberGroup3 = null;
            IXRInteractionGroup registeredMember1Group = null;
            IXRInteractionGroup registeredMember2Group = null;
            IXRInteractionGroup registeredMember3Group = null;

            memberGroup1.registered += args =>
            {
                registeredMemberGroup1 = args.interactionGroupObject;
                registeredMember1Group = args.containingGroupObject;
            };

            memberGroup2.registered += args =>
            {
                registeredMemberGroup2 = args.interactionGroupObject;
                registeredMember2Group = args.containingGroupObject;
            };

            memberGroup3.registered += args =>
            {
                registeredMemberGroup3 = args.interactionGroupObject;
                registeredMember3Group = args.containingGroupObject;
            };

            group.enabled = false;

            Assert.That(registeredMemberGroup1, Is.EqualTo(memberGroup1));
            Assert.That(registeredMemberGroup2, Is.EqualTo(memberGroup2));
            Assert.That(registeredMemberGroup3, Is.EqualTo(memberGroup3));
            Assert.That(registeredMember1Group, Is.Null);
            Assert.That(registeredMember2Group, Is.Null);
            Assert.That(registeredMember3Group, Is.Null);
            Assert.That(memberGroup1.containingGroup, Is.Null);
            Assert.That(memberGroup2.containingGroup, Is.Null);
            Assert.That(memberGroup3.containingGroup, Is.Null);

            // A disabled group should still reference its members even if they are not functionally part of the group
            Assert.That(group.ContainsGroupMember(memberGroup1), Is.True);
            Assert.That(group.ContainsGroupMember(memberGroup2), Is.True);
            Assert.That(group.ContainsGroupMember(memberGroup3), Is.True);
        }

        [Test]
        public void ReEnablingGroupReRegistersRegularGroupsAsMemberGroups()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithEmptyGroups(out var memberGroup1,
                out var memberGroup2, out var memberGroup3);

            group.enabled = false;

            IXRInteractionGroup registeredMemberGroup1 = null;
            IXRInteractionGroup registeredMemberGroup2 = null;
            IXRInteractionGroup registeredMemberGroup3 = null;
            IXRInteractionGroup registeredMember1Group = null;
            IXRInteractionGroup registeredMember2Group = null;
            IXRInteractionGroup registeredMember3Group = null;

            memberGroup1.registered += args =>
            {
                registeredMemberGroup1 = args.interactionGroupObject;
                registeredMember1Group = args.containingGroupObject;
            };

            memberGroup2.registered += args =>
            {
                registeredMemberGroup2 = args.interactionGroupObject;
                registeredMember2Group = args.containingGroupObject;
            };

            memberGroup3.registered += args =>
            {
                registeredMemberGroup3 = args.interactionGroupObject;
                registeredMember3Group = args.containingGroupObject;
            };

            group.enabled = true;

            Assert.That(registeredMemberGroup1, Is.EqualTo(memberGroup1));
            Assert.That(registeredMemberGroup2, Is.EqualTo(memberGroup2));
            Assert.That(registeredMemberGroup3, Is.EqualTo(memberGroup3));
            Assert.That(registeredMember1Group, Is.EqualTo(group));
            Assert.That(registeredMember2Group, Is.EqualTo(group));
            Assert.That(registeredMember3Group, Is.EqualTo(group));
            Assert.That(memberGroup1.containingGroup, Is.EqualTo(group));
            Assert.That(memberGroup1.containingGroup, Is.EqualTo(group));
            Assert.That(memberGroup1.containingGroup, Is.EqualTo(group));
            Assert.That(group.ContainsGroupMember(memberGroup1), Is.True);
            Assert.That(group.ContainsGroupMember(memberGroup2), Is.True);
            Assert.That(group.ContainsGroupMember(memberGroup3), Is.True);
        }

        [UnityTest]
        public IEnumerator InteractorsInGroupReceivePreprocessAndProcess()
        {
            TestUtilities.CreateInteractionManager();
            TestUtilities.CreateGroupWithMockInteractors(out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            var interactor1Preprocessed = false;
            var interactor2Preprocessed = false;
            var interactor3Preprocessed = false;
            var interactor1Processed = false;
            var interactor2Processed = false;
            var interactor3Processed = false;
            memberInteractor1.preprocessed += updatePhase => interactor1Preprocessed = true;
            memberInteractor2.preprocessed += updatePhase => interactor2Preprocessed = true;
            memberInteractor3.preprocessed += updatePhase => interactor3Preprocessed = true;
            memberInteractor1.processed += updatePhase => interactor1Processed = true;
            memberInteractor2.processed += updatePhase => interactor2Processed = true;
            memberInteractor3.processed += updatePhase => interactor3Processed = true;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(interactor1Preprocessed, Is.True);
            Assert.That(interactor2Preprocessed, Is.True);
            Assert.That(interactor3Preprocessed, Is.True);
            Assert.That(interactor1Processed, Is.True);
            Assert.That(interactor2Processed, Is.True);
            Assert.That(interactor3Processed, Is.True);
        }

        [UnityTest]
        public IEnumerator OnlyHighestPriorityInteractorInGroupCanHover()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithHoverOnlyMockInteractors(
                out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            var interactable1 = TestUtilities.CreateSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);
            memberInteractor2.validTargets.Add(interactable1);
            memberInteractor3.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor3.hasHover, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));

            // Now test with different interactables
            var interactable2 = TestUtilities.CreateSimpleInteractable();
            var interactable3 = TestUtilities.CreateSimpleInteractable();
            memberInteractor2.validTargets.Clear();
            memberInteractor2.validTargets.Add(interactable2);
            memberInteractor3.validTargets.Clear();
            memberInteractor3.validTargets.Add(interactable3);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor3.hasHover, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));
        }

        [UnityTest]
        public IEnumerator OnlyHighestPriorityGroupInGroupCanHover()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateInteractionGroup();

            var memberGroup1 = TestUtilities.CreateGroupWithHoverOnlyMockInteractors(
                out var group1MemberInteractor1, out var group1MemberInteractor2, out var group1MemberInteractor3);

            var memberGroup2 = TestUtilities.CreateGroupWithHoverOnlyMockInteractors(
                out var group2MemberInteractor1, out var group2MemberInteractor2, out var group2MemberInteractor3);

            var memberGroup3 = TestUtilities.CreateGroupWithHoverOnlyMockInteractors(
                out var group3MemberInteractor1, out var group3MemberInteractor2, out var group3MemberInteractor3);

            group.AddGroupMember(memberGroup1);
            group.AddGroupMember(memberGroup2);
            group.AddGroupMember(memberGroup3);

            var interactable = TestUtilities.CreateSimpleInteractable();
            group1MemberInteractor1.validTargets.Add(interactable);
            group1MemberInteractor2.validTargets.Add(interactable);
            group1MemberInteractor3.validTargets.Add(interactable);
            group2MemberInteractor1.validTargets.Add(interactable);
            group2MemberInteractor2.validTargets.Add(interactable);
            group2MemberInteractor3.validTargets.Add(interactable);
            group3MemberInteractor1.validTargets.Add(interactable);
            group3MemberInteractor2.validTargets.Add(interactable);
            group3MemberInteractor3.validTargets.Add(interactable);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(group1MemberInteractor1.IsHovering(interactable), Is.True);
            Assert.That(group1MemberInteractor2.hasHover, Is.False);
            Assert.That(group1MemberInteractor3.hasHover, Is.False);
            Assert.That(group2MemberInteractor1.hasHover, Is.False);
            Assert.That(group2MemberInteractor2.hasHover, Is.False);
            Assert.That(group2MemberInteractor3.hasHover, Is.False);
            Assert.That(group3MemberInteractor1.hasHover, Is.False);
            Assert.That(group3MemberInteractor2.hasHover, Is.False);
            Assert.That(group3MemberInteractor3.hasHover, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(group1MemberInteractor1));
            Assert.That(memberGroup1.activeInteractor, Is.EqualTo(group1MemberInteractor1));
            Assert.That(memberGroup2.activeInteractor, Is.Null);
            Assert.That(memberGroup3.activeInteractor, Is.Null);
        }

        [UnityTest]
        public IEnumerator HigherPriorityInteractorOvertakesHover()
        {
            TestUtilities.CreateInteractionManager();
            TestUtilities.CreateGroupWithHoverOnlyMockInteractors(out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            var interactable1 = TestUtilities.CreateSimpleInteractable();
            memberInteractor3.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor3.IsHovering(interactable1), Is.True);

            memberInteractor2.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor2.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor3.hasHover, Is.False);

            var interactable2 = TestUtilities.CreateSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable2);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable2), Is.True);
            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor3.hasHover, Is.False);
        }

        [UnityTest]
        public IEnumerator DynamicallyAddedHigherPriorityInteractorOvertakesHover()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateInteractionGroup();
            var memberInteractor1 = TestUtilities.CreateMockInteractor();
            var memberInteractor2 = TestUtilities.CreateMockInteractor();
            var memberInteractor3 = TestUtilities.CreateMockInteractor();
            memberInteractor1.allowSelect = false;
            memberInteractor2.allowSelect = false;
            memberInteractor3.allowSelect = false;

            var interactable1 = TestUtilities.CreateSimpleInteractable();
            memberInteractor2.validTargets.Add(interactable1);
            memberInteractor3.validTargets.Add(interactable1);

            var interactable2 = TestUtilities.CreateSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable2);

            group.AddGroupMember(memberInteractor3);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor3.IsHovering(interactable1), Is.True);

            group.MoveGroupMemberTo(memberInteractor2, 0);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor2.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor3.hasHover, Is.False);

            group.MoveGroupMemberTo(memberInteractor1, 0);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable2), Is.True);
            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor3.hasHover, Is.False);
        }

        [UnityTest]
        public IEnumerator EndingHigherPriorityHoverTriggersNextPriorityHover()
        {
            TestUtilities.CreateInteractionManager();
            TestUtilities.CreateGroupWithHoverOnlyMockInteractors(out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            var interactable1 = TestUtilities.CreateSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);
            memberInteractor2.validTargets.Add(interactable1);

            var interactable2 = TestUtilities.CreateSimpleInteractable();
            memberInteractor3.validTargets.Add(interactable2);

            yield return new WaitForFixedUpdate();
            yield return null;

            memberInteractor1.validTargets.Clear();

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasHover, Is.False);
            Assert.That(memberInteractor2.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor3.hasHover, Is.False);

            memberInteractor2.validTargets.Clear();

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasHover, Is.False);
            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor3.IsHovering(interactable2), Is.True);
        }

        [UnityTest]
        public IEnumerator DynamicallyRemovingHigherPriorityInteractorTriggersNextPriorityHover()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithHoverOnlyMockInteractors(
                out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            var interactable1 = TestUtilities.CreateSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);
            memberInteractor2.validTargets.Add(interactable1);

            var interactable2 = TestUtilities.CreateSimpleInteractable();
            memberInteractor3.validTargets.Add(interactable2);

            yield return new WaitForFixedUpdate();
            yield return null;

            group.RemoveGroupMember(memberInteractor1);

            yield return new WaitForFixedUpdate();
            yield return null;

            // Interactor 1 should have hover since it is no longer part of the group
            Assert.That(memberInteractor1.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor2.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor3.hasHover, Is.False);

            group.RemoveGroupMember(memberInteractor2);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor2.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor3.IsHovering(interactable2), Is.True);
        }

        [UnityTest]
        public IEnumerator ChangingInteractorPrioritiesChangesHover()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithHoverOnlyMockInteractors(
                out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            var interactable1 = TestUtilities.CreateSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);
            memberInteractor2.validTargets.Add(interactable1);

            var interactable2 = TestUtilities.CreateSimpleInteractable();
            memberInteractor3.validTargets.Add(interactable2);

            yield return new WaitForFixedUpdate();
            yield return null;

            group.MoveGroupMemberTo(memberInteractor1, 1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor2.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor1.hasHover, Is.False);
            Assert.That(memberInteractor3.hasHover, Is.False);

            group.MoveGroupMemberTo(memberInteractor3, 0);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor3.IsHovering(interactable2), Is.True);
            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor1.hasHover, Is.False);
        }

        [UnityTest]
        public IEnumerator DisabledMemberInteractorCannotHover()
        {
            TestUtilities.CreateInteractionManager();
            TestUtilities.CreateGroupWithHoverOnlyMockInteractors(out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            var interactable1 = TestUtilities.CreateSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);
            memberInteractor2.validTargets.Add(interactable1);

            var interactable2 = TestUtilities.CreateSimpleInteractable();
            memberInteractor3.validTargets.Add(interactable2);

            memberInteractor1.enabled = false;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasHover, Is.False);
            Assert.That(memberInteractor2.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor3.hasHover, Is.False);

            memberInteractor2.enabled = false;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasHover, Is.False);
            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor3.IsHovering(interactable2), Is.True);
        }

        [UnityTest]
        public IEnumerator ReEnabledMemberInteractorOvertakesHover()
        {
            TestUtilities.CreateInteractionManager();
            TestUtilities.CreateGroupWithHoverOnlyMockInteractors(out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            var interactable1 = TestUtilities.CreateSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);
            memberInteractor2.validTargets.Add(interactable1);

            var interactable2 = TestUtilities.CreateSimpleInteractable();
            memberInteractor3.validTargets.Add(interactable2);

            memberInteractor1.enabled = false;
            memberInteractor2.enabled = false;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasHover, Is.False);
            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor3.IsHovering(interactable2), Is.True);

            memberInteractor2.enabled = true;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasHover, Is.False);
            Assert.That(memberInteractor2.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor3.hasHover, Is.False);

            memberInteractor1.enabled = true;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor3.hasHover, Is.False);
        }

        [UnityTest]
        public IEnumerator DisablingGroupTriggersHoverForAllInteractors()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithHoverOnlyMockInteractors(
                out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            var interactable1 = TestUtilities.CreateSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);
            memberInteractor2.validTargets.Add(interactable1);

            var interactable2 = TestUtilities.CreateSimpleInteractable();
            memberInteractor3.validTargets.Add(interactable2);

            yield return new WaitForFixedUpdate();
            yield return null;

            group.enabled = false;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor2.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor3.IsHovering(interactable2), Is.True);
        }

        [UnityTest]
        public IEnumerator OnlyHighestPriorityInteractorInGroupCanSelect()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(
                out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);
            memberInteractor2.validTargets.Add(interactable1);
            memberInteractor3.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(memberInteractor3.hasSelection, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));

            // Now test with different interactables
            var interactable2 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            var interactable3 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor2.validTargets.Clear();
            memberInteractor2.validTargets.Add(interactable2);
            memberInteractor3.validTargets.Clear();
            memberInteractor3.validTargets.Add(interactable3);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(memberInteractor3.hasSelection, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));
        }

        [UnityTest]
        public IEnumerator OnlyHighestPriorityGroupInGroupCanSelect()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateInteractionGroup();

            var memberGroup1 = TestUtilities.CreateGroupWithMockInteractors(
                out var group1MemberInteractor1, out var group1MemberInteractor2, out var group1MemberInteractor3);

            var memberGroup2 = TestUtilities.CreateGroupWithMockInteractors(
                out var group2MemberInteractor1, out var group2MemberInteractor2, out var group2MemberInteractor3);

            var memberGroup3 = TestUtilities.CreateGroupWithMockInteractors(
                out var group3MemberInteractor1, out var group3MemberInteractor2, out var group3MemberInteractor3);

            group.AddGroupMember(memberGroup1);
            group.AddGroupMember(memberGroup2);
            group.AddGroupMember(memberGroup3);

            var interactable = TestUtilities.CreateMultiSelectableSimpleInteractable();
            group1MemberInteractor1.validTargets.Add(interactable);
            group1MemberInteractor2.validTargets.Add(interactable);
            group1MemberInteractor3.validTargets.Add(interactable);
            group2MemberInteractor1.validTargets.Add(interactable);
            group2MemberInteractor2.validTargets.Add(interactable);
            group2MemberInteractor3.validTargets.Add(interactable);
            group3MemberInteractor1.validTargets.Add(interactable);
            group3MemberInteractor2.validTargets.Add(interactable);
            group3MemberInteractor3.validTargets.Add(interactable);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(group1MemberInteractor1.IsSelecting(interactable), Is.True);
            Assert.That(group1MemberInteractor2.hasSelection, Is.False);
            Assert.That(group1MemberInteractor3.hasSelection, Is.False);
            Assert.That(group2MemberInteractor1.hasSelection, Is.False);
            Assert.That(group2MemberInteractor2.hasSelection, Is.False);
            Assert.That(group2MemberInteractor3.hasSelection, Is.False);
            Assert.That(group3MemberInteractor1.hasSelection, Is.False);
            Assert.That(group3MemberInteractor2.hasSelection, Is.False);
            Assert.That(group3MemberInteractor3.hasSelection, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(group1MemberInteractor1));
            Assert.That(memberGroup1.activeInteractor, Is.EqualTo(group1MemberInteractor1));
            Assert.That(memberGroup2.activeInteractor, Is.Null);
            Assert.That(memberGroup3.activeInteractor, Is.Null);
        }

        [UnityTest]
        public IEnumerator HigherPriorityInteractorCanOnlyOvertakeInteractionOnceExistingSelectFinishes()
        {
            TestUtilities.CreateInteractionManager();
            TestUtilities.CreateGroupWithMockInteractors(out var memberInteractor1, out var memberInteractor2, out _);

            var interactable = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor2.validTargets.Add(interactable);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor2.IsSelecting(interactable), Is.True);

            memberInteractor1.validTargets.Add(interactable);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasHover, Is.False);
            Assert.That(memberInteractor1.hasSelection, Is.False);
            Assert.That(memberInteractor2.IsHovering(interactable), Is.True);
            Assert.That(memberInteractor2.IsSelecting(interactable), Is.True);

            memberInteractor2.allowSelect = false;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable), Is.True);
            Assert.That(memberInteractor1.IsSelecting(interactable), Is.True);
            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor2.hasSelection, Is.False);
        }

        [UnityTest]
        public IEnumerator HigherPriorityInteractorCanOnlyOvertakeInteractionOnceExistingGroupSelectFinishes()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateInteractionGroup();
            var memberInteractor = TestUtilities.CreateMockInteractor();
            var memberGroup = TestUtilities.CreateInteractionGroup();
            var memberGroupMemberInteractor = TestUtilities.CreateMockInteractor();
            memberGroup.AddGroupMember(memberGroupMemberInteractor);
            group.AddGroupMember(memberInteractor);
            group.AddGroupMember(memberGroup);

            var interactable = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberGroupMemberInteractor.validTargets.Add(interactable);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberGroupMemberInteractor.IsSelecting(interactable), Is.True);

            memberInteractor.validTargets.Add(interactable);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor.hasHover, Is.False);
            Assert.That(memberInteractor.hasSelection, Is.False);
            Assert.That(memberGroupMemberInteractor.IsHovering(interactable), Is.True);
            Assert.That(memberGroupMemberInteractor.IsSelecting(interactable), Is.True);

            memberGroupMemberInteractor.allowSelect = false;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor.IsHovering(interactable), Is.True);
            Assert.That(memberInteractor.IsSelecting(interactable), Is.True);
            Assert.That(memberGroupMemberInteractor.hasHover, Is.False);
            Assert.That(memberGroupMemberInteractor.hasSelection, Is.False);
        }

        [UnityTest]
        public IEnumerator HigherPriorityInteractorCannotOvertakeSelectThatIsKeptValid()
        {
            TestUtilities.CreateInteractionManager();
            TestUtilities.CreateGroupWithMockInteractors(out var memberInteractor1, out var memberInteractor2, out _);

            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor2.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            memberInteractor1.validTargets.Add(interactable1);
            memberInteractor2.validTargets.Clear();

            yield return new WaitForFixedUpdate();
            yield return null;

            // Interactor 2 should still be selecting since its select is kept valid, and Interactor 1 should not yet be able to select
            Assert.That(memberInteractor1.hasSelection, Is.False);
            Assert.That(memberInteractor2.IsSelecting(interactable1), Is.True);

            memberInteractor2.keepSelectedTargetValid = false;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor2.hasSelection, Is.False);
        }

        [UnityTest]
        public IEnumerator DynamicallyAddedHigherPriorityInteractorCannotOvertakeSelect()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateInteractionGroup();
            var memberInteractor1 = TestUtilities.CreateMockInteractor();
            var memberInteractor2 = TestUtilities.CreateMockInteractor();
            var memberInteractor3 = TestUtilities.CreateMockInteractor();

            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor2.validTargets.Add(interactable1);
            memberInteractor3.validTargets.Add(interactable1);

            var interactable2 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable2);

            group.AddGroupMember(memberInteractor3);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor3.IsSelecting(interactable1), Is.True);

            group.MoveGroupMemberTo(memberInteractor2, 0);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(memberInteractor3.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor3.IsSelecting(interactable1), Is.True);

            group.MoveGroupMemberTo(memberInteractor1, 0);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasHover, Is.False);
            Assert.That(memberInteractor1.hasSelection, Is.False);
            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(memberInteractor3.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor3.IsSelecting(interactable1), Is.True);
        }

        [UnityTest]
        public IEnumerator EndingHigherPrioritySelectTriggersNextPrioritySelect()
        {
            TestUtilities.CreateInteractionManager();
            TestUtilities.CreateGroupWithMockInteractors(out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);
            memberInteractor2.validTargets.Add(interactable1);

            var interactable2 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor3.validTargets.Add(interactable2);

            yield return new WaitForFixedUpdate();
            yield return null;

            memberInteractor1.validTargets.Clear();

            yield return new WaitForFixedUpdate();
            yield return null;

            // Interactor 1 should still be selecting since it keeps its selected target valid, and no other interactors should be selecting
            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(memberInteractor3.hasSelection, Is.False);

            memberInteractor1.keepSelectedTargetValid = false;

            yield return new WaitForFixedUpdate();
            yield return null;

            // Now interactor 1 should lose its selection
            Assert.That(memberInteractor1.hasSelection, Is.False);
            Assert.That(memberInteractor2.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor3.hasSelection, Is.False);

            memberInteractor2.validTargets.Clear();
            memberInteractor2.keepSelectedTargetValid = false;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasSelection, Is.False);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(memberInteractor3.IsSelecting(interactable2), Is.True);
        }

        [UnityTest]
        public IEnumerator DynamicallyRemovingHigherPriorityInteractorTriggersNextPrioritySelect()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(
                out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);
            memberInteractor2.validTargets.Add(interactable1);

            var interactable2 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor3.validTargets.Add(interactable2);

            yield return new WaitForFixedUpdate();
            yield return null;

            group.RemoveGroupMember(memberInteractor1);

            yield return new WaitForFixedUpdate();
            yield return null;

            // Interactor 1 should have select since it is no longer part of the group
            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor2.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor3.hasSelection, Is.False);

            group.RemoveGroupMember(memberInteractor2);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor2.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor3.IsSelecting(interactable2), Is.True);
        }

        [UnityTest]
        public IEnumerator ChangingInteractorPrioritiesDoesNotChangeSelect()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(
                out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);
            memberInteractor2.validTargets.Add(interactable1);

            var interactable2 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor3.validTargets.Add(interactable2);

            yield return new WaitForFixedUpdate();
            yield return null;

            group.MoveGroupMemberTo(memberInteractor1, 1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor3.hasSelection, Is.False);

            group.MoveGroupMemberTo(memberInteractor3, 0);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor3.hasSelection, Is.False);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
        }

        [UnityTest]
        public IEnumerator DisabledMemberInteractorCannotSelect()
        {
            TestUtilities.CreateInteractionManager();
            TestUtilities.CreateGroupWithMockInteractors(out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);
            memberInteractor2.validTargets.Add(interactable1);

            var interactable2 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor3.validTargets.Add(interactable2);

            memberInteractor1.enabled = false;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasSelection, Is.False);
            Assert.That(memberInteractor2.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor3.hasSelection, Is.False);

            memberInteractor2.enabled = false;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasSelection, Is.False);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(memberInteractor3.IsSelecting(interactable2), Is.True);
        }

        [UnityTest]
        public IEnumerator ReEnabledMemberInteractorDoesNotOvertakeSelect()
        {
            TestUtilities.CreateInteractionManager();
            TestUtilities.CreateGroupWithMockInteractors(out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);
            memberInteractor2.validTargets.Add(interactable1);

            var interactable2 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor3.validTargets.Add(interactable2);

            memberInteractor1.enabled = false;
            memberInteractor2.enabled = false;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasSelection, Is.False);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(memberInteractor3.IsSelecting(interactable2), Is.True);

            memberInteractor2.enabled = true;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasSelection, Is.False);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(memberInteractor3.IsSelecting(interactable2), Is.True);

            memberInteractor1.enabled = true;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasSelection, Is.False);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(memberInteractor3.IsSelecting(interactable2), Is.True);
        }

        [UnityTest]
        public IEnumerator DisablingGroupTriggersSelectForAllInteractors()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(
                out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);
            memberInteractor2.validTargets.Add(interactable1);

            var interactable2 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor3.validTargets.Add(interactable2);

            yield return new WaitForFixedUpdate();
            yield return null;

            group.enabled = false;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor2.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor3.IsSelecting(interactable2), Is.True);
        }

        [UnityTest]
        public IEnumerator LowerPriorityInteractorCannotSelectWhenHigherPriorityInteractorIsHovering()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateInteractionGroup();
            var memberInteractor1 = TestUtilities.CreateMockInteractor();
            var memberInteractor2 = TestUtilities.CreateMockInteractor();
            var memberInteractor3 = TestUtilities.CreateMockInteractor();
            memberInteractor1.allowSelect = false;
            group.AddGroupMember(memberInteractor1);
            group.AddGroupMember(memberInteractor2);
            group.AddGroupMember(memberInteractor3);

            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);
            memberInteractor2.validTargets.Add(interactable1);

            var interactable2 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor3.validTargets.Add(interactable2);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable1));
            Assert.That(memberInteractor1.hasSelection, Is.False);
            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(memberInteractor3.hasHover, Is.False);
            Assert.That(memberInteractor3.hasSelection, Is.False);
        }

        [UnityTest]
        public IEnumerator IsBlockedByInteractionWithinGroupReturnsCorrectStatus()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(
                out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            yield return new WaitForFixedUpdate();
            yield return null;

            // None of the interactors in group should be blocked initially
            Assert.That(memberInteractor1.IsBlockedByInteractionWithinGroup(), Is.False);
            Assert.That(memberInteractor2.IsBlockedByInteractionWithinGroup(), Is.False);
            Assert.That(memberInteractor3.IsBlockedByInteractionWithinGroup(), Is.False);

            var interactable = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable);
            memberInteractor2.validTargets.Add(interactable);
            memberInteractor3.validTargets.Add(interactable);

            var nonGroupMemberInteractor = TestUtilities.CreateMockInteractor();

            yield return new WaitForFixedUpdate();
            yield return null;

            // Lower priority interactors in group should be blocked by highest priority one
            Assert.That(memberInteractor1.IsBlockedByInteractionWithinGroup(), Is.False);
            Assert.That(memberInteractor2.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(memberInteractor3.IsBlockedByInteractionWithinGroup(), Is.True);

            // Interactor that is not in a group should never be blocked
            Assert.That(nonGroupMemberInteractor.IsBlockedByInteractionWithinGroup(), Is.False);

            memberInteractor1.allowSelect = false;
            memberInteractor1.allowHover = false;

            yield return new WaitForFixedUpdate();
            yield return null;

            // After higher priority interactor stops interaction, the next priority one should block the others
            Assert.That(memberInteractor1.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(memberInteractor2.IsBlockedByInteractionWithinGroup(), Is.False);
            Assert.That(memberInteractor3.IsBlockedByInteractionWithinGroup(), Is.True);

            // Now test with some nested groups
            var nestedGroup = TestUtilities.CreateGroupWithMockInteractors(
                out var nestedGroupMemberInteractor1, out var nestedGroupMemberInteractor2, out var nestedGroupMemberInteractor3);

            var nextNestedGroup = TestUtilities.CreateGroupWithMockInteractors(
                out var nextNestedGroupMemberInteractor1, out var nextNestedGroupMemberInteractor2, out var nextNestedGroupMemberInteractor3);

            group.AddGroupMember(nestedGroup);
            nestedGroup.AddGroupMember(nextNestedGroup);
            nestedGroupMemberInteractor1.validTargets.Add(interactable);
            nestedGroupMemberInteractor2.validTargets.Add(interactable);
            nestedGroupMemberInteractor3.validTargets.Add(interactable);
            nextNestedGroupMemberInteractor1.validTargets.Add(interactable);
            nextNestedGroupMemberInteractor2.validTargets.Add(interactable);
            nextNestedGroupMemberInteractor3.validTargets.Add(interactable);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(memberInteractor2.IsBlockedByInteractionWithinGroup(), Is.False);
            Assert.That(memberInteractor3.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(nestedGroupMemberInteractor1.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(nestedGroupMemberInteractor2.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(nestedGroupMemberInteractor3.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(nextNestedGroupMemberInteractor1.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(nextNestedGroupMemberInteractor2.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(nextNestedGroupMemberInteractor3.IsBlockedByInteractionWithinGroup(), Is.True);

            memberInteractor2.allowSelect = false;
            memberInteractor2.allowHover = false;
            memberInteractor3.allowSelect = false;
            memberInteractor3.allowHover = false;

            yield return new WaitForFixedUpdate();
            yield return null;

            // First nested group should now be able to interact with its highest priority interactor
            Assert.That(memberInteractor1.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(memberInteractor2.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(memberInteractor3.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(nestedGroupMemberInteractor1.IsBlockedByInteractionWithinGroup(), Is.False);
            Assert.That(nestedGroupMemberInteractor2.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(nestedGroupMemberInteractor3.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(nextNestedGroupMemberInteractor1.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(nextNestedGroupMemberInteractor2.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(nextNestedGroupMemberInteractor3.IsBlockedByInteractionWithinGroup(), Is.True);

            nestedGroupMemberInteractor1.allowSelect = false;
            nestedGroupMemberInteractor1.allowHover = false;
            nestedGroupMemberInteractor2.allowSelect = false;
            nestedGroupMemberInteractor2.allowHover = false;
            nestedGroupMemberInteractor3.allowSelect = false;
            nestedGroupMemberInteractor3.allowHover = false;

            yield return new WaitForFixedUpdate();
            yield return null;

            // Next nested group should now be able to interact with its highest priority interactor
            Assert.That(memberInteractor1.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(memberInteractor2.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(memberInteractor3.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(nestedGroupMemberInteractor1.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(nestedGroupMemberInteractor2.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(nestedGroupMemberInteractor3.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(nextNestedGroupMemberInteractor1.IsBlockedByInteractionWithinGroup(), Is.False);
            Assert.That(nextNestedGroupMemberInteractor2.IsBlockedByInteractionWithinGroup(), Is.True);
            Assert.That(nextNestedGroupMemberInteractor3.IsBlockedByInteractionWithinGroup(), Is.True);
        }

        [Test]
        public void StartingInteractionOverridesAreAddedToMap()
        {
            TestUtilities.CreateInteractionManager();

            // Start inactive so we can set starting members before Awake
            var groupObj = new GameObject("Interaction Group");
            groupObj.SetActive(false);
            var group = groupObj.AddComponent<XRInteractionGroup>();

            var memberInteractor1 = TestUtilities.CreateMockInteractor();
            var memberInteractor2 = TestUtilities.CreateMockInteractor();
            var memberInteractor3 = TestUtilities.CreateMockInteractor();
            group.startingGroupMembers = new List<Object>
            {
                memberInteractor1,
                memberInteractor2,
                memberInteractor3
            };

            var member1OverrideMembers = new List<Object> { memberInteractor2, memberInteractor3 };
            var member2OverrideMembers = new List<Object> { memberInteractor3 };
            var member3OverrideMembers = new List<Object>();

            foreach (var overrideMember in member1OverrideMembers)
            {
                group.AddStartingInteractionOverride(memberInteractor1, overrideMember);
            }

            foreach (var overrideMember in member2OverrideMembers)
            {
                group.AddStartingInteractionOverride(memberInteractor2, overrideMember);
            }

            foreach (var overrideMember in member3OverrideMembers)
            {
                group.AddStartingInteractionOverride(memberInteractor3, overrideMember);
            }

            groupObj.SetActive(true);

            group.GetInteractionOverridesForGroupMember(memberInteractor1, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.EqualTo(member1OverrideMembers.Count));
            foreach (var overrideGroupMember in member1OverrideMembers)
            {
                Assert.That(s_OverrideGroupMembers.Contains((IXRGroupMember)overrideGroupMember), Is.True);
            }

            group.GetInteractionOverridesForGroupMember(memberInteractor2, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.EqualTo(member2OverrideMembers.Count));
            foreach (var overrideGroupMember in member2OverrideMembers)
            {
                Assert.That(s_OverrideGroupMembers.Contains((IXRGroupMember)overrideGroupMember), Is.True);
            }

            group.GetInteractionOverridesForGroupMember(memberInteractor3, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.EqualTo(member3OverrideMembers.Count));
            foreach (var overrideGroupMember in member3OverrideMembers)
            {
                Assert.That(s_OverrideGroupMembers.Contains((IXRGroupMember)overrideGroupMember), Is.True);
            }
        }

        [Test]
        public void AddInteractionOverrides()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(
                out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            group.GetInteractionOverridesForGroupMember(memberInteractor1, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.EqualTo(0));
            group.AddInteractionOverrideForGroupMember(memberInteractor1, memberInteractor2);
            group.GetInteractionOverridesForGroupMember(memberInteractor1, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.EqualTo(1));
            Assert.That(s_OverrideGroupMembers.Contains(memberInteractor2), Is.True);
            group.AddInteractionOverrideForGroupMember(memberInteractor1, memberInteractor3);
            group.GetInteractionOverridesForGroupMember(memberInteractor1, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.EqualTo(2));
            Assert.That(s_OverrideGroupMembers.Contains(memberInteractor3), Is.True);

            group.GetInteractionOverridesForGroupMember(memberInteractor2, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.EqualTo(0));
            group.AddInteractionOverrideForGroupMember(memberInteractor2, memberInteractor3);
            group.GetInteractionOverridesForGroupMember(memberInteractor2, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.EqualTo(1));
            Assert.That(s_OverrideGroupMembers.Contains(memberInteractor3), Is.True);

            group.GetInteractionOverridesForGroupMember(memberInteractor3, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveInteractionOverrides()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(out var memberInteractor1,
                out var memberInteractor2, out var memberInteractor3);

            group.AddInteractionOverrideForGroupMember(memberInteractor1, memberInteractor2);
            group.AddInteractionOverrideForGroupMember(memberInteractor1, memberInteractor3);
            group.AddInteractionOverrideForGroupMember(memberInteractor2, memberInteractor3);

            Assert.That(group.RemoveInteractionOverrideForGroupMember(memberInteractor1, memberInteractor2), Is.True);
            group.GetInteractionOverridesForGroupMember(memberInteractor1, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.EqualTo(1));
            Assert.That(s_OverrideGroupMembers.Contains(memberInteractor2), Is.False);
            Assert.That(group.RemoveInteractionOverrideForGroupMember(memberInteractor1, memberInteractor2), Is.False);

            Assert.That(group.RemoveInteractionOverrideForGroupMember(memberInteractor1, memberInteractor3), Is.True);
            group.GetInteractionOverridesForGroupMember(memberInteractor1, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.Zero);
            Assert.That(group.RemoveInteractionOverrideForGroupMember(memberInteractor1, memberInteractor3), Is.False);

            Assert.That(group.RemoveInteractionOverrideForGroupMember(memberInteractor2, memberInteractor3), Is.True);
            group.GetInteractionOverridesForGroupMember(memberInteractor2, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.Zero);
            Assert.That(group.RemoveInteractionOverrideForGroupMember(memberInteractor2, memberInteractor3), Is.False);
        }

        [Test]
        public void ClearInteractionOverrides()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(out var memberInteractor1,
                out var memberInteractor2, out var memberInteractor3);

            Assert.That(group.ClearInteractionOverridesForGroupMember(memberInteractor1), Is.False);
            Assert.That(group.ClearInteractionOverridesForGroupMember(memberInteractor2), Is.False);
            Assert.That(group.ClearInteractionOverridesForGroupMember(memberInteractor3), Is.False);

            group.AddInteractionOverrideForGroupMember(memberInteractor1, memberInteractor2);
            group.AddInteractionOverrideForGroupMember(memberInteractor1, memberInteractor3);
            Assert.That(group.ClearInteractionOverridesForGroupMember(memberInteractor1), Is.True);
            group.GetInteractionOverridesForGroupMember(memberInteractor1, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.Zero);

            group.AddInteractionOverrideForGroupMember(memberInteractor2, memberInteractor1);
            group.AddInteractionOverrideForGroupMember(memberInteractor2, memberInteractor3);
            Assert.That(group.ClearInteractionOverridesForGroupMember(memberInteractor2), Is.True);
            group.GetInteractionOverridesForGroupMember(memberInteractor2, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.Zero);

            group.AddInteractionOverrideForGroupMember(memberInteractor3, memberInteractor1);
            group.AddInteractionOverrideForGroupMember(memberInteractor3, memberInteractor2);
            Assert.That(group.ClearInteractionOverridesForGroupMember(memberInteractor3), Is.True);
            group.GetInteractionOverridesForGroupMember(memberInteractor3, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.Zero);
        }

        [Test]
        public void CannotAddInteractionOverrideThatIsNotASelectInteractorOrOverrideGroup()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateInteractionGroup();
            var memberInteractor1 = new InvalidInteractionOverride();
            var memberInteractor2 = new InvalidInteractionOverride();
            var memberInteractor3 = new InvalidInteractionOverride();
            group.AddGroupMember(memberInteractor1);
            group.AddGroupMember(memberInteractor2);
            group.AddGroupMember(memberInteractor3);

            LogAssert.Expect(LogType.Error, k_AnyString);
            group.AddInteractionOverrideForGroupMember(memberInteractor1, memberInteractor2);
            group.GetInteractionOverridesForGroupMember(memberInteractor1, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.Zero);

            LogAssert.Expect(LogType.Error, k_AnyString);
            group.AddInteractionOverrideForGroupMember(memberInteractor2, memberInteractor3);
            group.GetInteractionOverridesForGroupMember(memberInteractor2, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.Zero);

            LogAssert.Expect(LogType.Error, k_AnyString);
            group.AddInteractionOverrideForGroupMember(memberInteractor3, memberInteractor1);
            group.GetInteractionOverridesForGroupMember(memberInteractor3, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.Zero);
        }

        [Test]
        public void CannotAddGroupMemberAsOverrideForItself()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(out var memberInteractor1,
                out var memberInteractor2, out var memberInteractor3);

            LogAssert.Expect(LogType.Error, k_AnyString);
            group.AddInteractionOverrideForGroupMember(memberInteractor1, memberInteractor1);

            LogAssert.Expect(LogType.Error, k_AnyString);
            group.AddInteractionOverrideForGroupMember(memberInteractor2, memberInteractor2);

            LogAssert.Expect(LogType.Error, k_AnyString);
            group.AddInteractionOverrideForGroupMember(memberInteractor3, memberInteractor3);
        }

        [Test]
        public void CannotCreateLoopOfInteractionOverrides()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(out var memberInteractor1,
                out var memberInteractor2, out var memberInteractor3);

            group.AddInteractionOverrideForGroupMember(memberInteractor1, memberInteractor2);

            LogAssert.Expect(LogType.Error, k_AnyString);
            group.AddInteractionOverrideForGroupMember(memberInteractor2, memberInteractor1);

            group.AddInteractionOverrideForGroupMember(memberInteractor2, memberInteractor3);

            LogAssert.Expect(LogType.Error, k_AnyString);
            group.AddInteractionOverrideForGroupMember(memberInteractor3, memberInteractor1);
        }

        [Test]
        public void CannotManageInteractionOverridesForUnregisteredMembers()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateInteractionGroup();
            var memberInteractor1 = TestUtilities.CreateMockInteractor();
            var memberInteractor2 = TestUtilities.CreateMockInteractor();
            var memberInteractor3 = TestUtilities.CreateMockInteractor();

            group.AddGroupMember(memberInteractor1);

            // Add should fail because other group member is not registered
            LogAssert.Expect(LogType.Error, k_AnyString);
            group.AddInteractionOverrideForGroupMember(memberInteractor1, memberInteractor2);
            group.GetInteractionOverridesForGroupMember(memberInteractor1, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.EqualTo(0));

            // Add and get overrides should fail because group member is not registered
            LogAssert.Expect(LogType.Error, k_AnyString);
            group.AddInteractionOverrideForGroupMember(memberInteractor2, memberInteractor1);
            LogAssert.Expect(LogType.Error, k_AnyString);
            group.GetInteractionOverridesForGroupMember(memberInteractor2, s_OverrideGroupMembers);
            Assert.That(s_OverrideGroupMembers.Count, Is.EqualTo(0));

            LogAssert.Expect(LogType.Error, k_AnyString);
            Assert.That(group.RemoveInteractionOverrideForGroupMember(memberInteractor2, memberInteractor3), Is.False);

            LogAssert.Expect(LogType.Error, k_AnyString);
            Assert.That(group.ClearInteractionOverridesForGroupMember(memberInteractor3), Is.False);
        }

        [UnityTest]
        public IEnumerator CannotOverrideInteractionWhenNoOverridesConfigured()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(
                out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));

            // Trigger a change in selectability for another interactor
            memberInteractor2.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));

            memberInteractor3.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(memberInteractor3.hasHover, Is.False);
            Assert.That(memberInteractor3.hasSelection, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));
        }

        [UnityTest]
        public IEnumerator OverrideHoverWithSelectFromConfiguredOverrides()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(
                out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            memberInteractor1.keepSelectedTargetValid = false;
            memberInteractor2.keepSelectedTargetValid = false;
            memberInteractor3.keepSelectedTargetValid = false;

            group.AddInteractionOverrideForGroupMember(memberInteractor1, memberInteractor2);
            group.AddInteractionOverrideForGroupMember(memberInteractor1, memberInteractor3);

            // Make the interactable not selectable by the first interactor, to make sure override can occur when only hover is happening.
            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            interactable1.selectFilters.Add(new XRSelectFilterDelegate((interactor, interactable) => !ReferenceEquals(interactor, memberInteractor1)));
            memberInteractor1.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable1), Is.True);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));

            // Trigger a change in selectability for an override interactor. It should not yet override because it cannot select
            // the interactable that is currently hovered.
            var interactable2 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor3.validTargets.Add(interactable2);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor3.hasHover, Is.False);
            Assert.That(memberInteractor3.hasSelection, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));

            // 3 should override since it can select the hovered interactable now.
            memberInteractor3.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasHover, Is.False);
            Assert.That(memberInteractor3.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor3.IsSelecting(interactable2), Is.True);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor3));

            memberInteractor3.validTargets.Clear();

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor3.hasHover, Is.False);
            Assert.That(memberInteractor3.hasSelection, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));

            // Interactor 2 is now capable of hovering the hovered interactable, but it should not override yet because
            // it isn't able to select the interactable.
            interactable1.selectFilters.Add(new XRSelectFilterDelegate((interactor, interactable) => !ReferenceEquals(interactor, memberInteractor2)));
            memberInteractor2.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));

            // Now 2 should override 1 since it can select the interactable.
            interactable1.selectFilters.Clear();

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasHover, Is.False);
            Assert.That(memberInteractor2.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor2.IsSelecting(interactable1), Is.True);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor2));
        }

        [UnityTest]
        public IEnumerator OverrideSelectWithSelectFromConfiguredOverrides()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(
                out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            memberInteractor1.allowHover = false;
            memberInteractor2.allowHover = false;
            memberInteractor3.allowHover = false;
            memberInteractor1.keepSelectedTargetValid = false;
            memberInteractor2.keepSelectedTargetValid = false;
            memberInteractor3.keepSelectedTargetValid = false;

            group.AddInteractionOverrideForGroupMember(memberInteractor1, memberInteractor2);
            group.AddInteractionOverrideForGroupMember(memberInteractor1, memberInteractor3);

            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));

            // Trigger a change in selectability for an override interactor. It should not yet override because it cannot select
            // the interactable that is currently selected.
            var interactable2 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor3.validTargets.Add(interactable2);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor3.hasSelection, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));

            // 3 is able to select the selected interactable now, so it should override.
            memberInteractor3.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasSelection, Is.False);
            Assert.That(memberInteractor3.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor3.IsSelecting(interactable2), Is.True);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor3));

            memberInteractor3.validTargets.Clear();

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor3.hasSelection, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));

            // Interactor 2 is now capable of selecting the selected interactable, so it should override.
            memberInteractor2.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasSelection, Is.False);
            Assert.That(memberInteractor2.IsSelecting(interactable1), Is.True);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor2));
        }

        [UnityTest]
        public IEnumerator OverrideHoverAndSelectWithSelectFromConfiguredOverrides()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithMockInteractors(
                out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            group.AddInteractionOverrideForGroupMember(memberInteractor1, memberInteractor2);

            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));

            // Trigger a change in selectability for an override interactor. It should not yet override because it cannot select
            // the interactable that is currently hovered/selected.
            var interactable2 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor2.validTargets.Add(interactable2);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));

            // 2 can now potentially select the hovered/selected interactable, so it should override
            memberInteractor2.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasHover, Is.False);
            Assert.That(memberInteractor1.hasSelection, Is.False);
            Assert.That(memberInteractor2.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor2.IsHovering(interactable2), Is.True);
            Assert.That(memberInteractor2.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor2.IsSelecting(interactable2), Is.True);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor2));

            // Now give interactor 2 a third interactable. 3 should be able to override 2 when it can potentially select
            // any of the three interactables.
            group.AddInteractionOverrideForGroupMember(memberInteractor2, memberInteractor3);
            var interactable3 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor2.validTargets.Add(interactable3);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor2.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor2.IsHovering(interactable2), Is.True);
            Assert.That(memberInteractor2.IsHovering(interactable3), Is.True);
            Assert.That(memberInteractor2.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor2.IsSelecting(interactable2), Is.True);
            Assert.That(memberInteractor2.IsSelecting(interactable3), Is.True);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor2));

            memberInteractor3.validTargets.Add(interactable3);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(memberInteractor3.IsHovering(interactable1), Is.False);
            Assert.That(memberInteractor3.IsHovering(interactable2), Is.False);
            Assert.That(memberInteractor3.IsHovering(interactable3), Is.True);
            Assert.That(memberInteractor3.IsSelecting(interactable1), Is.False);
            Assert.That(memberInteractor3.IsSelecting(interactable2), Is.False);
            Assert.That(memberInteractor3.IsSelecting(interactable3), Is.True);
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor3));
        }

        [UnityTest]
        public IEnumerator OverrideSubGroupHover()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithEmptyGroups(
                out var subGroup1, out var subGroup2, out var subGroup3);

            var subGroup1MemberInteractor = TestUtilities.CreateMockInteractor();
            var subGroup2MemberInteractor = TestUtilities.CreateMockInteractor();
            var subGroup3MemberInteractor = TestUtilities.CreateMockInteractor();
            subGroup1MemberInteractor.keepSelectedTargetValid = false;
            subGroup2MemberInteractor.keepSelectedTargetValid = false;
            subGroup3MemberInteractor.keepSelectedTargetValid = false;
            subGroup1.AddGroupMember(subGroup1MemberInteractor);
            subGroup2.AddGroupMember(subGroup2MemberInteractor);
            subGroup3.AddGroupMember(subGroup3MemberInteractor);

            group.AddInteractionOverrideForGroupMember(subGroup1, subGroup2);
            group.AddInteractionOverrideForGroupMember(subGroup1, subGroup3);

            // Make the interactable not selectable by the first interactor, to make sure override can occur when only hover is happening.
            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            interactable1.selectFilters.Add(new XRSelectFilterDelegate((interactor, interactable) => !ReferenceEquals(interactor, subGroup2MemberInteractor)));
            subGroup1MemberInteractor.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(subGroup1MemberInteractor.IsHovering(interactable1), Is.True);
            Assert.That(group.activeInteractor, Is.EqualTo(subGroup1MemberInteractor));

            // Trigger a change in selectability for an override group. It should not yet override because it cannot select
            // the interactable that is currently hovered.
            var interactable2 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            subGroup3MemberInteractor.validTargets.Add(interactable2);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(subGroup1MemberInteractor.IsHovering(interactable1), Is.True);
            Assert.That(subGroup3MemberInteractor.hasHover, Is.False);
            Assert.That(subGroup3MemberInteractor.hasSelection, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(subGroup1MemberInteractor));

            // 3 should override since it can select the hovered interactable now.
            subGroup3MemberInteractor.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(subGroup1MemberInteractor.hasHover, Is.False);
            Assert.That(subGroup3MemberInteractor.IsHovering(interactable1), Is.True);
            Assert.That(subGroup3MemberInteractor.IsSelecting(interactable2), Is.True);
            Assert.That(group.activeInteractor, Is.EqualTo(subGroup3MemberInteractor));

            subGroup3MemberInteractor.validTargets.Clear();

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(subGroup1MemberInteractor.IsHovering(interactable1), Is.True);
            Assert.That(subGroup3MemberInteractor.hasHover, Is.False);
            Assert.That(subGroup3MemberInteractor.hasSelection, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(subGroup1MemberInteractor));

            // Sub group 2 is now capable of hovering the hovered interactable, but it should not override yet because
            // it isn't able to select the interactable.
            interactable1.selectFilters.Add(new XRSelectFilterDelegate((interactor, interactable) => !ReferenceEquals(interactor, subGroup2MemberInteractor)));
            subGroup2MemberInteractor.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(subGroup1MemberInteractor.IsHovering(interactable1), Is.True);
            Assert.That(subGroup2MemberInteractor.hasHover, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(subGroup1MemberInteractor));

            // Now 2 should override 1 since it can select the interactable.
            interactable1.selectFilters.Clear();

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(subGroup1MemberInteractor.hasHover, Is.False);
            Assert.That(subGroup2MemberInteractor.IsHovering(interactable1), Is.True);
            Assert.That(subGroup2MemberInteractor.IsSelecting(interactable1), Is.True);
            Assert.That(group.activeInteractor, Is.EqualTo(subGroup2MemberInteractor));
        }

        [UnityTest]
        public IEnumerator OverrideSubGroupSelect()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateGroupWithEmptyGroups(
                out var subGroup1, out var subGroup2, out var subGroup3);

            var subGroup1MemberInteractor = TestUtilities.CreateMockInteractor();
            var subGroup2MemberInteractor = TestUtilities.CreateMockInteractor();
            var subGroup3MemberInteractor = TestUtilities.CreateMockInteractor();
            subGroup1MemberInteractor.allowHover = false;
            subGroup2MemberInteractor.allowHover = false;
            subGroup3MemberInteractor.allowHover = false;
            subGroup1MemberInteractor.keepSelectedTargetValid = false;
            subGroup2MemberInteractor.keepSelectedTargetValid = false;
            subGroup3MemberInteractor.keepSelectedTargetValid = false;
            subGroup1.AddGroupMember(subGroup1MemberInteractor);
            subGroup2.AddGroupMember(subGroup2MemberInteractor);
            subGroup3.AddGroupMember(subGroup3MemberInteractor);

            group.AddInteractionOverrideForGroupMember(subGroup1, subGroup2);
            group.AddInteractionOverrideForGroupMember(subGroup1, subGroup3);

            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            subGroup1MemberInteractor.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(subGroup1MemberInteractor.IsSelecting(interactable1), Is.True);
            Assert.That(group.activeInteractor, Is.EqualTo(subGroup1MemberInteractor));

            // Trigger a change in selectability for an override group. It should not yet override because it cannot select
            // the interactable that is currently selected.
            var interactable2 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            subGroup3MemberInteractor.validTargets.Add(interactable2);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(subGroup1MemberInteractor.IsSelecting(interactable1), Is.True);
            Assert.That(subGroup3MemberInteractor.hasSelection, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(subGroup1MemberInteractor));

            // 3 is able to select the selected interactable now, so it should override.
            subGroup3MemberInteractor.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(subGroup1MemberInteractor.hasSelection, Is.False);
            Assert.That(subGroup3MemberInteractor.IsSelecting(interactable1), Is.True);
            Assert.That(subGroup3MemberInteractor.IsSelecting(interactable2), Is.True);
            Assert.That(group.activeInteractor, Is.EqualTo(subGroup3MemberInteractor));

            subGroup3MemberInteractor.validTargets.Clear();

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(subGroup1MemberInteractor.IsSelecting(interactable1), Is.True);
            Assert.That(subGroup3MemberInteractor.hasSelection, Is.False);
            Assert.That(group.activeInteractor, Is.EqualTo(subGroup1MemberInteractor));

            // Sub group 2 is now capable of selecting the selected interactable, so it should override.
            subGroup2MemberInteractor.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(subGroup1MemberInteractor.hasSelection, Is.False);
            Assert.That(subGroup2MemberInteractor.IsSelecting(interactable1), Is.True);
            Assert.That(group.activeInteractor, Is.EqualTo(subGroup2MemberInteractor));
        }

        [UnityTest]
        public IEnumerator OverrideHoverWithinSubGroup()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateInteractionGroup();
            var subGroup = TestUtilities.CreateGroupWithMockInteractors(
                out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            memberInteractor1.keepSelectedTargetValid = false;
            memberInteractor2.keepSelectedTargetValid = false;
            memberInteractor3.keepSelectedTargetValid = false;

            group.AddGroupMember(subGroup);

            subGroup.AddInteractionOverrideForGroupMember(memberInteractor1, memberInteractor2);

            // Make the interactable not selectable by the first interactor, to make sure override can occur when only hover is happening.
            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            interactable1.selectFilters.Add(new XRSelectFilterDelegate((interactor, interactable) => !ReferenceEquals(interactor, memberInteractor1)));
            memberInteractor1.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable1), Is.True);
            Assert.That(subGroup.activeInteractor, Is.EqualTo(memberInteractor1));
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));

            // Trigger a change in selectability for an override interactor. It should not yet override because it cannot select
            // the interactable that is currently hovered.
            var interactable2 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor2.validTargets.Add(interactable2);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor2.hasHover, Is.False);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(subGroup.activeInteractor, Is.EqualTo(memberInteractor1));
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));

            // 2 should override since it can select the hovered interactable now.
            memberInteractor2.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasHover, Is.False);
            Assert.That(memberInteractor2.IsHovering(interactable1), Is.True);
            Assert.That(memberInteractor2.IsHovering(interactable2), Is.True);
            Assert.That(memberInteractor2.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor2.IsSelecting(interactable2), Is.True);
            Assert.That(subGroup.activeInteractor, Is.EqualTo(memberInteractor2));
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor2));
        }

        [UnityTest]
        public IEnumerator OverrideSelectWithinSubGroup()
        {
            TestUtilities.CreateInteractionManager();
            var group = TestUtilities.CreateInteractionGroup();
            var subGroup = TestUtilities.CreateGroupWithMockInteractors(
                out var memberInteractor1, out var memberInteractor2, out var memberInteractor3);

            memberInteractor1.allowHover = false;
            memberInteractor2.allowHover = false;
            memberInteractor3.allowHover = false;
            memberInteractor1.keepSelectedTargetValid = false;
            memberInteractor2.keepSelectedTargetValid = false;
            memberInteractor3.keepSelectedTargetValid = false;

            group.AddGroupMember(subGroup);

            subGroup.AddInteractionOverrideForGroupMember(memberInteractor1, memberInteractor2);

            var interactable1 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor1.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
            Assert.That(subGroup.activeInteractor, Is.EqualTo(memberInteractor1));
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));

            // Trigger a change in selectability for an override interactor. It should not yet override because it cannot select
            // the interactable that is currently selected.
            var interactable2 = TestUtilities.CreateMultiSelectableSimpleInteractable();
            memberInteractor2.validTargets.Add(interactable2);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor2.hasSelection, Is.False);
            Assert.That(subGroup.activeInteractor, Is.EqualTo(memberInteractor1));
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor1));

            // 2 is able to select the selected interactable now, so it should override.
            memberInteractor2.validTargets.Add(interactable1);

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(memberInteractor1.hasSelection, Is.False);
            Assert.That(memberInteractor2.IsSelecting(interactable1), Is.True);
            Assert.That(memberInteractor2.IsSelecting(interactable2), Is.True);
            Assert.That(subGroup.activeInteractor, Is.EqualTo(memberInteractor2));
            Assert.That(group.activeInteractor, Is.EqualTo(memberInteractor2));
        }

        /// <summary>
        /// An <see cref="IXRGroupMember"/> that is not also a <see cref="IXRInteractor"/> or <see cref="IXRInteractionGroup"/>.
        /// </summary>
        class InvalidGroupMember : IXRGroupMember
        {
            // ReSharper disable once UnassignedGetOnlyAutoProperty -- Not needed for test, class used to cause error
            public IXRInteractionGroup containingGroup { get; }
            public void OnRegisteringAsGroupMember(IXRInteractionGroup group) { }
            public void OnRegisteringAsNonGroupMember() { }
        }

        /// <summary>
        /// An <see cref="IXRGroupMember"/> that is not also a <see cref="IXRSelectInteractor"/> or <see cref="IXRInteractionOverrideGroup"/>.
        /// </summary>
        class InvalidInteractionOverride : IXRGroupMember, IXRInteractor
        {
            // ReSharper disable UnassignedGetOnlyAutoProperty -- Not needed for test, class used to cause error
            public IXRInteractionGroup containingGroup { get; }
            public InteractionLayerMask interactionLayers { get; }
            public Transform transform { get; }
            // ReSharper restore UnassignedGetOnlyAutoProperty

            // Not needed for test, class used to cause error
#pragma warning disable 67
            public event Action<InteractorRegisteredEventArgs> registered;
            public event Action<InteractorUnregisteredEventArgs> unregistered;
#pragma warning restore 67

            public void OnRegisteringAsGroupMember(IXRInteractionGroup group) { }
            public void OnRegisteringAsNonGroupMember() { }
            public Transform GetAttachTransform(IXRInteractable interactable) { return null; }
            public void GetValidTargets(List<IXRInteractable> targets) { }
            public void OnRegistered(InteractorRegisteredEventArgs args) { }
            public void OnUnregistered(InteractorUnregisteredEventArgs args) { }
            public void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase) { }
            public void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase) { }
        }
    }
}