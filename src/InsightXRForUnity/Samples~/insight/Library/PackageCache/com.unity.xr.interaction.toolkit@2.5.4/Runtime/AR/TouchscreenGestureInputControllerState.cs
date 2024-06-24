#if AR_FOUNDATION_PRESENT || PACKAGE_DOCS_GENERATION
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.AR.Inputs
{
    /// <summary>
    /// State for input device representing touchscreen gestures.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 116)]
    public struct TouchscreenGestureInputControllerState : IInputStateTypeInfo
    {
        /// <summary>
        /// Memory format identifier for <see cref="TouchscreenGestureInputControllerState"/>.
        /// </summary>
        /// <seealso cref="InputStateBlock.format"/>
        public static FourCC formatId => new FourCC('T', 'S', 'G', 'C');

        /// <summary>
        /// See <a href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/api/UnityEngine.InputSystem.LowLevel.IInputStateTypeInfo.html">IInputStateTypeInfo</a>.format.
        /// </summary>
        public FourCC format => formatId;

        /// <summary>
        /// The screen position where the tap gesture started.
        /// </summary>
        [InputControl(usage = "tapStartPosition", offset = 0)]
        [FieldOffset(0)]
        public Vector2 tapStartPosition;

        /// <summary>
        /// The screen position where the drag gesture started.
        /// </summary>
        [InputControl(usage = "dragStartPosition", offset = 8)]
        [FieldOffset(8)]
        public Vector2 dragStartPosition;

        /// <summary>
        /// The current screen position of the drag gesture.
        /// </summary>
        [InputControl(usage = "dragCurrentPosition", offset = 16)]
        [FieldOffset(16)]
        public Vector2 dragCurrentPosition;

        /// <summary>
        /// The delta screen position of the drag gesture.
        /// </summary>
        [InputControl(usage = "dragDelta", offset = 24)]
        [FieldOffset(24)]
        public Vector2 dragDelta;

        /// <summary>
        /// The screen position of the first finger where the pinch gesture started.
        /// </summary>
        [InputControl(usage = "pinchStartPosition1", offset = 32)]
        [FieldOffset(32)]
        public Vector2 pinchStartPosition1;

        /// <summary>
        /// The screen position of the second finger where the pinch gesture started.
        /// </summary>
        [InputControl(usage = "pinchStartPosition2", offset = 40)]
        [FieldOffset(40)]
        public Vector2 pinchStartPosition2;

        /// <summary>
        /// The gap between then position of the first and second fingers for the pinch gesture.
        /// </summary>
        [InputControl(usage = "pinchGap", offset = 48, layout = "Axis")]
        [FieldOffset(48)]
        public float pinchGap;

        /// <summary>
        /// The gap delta between then position of the first and second fingers for the pinch gesture.
        /// </summary>
        [InputControl(usage = "pinchGapDelta", offset = 52, layout = "Axis")]
        [FieldOffset(52)]
        public float pinchGapDelta;

        /// <summary>
        /// The screen position of the first finger where the twist gesture started.
        /// </summary>
        [InputControl(usage = "twistStartPosition1", offset = 56)]
        [FieldOffset(56)]
        public Vector2 twistStartPosition1;

        /// <summary>
        /// The screen position of the second finger where the twist gesture started.
        /// </summary>
        [InputControl(usage = "twistStartPosition2", offset = 64)]
        [FieldOffset(64)]
        public Vector2 twistStartPosition2;

        /// <summary>
        /// The delta rotation of the twist gesture.
        /// </summary>
        [InputControl(usage = "twistDeltaRotation", offset = 72, layout = "Axis")]
        [FieldOffset(72)]
        public float twistDeltaRotation;

        /// <summary>
        /// The screen position of the first finger where the two-finger drag gesture started.
        /// </summary>
        [InputControl(usage = "twoFingerDragStartPosition1", offset = 76)]
        [FieldOffset(76)]
        public Vector2 twoFingerDragStartPosition1;

        /// <summary>
        /// The screen position of the second finger where the two-finger drag gesture started.
        /// </summary>
        [InputControl(usage = "twoFingerDragStartPosition2", offset = 84)]
        [FieldOffset(84)]
        public Vector2 twoFingerDragStartPosition2;

        /// <summary>
        /// The current screen position of the two-finger drag gesture.
        /// </summary>
        [InputControl(usage = "twoFingerDragCurrentPosition", offset = 92)]
        [FieldOffset(92)]
        public Vector2 twoFingerDragCurrentPosition;

        /// <summary>
        /// The delta screen position of the two-finger drag gesture.
        /// </summary>
        [InputControl(usage = "twoFingerDragDelta", offset = 100)]
        [FieldOffset(100)]
        public Vector2 twoFingerDragDelta;
        
        /// <summary>
        /// The number of fingers on the touchscreen.
        /// </summary>
        [InputControl(usage = "fingerCount", offset = 108, layout = "Integer")]
        [FieldOffset(108)]
        public int fingerCount;
    }
}
#endif