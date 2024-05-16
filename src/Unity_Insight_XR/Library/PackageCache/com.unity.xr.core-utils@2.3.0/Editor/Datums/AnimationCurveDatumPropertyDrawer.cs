using UnityEditor;

namespace Unity.XR.CoreUtils.Datums.Editor
{
    /// <summary>
    /// Variable reference drawer used to represent an Animation Curve reference
    /// and draw an <see cref="AnimationCurveDatumProperty"/>.
    /// </summary>
    /// <seealso cref="AnimationCurveDatumProperty"/>
    /// <seealso cref="DatumPropertyDrawer"/>
    [CustomPropertyDrawer(typeof(AnimationCurveDatumProperty))]
    public class AnimationCurveDatumPropertyDrawer : DatumPropertyDrawer
    {
    }
}
