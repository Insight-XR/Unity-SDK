using Unity.XR.CoreUtils.Datums;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// <see cref="ScriptableObject"/> container class that holds a <see cref="PokeThresholdData"/> value.
    /// </summary>
    [CreateAssetMenu(fileName = "PokeThresholdDatum", menuName = "XR/Value Datums/Poke Threshold Datum", order = 0)]
    [HelpURL(XRHelpURLConstants.k_PokeThresholdDatum)]
    public class PokeThresholdDatum : Datum<PokeThresholdData>
    {

    }
}