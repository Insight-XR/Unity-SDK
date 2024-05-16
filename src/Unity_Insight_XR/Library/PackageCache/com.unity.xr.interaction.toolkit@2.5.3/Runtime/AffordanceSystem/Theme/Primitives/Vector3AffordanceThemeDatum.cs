using System;
using Unity.Mathematics;
using Unity.XR.CoreUtils.Datums;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives
{
    /// <summary>
    /// Affordance state theme data structure for for Vector3 affordances. 
    /// </summary>
    [Serializable]
    public class Vector3AffordanceTheme : BaseAffordanceTheme<float3>
    {
    }

    /// <summary>
    /// Serializable container class that holds a Vector3 affordance theme value or container asset reference.
    /// </summary>
    /// <seealso cref="Vector3AffordanceThemeDatum"/>
    [Serializable]
    public class Vector3AffordanceThemeDatumProperty : DatumProperty<Vector3AffordanceTheme, Vector3AffordanceThemeDatum>
    {
        /// <inheritdoc/>
        public Vector3AffordanceThemeDatumProperty(Vector3AffordanceTheme value) : base(value)
        {
        }

        /// <inheritdoc/>
        public Vector3AffordanceThemeDatumProperty(Vector3AffordanceThemeDatum datum) : base(datum)
        {
        }
    }

    /// <summary>
    /// <see cref="ScriptableObject"/> container class that holds a Vector3 affordance theme value.
    /// </summary>
    [CreateAssetMenu(fileName = "Vector3AffordanceTheme", menuName = "Affordance Theme/Vector3 Affordance Theme", order = 0)]
    [HelpURL(XRHelpURLConstants.k_Vector3AffordanceThemeDatum)]
    public class Vector3AffordanceThemeDatum : Datum<Vector3AffordanceTheme>
    {
    }
}