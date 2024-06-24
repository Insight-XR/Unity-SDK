using System;
using UnityEngine;

namespace Unity.XR.CoreUtils.Datums
{
    /// <summary>
    /// Serializable container class that holds an animation curve value or container asset reference.
    /// </summary>
    /// <seealso cref="AnimationCurveDatum"/>
    [Serializable]
    public class AnimationCurveDatumProperty : DatumProperty<AnimationCurve, AnimationCurveDatum>
    {
        /// <summary>
        /// Constructor setting initial value for the embedded constant.
        /// </summary>
        /// <param name="value">Initial value.</param>
        public AnimationCurveDatumProperty(AnimationCurve value) : base(value) { }

        /// <summary>
        /// Constructor setting initial datum asset reference.
        /// </summary>
        /// <param name="datum">Datum asset reference.</param>
        public AnimationCurveDatumProperty(AnimationCurveDatum datum) : base(datum) { }
    }
}
