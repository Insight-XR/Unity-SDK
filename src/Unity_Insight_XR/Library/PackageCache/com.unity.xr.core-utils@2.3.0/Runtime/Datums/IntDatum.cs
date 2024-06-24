using UnityEngine;

namespace Unity.XR.CoreUtils.Datums
{
    /// <summary>
    /// <see cref="ScriptableObject"/> container class that holds an int value.
    /// </summary>
    [CreateAssetMenu(fileName = "IntDatum", menuName = "XR/Value Datums/Int Datum", order = 0)]
    public class IntDatum : Datum<int>
    {
        /// <summary>
        /// Snap to nearest int.
        /// </summary>
        /// <param name="value">Value to round to nearest int value.</param>
        public void SetValueRounded(float value)
        {
            Value = Mathf.RoundToInt(value);
        }
    }
}
