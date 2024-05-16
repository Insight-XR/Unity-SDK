using System;

namespace Unity.XR.CoreUtils.Datums
{
    /// <summary>
    /// Serializable container class that holds an int value or container asset reference.
    /// </summary>
    /// <seealso cref="IntDatum"/>
    [Serializable]
    public class IntDatumProperty : DatumProperty<int, IntDatum>
    {
        /// <summary>
        /// Constructor setting initial integer value for the embedded constant.
        /// </summary>
        /// <param name="value">Initial integer value.</param>
        public IntDatumProperty(int value) : base(value)
        {
        }

        /// <summary>
        /// Constructor setting initial datum asset reference.
        /// </summary>
        /// <param name="datum">Datum asset reference.</param>
        public IntDatumProperty(IntDatum datum) : base(datum)
        {
        }
    }
}
