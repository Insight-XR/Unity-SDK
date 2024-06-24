using UnityEditor;

namespace Unity.XR.CoreUtils.Datums.Editor
{
    /// <summary>
    /// Variable reference drawer used to represent a float reference
    /// and draw a <see cref="FloatDatumProperty"/>.
    /// </summary>
    /// <seealso cref="FloatDatumProperty"/>
    /// <seealso cref="DatumPropertyDrawer"/>
    [CustomPropertyDrawer(typeof(FloatDatumProperty))]
    public class FloatDatumPropertyDrawer : DatumPropertyDrawer
    {
    }
}
