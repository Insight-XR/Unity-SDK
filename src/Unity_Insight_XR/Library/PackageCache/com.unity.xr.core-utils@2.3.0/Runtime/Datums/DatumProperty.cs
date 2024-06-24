using System;
using UnityEngine;

namespace Unity.XR.CoreUtils.Datums
{
    /// <summary>
    /// Class used as a serialized field in a component for containing a typed value that is either
    /// directly serialized as a constant value or a reference to a <see cref="ScriptableObject"/> container for that data.
    /// </summary>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <typeparam name="TDatum">Datum asset type.</typeparam>
    /// <seealso cref="Datum{T}"/>
    [Serializable]
    public abstract class DatumProperty<TValue, TDatum> where TDatum : Datum<TValue>
    {
        /// <summary>
        /// Signifies whether the property is a constant value or a datum asset reference.
        /// </summary>
        [SerializeField]
        bool m_UseConstant;

        /// <summary>
        /// The constant value used if <c>m_UseConstant</c> is flagged as true.
        /// </summary>
        [SerializeField]
        TValue m_ConstantValue;

        /// <summary>
        /// The datum asset reference used if <c>m_UseConstant</c> is flagged as false.
        /// </summary>
        [SerializeField]
        TDatum m_Variable;

        /// <summary>
        /// Default constructor which creates an empty datum asset reference.
        /// </summary>
        protected DatumProperty()
        {
            m_UseConstant = false;
        }

        /// <summary>
        /// Constructor setting initial value for the embedded constant.
        /// </summary>
        /// <param name="value">Initial value.</param>
        protected DatumProperty(TValue value)
        {
            m_UseConstant = true;
            m_ConstantValue = value;
        }

        /// <summary>
        /// Constructor setting initial datum asset reference.
        /// </summary>
        /// <param name="datum">Datum asset reference.</param>
        protected DatumProperty(TDatum datum)
        {
            m_UseConstant = false;
            m_Variable = datum;
        }

        /// <summary>
        /// Accessor for internal value held by this container.
        /// Getter/Setter uses the constant value if this property is set to "Use Value"
        /// and the associated datum's value is referenced if this property is set to "Use Asset".
        /// </summary>
        public TValue Value
        {
            get => m_UseConstant ? m_ConstantValue : Datum != null ? Datum.Value : default;
            set
            {
                if (m_UseConstant)
                {
                    m_ConstantValue = value;
                }
                else
                {
                    Datum.Value = value;
                }
            }
        }

        /// <summary>
        /// The current datum asset reference used when this property is set to "Use Asset".
        /// </summary>
        protected Datum<TValue> Datum => m_Variable;

        /// <summary>
        /// The current constant value used when this property is set to "Use Value".
        /// </summary>
        protected TValue ConstantValue => m_ConstantValue;

        /// <summary>
        /// Operator making it easy to treat the container property as the underlying internal value.
        /// </summary>
        /// <param name="datumProperty">Property to get the internal value of.</param>
        /// <returns>Returns internal value represented by the property.</returns>
        public static implicit operator TValue(DatumProperty<TValue, TDatum> datumProperty)
        {
            return datumProperty.Value;
        }
    }
}
