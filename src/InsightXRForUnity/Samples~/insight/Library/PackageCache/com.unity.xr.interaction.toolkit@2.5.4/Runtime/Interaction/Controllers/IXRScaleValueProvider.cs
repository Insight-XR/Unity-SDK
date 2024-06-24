namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Enum representing the two modes of scaling: <see cref="Input"/> and <see cref="Distance"/>.
    /// </summary>
    /// <seealso cref="IXRScaleValueProvider.scaleMode"/>
    public enum ScaleMode
    {
        /// <summary>
        /// No scale mode is active or supported. 
        /// Use this when a controller does not support scaling or when scaling is not needed.
        /// </summary>
        None,
        
        /// <summary>
        /// Input scale mode: The scale is represented by a range of -1 to 1. 
        /// This mode is typically used with <seealso cref="ActionBasedController"/>, 
        /// where the value is based on the scale toggle and scale delta input actions.
        /// </summary>
        Input,
    
        /// <summary>
        /// Distance scale mode: The scale is based on the distance between 2 physical (or virtual) inputs, such as
        /// the pinch gap between fingers where the distance is calculated based on the screen DPI, and delta from the previous frame.
        /// This mode is typically used with <seealso cref="XRScreenSpaceController"/>.
        /// </summary>
        Distance,
    }

    /// <summary>
    /// Defines an interface for scale value providers.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface provide a mechanism to get a scale value (a change in scale) 
    /// from an input control, such as a gesture or controller stick movement. The provided scale value is in the 
    /// mode supported by the upstream controller.
    /// </remarks>
    /// <seealso cref="XRRayInteractor"/>
    /// <seealso cref="XRScreenSpaceController"/>
    public interface IXRScaleValueProvider
    {
        /// <summary>
        /// Property representing the scale mode that is supported by the implementation of the interface.
        /// </summary>
        /// <seealso cref="ScaleMode"/>
        ScaleMode scaleMode { get; set; }

        /// <summary>
        /// This is the current scale value for the specified scale mode. This value should be updated
        /// by the implementing class when other inputs are handled during the standard interaction processing loop.
        /// </summary>
        /// <seealso cref="scaleMode"/>
        float scaleValue { get; }
    }
}