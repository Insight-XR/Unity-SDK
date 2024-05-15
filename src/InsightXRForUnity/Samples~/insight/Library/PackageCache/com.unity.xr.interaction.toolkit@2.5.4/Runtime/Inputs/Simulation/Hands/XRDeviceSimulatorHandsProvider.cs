#if XR_HANDS_1_1_OR_NEWER
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.ProviderImplementation;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Hands
{
    /// <summary>
    /// Hand tracking provider for the XR Device Simulator
    /// </summary>
    class XRDeviceSimulatorHandsProvider : XRHandSubsystemProvider
    {
        class HandState
        {
            public Pose rootHandPose { get; set; } = Pose.identity;
            public HandExpressionName expressionName { get; set; } = HandExpressionName.Default;
            public bool isTracked { get; set; } = true;

            public bool needsToUpdateExpression { get; set; } = true;
            public bool needsToUpdatePose { get; set; } = true;
        }

        /// <summary>
        /// The string identifier used to name this subsystem provider.
        /// </summary>
        public static string id { get; }

        /// <summary>
        /// Whether current hand-tracking data is allowed to be updated and returned to the subsystem.
        /// Set to <see langword="true"/> to allow updates. The overall success flags still depends on the tracked state of each hand.
        /// Set to <see langword="false"/> to disallow updates and return <see cref="XRHandSubsystem.UpdateSuccessFlags.None"/> to the subsystem.
        /// </summary>
        /// <remarks>
        /// <see langword="true"/> is the expected value when hand tracking should be active (and controllers disconnected).
        /// <see langword="false"/> is the expected value when hand tracking should stop (and controllers are used instead).
        /// </remarks>
        /// <seealso cref="TryUpdateHands"/>
        public bool updateHandsAllowed { get; set; } = true;

        readonly HandState m_LeftHandState = new HandState();
        readonly HandState m_RightHandState = new HandState();

        readonly Dictionary<HandExpressionName, HandExpressionCapture> m_CapturedHandExpressions = new Dictionary<HandExpressionName, HandExpressionCapture>();
        readonly Dictionary<HandExpressionName, NativeArray<XRHandJoint>> m_CapturedLeftHandJointArrays = new Dictionary<HandExpressionName, NativeArray<XRHandJoint>>();
        readonly Dictionary<HandExpressionName, NativeArray<XRHandJoint>> m_CapturedRightHandJointArrays = new Dictionary<HandExpressionName, NativeArray<XRHandJoint>>();

        static XRDeviceSimulatorHandsProvider() => id = "XRI Device Simulator Hands Provider";

        /// <inheritdoc/>
        public override void Start()
        {
        }

        /// <inheritdoc/>
        public override void Stop()
        {
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            foreach (var jointArray in m_CapturedLeftHandJointArrays.Values)
            {
                if (jointArray.IsCreated)
                    jointArray.Dispose();
            }
            
            m_CapturedLeftHandJointArrays.Clear();
            
            foreach (var jointArray in m_CapturedRightHandJointArrays.Values)
            {
                if (jointArray.IsCreated)
                    jointArray.Dispose();
            }
            
            m_CapturedRightHandJointArrays.Clear();
        }

        /// <inheritdoc/>
        public override void GetHandLayout(NativeArray<bool> handJointsInLayout)
        {
            handJointsInLayout[XRHandJointID.Palm.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.Wrist.ToIndex()] = true;

            handJointsInLayout[XRHandJointID.ThumbMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.ThumbProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.ThumbDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.ThumbTip.ToIndex()] = true;

            handJointsInLayout[XRHandJointID.IndexMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.IndexProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.IndexIntermediate.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.IndexDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.IndexTip.ToIndex()] = true;

            handJointsInLayout[XRHandJointID.MiddleMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.MiddleProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.MiddleIntermediate.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.MiddleDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.MiddleTip.ToIndex()] = true;

            handJointsInLayout[XRHandJointID.RingMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.RingProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.RingIntermediate.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.RingDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.RingTip.ToIndex()] = true;

            handJointsInLayout[XRHandJointID.LittleMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.LittleProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.LittleIntermediate.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.LittleDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.LittleTip.ToIndex()] = true;
        }

        /// <inheritdoc/>
        public override XRHandSubsystem.UpdateSuccessFlags TryUpdateHands(XRHandSubsystem.UpdateType updateType,
            ref Pose leftHandRootPose, NativeArray<XRHandJoint> leftHandJoints,
            ref Pose rightHandRootPose, NativeArray<XRHandJoint> rightHandJoints)
        {
            if (!updateHandsAllowed)
                return XRHandSubsystem.UpdateSuccessFlags.None;

            if (m_LeftHandState.needsToUpdateExpression || m_LeftHandState.needsToUpdatePose)
                UpdateData(Handedness.Left, m_LeftHandState, m_CapturedLeftHandJointArrays, leftHandJoints, ref leftHandRootPose);

            if (m_RightHandState.needsToUpdateExpression || m_RightHandState.needsToUpdatePose)
                UpdateData(Handedness.Right, m_RightHandState, m_CapturedRightHandJointArrays, rightHandJoints, ref rightHandRootPose);

            var successFlags = XRHandSubsystem.UpdateSuccessFlags.None;
            if (m_LeftHandState.isTracked)
                successFlags |= XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose | XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints;

            if (m_RightHandState.isTracked)
                successFlags |= XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose | XRHandSubsystem.UpdateSuccessFlags.RightHandJoints;

            return successFlags;
        }

        void UpdateData(Handedness handedness, HandState handState, Dictionary<HandExpressionName, NativeArray<XRHandJoint>> cachedJointArrays, NativeArray<XRHandJoint> handJointArray, ref Pose rootPose)
        {
            // Check if joint array has already been created before
            if (!cachedJointArrays.TryGetValue(handState.expressionName, out var cachedArray))
            {
                cachedArray = new NativeArray<XRHandJoint>(handJointArray.Length, Allocator.Persistent);

                // If not, get serialized data and fill native array
                var capturedPoses = GetCapturedExpressionPoses(handedness, handState);
                if (capturedPoses != null)
                {
                    for (int jointIndex = XRHandJointID.BeginMarker.ToIndex(); jointIndex < XRHandJointID.EndMarker.ToIndex(); ++jointIndex)
                    {
                        cachedArray[jointIndex] = XRHandProviderUtility.CreateJoint(
                            handedness,
                            XRHandJointTrackingState.Pose,
                            XRHandJointIDUtility.FromIndex(jointIndex),
                            capturedPoses[jointIndex]);
                    }
                }

                // Cache native array for re-use later
                cachedJointArrays[handState.expressionName] = cachedArray;
            }

            cachedArray.CopyTo(handJointArray);
            handState.needsToUpdateExpression = false;

            if (handState.needsToUpdatePose && handJointArray[XRHandJointID.Wrist.ToIndex()].TryGetPose(out var wristPose))
            {
                rootPose = handState.rootHandPose;
                var offsetRotation = rootPose.rotation * Quaternion.Inverse(wristPose.rotation);
                for (int jointIndex = 0; jointIndex < handJointArray.Length; ++jointIndex)
                {
                    if (handJointArray[jointIndex].TryGetPose(out var pose))
                    {
                        pose.position = offsetRotation * (pose.position - wristPose.position) + rootPose.position;
                        pose.rotation = offsetRotation * pose.rotation;

                        handJointArray[jointIndex] = XRHandProviderUtility.CreateJoint(
                            handedness,
                            XRHandJointTrackingState.Pose,
                            XRHandJointIDUtility.FromIndex(jointIndex),
                            pose);
                    }
                }

                handState.needsToUpdatePose = false;
            }
        }

        /// <summary>
        /// Assign a captured expression for a specific expression type.
        /// When a capture is added, this provider is added as a reference to the native array data that will be allocated if needed.
        /// </summary>
        /// <param name="expressionName">The expression that the data represents.</param>
        /// <param name="capture">The captured data for the expression.</param>
        public void SetCapturedExpression(HandExpressionName expressionName, HandExpressionCapture capture)
        {
            if (m_CapturedHandExpressions.ContainsKey(expressionName))
            {
                Debug.LogWarning($"Hand Expression {expressionName} has already been added to the simulated hands provider. The new capture will replace the previous one.");

                // Clear the cached joint arrays since there's a new capture with new poses
                if (m_CapturedLeftHandJointArrays.TryGetValue(expressionName, out var cachedArray))
                {
                    if (cachedArray.IsCreated)
                        cachedArray.Dispose();

                    m_CapturedLeftHandJointArrays.Remove(expressionName);
                }

                if (m_CapturedRightHandJointArrays.TryGetValue(expressionName, out cachedArray))
                {
                    if (cachedArray.IsCreated)
                        cachedArray.Dispose();

                    m_CapturedRightHandJointArrays.Remove(expressionName);
                }
            }
            
            m_CapturedHandExpressions[expressionName] = capture;
        }
        
        Pose[] GetCapturedExpressionPoses(Handedness handedness, HandState handState)
        {
            if (handedness == Handedness.Invalid)
                return null;

            if (m_CapturedHandExpressions.TryGetValue(handState.expressionName, out var capturedExpression))
            {
                return handedness == Handedness.Left
                    ? capturedExpression.leftHandCapturedPoses
                    : capturedExpression.rightHandCapturedPoses;
            }
            
            throw new InvalidOperationException($"Unrecognized Hand Expression: {handState.expressionName}. A expression must be added to the simulated hands provider before it can be used.");
        }

        public void SetIsTracked(Handedness handedness, bool isTracked)
        {
            if (handedness == Handedness.Invalid)
                return;

            var handState = handedness == Handedness.Left ? m_LeftHandState : m_RightHandState;
            handState.isTracked = isTracked;
        }

        public void SetRootHandPose(Handedness handedness, Pose newPose)
        {
            if (handedness == Handedness.Invalid)
                return;

            var handState = handedness == Handedness.Left ? m_LeftHandState : m_RightHandState;
            if (handState.rootHandPose != newPose)
            {
                handState.rootHandPose = newPose;
                handState.needsToUpdatePose = true;
            }
        }

        public void SetHandExpression(Handedness handedness, HandExpressionName expressionName)
        {
            if (handedness == Handedness.Invalid)
                return;

            var handState = handedness == Handedness.Left ? m_LeftHandState : m_RightHandState;
            if (handState.expressionName != expressionName)
            {
                handState.expressionName = expressionName;
                handState.needsToUpdateExpression = true;
                handState.needsToUpdatePose = true;
            }
        }
         
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Register()
        {
            var handsSubsystemCinfo = new XRHandSubsystemDescriptor.Cinfo
            {
                id = id,
                providerType = typeof(XRDeviceSimulatorHandsProvider),
                subsystemTypeOverride = typeof(XRDeviceSimulatorHandsSubsystem),
            };
            XRHandSubsystemDescriptor.Register(handsSubsystemCinfo);
        }
    }
}
#endif