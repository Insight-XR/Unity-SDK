using System;
using Unity.Mathematics;
using Unity.XR.CoreUtils.Datums;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives
{
    /// <summary>
    /// Affordance state theme data structure for for Vector4 affordances. 
    /// </summary>
    [Serializable]
    public class Vector4AffordanceTheme : BaseAffordanceTheme<float4>
    {
    }

    /// <summary>
    /// Serializable container class that holds a Vector4 affordance theme value or container asset reference.
    /// </summary>
    /// <seealso cref="Vector4AffordanceThemeDatum"/>
    [Serializable]
    public class Vector4AffordanceThemeDatumProperty : DatumProperty<Vector4AffordanceTheme, Vector4AffordanceThemeDatum>
    {
        /// <inheritdoc/>
        public Vector4AffordanceThemeDatumProperty(Vector4AffordanceTheme value) : base(value)
        {
        }

        /// <inheritdoc/>
        public Vector4AffordanceThemeDatumProperty(Vector4AffordanceThemeDatum datum) : base(datum)
        {
        }
    }

    /// <summary>
    /// <see cref="ScriptableObject"/> container class that holds a Vector4 affordance theme value.
    /// </summary>
    [CreateAssetMenu(fileName = "Vector4AffordanceTheme", menuName = "Affordance Theme/Vector4 Affordance Theme", order = 0)]
    [HelpURL(XRHelpURLConstants.k_Vector4AffordanceThemeDatum)]
    public class Vector4AffordanceThemeDatum : Datum<Vector4AffordanceTheme>
    {
    }
}