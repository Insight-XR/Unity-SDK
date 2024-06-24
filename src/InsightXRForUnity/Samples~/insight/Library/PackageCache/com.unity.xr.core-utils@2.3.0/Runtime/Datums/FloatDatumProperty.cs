using System;

namespace Unity.XR.CoreUtils.Datums
{
    /// <summary>
    /// Serializable container class that holds a float value or container asset reference.
    /// </summary>
    /// <seealso cref="FloatDatum"/>
    [Serializable]
    public class FloatDatumProperty : DatumProperty<float, FloatDatum>
    {
        /// <summary>
        /// Constructor setting initial float value for the embedded constant.
        /// </summary>
        /// <param name="value">Initial float value.</param>
        public FloatDatumProperty(float value) : base(value)
        {
        }

        /// <summary>
        /// Constructor setting initial datum asset reference.
        /// </summary>
        /// <param name="datum">Datum asset reference.</param>
        public FloatDatumProperty(FloatDatum datum) : base(datum)
        {
        }
    }
}
