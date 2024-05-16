using UnityEngine;

namespace Unity.XR.CoreUtils.Datums
{
    /// <summary>
    /// <see cref="ScriptableObject"/> container class that holds a float value.
    /// </summary>
    [CreateAssetMenu(fileName = "FloatDatum", menuName = "XR/Value Datums/Float Datum", order = 0)]
    public class FloatDatum : Datum<float>
    {
    }
}
