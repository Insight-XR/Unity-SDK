#if XR_HANDS_1_1_OR_NEWER
using UnityEngine.XR.Hands;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Hands
{
    class XRDeviceSimulatorHandsSubsystem : XRHandSubsystem
    {
        XRDeviceSimulatorHandsProvider handsProvider => provider as XRDeviceSimulatorHandsProvider;

        internal void SetCapturedExpression(HandExpressionName expressionName, HandExpressionCapture capture)
        {
            handsProvider.SetCapturedExpression(expressionName, capture);
        }

        internal void SetHandExpression(Handedness handedness, HandExpressionName expressionName)
        {
            handsProvider.SetHandExpression(handedness, expressionName);
        }

        internal void SetRootHandPose(Handedness handedness, Pose pose)
        {
            handsProvider.SetRootHandPose(handedness, pose);
        }

        internal void SetUpdateHandsAllowed(bool allowed)
        {
            handsProvider.updateHandsAllowed = allowed;
        }

        internal void SetIsTracked(Handedness handedness, bool isTracked)
        {
            handsProvider.SetIsTracked(handedness, isTracked);
        }
    }
}
#endif