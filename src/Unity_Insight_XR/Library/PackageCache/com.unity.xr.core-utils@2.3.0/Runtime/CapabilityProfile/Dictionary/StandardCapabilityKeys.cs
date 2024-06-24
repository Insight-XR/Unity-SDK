namespace Unity.XR.CoreUtils.Capabilities
{
    /// <summary>
    /// Class that stores standard capability keys for packages to use.
    /// Be it support for face tracking, body tracking, controllers input, etc.
    /// </summary>
    public static class StandardCapabilityKeys
    {
        /// <summary>
        /// The controller input capability key.
        /// </summary>
        public const string ControllersInput = "Controllers Input";

        /// <summary>
        /// The hands input capability key.
        /// </summary>
        public const string HandsInput = "Hands Input";

        /// <summary>
        /// The eye gaze input capability key.
        /// </summary>
        public const string EyeGazeInput = "Eye Gaze Input";

        /// <summary>
        /// The world data input capability key.
        /// </summary>
        public const string WorldDataInput = "World Data Input";
        
        /// <summary>
        /// Face tracking capability key.
        /// </summary>
        public const string FaceTracking = "Face Tracking";
    }
}
