#if AR_FOUNDATION_PRESENT || PACKAGE_DOCS_GENERATION
using System.Text;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Scripting;

namespace UnityEngine.XR.Interaction.Toolkit.AR.Inputs
{
    /// <summary>
    /// An input device representing mobile 2D screen gestures.
    /// </summary>
    [InputControlLayout(stateType = typeof(TouchscreenGestureInputControllerState), displayName = "Touchscreen Gesture Input Controller", isGenericTypeOfDevice = true, updateBeforeRender = true)]
    [Preserve]
    public class TouchscreenGestureInputController : InputSystem.XR.XRController, IInputUpdateCallbackReceiver
    {
        /// <summary>
        /// The screen position where the tap gesture started.
        /// </summary>
        public Vector2Control tapStartPosition { get; private set; }

        /// <summary>
        /// The screen position where the drag gesture started.
        /// </summary>
        public Vector2Control dragStartPosition { get; private set; }

        /// <summary>
        /// The current screen position of the drag gesture.
        /// </summary>
        public Vector2Control dragCurrentPosition { get; private set; }

        /// <summary>
        /// The delta screen position of the drag gesture.
        /// </summary>
        public Vector2Control dragDelta { get; private set; }

        /// <summary>
        /// The screen position of the first finger where the pinch gesture started.
        /// </summary>
        public Vector2Control pinchStartPosition1 { get; private set; }

         /// <summary>
        /// The screen position of the second finger where the pinch gesture started.
        /// </summary>
        public Vector2Control pinchStartPosition2 { get; private set; }

        /// <summary>
        /// The gap between then position of the first and second fingers for the pinch gesture.
        /// </summary>
        public AxisControl pinchGap { get; private set; }

        /// <summary> 
        /// The gap delta between then position of the first and second fingers for the pinch gesture.
        /// </summary>
        public AxisControl pinchGapDelta { get; private set; }

        /// <summary>
        /// The screen position of the first finger where the twist gesture started.
        /// </summary>
        public Vector2Control twistStartPosition1 { get; private set; }

        /// <summary>
        /// The screen position of the second finger where the twist gesture started.
        /// </summary>
        public Vector2Control twistStartPosition2 { get; private set; }

        /// <summary>
        /// The delta rotation of the twist gesture.
        /// </summary>
        public AxisControl twistDeltaRotation { get; private set; }

        /// <summary>
        /// The screen position of the first finger where the two-finger drag gesture started.
        /// </summary>
        public Vector2Control twoFingerDragStartPosition1 { get; private set; }

        /// <summary>
        /// The screen position of the second finger where the two-finger drag gesture started.
        /// </summary>
        public Vector2Control twoFingerDragStartPosition2 { get; private set; }

        /// <summary>
        /// The current screen position of the two-finger drag gesture.
        /// </summary>
        public Vector2Control twoFingerDragCurrentPosition { get; private set; }

        /// <summary>
        /// The delta screen position of the two-finger drag gesture.
        /// </summary>
        public Vector2Control twoFingerDragDelta { get; private set; }

        /// <summary>
        /// The number of fingers on the touchscreen.
        /// </summary>
        public IntegerControl fingerCount { get; private set; }

        TapGestureRecognizer m_TapGestureRecognizer;
        DragGestureRecognizer m_DragGestureRecognizer;
        PinchGestureRecognizer m_PinchGestureRecognizer;
        TwistGestureRecognizer m_TwistGestureRecognizer;
        TwoFingerDragGestureRecognizer  m_TwoFingerDragGestureRecognizer;

        TouchscreenGestureInputControllerState m_ControllerState;

