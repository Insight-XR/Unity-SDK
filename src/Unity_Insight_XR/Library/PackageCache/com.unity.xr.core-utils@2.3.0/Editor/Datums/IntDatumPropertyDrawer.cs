using UnityEditor;

namespace Unity.XR.CoreUtils.Datums.Editor
{
    /// <summary>
    /// Variable reference drawer used to represent an int reference
    /// and draw an <see cref="IntDatumProperty"/>.
    /// </summary>
    /// <seealso cref="IntDatumProperty"/>
    /// <seealso cref="DatumPropertyDrawer"/>
    [CustomPropertyDrawer(typeof(IntDatumProperty))]
    public class IntDatumPropertyDrawer : DatumPropertyDrawer
    {
    }
}
