using System;
using Unity.Mathematics;
using Unity.XR.CoreUtils.Datums;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives
{
    /// <summary>
    /// Affordance state theme data structure for for Vector2 affordances. 
    /// </summary>
    [Serializable]
    public class Vector2AffordanceTheme : BaseAffordanceTheme<float2>
    {
    }

    /// <summary>
    /// Serializable container class that holds a Vector2 affordance theme value or container asset reference.
    /// </summary>
    /// <seealso cref="Vector2AffordanceThemeDatum"/>
    [Serializable]
    public class Vector2AffordanceThemeDatumProperty : DatumProperty<Vector2AffordanceTheme, Vector2AffordanceThemeDatum>
    {
        /// <inheritdoc/>
        public Vector2AffordanceThemeDatumProperty(Vector2AffordanceTheme value) : base(value)
        {
        }

        /// <inheritdoc/>
        public Vector2AffordanceThemeDatumProperty(Vector2AffordanceThemeDatum datum) : base(datum)
        {
        }
    }

    /// <summary>
    /// <see cref="ScriptableObject"/> container class that holds a Vector2 affordance theme value.
    /// </summary>
    [CreateAssetMenu(fileName = "Vector2AffordanceTheme", menuName = "Affordance Theme/Vector2 Affordance Theme", order = 0)]
    [HelpURL(XRHelpURLConstants.k_Vector2AffordanceThemeDatum)]
    public class Vector2AffordanceThemeDatum : Datum<Vector2AffordanceTheme>
    {
    }
}