using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// An interface that represents an Interaction Group component that is capable of overriding the interaction of the
    /// <see cref="IXRInteractionGroup.activeInteractor"/> when another interactor tries to select any of the interactables
    /// being hovered or selected. An interactor can only override interaction when it is or is contained within a Group
    /// member that is configured as a possible override for the active Group member.
    /// </summary>
    /// <seealso cref="XRInteractionGroup"/>
    /// <seealso cref="IXRGroupMember"/>
    public interface IXRInteractionOverrideGroup : IXRInteractionGroup
    {
        /// <summary>
        /// Adds <paramref name="overrideGroupMember"/> as a possible interaction override for <paramref name="sourceGroupMember"/>.
        /// </summary>
        /// <param name="sourceGroupMember">The Group member whose interaction can be potentially overridden by <paramref name="overrideGroupMember"/>.</param>
        /// <param name="overrideGroupMember">The Group member to add as a possible interaction override.</param>
        /// <remarks>
        /// Both members must be registered with the Group. Additionally, <paramref name="overrideGroupMember"/> must
        /// implement either <see cref="IXRSelectInteractor"/> or <see cref="IXRInteractionOverrideGroup"/>.
        /// This method must not create a loop in the chain of overrides for <paramref name="sourceGroupMember"/>. Use the
        /// implementation of <see cref="GroupMemberIsPartOfOverrideChain"/> to ensure there is no loop before adding override.
        /// </remarks>
        void AddInteractionOverrideForGroupMember(IXRGroupMember sourceGroupMember,
            IXRGroupMember overrideGroupMember);

        /// <summary>
        /// Checks whether <paramref name="potentialOverrideGroupMember"/> is either the same as <paramref name="sourceGroupMember"/>
        /// or part of a chain of its override Group members and their overrides.
        /// </summary>
        /// <param name="sourceGroupMember">The source Group member for the potential chain of override members.</param>
        /// <param name="potentialOverrideGroupMember">The Group member to check for as part of a chain of overrides.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="potentialOverrideGroupMember"/> is either the same as
        /// <paramref name="sourceGroupMember"/> or part of a chain of its override Group members and their overrides.
        /// Otherwise, returns <see langword="false"/>.
        /// </returns>
        bool GroupMemberIsPartOfOverrideChain(IXRGroupMember sourceGroupMember,
            IXRGroupMember potentialOverrideGroupMember);

        /// <summary>
        /// Removes <paramref name="overrideGroupMember"/> as a possible interaction override for <paramref name="sourceGroupMember"/>.
        /// </summary>
        /// <param name="sourceGroupMember">The Group member whose interaction can no longer be overridden by <paramref name="overrideGroupMember"/>.</param>
        /// <param name="overrideGroupMember">The Group member to remove as a possible interaction override.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="overrideGroupMember"/> was removed from the set of
        /// potential overrides for <paramref name="sourceGroupMember"/>. Otherwise, returns <see langword="false"/>
        /// if <paramref name="overrideGroupMember"/> was not part of the set.
        /// </returns>
        /// <remarks>
        /// <paramref name="sourceGroupMember"/> must be registered with the Group.
        /// </remarks>
        bool RemoveInteractionOverrideForGroupMember(IXRGroupMember sourceGroupMember,
            IXRGroupMember overrideGroupMember);

        /// <summary>
        /// Clears the set of possible interaction overrides for <paramref name="sourceGroupMember"/>.
        /// </summary>
        /// <param name="sourceGroupMember">The Group member whose interaction can no longer be overridden.</param>
        /// <returns>
        /// Returns <see langword="true"/> if there is a set of overrides for <paramref name="sourceGroupMember"/>.
        /// Otherwise, returns <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// <paramref name="sourceGroupMember"/> must be registered with the Group.
        /// </remarks>
        bool ClearInteractionOverridesForGroupMember(IXRGroupMember sourceGroupMember);

        /// <summary>
        /// Returns all members in the set of possible interaction overrides for <paramref name="sourceGroupMember"/>
        /// into set <paramref name="results"/>.
        /// </summary>
        /// <param name="sourceGroupMember">The Group member whose overrides to get.</param>
        /// <param name="results">Set to receive override Group members.</param>
        /// <remarks>
        /// <paramref name="sourceGroupMember"/> must be registered with the Group.
        /// This method populates the set with the Group members at the time the method is called. It is not a live view,
        /// meaning override Group members added or removed afterward will not be reflected in the results of this method.
        /// Clears <paramref name="results"/> before adding to it.
        /// </remarks>
        void GetInteractionOverridesForGroupMember(IXRGroupMember sourceGroupMember, HashSet<IXRGroupMember> results);

        /// <summary>
        /// Checks whether the Group should end the interactions of the <see cref="IXRInteractionGroup.activeInteractor"/>
        /// and instead prioritize an override interactor for interaction. An interactor should only override if it exists
        /// in the set of override Group members for the active member and is capable of selecting any interactable that
        /// <see cref="IXRInteractionGroup.activeInteractor"/> is interacting with. If multiple Group members are capable
        /// of overriding, only the highest priority one should override.
        /// </summary>
        /// <param name="overridingInteractor">The interactor that should override interaction.</param>
        /// <returns>
        /// Returns <see langword="true"/> if the Group should end the interactions of the
        /// <see cref="IXRInteractionGroup.activeInteractor"/> and instead prioritize an override interactor for
        /// interaction. Otherwise, returns <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The implementation of <see cref="IXRInteractionGroup.UpdateGroupMemberInteractions(IXRInteractor, out IXRInteractor)"/>
        /// should call this method at the start to determine whether <paramref name="overridingInteractor"/> should
        /// override the pre-prioritized interactor.
        /// The implementation of this method should call <see cref="ShouldAnyMemberOverrideInteraction"/> on each
        /// override Group member that is an <see cref="IXRInteractionOverrideGroup"/>.
        /// </remarks>
        bool ShouldOverrideActiveInteraction(out IXRSelectInteractor overridingInteractor);

        /// <summary>
        /// Checks whether any member of the Group should override the interactions of <paramref name="interactingInteractor"/>.
        /// An interactor should only override if it is capable of selecting any interactable that <paramref name="interactingInteractor"/>
        /// is interacting with. If multiple Group members are capable of overriding, only the highest priority one should override.
        /// </summary>
        /// <param name="interactingInteractor">The interactor that is interacting with an interactable.</param>
        /// <param name="overridingInteractor">The interactor that should override interaction.</param>
        /// <returns>
        /// Returns <see langword="true"/> if any member of the Group should override the interactions of
        /// <paramref name="interactingInteractor"/>. Otherwise, returns <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The implementation of this method should call this method on each Group member that is an
        /// <see cref="IXRInteractionOverrideGroup"/>.
        /// </remarks>
        bool ShouldAnyMemberOverrideInteraction(IXRInteractor interactingInteractor,
            out IXRSelectInteractor overridingInteractor);
    }
}