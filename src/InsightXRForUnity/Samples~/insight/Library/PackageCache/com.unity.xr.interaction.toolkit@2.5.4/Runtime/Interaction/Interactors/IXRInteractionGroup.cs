using System;
using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// An interface that represents an Interaction Group component that enforces that only one <see cref="IXRGroupMember"/>
    /// within the Group can interact at a time.
    /// </summary>
    /// <seealso cref="XRInteractionGroup"/>
    /// <seealso cref="IXRGroupMember"/>
    public interface IXRInteractionGroup
    {
        /// <summary>
        /// Calls the methods in its invocation list when this Interaction Group is registered with an Interaction Manager.
        /// </summary>
        /// <remarks>
        /// The <see cref="InteractionGroupRegisteredEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="XRInteractionManager.interactionGroupRegistered"/>
        event Action<InteractionGroupRegisteredEventArgs> registered;

        /// <summary>
        /// Calls the methods in its invocation list when this Interaction Group is unregistered from an Interaction Manager.
        /// </summary>
        /// <remarks>
        /// The <see cref="InteractionGroupUnregisteredEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="XRInteractionManager.interactionGroupUnregistered"/>
        event Action<InteractionGroupUnregisteredEventArgs> unregistered;

        /// <summary>
        /// The name of the interaction group, which can be used to retrieve it from the Interaction Manager.
        /// </summary>
        string groupName { get; }

        /// <summary>
        /// The Interactor in this Interaction Group or any of its member Groups that is currently performing interaction.
        /// </summary>
        IXRInteractor activeInteractor { get; }

        /// <summary>
        /// The Interactor in this Interaction Group or any of its member Groups that initiated the last focus event.
        /// </summary>
        IXRInteractor focusInteractor { get; }

        /// <summary>
        /// The Interactable that is currently being focused by an Interactor in this Interaction Group or any of its member Groups.
        /// </summary>
        IXRFocusInteractable focusInteractable { get; }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method when this Interaction Group is registered with it.
        /// </summary>
        /// <param name="args">Event data containing the Interaction Manager that registered this Interaction Group.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="XRInteractionManager.RegisterInteractionGroup(IXRInteractionGroup)"/>
        void OnRegistered(InteractionGroupRegisteredEventArgs args);

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method just before this Interaction Group is unregistered from it.
        /// </summary>
        /// <remarks>
        /// This is where the Group should re-register its members with the Interaction Manager so that they are registered
        /// as not belonging to a Group.
        /// </remarks>
        /// <seealso cref="XRInteractionManager.UnregisterInteractionGroup(IXRInteractionGroup)"/>
        void OnBeforeUnregistered();

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method when this Interaction Group is unregistered from it.
        /// </summary>
        /// <param name="args">Event data containing the Interaction Manager that unregistered this Interaction Group.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="XRInteractionManager.UnregisterInteractionGroup(IXRInteractionGroup)"/>
        void OnUnregistered(InteractionGroupUnregisteredEventArgs args);

        /// <summary>
        /// Adds the given Group member to the end of the ordered list of members in the Group.
        /// Causes no change if the Group member is already added.
        /// </summary>
        /// <param name="groupMember">The Group member to add.</param>
        /// <remarks>
        /// <paramref name="groupMember"/> must implement either <see cref="IXRInteractor"/> or <see cref="IXRInteractionGroup"/>.
        /// </remarks>
        void AddGroupMember(IXRGroupMember groupMember);

        /// <summary>
        /// Moves the given Group member to the specified index in the ordered list of members in the Group.
        /// If the member is not in the list, this can be used to insert the member at the specified index.
        /// </summary>
        /// <param name="groupMember">The Group member to move or add.</param>
        /// <param name="newIndex">New index of the Group member.</param>
        /// <remarks>
        /// <paramref name="groupMember"/> must implement either <see cref="IXRInteractor"/> or <see cref="IXRInteractionGroup"/>.
        /// </remarks>
        void MoveGroupMemberTo(IXRGroupMember groupMember, int newIndex);

        /// <summary>
        /// Removes the given Group member from the list of members.
        /// </summary>
        /// <param name="groupMember">The Group member to remove.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="groupMember"/> was removed from the list.
        /// Otherwise, returns <see langword="false"/> if <paramref name="groupMember"/> was not found in the list.
        /// </returns>
        bool RemoveGroupMember(IXRGroupMember groupMember);

        /// <summary>
        /// Removes all Group members from the list of members.
        /// </summary>
        void ClearGroupMembers();

        /// <summary>
        /// Checks whether the given Group member exists in the list of members.
        /// </summary>
        /// <param name="groupMember">The Group member to check for in the list.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="groupMember"/> exists in the list.
        /// Otherwise, returns <see langword="false"/>.
        /// </returns>
        bool ContainsGroupMember(IXRGroupMember groupMember);

        /// <summary>
        /// Returns all members in the ordered list of Group members into List <paramref name="results"/>.
        /// </summary>
        /// <param name="results">List to receive Group members.</param>
        /// <remarks>
        /// This method populates the list with the Group members at the time the method is called. It is not a live view,
        /// meaning Group members added or removed afterward will not be reflected in the results of this method.
        /// Clears <paramref name="results"/> before adding to it.
        /// </remarks>
        void GetGroupMembers(List<IXRGroupMember> results);

        /// <summary>
        /// Checks whether the given Group is either the same as this Group or a dependency of any member Group.
        /// </summary>
        /// <param name="group">The Group to check for as a dependency.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="group"/> is either the same as this Group or a dependency
        /// of this Group. Otherwise, returns <see langword="false"/>.
        /// </returns>
        bool HasDependencyOnGroup(IXRInteractionGroup group);

        /// <summary>
        /// The <see cref="XRInteractionManager"/> or containing <see cref="IXRInteractionGroup"/> calls this method to
        /// update the Group and its members before interaction events occur.
        /// </summary>
        /// <param name="updatePhase">The update phase during which this method is called.</param>
        /// <remarks>
        /// Please see the <see cref="XRInteractionManager"/> and <see cref="XRInteractionUpdateOrder.UpdatePhase"/> documentation for more
        /// details on update order.
        /// </remarks>
        /// <seealso cref="XRInteractionUpdateOrder.UpdatePhase"/>
        /// <seealso cref="IXRInteractor.PreprocessInteractor"/>
        void PreprocessGroupMembers(XRInteractionUpdateOrder.UpdatePhase updatePhase);

        /// <summary>
        /// The <see cref="XRInteractionManager"/> or containing <see cref="IXRInteractionGroup"/> calls this method to
        /// update the Group and its members after interaction events occur.
        /// </summary>
        /// <param name="updatePhase">The update phase during which this method is called.</param>
        /// <remarks>
        /// Please see the <see cref="XRInteractionManager"/> and <see cref="XRInteractionUpdateOrder.UpdatePhase"/> documentation for more
        /// details on update order.
        /// </remarks>
        /// <seealso cref="XRInteractionUpdateOrder.UpdatePhase"/>
        /// <seealso cref="IXRInteractor.ProcessInteractor"/>
        void ProcessGroupMembers(XRInteractionUpdateOrder.UpdatePhase updatePhase);

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method to update interactions for this Group's members.
        /// This is where interaction events occur for any <see cref="IXRInteractor"/> member.
        /// </summary>
        /// <remarks>
        /// The implementation of this method should call <see cref="UpdateGroupMemberInteractions(IXRInteractor, out IXRInteractor)"/>.
        /// </remarks>
        void UpdateGroupMemberInteractions();

        /// <summary>
        /// Updates interactions for this Group's members, given an Interactor that has already been prioritized for interaction.
        /// This is where interaction events occur for any <see cref="IXRInteractor"/> member.
        /// </summary>
        /// <param name="prePrioritizedInteractor">The Interactor that has already been prioritized for interaction.
        /// If not <see langword="null"/>, this prevents all other members in this Group from interacting.</param>
        /// <param name="interactorThatPerformedInteraction">The Interactor in this Group or any of its member Groups
        /// that performed interaction as a result of this method call. This will be <see langword="null"/> if no
        /// Interactor performed interaction.</param>
        /// <remarks>
        /// The implementation of this method should call this method on each member that is an <see cref="IXRInteractionGroup"/>.
        /// After this method is called, <see cref="activeInteractor"/> should return the same reference as
        /// <paramref name="interactorThatPerformedInteraction"/>.
        /// </remarks>
        void UpdateGroupMemberInteractions(IXRInteractor prePrioritizedInteractor, out IXRInteractor interactorThatPerformedInteraction);

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// right before the Interaction group first gains focus of an Interactable
        /// in a first pass.
        /// </summary>
        /// <param name="args">Event data containing the Interaction group that is initiating the focus.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="IXRFocusInteractable.OnFocusEntered(FocusEnterEventArgs)"/>
        void OnFocusEntering(FocusEnterEventArgs args);

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// right before the Interaction group loses focus of an Interactable
        /// in a first pass.
        /// </summary>
        /// <param name="args">Event data containing the Interaction group that is losing focus.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="IXRFocusInteractable.OnFocusExited(FocusExitEventArgs)"/>
        void OnFocusExiting(FocusExitEventArgs args);
    }
}