using UnityEditor;

namespace Unity.XR.CoreUtils.Datums.Editor
{
    /// <summary>
    /// Variable reference drawer used to represent string references
    /// and draw an <see cref="StringDatumProperty"/>.
    /// </summary>
    /// <seealso cref="DatumPropertyDrawer"/>
    [CustomPropertyDrawer(typeof(StringDatumProperty))]
    public class StringDatumPropertyDrawer : DatumPropertyDrawer
    {
    }
}
