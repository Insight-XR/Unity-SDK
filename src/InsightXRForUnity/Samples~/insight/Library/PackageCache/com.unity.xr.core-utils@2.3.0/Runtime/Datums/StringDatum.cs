using UnityEngine;

namespace Unity.XR.CoreUtils.Datums
{
    /// <summary>
    /// <see cref="ScriptableObject"/> container class that holds a string value.
    /// </summary>
    [CreateAssetMenu(fileName = "StringDatum", menuName = "XR/Value Datums/String Datum", order = 0)]
    public class StringDatum : Datum<string>
    {
    }
}
