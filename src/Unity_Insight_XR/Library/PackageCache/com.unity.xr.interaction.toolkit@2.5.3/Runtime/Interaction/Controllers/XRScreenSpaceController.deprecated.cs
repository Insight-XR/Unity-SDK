using System;
using UnityEngine.InputSystem;

namespace UnityEngine.XR.Interaction.Toolkit
{
    public partial class XRScreenSpaceController
    {
#pragma warning disable 618
        /// <summary>
        /// (Deprecated) pinchStartPosition has been deprecated. Use pinchStartPositionAction instead.
        /// </summary>
        [Obsolete("pinchStartPosition has been deprecated. Use pinchStartPositionAction instead. (UnityUpgradable) -> pinchStartPositionAction")]
        public InputActionProperty pinchStartPosition
        {
            get => pinchStartPositionAction;
            set => pinchStartPositionAction = value;
        }

        /// <summary>
        /// (Deprecated) pinchGapDelta has been deprecated. Use pinchGapDeltaAction instead.
        /// </summary>
        [Obsolete("pinchGapDelta has been deprecated. Use pinchGapDeltaAction instead. (UnityUpgradable) -> pinchGapDeltaAction")]
        public InputActionProperty pinchGapDelta
        {
            get => pinchGapDeltaAction;
            set => pinchGapDeltaAction = value;
        }

        /// <summary>
        /// (Deprecated) twistStartPosition has been deprecated. Use twistStartPositionAction instead.
        /// </summary>
        [Obsolete("twistStartPosition has been deprecated. Use twistStartPositionAction instead. (UnityUpgradable) -> twistStartPositionAction")]
        public InputActionProperty twistStartPosition
        {
            get => twistStartPositionAction;
            set => twistStartPositionAction = value;
        }

        /// <summary>
        /// (Deprecated) twistRotationDeltaAction has been deprecated. Use twistDeltaRotationAction instead.
        /// </summary>
        [Obsolete("twistRotationDeltaAction has been deprecated. Use twistDeltaRotationAction instead. (UnityUpgradable) -> twistDeltaRotationAction")]
        public InputActionProperty twistRotationDeltaAction
        {
            get => twistDeltaRotationAction;
            set => twistDeltaRotationAction = value;
        }

        /// <summary>
        /// (Deprecated) screenTouchCount has been deprecated. Use screenTouchCountAction instead.
        /// </summary>
        [Obsolete("screenTouchCount has been deprecated. Use screenTouchCountAction instead. (UnityUpgradable) -> screenTouchCountAction")]
        public InputActionProperty screenTouchCount
        {
            get => screenTouchCountAction;
            set => screenTouchCountAction = value;
        }
#pragma warning restore 618
    }
}
