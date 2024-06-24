using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Behaviour implementation of <see cref="IXRInteractionGroup"/>. An Interaction Group hooks into the interaction system
    /// (via <see cref="XRInteractionManager"/>) and enforces that only one <see cref="IXRGroupMember"/> within the Group
    /// can interact at a time. Each Group member must be either an <see cref="IXRInteractor"/> or an <see cref="IXRInteractionGroup"/>.
    /// </summary>
    /// <remarks>
    /// The member prioritized for interaction in any given frame is whichever member was interacting the previous frame
    /// if it can select in the current frame. If there is no such member, then the interacting member is whichever one
    /// in the ordered list of members interacts first.
    /// </remarks>
    [DisallowMultipleComponent]
    [AddComponentMenu("XR/XR Interaction Group", 11)]
    [HelpURL(XRHelpURLConstants.k_XRInteractionGroup)]
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_InteractionGroups)]
    public class XRInteractionGroup : MonoBehaviour, IXRInteractionOverrideGroup, IXRGroupMember
    {
        /// <summary>
        /// These correspond to the default names of the Interaction Groups in the sample XR Rig.
        /// </summary>
        public static class GroupNames
        {
            /// <summary> Left controller and hand interactors </summary>
            public static readonly string k_Left = "Left";
            /// <summary> Right controller and hand interactors </summary>
            public static readonly string k_Right = "Right";
            /// <summary> Head/eye interactors </summary>
            public static readonly string k_Center = "Center";
        }

        [Serializable]
        internal class GroupMemberAndOverridesPair
        {
            [RequireInterface(typeof(IXRGroupMember))]
            public Object groupMember;

            [RequireInterface(typeof(IXRGroupMember))]
            public List<Object> overrideGroupMembers = new List<Object>();
        }

        /// <inheritdoc />
        public event Action<InteractionGroupRegisteredEventArgs> registered;

        /// <inheritdoc />
        public event Action<InteractionGroupUnregisteredEventArgs> unregistered;

        [SerializeField]
        [Tooltip("The name of the interaction group, which can be used to retrieve it from the Interaction Manager.")]
        string m_GroupName;

        /// <inheritdoc />
        public string groupName => m_GroupName;

        [SerializeField]
        [Tooltip("The XR Interaction Manager that this Interaction Group will communicate with (will find one if not set manually).")]
        XRInteractionManager m_InteractionManager;

        XRInteractionManager m_RegisteredInteractionManager;

        /// <summary>
        /// The <see cref="XRInteractionManager"/> that this Interaction Group will communicate with (will find one if <see langword="null"/>).
        /// </summary>
        public XRInteractionManager interactionManager
        {
            get => m_InteractionManager;
            set
            {
                m_InteractionManager = value;
                if (Application.isPlaying && isActiveAndEnabled)
                    RegisterWithInteractionManager();
            }
        }

        /// <inheritdoc />
        public IXRInteractionGroup containingGroup { get; private set; }

        [SerializeField]
        [Tooltip("Ordered list of Interactors or Interaction Groups that are registered with the Group on Awake.")]
        [RequireInterface(typeof(IXRGroupMember))]
        List<Object> m_StartingGroupMembers = new List<Object>();

        /// <summary>
        /// Ordered list of Interactors or Interaction Groups that are registered with the Group on Awake.
        /// All objects in this list should implement the <see cref="IXRGroupMember"/> interface and either the
        /// <see cref="IXRInteractor"/> interface or the <see cref="IXRInteractionGroup"/> interface.
        /// </summary>
        /// <remarks>
        /// There are separate methods to access and modify the Group members used after Awake.
        /// </remarks>
        /// <seealso cref="AddGroupMember"/>
        /// <seealso cref="MoveGroupMemberTo"/>
        /// <seealso cref="RemoveGroupMember"/>
        /// <seealso cref="ClearGroupMembers"/>
        /// <seealso cref="ContainsGroupMember"/>
        /// <seealso cref="GetGroupMembers"/>
        public List<Object> startingGroupMembers
        {
            get => m_StartingGroupMembers;
            set
            {
                m_StartingGroupMembers = value;
                RemoveMissingMembersFromStartingOverridesMap();
            }
        }

        [SerializeField]
        [Tooltip("Configuration for each Group Member of which other Members are able to override its interaction " +
            "when they attempt to select, despite the difference in priority order.")]
        List<GroupMemberAndOverridesPair> m_StartingInteractionOverridesMap = new List<GroupMemberAndOverridesPair>();

        /// <inheritdoc />
        public IXRInteractor activeInteractor { get; private set; }
        
        /// <inheritdoc />
        public IXRInteractor focusInteractor { get; private set; }
        
        /// <inheritdoc />
        public IXRFocusInteractable focusInteractable { get; private set; }

        // Used by custom editor to check if we can edit the starting configuration
        internal bool isRegisteredWithInteractionManager => m_RegisteredInteractionManager != null;
        internal bool hasRegisteredStartingMembers { get; private set; }

        readonly RegistrationList<IXRGroupMember> m_GroupMembers = new RegistrationList<IXRGroupMember>();
        readonly List<IXRGroupMember> m_TempGroupMembers = new List<IXRGroupMember>();
        bool m_IsProcessingGroupMembers;

        /// <summary>
        /// Mapping of each group member to a set of other members that can override its interaction via selection.
        /// </summary>
        readonly Dictionary<IXRGroupMember, HashSet<IXRGroupMember>> m_InteractionOverridesMap =
            new Dictionary<IXRGroupMember, HashSet<IXRGroupMember>>();

        readonly List<IXRInteractable> m_ValidTargets = new List<IXRInteractable>();
        readonly List<XRBaseInteractable> m_DeprecatedValidTargets = new List<XRBaseInteractable>();

        static readonly List<IXRSelectInteractable> s_InteractablesSelected = new List<IXRSelectInteractable>();
        static readonly List<IXRHoverInteractable> s_InteractablesHovered = new List<IXRHoverInteractable>();

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        protected virtual void Reset()
        {
#if UNITY_EDITOR
            // Don't need to do anything; method kept for backwards compatibility.
#endif
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Awake()
        {
            // Setup Interaction Manager
            FindCreateInteractionManager();

            // Starting member interactors will be re-registered with the manager below when they are added to the group.
            // Make sure the group is registered first.
            RegisterWithInteractionManager();

            // It is more efficient to add than move, but if there are existing items
            // use move to ensure the correct order dictated by the starting lists.
            if (m_GroupMembers.flushedCount > 0)
            {
                var index = 0;
                foreach (var obj in m_StartingGroupMembers)
                {
                    if (obj != null && obj is IXRGroupMember groupMember)
                        MoveGroupMemberTo(groupMember, index++);
                }
            }
            else
            {
                foreach (var obj in m_StartingGroupMembers)
                {
                    if (obj != null && obj is IXRGroupMember groupMember)
                        AddGroupMember(groupMember);
                }
            }

            if (string.IsNullOrWhiteSpace(m_GroupName))
                m_GroupName = gameObject.name;

            RemoveMissingMembersFromStartingOverridesMap();
            foreach (var groupMemberAndOverridesPair in m_StartingInteractionOverridesMap)
            {
                var groupMemberObj = groupMemberAndOverridesPair.groupMember;
                if (groupMemberObj == null || !(groupMemberObj is IXRGroupMember groupMember))
                    continue;

                foreach (var overrideGroupMemberObj in groupMemberAndOverridesPair.overrideGroupMembers)
                {
                    if (overrideGroupMemberObj != null && overrideGroupMemberObj is IXRGroupMember overrideGroupMember)
                        AddInteractionOverrideForGroupMember(groupMember, overrideGroupMember);
                }
            }

            hasRegisteredStartingMembers = true;
        }

        internal void RemoveMissingMembersFromStartingOverridesMap()
        {
            for (var i = m_StartingInteractionOverridesMap.Count - 1; i >= 0; i--)
            {
                var groupMemberAndOverrides = m_StartingInteractionOverridesMap[i];
                if (!m_StartingGroupMembers.Contains(groupMemberAndOverrides.groupMember))
                {
                    m_StartingInteractionOverridesMap.RemoveAt(i);
                }
                else
                {
                    var overrides = groupMemberAndOverrides.overrideGroupMembers;
                    for (var j = overrides.Count - 1; j >= 0; j--)
                    {
                        if (!m_StartingGroupMembers.Contains(overrides[j]))
                            overrides.RemoveAt(j);
                    }
                }
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnEnable()
        {
            FindCreateInteractionManager();
            RegisterWithInteractionManager();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnDisable()
        {
            UnregisterWithInteractionManager();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnDestroy()
        {
            hasRegisteredStartingMembers = false;
            m_InteractionOverridesMap.Clear();
            ClearGroupMembers();
        }

        /// <summary>
        /// Adds <paramref name="overrideGroupMember"/> to the list of Group members that are to be added as
        /// interaction overrides for <paramref name="sourceGroupMember"/> on Awake. Both objects must already be
        /// included in the <see cref="startingGroupMembers"/> list. The override object should implement either the
        /// <see cref="IXRSelectInteractor"/> interface or the <see cref="IXRInteractionOverrideGroup"/> interface.
        /// </summary>
        /// <param name="sourceGroupMember">The Group member whose interaction can be potentially overridden by
        /// <paramref name="overrideGroupMember"/>.</param>
        /// <param name="overrideGroupMember">The Group member to add as a possible interaction override.</param>
        /// <remarks>
        /// Use <see cref="AddInteractionOverrideForGroupMember"/> to add to the interaction overrides used after Awake.
        /// </remarks>
        /// <seealso cref="RemoveStartingInteractionOverride"/>
        /// <seealso cref="AddInteractionOverrideForGroupMember"/>
        public void AddStartingInteractionOverride(Object sourceGroupMember, Object overrideGroupMember)
        {
            if (sourceGroupMember == null)
            {
                Debug.LogError($"{nameof(sourceGroupMember)} cannot be null.");
                return;
            }

            if (overrideGroupMember == null)
            {
                Debug.LogError($"{nameof(overrideGroupMember)} cannot be null.");
                return;
            }

            if (!m_StartingGroupMembers.Contains(sourceGroupMember))
            {
                Debug.LogError($"Cannot add starting override group member for source member {sourceGroupMember} " +
                    $"because {sourceGroupMember} is not included in the starting group members.", this);

                return;
            }

            if (!m_StartingGroupMembers.Contains(overrideGroupMember))
            {
                Debug.LogError($"Cannot add override group member {overrideGroupMember} for source member " +
                    $"because {overrideGroupMember} is not included in the starting group members.", this);

                return;
            }

            if (TryGetStartingGroupMemberAndOverridesPair(sourceGroupMember, out var groupMemberAndOverrides))
            {
                groupMemberAndOverrides.overrideGroupMembers.Add(overrideGroupMember);
            }
            else
            {
                m_StartingInteractionOverridesMap.Add(new GroupMemberAndOverridesPair
                {
                    groupMember = sourceGroupMember,
                    overrideGroupMembers = new List<Object> { overrideGroupMember }
                });
            }
        }

        /// <summary>
        /// Removes <paramref name="overrideGroupMember"/> from the list of Group members that are to be added as
        /// interaction overrides for <paramref name="sourceGroupMember"/> on Awake.
        /// </summary>
        /// <param name="sourceGroupMember">The Group member whose interaction can no longer be overridden by
        /// <paramref name="overrideGroupMember"/>.</param>
        /// <param name="overrideGroupMember">The Group member to remove as a possible interaction override.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="overrideGroupMember"/> was removed from the list of
        /// potential overrides for <paramref name="sourceGroupMember"/>. Otherwise, returns <see langword="false"/>
        /// if <paramref name="overrideGroupMember"/> was not part of the list.
        /// </returns>
        /// <remarks>
        /// Use <see cref="RemoveInteractionOverrideForGroupMember"/> to remove from the interaction overrides used after Awake.
        /// </remarks>
        /// <seealso cref="AddStartingInteractionOverride"/>
        /// <seealso cref="RemoveInteractionOverrideForGroupMember"/>
        public bool RemoveStartingInteractionOverride(Object sourceGroupMember, Object overrideGroupMember)
        {
            if (sourceGroupMember == null)
            {
                Debug.LogError($"{nameof(sourceGroupMember)} cannot be null.");
                return false;
            }

            return TryGetStartingGroupMemberAndOverridesPair(sourceGroupMember, out var groupMemberAndOverrides) &&
                groupMemberAndOverrides.overrideGroupMembers.Remove(overrideGroupMember);
        }

        bool TryGetStartingGroupMemberAndOverridesPair(Object sourceGroupMember,
            out GroupMemberAndOverridesPair groupMemberAndOverrides)
        {
            if (sourceGroupMember == null)
            {
                groupMemberAndOverrides = null;
                return false;
            }

            foreach (var pair in m_StartingInteractionOverridesMap)
            {
                if (pair.groupMember != sourceGroupMember)
                    continue;

                groupMemberAndOverrides = pair;
                return true;
            }

            groupMemberAndOverrides = null;
            return false;
        }

        /// <inheritdoc />
        void IXRInteractionGroup.OnRegistered(InteractionGroupRegisteredEventArgs args)
        {
            if (args.manager != m_InteractionManager)
                Debug.LogWarning($"An Interaction Group was registered with an unexpected {nameof(XRInteractionManager)}." +
                                 $" {this} was expecting to communicate with \"{m_InteractionManager}\" but was registered with \"{args.manager}\".", this);

            m_RegisteredInteractionManager = args.manager;

            m_GroupMembers.Flush();
            m_IsProcessingGroupMembers = true;
            foreach (var groupMember in m_GroupMembers.registeredSnapshot)
            {
                if (!m_GroupMembers.IsStillRegistered(groupMember))
                    continue;

                if (groupMember.containingGroup == null)
                    RegisterAsGroupMember(groupMember);
            }

            m_IsProcessingGroupMembers = false;

            registered?.Invoke(args);
        }

        /// <inheritdoc />
        void IXRInteractionGroup.OnBeforeUnregistered()
        {
            m_GroupMembers.Flush();
            m_IsProcessingGroupMembers = true;
            foreach (var groupMember in m_GroupMembers.registeredSnapshot)
            {
                if (!m_GroupMembers.IsStillRegistered(groupMember))
                    continue;

                RegisterAsNonGroupMember(groupMember);
            }

            m_IsProcessingGroupMembers = false;
        }

        /// <inheritdoc />
        void IXRInteractionGroup.OnUnregistered(InteractionGroupUnregisteredEventArgs args)
        {
            if (args.manager != m_RegisteredInteractionManager)
                Debug.LogWarning($"An Interaction Group was unregistered from an unexpected {nameof(XRInteractionManager)}." +
                                 $" {this} was expecting to communicate with \"{m_RegisteredInteractionManager}\" but was unregistered from \"{args.manager}\".", this);

            m_RegisteredInteractionManager = null;
            unregistered?.Invoke(args);
        }

        /// <inheritdoc />
        public void AddGroupMember(IXRGroupMember groupMember)
        {
            if (groupMember == null)
                throw new ArgumentNullException(nameof(groupMember));

            if (!ValidateAddGroupMember(groupMember))
                return;

            if (m_IsProcessingGroupMembers)
                Debug.LogWarning($"{groupMember} added while {name} is processing Group members. It won't be processed until the next process.", this);

            if (m_GroupMembers.Register(groupMember))
                RegisterAsGroupMember(groupMember);
        }

        /// <inheritdoc />
        public void MoveGroupMemberTo(IXRGroupMember groupMember, int newIndex)
        {
            if (groupMember == null)
                throw new ArgumentNullException(nameof(groupMember));

            if (!ValidateAddGroupMember(groupMember))
                return;

            // BaseRegistrationList<T> does not yet support reordering with pending registration changes.
            if (m_IsProcessingGroupMembers)
            {
                Debug.LogError($"Cannot move {groupMember} while {name} is processing Group members.", this);
                return;
            }

            m_GroupMembers.Flush();
            if (m_GroupMembers.MoveItemImmediately(groupMember, newIndex) && groupMember.containingGroup == null)
                RegisterAsGroupMember(groupMember);
        }

        bool ValidateAddGroupMember(IXRGroupMember groupMember)
        {
            if (!(groupMember is IXRInteractor || groupMember is IXRInteractionGroup))
            {
                Debug.LogError($"Group member {groupMember} must be either an Interactor or an Interaction Group.", this);
                return false;
            }

            if (groupMember.containingGroup != null && !ReferenceEquals(groupMember.containingGroup, this))
            {
                Debug.LogError($"Cannot add/move {groupMember} because it is already part of a Group. Remove the member from the Group first.", this);
                return false;
            }

            if (groupMember is IXRInteractionGroup subGroup && subGroup.HasDependencyOnGroup(this))
            {
                Debug.LogError($"Cannot add/move {groupMember} because this would create a circular dependency of groups.", this);
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public bool RemoveGroupMember(IXRGroupMember groupMember)
        {
            if (m_GroupMembers.Unregister(groupMember))
            {
                // Reset active interactor if it was part of the member that was removed
                if (activeInteractor != null && GroupMemberIsOrContainsInteractor(groupMember, activeInteractor))
                    activeInteractor = null;

                m_InteractionOverridesMap.Remove(groupMember);
                RegisterAsNonGroupMember(groupMember);
                return true;
            }

            return false;
        }

        bool GroupMemberIsOrContainsInteractor(IXRGroupMember groupMember, IXRInteractor interactor)
        {
            if (ReferenceEquals(groupMember, interactor))
                return true;

            if (!(groupMember is IXRInteractionGroup memberGroup))
                return false;

            memberGroup.GetGroupMembers(m_TempGroupMembers);
            foreach (var subGroupMember in m_TempGroupMembers)
            {
                if (GroupMemberIsOrContainsInteractor(subGroupMember, interactor))
                    return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void ClearGroupMembers()
        {
            m_GroupMembers.Flush();
            for (var index = m_GroupMembers.flushedCount - 1; index >= 0; --index)
            {
                var groupMember = m_GroupMembers.GetRegisteredItemAt(index);
                RemoveGroupMember(groupMember);
            }
        }

        /// <inheritdoc />
        public bool ContainsGroupMember(IXRGroupMember groupMember)
        {
            return m_GroupMembers.IsRegistered(groupMember);
        }

        /// <inheritdoc />
        public void GetGroupMembers(List<IXRGroupMember> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            m_GroupMembers.GetRegisteredItems(results);
        }

        /// <inheritdoc />
        public bool HasDependencyOnGroup(IXRInteractionGroup group)
        {
            if (ReferenceEquals(group, this))
                return true;

            GetGroupMembers(m_TempGroupMembers);
            foreach (var groupMember in m_TempGroupMembers)
            {
                if (groupMember is IXRInteractionGroup subGroup && subGroup.HasDependencyOnGroup(group))
                    return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void AddInteractionOverrideForGroupMember(IXRGroupMember sourceGroupMember, IXRGroupMember overrideGroupMember)
        {
            if (sourceGroupMember == null)
            {
                Debug.LogError($"{nameof(sourceGroupMember)} cannot be null.");
                return;
            }

            if (overrideGroupMember == null)
            {
                Debug.LogError($"{nameof(overrideGroupMember)} cannot be null.");
                return;
            }

            if (!(overrideGroupMember is IXRSelectInteractor || overrideGroupMember is IXRInteractionOverrideGroup))
            {
                Debug.LogError($"Override group member {overrideGroupMember} must implement either " +
                    $"{nameof(IXRSelectInteractor)} or {nameof(IXRInteractionOverrideGroup)}.", this);

                return;
            }

            if (!ContainsGroupMember(sourceGroupMember))
            {
                Debug.LogError($"Cannot add override group member for source member {sourceGroupMember} because {sourceGroupMember} " +
                    "is not registered with the Group. Call AddGroupMember first.", this);

                return;
            }

            if (!ContainsGroupMember(overrideGroupMember))
            {
                Debug.LogError($"Cannot add override group member {overrideGroupMember} for source member because {overrideGroupMember} " +
                    "is not registered with the Group. Call AddGroupMember first.", this);

                return;
            }

            if (GroupMemberIsPartOfOverrideChain(overrideGroupMember, sourceGroupMember))
            {
                Debug.LogError($"Cannot add {overrideGroupMember} as an override group member for {sourceGroupMember} " +
                    "because this would create a loop of group member overrides.", this);

                return;
            }

            if (m_InteractionOverridesMap.TryGetValue(sourceGroupMember, out var overrides))
                overrides.Add(overrideGroupMember);
            else
                m_InteractionOverridesMap[sourceGroupMember] = new HashSet<IXRGroupMember> { overrideGroupMember };
        }

        /// <inheritdoc />
        public bool GroupMemberIsPartOfOverrideChain(IXRGroupMember sourceGroupMember, IXRGroupMember potentialOverrideGroupMember)
        {
            if (ReferenceEquals(potentialOverrideGroupMember, sourceGroupMember))
                return true;

            if (!m_InteractionOverridesMap.TryGetValue(sourceGroupMember, out var overrides))
                return false;

            foreach (var nextOverride in overrides)
            {
                if (GroupMemberIsPartOfOverrideChain(nextOverride, potentialOverrideGroupMember))
                    return true;
            }

            return false;
        }

        /// <inheritdoc />
        public bool RemoveInteractionOverrideForGroupMember(IXRGroupMember sourceGroupMember, IXRGroupMember overrideGroupMember)
        {
            if (sourceGroupMember == null)
            {
                Debug.LogError($"{nameof(sourceGroupMember)} cannot be null.");
                return false;
            }

            if (!ContainsGroupMember(sourceGroupMember))
            {
                Debug.LogError($"Cannot remove override group member for source member {sourceGroupMember} because {sourceGroupMember} " +
                    "is not registered with the Group.", this);

                return false;
            }

            return m_InteractionOverridesMap.TryGetValue(sourceGroupMember, out var overrides) && overrides.Remove(overrideGroupMember);
        }

        /// <inheritdoc />
        public bool ClearInteractionOverridesForGroupMember(IXRGroupMember sourceGroupMember)
        {
            if (sourceGroupMember == null)
            {
                Debug.LogError($"{nameof(sourceGroupMember)} cannot be null.");
                return false;
            }

            if (!ContainsGroupMember(sourceGroupMember))
            {
                Debug.LogError($"Cannot clear override group members for source member {sourceGroupMember} because {sourceGroupMember} " +
                    "is not registered with the Group.", this);

                return false;
            }

            if (!m_InteractionOverridesMap.TryGetValue(sourceGroupMember, out var overrides))
                return false;

            overrides.Clear();
            return true;

        }

        /// <inheritdoc />
        public void GetInteractionOverridesForGroupMember(IXRGroupMember sourceGroupMember, HashSet<IXRGroupMember> results)
        {
            if (sourceGroupMember == null)
            {
                Debug.LogError($"{nameof(sourceGroupMember)} cannot be null.");
                return;
            }

            if (results == null)
            {
                Debug.LogError($"{nameof(results)} cannot be null.");
                return;
            }

            if (!ContainsGroupMember(sourceGroupMember))
            {
                Debug.LogError($"Cannot get override group members for source member {sourceGroupMember} because {sourceGroupMember} " +
                    "is not registered with the Group.", this);

                return;
            }

            results.Clear();
            if (m_InteractionOverridesMap.TryGetValue(sourceGroupMember, out var overrides))
                results.UnionWith(overrides);
        }

        void FindCreateInteractionManager()
        {
            if (m_InteractionManager != null)
                return;

            m_InteractionManager = ComponentLocatorUtility<XRInteractionManager>.FindOrCreateComponent();
        }

        void RegisterWithInteractionManager()
        {
            if (m_RegisteredInteractionManager == m_InteractionManager)
                return;

            UnregisterWithInteractionManager();

            if (m_InteractionManager != null)
            {
                m_InteractionManager.RegisterInteractionGroup(this);
            }
        }

        void UnregisterWithInteractionManager()
        {
            if (m_RegisteredInteractionManager == null)
                return;

            m_RegisteredInteractionManager.UnregisterInteractionGroup(this);
        }

        void RegisterAsGroupMember(IXRGroupMember groupMember)
        {
            if (m_RegisteredInteractionManager == null)
                return;

            groupMember.OnRegisteringAsGroupMember(this);
            ReRegisterGroupMemberWithInteractionManager(groupMember);
        }

        void RegisterAsNonGroupMember(IXRGroupMember groupMember)
        {
            if (m_RegisteredInteractionManager == null)
                return;

            groupMember.OnRegisteringAsNonGroupMember();
            ReRegisterGroupMemberWithInteractionManager(groupMember);
        }

        void ReRegisterGroupMemberWithInteractionManager(IXRGroupMember groupMember)
        {
            if (m_RegisteredInteractionManager == null)
                return;

            // Re-register the interactor or group so the manager can update its status as part of a group
            switch (groupMember)
            {
                case IXRInteractor interactor:
                    if (m_RegisteredInteractionManager.IsRegistered(interactor))
                    {
                        m_RegisteredInteractionManager.UnregisterInteractor(interactor);
                        m_RegisteredInteractionManager.RegisterInteractor(interactor);
                    }
                    break;
                case IXRInteractionGroup group:
                    if (m_RegisteredInteractionManager.IsRegistered(group))
                    {
                        m_RegisteredInteractionManager.UnregisterInteractionGroup(group);
                        m_RegisteredInteractionManager.RegisterInteractionGroup(group);
                    }
                    break;
                default:
                    Debug.LogError($"Group member {groupMember} must be either an Interactor or an Interaction Group.", this);
                    break;
            }
        }

        /// <inheritdoc />
        void IXRInteractionGroup.PreprocessGroupMembers(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            // Flush once at the start of the update phase, and this is the first method invoked by the manager
            m_GroupMembers.Flush();

            m_IsProcessingGroupMembers = true;
            foreach (var groupMember in m_GroupMembers.registeredSnapshot)
            {
                if (!m_GroupMembers.IsStillRegistered(groupMember))
                    continue;

                switch (groupMember)
                {
                    case IXRInteractor interactor:
                        if (!m_RegisteredInteractionManager.IsRegistered(interactor))
                            continue;

                        interactor.PreprocessInteractor(updatePhase);
                        break;
                    case IXRInteractionGroup group:
                        if (!m_RegisteredInteractionManager.IsRegistered(group))
                            continue;

                        group.PreprocessGroupMembers(updatePhase);
                        break;
                }
            }

            m_IsProcessingGroupMembers = false;
        }

        /// <inheritdoc />
        void IXRInteractionGroup.ProcessGroupMembers(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            m_IsProcessingGroupMembers = true;
            foreach (var groupMember in m_GroupMembers.registeredSnapshot)
            {
                if (!m_GroupMembers.IsStillRegistered(groupMember))
                    continue;

                switch (groupMember)
                {
                    case IXRInteractor interactor:
                        if (!m_RegisteredInteractionManager.IsRegistered(interactor))
                            continue;

                        interactor.ProcessInteractor(updatePhase);
                        break;
                    case IXRInteractionGroup group:
                        if (!m_RegisteredInteractionManager.IsRegistered(group))
                            continue;

                        group.ProcessGroupMembers(updatePhase);
                        break;
                }
            }

            m_IsProcessingGroupMembers = false;
        }

        /// <inheritdoc />
        void IXRInteractionGroup.UpdateGroupMemberInteractions()
        {
            // Prioritize previous active interactor if it can select this frame
            IXRInteractor prePrioritizedInteractor = null;
            if (activeInteractor != null && m_RegisteredInteractionManager.IsRegistered(activeInteractor) &&
                activeInteractor is IXRSelectInteractor activeSelectInteractor &&
                CanStartOrContinueAnySelect(activeSelectInteractor))
            {
                prePrioritizedInteractor = activeInteractor;
            }

            ((IXRInteractionGroup)this).UpdateGroupMemberInteractions(prePrioritizedInteractor, out var interactorThatPerformedInteraction);
            activeInteractor = interactorThatPerformedInteraction;
        }

        bool CanStartOrContinueAnySelect(IXRSelectInteractor selectInteractor)
        {
            if (selectInteractor.keepSelectedTargetValid)
            {
                foreach (var interactable in selectInteractor.interactablesSelected)
                {
                    if (m_RegisteredInteractionManager.CanSelect(selectInteractor, interactable))
                        return true;
                }
            }

            m_RegisteredInteractionManager.GetValidTargets(selectInteractor, m_ValidTargets);
            foreach (var target in m_ValidTargets)
            {
                if (!(target is IXRSelectInteractable selectInteractable))
                    continue;

                if (m_RegisteredInteractionManager.CanSelect(selectInteractor, selectInteractable))
                    return true;
            }

            return false;
        }

        /// <inheritdoc />
        void IXRInteractionGroup.UpdateGroupMemberInteractions(IXRInteractor prePrioritizedInteractor, out IXRInteractor interactorThatPerformedInteraction)
        {
            if (((IXRInteractionOverrideGroup)this).ShouldOverrideActiveInteraction(out var overridingInteractor))
                prePrioritizedInteractor = overridingInteractor;

            interactorThatPerformedInteraction = null;
            m_IsProcessingGroupMembers = true;
            foreach (var groupMember in m_GroupMembers.registeredSnapshot)
            {
                if (!m_GroupMembers.IsStillRegistered(groupMember))
                    continue;

                switch (groupMember)
                {
                    case IXRInteractor interactor:
                        if (!m_RegisteredInteractionManager.IsRegistered(interactor))
                            continue;

                        var preventInteraction = prePrioritizedInteractor != null && interactor != prePrioritizedInteractor;
                        UpdateInteractorInteractions(interactor, preventInteraction, out var performedInteraction);
                        if (performedInteraction)
                        {
                            interactorThatPerformedInteraction = interactor;
                            prePrioritizedInteractor = interactor;
                        }

                        break;
                    case IXRInteractionGroup group:
                        if (!m_RegisteredInteractionManager.IsRegistered(group))
                            continue;

                        group.UpdateGroupMemberInteractions(prePrioritizedInteractor, out var interactorInSubGroupThatPerformedInteraction);
                        if (interactorInSubGroupThatPerformedInteraction != null)
                        {
                            interactorThatPerformedInteraction = interactorInSubGroupThatPerformedInteraction;
                            prePrioritizedInteractor = interactorInSubGroupThatPerformedInteraction;
                        }

                        break;
                }
            }

            m_IsProcessingGroupMembers = false;
            activeInteractor = interactorThatPerformedInteraction;
        }

        /// <inheritdoc />
        bool IXRInteractionOverrideGroup.ShouldOverrideActiveInteraction(out IXRSelectInteractor overridingInteractor)
        {
            overridingInteractor = null;
            if (activeInteractor == null ||
                !TryGetOverridesForContainedInteractor(activeInteractor, out var activeMemberOverrides))
            {
                return false;
            }

            // Iterate through group members rather than the overrides set so we can ensure that priority is respected
            var shouldOverride = false;
            m_IsProcessingGroupMembers = true;
            foreach (var groupMember in m_GroupMembers.registeredSnapshot)
            {
                if (!m_GroupMembers.IsStillRegistered(groupMember) || !activeMemberOverrides.Contains(groupMember))
                    continue;

                if (ShouldGroupMemberOverrideInteraction(activeInteractor, groupMember, out overridingInteractor))
                {
                    shouldOverride = true;
                    break;
                }
            }

            m_IsProcessingGroupMembers = false;
            return shouldOverride;
        }

        /// <summary>
        /// Tries to find the set of overrides for <paramref name="interactor"/> or overrides for the member Group that
        /// contains <paramref name="interactor"/> if <paramref name="interactor"/> is nested.
        /// </summary>
        /// <param name="interactor">The contained interactor to check against.</param>
        /// <param name="overrideGroupMembers">The set of override Group members for <paramref name="interactor"/> or
        /// overrides for the member Group that contains <paramref name="interactor"/>.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="interactor"/> has overrides or a member Group
        /// containing <paramref name="interactor"/> has overrides, <see langword="false"/> otherwise.
        /// </returns>
        bool TryGetOverridesForContainedInteractor(IXRInteractor interactor, out HashSet<IXRGroupMember> overrideGroupMembers)
        {
            overrideGroupMembers = null;
            if (!(interactor is IXRGroupMember interactorAsGroupMember))
            {
                Debug.LogError($"Interactor {interactor} must be a {nameof(IXRGroupMember)}.", this);
                return false;
            }

            // If the interactor is nested, bubble up to find the top-level member Group that contains the interactor.
            var nextContainingGroup = interactorAsGroupMember.containingGroup;
            var groupMemberForInteractor = interactorAsGroupMember;
            while (nextContainingGroup != null && !ReferenceEquals(nextContainingGroup, this))
            {
                if (nextContainingGroup is IXRGroupMember groupMemberGroup)
                {
                    nextContainingGroup = groupMemberGroup.containingGroup;
                    groupMemberForInteractor = groupMemberGroup;
                }
                else
                {
                    nextContainingGroup = null;
                }
            }

            if (nextContainingGroup == null)
            {
                Debug.LogError($"Interactor {interactor} must be contained by this group or one of its sub-groups.", this);
                return false;
            }

            return m_InteractionOverridesMap.TryGetValue(groupMemberForInteractor, out overrideGroupMembers);
        }

        /// <inheritdoc />
        bool IXRInteractionOverrideGroup.ShouldAnyMemberOverrideInteraction(IXRInteractor interactingInteractor,
            out IXRSelectInteractor overridingInteractor)
        {
            overridingInteractor = null;
            var shouldOverride = false;
            m_IsProcessingGroupMembers = true;
            foreach (var groupMember in m_GroupMembers.registeredSnapshot)
            {
                if (!m_GroupMembers.IsStillRegistered(groupMember))
                    continue;

                if (ShouldGroupMemberOverrideInteraction(interactingInteractor, groupMember, out overridingInteractor))
                {
                    shouldOverride = true;
                    break;
                }
            }

            m_IsProcessingGroupMembers = false;
            return shouldOverride;
        }

        bool ShouldGroupMemberOverrideInteraction(IXRInteractor interactingInteractor,
            IXRGroupMember overrideGroupMember, out IXRSelectInteractor overridingInteractor)
        {
            overridingInteractor = null;
            switch (overrideGroupMember)
            {
                case IXRSelectInteractor interactor:
                    if (!m_RegisteredInteractionManager.IsRegistered(interactor))
                        return false;

                    if (ShouldInteractorOverrideInteraction(interactingInteractor, interactor))
                    {
                        overridingInteractor = interactor;
                        return true;
                    }

                    break;
                case IXRInteractionOverrideGroup group:
                    if (!m_RegisteredInteractionManager.IsRegistered(group))
                        return false;

                    if (group.ShouldAnyMemberOverrideInteraction(interactingInteractor, out overridingInteractor))
                        return true;

                    break;
            }

            return false;
        }

        /// <summary>
        /// Checks if the given <paramref name="overridingInteractor"/> should override the active interaction of
        /// <paramref name="interactingInteractor"/> - that is, whether <paramref name="overridingInteractor"/> can
        /// select any interactable that <paramref name="interactingInteractor"/> is interacting with.
        /// </summary>
        /// <param name="interactingInteractor">The interactor that is currently interacting with at least one interactable.</param>
        /// <param name="overridingInteractor">The interactor that is capable of overriding the interaction of <paramref name="interactingInteractor"/>.</param>
        /// <returns>True if <paramref name="overridingInteractor"/> should override the active interaction of
        /// <paramref name="interactingInteractor"/>, false otherwise.</returns>
        bool ShouldInteractorOverrideInteraction(IXRInteractor interactingInteractor, IXRSelectInteractor overridingInteractor)
        {
            var interactingSelectInteractor = interactingInteractor as IXRSelectInteractor;
            var interactingHoverInteractor = interactingInteractor as IXRHoverInteractor;
            m_RegisteredInteractionManager.GetValidTargets(overridingInteractor, m_ValidTargets);
            foreach (var target in m_ValidTargets)
            {
                if (!(target is IXRSelectInteractable selectInteractable) ||
                    !m_RegisteredInteractionManager.CanSelect(overridingInteractor, selectInteractable))
                {
                    continue;
                }

                if (interactingSelectInteractor != null && interactingSelectInteractor.IsSelecting(selectInteractable))
                    return true;

                if (interactingHoverInteractor != null && target is IXRHoverInteractable hoverInteractable &&
                    interactingHoverInteractor.IsHovering(hoverInteractable))
                {
                    return true;
                }
            }

            return false;
        }

        void UpdateInteractorInteractions(IXRInteractor interactor, bool preventInteraction, out bool performedInteraction)
        {
            performedInteraction = false;

            using (XRInteractionManager.s_GetValidTargetsMarker.Auto())
                m_RegisteredInteractionManager.GetValidTargets(interactor, m_ValidTargets);

            // Cast to the abstract base classes to assist with backwards compatibility with existing user code.
            XRInteractionManager.GetOfType(m_ValidTargets, m_DeprecatedValidTargets);

            var selectInteractor = interactor as IXRSelectInteractor;
            var hoverInteractor = interactor as IXRHoverInteractor;

            if (selectInteractor != null)
            {
                using (XRInteractionManager.s_EvaluateInvalidSelectionsMarker.Auto())
                {
                    if (preventInteraction)
                        ClearAllInteractorSelections(selectInteractor);
                    else
                        m_RegisteredInteractionManager.ClearInteractorSelectionInternal(selectInteractor, m_ValidTargets);
                }
            }

            if (hoverInteractor != null)
            {
                using (XRInteractionManager.s_EvaluateInvalidHoversMarker.Auto())
                {
                    if (preventInteraction)
                        ClearAllInteractorHovers(hoverInteractor);
                    else
                        m_RegisteredInteractionManager.ClearInteractorHoverInternal(hoverInteractor, m_ValidTargets, m_DeprecatedValidTargets);
                }
            }

            if (preventInteraction)
                return;

            if (selectInteractor != null)
            {
                using (XRInteractionManager.s_EvaluateValidSelectionsMarker.Auto())
                    m_RegisteredInteractionManager.InteractorSelectValidTargetsInternal(selectInteractor, m_ValidTargets, m_DeprecatedValidTargets);

                // Alternatively check if the base interactor is interacting with UGUI
                // TODO move this api call to IUIInteractor for XRI 3.0
                if (selectInteractor.hasSelection || (interactor is XRBaseInteractor baseInteractor && baseInteractor.isInteractingWithUI))
                    performedInteraction = true;
            }

            if (hoverInteractor != null)
            {
                using (XRInteractionManager.s_EvaluateValidHoversMarker.Auto())
                    m_RegisteredInteractionManager.InteractorHoverValidTargetsInternal(hoverInteractor, m_ValidTargets, m_DeprecatedValidTargets);

                if (hoverInteractor.hasHover)
                    performedInteraction = true;
            }
        }

        void ClearAllInteractorSelections(IXRSelectInteractor selectInteractor)
        {
            if (selectInteractor.interactablesSelected.Count == 0)
                return;

            s_InteractablesSelected.Clear();
            s_InteractablesSelected.AddRange(selectInteractor.interactablesSelected);
            for (var i = s_InteractablesSelected.Count - 1; i >= 0; --i)
            {
                var interactable = s_InteractablesSelected[i];
                m_RegisteredInteractionManager.SelectExitInternal(selectInteractor, interactable);
            }
        }

        void ClearAllInteractorHovers(IXRHoverInteractor hoverInteractor)
        {
            if (hoverInteractor.interactablesHovered.Count == 0)
                return;

            s_InteractablesHovered.Clear();
            s_InteractablesHovered.AddRange(hoverInteractor.interactablesHovered);
            for (var i = s_InteractablesHovered.Count - 1; i >= 0; --i)
            {
                var interactable = s_InteractablesHovered[i];
                m_RegisteredInteractionManager.HoverExitInternal(hoverInteractor, interactable);
            }
        }

        /// <inheritdoc />
        public void OnFocusEntering(FocusEnterEventArgs args)
        {
            focusInteractable = args.interactableObject;
            focusInteractor = args.interactorObject;
        }

        /// <inheritdoc />
        public void OnFocusExiting(FocusExitEventArgs args)
        {
            if (focusInteractable == args.interactableObject)
            {
                focusInteractable = null;
                focusInteractor = null;
            }
        }

        /// <inheritdoc />
        void IXRGroupMember.OnRegisteringAsGroupMember(IXRInteractionGroup group)
        {
            if (containingGroup != null)
            {
                Debug.LogError($"{name} is already part of a Group. Remove the member from the Group first.", this);
                return;
            }

            if (!group.ContainsGroupMember(this))
            {
                Debug.LogError($"{nameof(IXRGroupMember.OnRegisteringAsGroupMember)} was called but the Group does not contain {name}. " +
                               "Add the member to the Group rather than calling this method directly.", this);
                return;
            }

            containingGroup = group;
        }

        /// <inheritdoc />
        void IXRGroupMember.OnRegisteringAsNonGroupMember()
        {
            containingGroup = null;
        }
    }
}
