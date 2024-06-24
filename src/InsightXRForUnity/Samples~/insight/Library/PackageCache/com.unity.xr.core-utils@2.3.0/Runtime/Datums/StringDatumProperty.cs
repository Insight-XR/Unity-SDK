using System;

namespace Unity.XR.CoreUtils.Datums
{
    /// <summary>
    /// Serializable container class that holds a string value or container asset reference.
    /// </summary>
    /// <seealso cref="StringDatum"/>
    [Serializable]
    public class StringDatumProperty : DatumProperty<string, StringDatum>
    {
        /// <summary>
        /// Constructor setting initial string value for the embedded constant.
        /// </summary>
        /// <param name="value">Initial string value.</param>
        public StringDatumProperty(string value) : base(value)
        {
        }

        /// <summary>
        /// Constructor setting initial datum asset reference.
        /// </summary>
        /// <param name="datum">Datum asset reference.</param>
        public StringDatumProperty(StringDatum datum) : base(datum)
        {
        }
    }
}
