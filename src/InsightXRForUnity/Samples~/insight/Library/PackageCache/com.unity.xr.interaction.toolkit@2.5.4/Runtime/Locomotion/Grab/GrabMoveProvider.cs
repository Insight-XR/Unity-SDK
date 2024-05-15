using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Locomotion provider that allows the user to move as if grabbing the whole world around them.
    /// When the controller moves, the XR Origin counter-moves in order to keep the controller fixed relative to the world.
    /// </summary>
    /// <seealso cref="TwoHandedGrabMoveProvider"/>
    /// <seealso cref="LocomotionProvider"/>
    [AddComponentMenu("XR/Locomotion/Grab Move Provider", 11)]
    [HelpURL(XRHelpURLConstants.k_GrabMoveProvider)]
    public class GrabMoveProvider : ConstrainedMoveProvider
    {
        [SerializeField]
        [Tooltip("The controller Transform that will drive grab movement with its local position. Will use this " +
                 "GameObject's Transform if not set.")]
        Transform m_ControllerTransform;
        /// <summary>
        /// The controller Transform that will drive grab movement with its local position. Will use this GameObject's
        /// Transform if not set.
        /// </summary>
        public Transform controllerTransform
        {
            get => m_ControllerTransform;
            set
            {
                m_ControllerTransform = value;
                GatherControllerInteractors();
            }
        }

        [SerializeField]
        [Tooltip("Controls whether to allow grab move locomotion while the controller is selecting an interactable.")]
        bool m_EnableMoveWhileSelecting;
        /// <summary>
        /// Controls whether to allow grab move locomotion while the controller is selecting an interactable.
        /// </summary>
        public bool enableMoveWhileSelecting
        {
            get => m_EnableMoveWhileSelecting;
            set => m_EnableMoveWhileSelecting = value;
        }

        [SerializeField]
        [Tooltip("The ratio of actual movement distance to controller movement distance.")]
        float m_MoveFactor = 1f;
        /// <summary>
        /// The ratio of actual movement distance to controller movement distance.
        /// </summary>
        public float moveFactor
        {
            get => m_MoveFactor;
            set => m_MoveFactor = value;
        }

        [SerializeField]
        [Tooltip("The Input System Action that will be used to perform grab movement while held. Must be a Button Control.")]
        InputActionProperty m_GrabMoveAction = new InputActionProperty(new InputAction("Grab Move", type: InputActionType.Button));
        /// <summary>
        /// The Input System Action that Unity uses to perform grab movement while held. Must be a <see cref="ButtonControl"/> Control.
        /// </summary>
        public InputActionProperty grabMoveAction
        {
            get => m_GrabMoveAction;
            set => SetInputActionProperty(ref m_GrabMoveAction, value);
        }

        /// <summary>
        /// Controls whether this provider can move the XR Origin.
        /// </summary>
        public bool canMove { get; set; } = true;

        bool m_IsMoving;

        Vector3 m_PreviousControllerLocalPosition;

        readonly List<IXRSelectInteractor> m_ControllerInteractors = new List<IXRSelectInteractor>();

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (m_ControllerTransform == null)
                m_ControllerTransform = transform;

            GatherControllerInteractors();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            m_GrabMoveAction.EnableDirectAction();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            m_GrabMoveAction.DisableDirectAction();
        }

        /// <inheritdoc/>
        protected override Vector3 ComputeDesiredMove(out bool attemptingMove)
        {
            attemptingMove = false;
            var xrOrigin = system.xrOrigin?.Origin;
            var wasMoving = m_IsMoving;
            m_IsMoving = canMove && IsGrabbing() && xrOrigin != null;
            if (!m_IsMoving)
                return Vector3.zero;

            var controllerLocalPosition = controllerTransform.localPosition;
            if (!wasMoving && m_IsMoving) // Cannot simply check locomotionPhase because it might always be in moving state, due to gravity application mode
            {
                // Do not move the first frame of grab
                m_PreviousControllerLocalPosition = controllerLocalPosition;
                return Vector3.zero;
            }

            attemptingMove = true;
            var originTransform = xrOrigin.transform;
            var move = originTransform.TransformVector(m_PreviousControllerLocalPosition - controllerLocalPosition) * m_MoveFactor;
            m_PreviousControllerLocalPosition = controllerLocalPosition;
            return move;
        }

        /// <summary>
        /// Determines whether grab move is active.
        /// </summary>
        /// <returns>Whether grab move is active.</returns>
        public bool IsGrabbing()
        {
            return m_GrabMoveAction.action.IsPressed() && (m_EnableMoveWhileSelecting || !ControllerHasSelection());
        }

        void GatherControllerInteractors()
        {
            m_ControllerInteractors.Clear();
            if (m_ControllerTransform != null)
                m_ControllerTransform.transform.GetComponentsInChildren(m_ControllerInteractors);
        }

        bool ControllerHasSelection()
        {
            foreach (var interactor in m_ControllerInteractors)
            {
                if (interactor.hasSelection)
                    return true;
            }

            return false;
        }

        void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
        {
            if (Application.isPlaying)
                property.DisableDirectAction();

            property = value;

            if (Application.isPlaying && isActiveAndEnabled)
                property.EnableDirectAction();
        }
    }
}