        /// <summary>
        /// Finishes setting up all the input values for the controller.
        /// </summary>
        protected override void FinishSetup()
        {
            base.FinishSetup();
            
            tapStartPosition = GetChildControl<Vector2Control>(nameof(tapStartPosition));
            dragStartPosition = GetChildControl<Vector2Control>(nameof(dragStartPosition));
            dragCurrentPosition = GetChildControl<Vector2Control>(nameof(dragCurrentPosition));
            dragDelta = GetChildControl<Vector2Control>(nameof(dragDelta));
            pinchStartPosition1 = GetChildControl<Vector2Control>(nameof(pinchStartPosition1));
            pinchStartPosition2 = GetChildControl<Vector2Control>(nameof(pinchStartPosition2));
            pinchGap = GetChildControl<AxisControl>(nameof(pinchGap));
            pinchGapDelta = GetChildControl<AxisControl>(nameof(pinchGapDelta));
            twistStartPosition1 = GetChildControl<Vector2Control>(nameof(twistStartPosition1));
            twistStartPosition2 = GetChildControl<Vector2Control>(nameof(twistStartPosition2));
            twistDeltaRotation = GetChildControl<AxisControl>(nameof(twistDeltaRotation));
            twoFingerDragStartPosition1 = GetChildControl<Vector2Control>(nameof(twoFingerDragStartPosition1));
            twoFingerDragStartPosition2 = GetChildControl<Vector2Control>(nameof(twoFingerDragStartPosition2));
            twoFingerDragCurrentPosition = GetChildControl<Vector2Control>(nameof(twoFingerDragCurrentPosition));
            twoFingerDragDelta = GetChildControl<Vector2Control>(nameof(twoFingerDragDelta));
            fingerCount = GetChildControl<IntegerControl>(nameof(fingerCount));
            
            m_TapGestureRecognizer = new TapGestureRecognizer();
            m_TapGestureRecognizer.onGestureStarted += OnGestureStarted;

            m_DragGestureRecognizer = new DragGestureRecognizer();
            m_DragGestureRecognizer.onGestureStarted += OnGestureStarted;

            m_PinchGestureRecognizer = new PinchGestureRecognizer();
            m_PinchGestureRecognizer.onGestureStarted += OnGestureStarted;

            m_TwistGestureRecognizer = new TwistGestureRecognizer();
            m_TwistGestureRecognizer.onGestureStarted += OnGestureStarted;

            m_TwoFingerDragGestureRecognizer = new TwoFingerDragGestureRecognizer();
            m_TwoFingerDragGestureRecognizer.onGestureStarted += OnGestureStarted;
        }

        /// <inheritdoc />
        protected override void OnAdded()
        {
            EnhancedTouchSupport.Enable();
        }

        /// <inheritdoc />
        protected override void OnRemoved()
        {
            EnhancedTouchSupport.Disable();
        }

        /// <inheritdoc />
        public void OnUpdate()
        {
            m_TapGestureRecognizer.Update();
            m_DragGestureRecognizer.Update();
            m_PinchGestureRecognizer.Update();
            m_TwistGestureRecognizer.Update();
            m_TwoFingerDragGestureRecognizer.Update();

            var updatedTouchCount = GestureTouchesUtility.touches.Count;
            if (m_ControllerState.fingerCount != updatedTouchCount)
            {
                m_ControllerState.fingerCount = updatedTouchCount;
                InputState.Change(this, m_ControllerState);
            }
        }

        void OnGestureStarted<T>(Gesture<T> gesture) where T : Gesture<T>
        {   
            gesture.onUpdated += OnGestureUpdated;
            gesture.onFinished += OnGestureFinished;
        }

