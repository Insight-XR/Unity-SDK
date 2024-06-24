using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// The option of which object's orientation in the rig Unity matches with the destination after teleporting.
    /// </summary>
    public enum MatchOrientation
    {
        /// <summary>
        /// After teleporting the XR Origin will be positioned such that its up vector matches world space up.
        /// </summary>
        WorldSpaceUp,

        /// <summary>
        /// After teleporting the XR Origin will be positioned such that its up vector matches target up.
        /// </summary>
        TargetUp,

        /// <summary>
        /// After teleporting the XR Origin will be positioned such that its up and forward vectors match target up and forward, respectively.
        /// </summary>
        TargetUpAndForward,

        /// <summary>
        /// After teleporting the XR Origin will not attempt to match any orientation.
        /// </summary>
        None,
    }

    /// <summary>
    /// The Teleport Request that describes the result of the teleportation action. Each Teleportation Interactable must fill out a Teleport Request
    /// for each teleport action.
    /// </summary>
    public struct TeleportRequest
    {
        /// <summary>
        /// The position in world space of the Teleportation Destination.
        /// </summary>
        public Vector3 destinationPosition;
        /// <summary>
        /// The rotation in world space of the Teleportation Destination. This is used primarily for matching world rotations directly.
        /// </summary>
        public Quaternion destinationRotation;
        /// <summary>
        ///  The Time (in unix epoch) of the request.
        /// </summary>
        public float requestTime;
        /// <summary>
        /// The option of how to orient the rig after teleportation.
        /// </summary>
        public MatchOrientation matchOrientation;
    }

    /// <summary>
    /// This is intended to be the base class for all Teleportation Interactables. This abstracts the teleport request process for specializations of this class.
    /// </summary>
    public abstract partial class BaseTeleportationInteractable : XRBaseInteractable, IXRReticleDirectionProvider
    {
        /// <summary>
        /// Indicates when the teleportation action happens.
        /// </summary>
        public enum TeleportTrigger
        {
            /// <summary>
            /// Teleportation occurs once selection is released without being canceled.
            /// </summary>
            OnSelectExited,

            /// <summary>
            /// Teleportation occurs right when area is selected.
            /// </summary>
            OnSelectEntered,

            /// <summary>
            /// Teleportation occurs on activate.
            /// </summary>
            OnActivated,

            /// <summary>
            /// Teleportation occurs on deactivate.
            /// </summary>
            OnDeactivated,

            /// <summary>
            /// (Deprecated) OnSelectExit has been deprecated. Use OnSelectExited instead.
            /// </summary>
            [Obsolete("OnSelectExit has been deprecated. Use OnSelectExited instead. (UnityUpgradable) -> OnSelectExited")]
            OnSelectExit = OnSelectExited,

            /// <summary>
            /// (Deprecated) OnSelectEnter has been deprecated. Use OnSelectEntered instead.
            /// </summary>
            [Obsolete("OnSelectEnter has been deprecated. Use OnSelectEntered instead. (UnityUpgradable) -> OnSelectEntered")]
            OnSelectEnter = OnSelectEntered,

            /// <summary>
            /// (Deprecated) OnSelectEnter has been deprecated. Use OnSelectEntered instead.
            /// </summary>
            [Obsolete("OnActivate has been deprecated. Use OnActivated instead. (UnityUpgradable) -> OnActivated")]
            OnActivate = OnActivated,

            /// <summary>
            /// (Deprecated) OnDeactivate has been deprecated. Use OnDeactivated instead.
            /// </summary>
            [Obsolete("OnDeactivate has been deprecated. Use OnDeactivated instead. (UnityUpgradable) -> OnDeactivated")]
            OnDeactivate = OnDeactivated,
        }

        const float k_DefaultNormalToleranceDegrees = 30f;

        [SerializeField]
        [Tooltip("The teleportation provider that this teleportation interactable will communicate teleport requests to." +
            " If no teleportation provider is configured, will attempt to find a teleportation provider.")]
        TeleportationProvider m_TeleportationProvider;

        /// <summary>
        /// The teleportation provider that this teleportation interactable communicates teleport requests to.
        /// If no teleportation provider is configured, will attempt to find a teleportation provider.
        /// </summary>
        public TeleportationProvider teleportationProvider
        {
            get => m_TeleportationProvider;
            set => m_TeleportationProvider = value;
        }

        [SerializeField]
        [Tooltip("How to orient the rig after teleportation." +
            "\nSet to:" +
            "\n\nWorld Space Up to stay oriented according to the world space up vector." +
            "\n\nSet to Target Up to orient according to the target BaseTeleportationInteractable Transform's up vector." +
            "\n\nSet to Target Up And Forward to orient according to the target BaseTeleportationInteractable Transform's rotation." +
            "\n\nSet to None to maintain the same orientation before and after teleporting.")]
        MatchOrientation m_MatchOrientation = MatchOrientation.WorldSpaceUp;

        /// <summary>
        /// How to orient the rig after teleportation.
        /// </summary>
        /// <remarks>
        /// Set to:
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="MatchOrientation.WorldSpaceUp"/></term>
        /// <description> to stay oriented according to the world space up vector.</description>
        /// </item>
        /// <item>
        /// <term><see cref="MatchOrientation.TargetUp"/></term>
        /// <description> to orient according to the target <see cref="BaseTeleportationInteractable"/> Transform's up vector.</description>
        /// </item>
        /// <item>
        /// <term><see cref="MatchOrientation.TargetUpAndForward"/></term>
        /// <description> to orient according to the target <see cref="BaseTeleportationInteractable"/> Transform's rotation.</description>
        /// </item>
        /// <item>
        /// <term><see cref="MatchOrientation.None"/></term>
        /// <description> to maintain the same orientation before and after teleporting.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public MatchOrientation matchOrientation
        {
            get => m_MatchOrientation;
            set => m_MatchOrientation = value;
        }

        [SerializeField]
        [Tooltip("Whether or not to rotate the rig to match the forward direction of the attach transform of the selecting interactor.")]
        bool m_MatchDirectionalInput;

        /// <summary>
        /// Whether or not to rotate the rig to match the forward direction of the attach transform of the selecting interactor.
        /// This only applies when <see cref="matchOrientation"/> is set to <see cref="MatchOrientation.WorldSpaceUp"/> or <see cref="MatchOrientation.TargetUp"/>.
        /// </summary>
        public bool matchDirectionalInput
        {
            get => m_MatchDirectionalInput;
            set => m_MatchDirectionalInput = value;
        }

        [SerializeField]
        [Tooltip("Specify when the teleportation will be triggered. Options map to when the trigger is pressed or when it is released.")]
        TeleportTrigger m_TeleportTrigger = TeleportTrigger.OnSelectExited;

        /// <summary>
        /// Specifies when the teleportation triggers.
        /// </summary>
        public TeleportTrigger teleportTrigger
        {
            get => m_TeleportTrigger;
            set => m_TeleportTrigger = value;
        }

        [SerializeField]
        [Tooltip("When enabled, this teleportation interactable will only be selectable by a ray interactor if its current " +
                 "hit normal is aligned with this object's up vector.")]
        bool m_FilterSelectionByHitNormal;

        /// <summary>
        /// When set to <see langword="true"/>, this teleportation interactable will only be selectable by a ray interactor if its current
        /// hit normal is aligned with this object's up vector.
        /// </summary>
        /// <seealso cref="upNormalToleranceDegrees"/>
        public bool filterSelectionByHitNormal
        {
            get => m_FilterSelectionByHitNormal;
            set => m_FilterSelectionByHitNormal = value;
        }

        [SerializeField]
        [Tooltip("Sets the tolerance in degrees from this object's up vector for a hit normal to be considered aligned with the up vector.")]
        float m_UpNormalToleranceDegrees = k_DefaultNormalToleranceDegrees;

        /// <summary>
        /// The tolerance in degrees from this object's up vector for a hit normal to be considered aligned with the up vector.
        /// </summary>
        /// <seealso cref="filterSelectionByHitNormal"/>
        public float upNormalToleranceDegrees
        {
            get => m_UpNormalToleranceDegrees;
            set => m_UpNormalToleranceDegrees = value;
        }

        [SerializeField]
        TeleportingEvent m_Teleporting = new TeleportingEvent();

        /// <summary>
        /// Gets or sets the event that Unity calls when queuing to teleport via
        /// the <see cref="TeleportationProvider"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="TeleportingEventArgs"/> passed to each listener is only valid
        /// while the event is invoked, do not hold a reference to it.
        /// </remarks>
        public TeleportingEvent teleporting
        {
            get => m_Teleporting;
            set => m_Teleporting = value;
        }

        // Reusable event args
        readonly LinkedPool<TeleportingEventArgs> m_TeleportingEventArgs = new LinkedPool<TeleportingEventArgs>(() => new TeleportingEventArgs(), collectionCheck: false);

        readonly Dictionary<IXRInteractor, Vector3> m_TeleportForwardPerInteractor = new Dictionary<IXRInteractor, Vector3>();

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            if (m_TeleportationProvider == null)
                ComponentLocatorUtility<TeleportationProvider>.TryFindComponent(out m_TeleportationProvider);
        }

        /// <inheritdoc />
        protected override void Reset()
        {
            base.Reset();
            selectMode = InteractableSelectMode.Multiple;
        }

        /// <summary>
        /// Automatically called upon the teleport trigger event occurring to generate the teleport request.
        /// The teleportation destination pose should be filled out.
        /// </summary>
        /// <param name="interactor">The interactor that initiated the teleport trigger.</param>
        /// <param name="raycastHit">The ray cast hit information from the interactor.</param>
        /// <param name="teleportRequest">The teleport request that should be filled out during this method call.</param>
        /// <returns>Returns <see langword="true"/> if the teleport request was successfully updated and should be queued. Otherwise, returns <see langword="false"/>.</returns>
        /// <seealso cref="TeleportationProvider.QueueTeleportRequest"/>
        protected virtual bool GenerateTeleportRequest(IXRInteractor interactor, RaycastHit raycastHit, ref TeleportRequest teleportRequest) => false;

        void SendTeleportRequest(IXRInteractor interactor)
        {
            if (interactor == null)
                return;

            if (m_TeleportationProvider == null && !ComponentLocatorUtility<TeleportationProvider>.TryFindComponent(out m_TeleportationProvider))
                return;

            RaycastHit raycastHit = default;
            if (interactor is XRRayInteractor rayInteractor && rayInteractor != null)
            {
                // Are we still selecting this object and within the tolerated normal threshold?
                if (!rayInteractor.TryGetCurrent3DRaycastHit(out raycastHit) ||
                    !interactionManager.TryGetInteractableForCollider(raycastHit.collider, out var hitInteractable) ||
                    hitInteractable != (IXRInteractable)this ||
                    (m_FilterSelectionByHitNormal && Vector3.Angle(transform.up, raycastHit.normal) > m_UpNormalToleranceDegrees))
                {
                    return;
                }
            }

            var teleportRequest = new TeleportRequest
            {
                matchOrientation = m_MatchOrientation,
                requestTime = Time.time,
            };

            bool success;
            if (interactor is XRBaseInteractor baseInteractor)
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
                success = GenerateTeleportRequest(baseInteractor, raycastHit, ref teleportRequest);
#pragma warning restore 618
            else
                success = GenerateTeleportRequest(interactor, raycastHit, ref teleportRequest);

            if (success)
            {
                UpdateTeleportRequestRotation(interactor, ref teleportRequest);
                success = m_TeleportationProvider.QueueTeleportRequest(teleportRequest);

                if (success && m_Teleporting != null)
                {
                    using (m_TeleportingEventArgs.Get(out var args))
                    {
                        args.interactorObject = interactor;
                        args.interactableObject = this;
                        args.teleportRequest = teleportRequest;
                        m_Teleporting.Invoke(args);
                    }
                }
            }
        }

        void UpdateTeleportRequestRotation(IXRInteractor interactor, ref TeleportRequest teleportRequest)
        {
            if (!m_MatchDirectionalInput || !m_TeleportForwardPerInteractor.TryGetValue(interactor, out var forward))
                return;

            switch (teleportRequest.matchOrientation)
            {
                case MatchOrientation.WorldSpaceUp:
                    teleportRequest.destinationRotation = Quaternion.LookRotation(forward, Vector3.up);

                    // Change the match orientation value to request that the teleportation provider should apply the destination rotation with the directional input.
                    teleportRequest.matchOrientation = MatchOrientation.TargetUpAndForward;
                    break;

                case MatchOrientation.TargetUp:
                    teleportRequest.destinationRotation = Quaternion.LookRotation(forward, transform.up);

                    // Change the match orientation value to request that the teleportation provider should apply the destination rotation with the directional input.
                    teleportRequest.matchOrientation = MatchOrientation.TargetUpAndForward;
                    break;
            }
        }

        /// <inheritdoc />
        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic || !m_MatchDirectionalInput)
                return;

            // Update the reticle direction for each interactor that is hovering or selecting this interactable.
            for (int index = 0, count = interactorsHovering.Count; index < count; ++index)
            {
                var interactorHovering = interactorsHovering[index];
                CalculateTeleportForward(interactorHovering);
            }

            for (int index = 0, count = interactorsSelecting.Count; index < count; ++index)
            {
                var interactorSelecting = interactorsSelecting[index];
                // Skip if also hovered by the interactor since it would have already been computed above.
                if (IsHovered(interactorSelecting))
                    continue;

                CalculateTeleportForward(interactorSelecting);
            }

            void CalculateTeleportForward(IXRInteractor interactor)
            {
                var attachTransform = interactor.GetAttachTransform(this);
                switch (matchOrientation)
                {
                    case MatchOrientation.WorldSpaceUp:
                        m_TeleportForwardPerInteractor[interactor] = Vector3.ProjectOnPlane(attachTransform.forward, Vector3.up).normalized;
                        break;

                    case MatchOrientation.TargetUp:
                        m_TeleportForwardPerInteractor[interactor] = Vector3.ProjectOnPlane(attachTransform.forward, transform.up).normalized;
                        break;
                }
            }
        }

        /// <inheritdoc />
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (m_TeleportTrigger == TeleportTrigger.OnSelectEntered)
                SendTeleportRequest(args.interactorObject);

            base.OnSelectEntered(args);
        }

        /// <inheritdoc />
        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            if (m_TeleportTrigger == TeleportTrigger.OnSelectExited && !args.isCanceled)
                SendTeleportRequest(args.interactorObject);

            base.OnSelectExited(args);
        }

        /// <inheritdoc />
        protected override void OnActivated(ActivateEventArgs args)
        {
            if (m_TeleportTrigger == TeleportTrigger.OnActivated)
                SendTeleportRequest(args.interactorObject);

            base.OnActivated(args);
        }

        /// <inheritdoc />
        protected override void OnDeactivated(DeactivateEventArgs args)
        {
            if (m_TeleportTrigger == TeleportTrigger.OnDeactivated)
                SendTeleportRequest(args.interactorObject);

            base.OnDeactivated(args);
        }

        /// <inheritdoc />
        public override bool IsSelectableBy(IXRSelectInteractor interactor)
        {
            var isSelectable = base.IsSelectableBy(interactor);
            if (isSelectable && m_FilterSelectionByHitNormal &&
                interactor is XRRayInteractor rayInteractor && rayInteractor != null &&
                rayInteractor.TryGetCurrent3DRaycastHit(out var raycastHit) &&
                interactionManager.TryGetInteractableForCollider(raycastHit.collider, out var hitInteractable) &&
                hitInteractable == (IXRInteractable)this)
            {
                // The ray interactor should only be able to select if its current hit is this interactable
                // and the hit normal is within the tolerated threshold.
                isSelectable &= Vector3.Angle(transform.up, raycastHit.normal) <= m_UpNormalToleranceDegrees;
            }

            return isSelectable;
        }

        /// <inheritdoc />
        public void GetReticleDirection(IXRInteractor interactor, Vector3 hitNormal, out Vector3 reticleUp, out Vector3? optionalReticleForward)
        {
            optionalReticleForward = null;
            reticleUp = hitNormal;
            Vector3 reticleForward;
            var xrOrigin = teleportationProvider.system.xrOrigin;
            switch (matchOrientation)
            {
                case MatchOrientation.WorldSpaceUp:
                    reticleUp = Vector3.up;
                    if (m_MatchDirectionalInput && m_TeleportForwardPerInteractor.TryGetValue(interactor, out reticleForward))
                        optionalReticleForward = reticleForward;
                    else if (xrOrigin != null)
                        optionalReticleForward = xrOrigin.Camera.transform.forward;
                    break;

                case MatchOrientation.TargetUp:
                    reticleUp = transform.up;
                    if (m_MatchDirectionalInput && m_TeleportForwardPerInteractor.TryGetValue(interactor, out reticleForward))
                        optionalReticleForward = reticleForward;
                    else if (xrOrigin != null)
                        optionalReticleForward = xrOrigin.Camera.transform.forward;
                    break;

                case MatchOrientation.TargetUpAndForward:
                    reticleUp = transform.up;
                    optionalReticleForward = transform.forward;
                    break;

                case MatchOrientation.None:
                    if (xrOrigin != null)
                    {
                        reticleUp = xrOrigin.Origin.transform.up;
                        optionalReticleForward = xrOrigin.Camera.transform.forward;
                    }
                    break;

                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(MatchOrientation)}={matchOrientation}.");
                    break;
            }
        }
    }
}
