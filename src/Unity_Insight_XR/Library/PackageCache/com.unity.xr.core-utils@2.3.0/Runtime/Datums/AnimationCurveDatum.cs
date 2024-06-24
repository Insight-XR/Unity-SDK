using UnityEngine;

namespace Unity.XR.CoreUtils.Datums
{
    /// <summary>
    /// <see cref="ScriptableObject"/> container class that holds an animation curve.
    /// </summary>
    [CreateAssetMenu(fileName = "AnimationCurveDatum", menuName = "XR/Value Datums/AnimationCurve Datum", order = 0)]
    public class AnimationCurveDatum : Datum<AnimationCurve>
    {
    }
}
