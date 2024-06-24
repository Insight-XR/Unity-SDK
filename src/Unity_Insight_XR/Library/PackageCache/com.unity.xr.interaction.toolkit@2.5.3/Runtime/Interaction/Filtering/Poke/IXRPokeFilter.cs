namespace UnityEngine.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Instances that implement this interface are called XR Poke filters. Poke filters are used to
    /// customize basic poke functionality and to define constraints for when the interactable will be selected.
    /// </summary>
    public interface IXRPokeFilter : IXRSelectFilter, IXRInteractionStrengthFilter
    {
    }
}
