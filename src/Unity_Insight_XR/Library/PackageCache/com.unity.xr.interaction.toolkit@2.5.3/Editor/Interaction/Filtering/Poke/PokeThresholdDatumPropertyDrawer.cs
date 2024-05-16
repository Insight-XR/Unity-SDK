using Unity.XR.CoreUtils.Datums.Editor;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace UnityEditor.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Property drawer for the serializable container class that holds a poke threshold value or container asset reference.
    /// </summary>
    /// <seealso cref="PokeThresholdDatumProperty"/>
    /// <seealso cref="DatumPropertyDrawer"/>
    /// <summary>
    /// Class used to draw a <see cref="PokeThresholdDatumProperty"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(PokeThresholdDatumProperty))]
    public class PokeThresholdDatumPropertyDrawer : DatumPropertyDrawer
    {
    }
}