        void OnGestureUpdated<T>(Gesture<T> gesture) where T : Gesture<T>
        {   
            switch (gesture)
            {
                case TapGesture tapGesture:
                    m_ControllerState.tapStartPosition = tapGesture.startPosition;
                    break;
                case DragGesture dragGesture:
                    m_ControllerState.dragStartPosition = dragGesture.startPosition;
                    m_ControllerState.dragCurrentPosition = dragGesture.position;
                    m_ControllerState.dragDelta = dragGesture.delta;
                    break;
                case PinchGesture pinchGesture:
                    m_ControllerState.pinchStartPosition1 =  pinchGesture.startPosition1;
                    m_ControllerState.pinchStartPosition2 =  pinchGesture.startPosition2;
                    m_ControllerState.pinchGap = pinchGesture.gap;
                    m_ControllerState.pinchGapDelta = pinchGesture.gapDelta;
                    break;
                case TwistGesture twistGesture:
                    m_ControllerState.twistStartPosition1 =  twistGesture.startPosition1;
                    m_ControllerState.twistStartPosition2 =  twistGesture.startPosition2;
                    m_ControllerState.twistDeltaRotation = twistGesture.deltaRotation;
                    break;
                case TwoFingerDragGesture twoFingerDragGesture:
                    m_ControllerState.twoFingerDragStartPosition1 = twoFingerDragGesture.startPosition1;
                    m_ControllerState.twoFingerDragStartPosition2 = twoFingerDragGesture.startPosition2;
                    m_ControllerState.twoFingerDragCurrentPosition = twoFingerDragGesture.position;
                    m_ControllerState.twoFingerDragDelta = twoFingerDragGesture.delta;
                    break;
            }

            InputState.Change(this, m_ControllerState);
        }

        void OnGestureFinished<T>(Gesture<T> gesture) where T : Gesture<T>
        {   
           switch (gesture)
            {
                 case TapGesture tapGesture:
                    m_ControllerState.tapStartPosition = Vector2.zero;
                    break;
                case DragGesture dragGesture:
                    m_ControllerState.dragStartPosition = Vector2.zero;
                    m_ControllerState.dragCurrentPosition = Vector2.zero;
                    m_ControllerState.dragDelta = Vector2.zero;
                    break;
                case PinchGesture pinchGesture:
                    m_ControllerState.pinchStartPosition1 =  Vector2.zero;
                    m_ControllerState.pinchStartPosition2 =  Vector2.zero;
                    m_ControllerState.pinchGap = 0f;
                    m_ControllerState.pinchGapDelta = 0f;
                    break;
                case TwistGesture twistGesture:
                    m_ControllerState.twistStartPosition1 =  Vector2.zero;
                    m_ControllerState.twistStartPosition2 =  Vector2.zero;
                    m_ControllerState.twistDeltaRotation = 0f;
                    break;
                case TwoFingerDragGesture twoFingerDragGesture:
                    m_ControllerState.twoFingerDragStartPosition1 = Vector2.zero;
                    m_ControllerState.twoFingerDragStartPosition2 = Vector2.zero;
                    m_ControllerState.twoFingerDragCurrentPosition = Vector2.zero;
                    m_ControllerState.twoFingerDragDelta = Vector2.zero;
                    break;
            }

            InputState.Change(this, m_ControllerState);
        }

        /// <summary>
        /// Converts state data to a string.
        /// </summary>
        /// <returns>Returns a string representation.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"tapStartPosition: {tapStartPosition.value}, dragStartPosition: {dragStartPosition.value}, dragCurrentPosition: {dragCurrentPosition.value}, dragDelta: {dragDelta.value}, ");
            stringBuilder.Append($"pinchStartPosition1: {pinchStartPosition1.value}, pinchStartPosition2: {pinchStartPosition2.value}, pinchGap: {pinchGap.value}, pinchGapDelta: {pinchGapDelta.value}, ");
            stringBuilder.Append($"twistStartPosition1: {twistStartPosition1.value}, twistStartPosition2: {twistStartPosition2.value}, twistDeltaRotation: {twistDeltaRotation.value}, ");
            stringBuilder.Append($"twoFingerDragStartPosition1: {twoFingerDragStartPosition1.value}, twoFingerDragStartPosition2: {twoFingerDragStartPosition2.value}, twoFingerDragCurrentPosition: {twoFingerDragCurrentPosition.value}, twoFingerDragDelta: {twoFingerDragDelta.value}");
            return stringBuilder.ToString();
        }
    }
}
#endif