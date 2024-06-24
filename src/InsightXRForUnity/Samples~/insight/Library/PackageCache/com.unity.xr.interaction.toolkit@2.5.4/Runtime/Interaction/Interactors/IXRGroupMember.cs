namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// An interface that represents an object that can be contained as a member in an <see cref="IXRInteractionGroup"/>.
    /// A Group member can be either an Interactor or another Group. Only one member in a Group can perform interaction at a time.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface must also implement either <see cref="IXRInteractor"/> or
    /// <see cref="IXRInteractionGroup"/> to be functional within the Group.
    /// </remarks>
    /// <seealso cref="XRInteractionGroup"/>
    /// <seealso cref="XRBaseInteractor"/>
    public interface IXRGroupMember
    {
        /// <summary>
        /// The Interaction Group that contains this member.
        /// </summary>
        IXRInteractionGroup containingGroup { get; }

        /// <summary>
        /// An <see cref="IXRInteractionGroup"/> calls this method just after this member has been added to the Group's
        /// list or just after the Group has registered with the <see cref="XRInteractionManager"/>. This method is
        /// called just before the member is re-registered with the Interaction Manager so that it can be registered as
        /// being part of a Group.
        /// </summary>
        /// <remarks>
        /// Implementations of this method should ensure that <see cref="containingGroup"/> returns <paramref name="group"/>
        /// after this method is called.
        /// </remarks>
        /// <param name="group">The Interaction Group that this member is now a part of.</param>
        void OnRegisteringAsGroupMember(IXRInteractionGroup group);

        /// <summary>
        /// An <see cref="IXRInteractionGroup"/> calls this method just after this member has been removed from the Group's
        /// list or before the Group unregisters with the <see cref="XRInteractionManager"/>. This method is called
        /// just before the member is re-registered with the Interaction Manager so that it can be registered as being
        /// not part of a Group.
        /// </summary>
        /// <remarks>
        /// Implementations of this method should ensure that <see cref="containingGroup"/> returns <see langword="null"/>
        /// after this method is called.
        /// </remarks>
        void OnRegisteringAsNonGroupMember();
    }

    /// <summary>
    /// Extension methods for <see cref="IXRGroupMember"/>.
    /// </summary>
    /// <seealso cref="IXRGroupMember"/>
    public static class XRGroupMemberExtensions
    {
        /// <summary>
        /// Gets the last Interaction Group in this Group member's chain of containing Groups that are contained within
        /// other Groups. The Interaction Group returned will have no containing Group.
        /// </summary>
        /// <param name="groupMember">The Group member to operate on.</param>
        /// <returns>Returns the last Interaction Group in this Group member's chain of containing Groups that are
        /// contained within other Groups. The Interaction Group returned will have no containing Group. Returns
        /// <see langword="null"/> if <paramref name="groupMember"/> does not have a containing Group.</returns>
        public static IXRInteractionGroup GetTopLevelContainingGroup(this IXRGroupMember groupMember)
        {
            var topLevelContainingGroup = groupMember.containingGroup;
            var nextContainingGroup = topLevelContainingGroup;
            while (nextContainingGroup != null)
            {
                topLevelContainingGroup = nextContainingGroup;
                nextContainingGroup = topLevelContainingGroup is IXRGroupMember groupMemberGroup
                    ? groupMemberGroup.containingGroup
                    : null;
            }

            return topLevelContainingGroup;
        }
    }
}