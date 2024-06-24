using System;
using Unity.XR.CoreUtils.Datums;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives
{
    /// <summary>
    /// Affordance state theme data structure for for float affordances. 
    /// </summary>
    [Serializable]
    public class FloatAffordanceTheme : BaseAffordanceTheme<float>
    {
    }

    /// <summary>
    /// Serializable container class that holds a float affordance theme value or container asset reference.
    /// </summary>
    /// <seealso cref="FloatAffordanceThemeDatum"/>
    [Serializable]
    public class FloatAffordanceThemeDatumProperty : DatumProperty<FloatAffordanceTheme, FloatAffordanceThemeDatum>
    {
        /// <inheritdoc/>
        public FloatAffordanceThemeDatumProperty(FloatAffordanceTheme value) : base(value)
        {
        }

        /// <inheritdoc/>
        public FloatAffordanceThemeDatumProperty(FloatAffordanceThemeDatum datum) : base(datum)
        {
        }
    }

    /// <summary>
    /// <see cref="ScriptableObject"/> container class that holds a float affordance theme value.
    /// </summary>
    [CreateAssetMenu(fileName = "FloatAffordanceTheme", menuName = "Affordance Theme/Float Affordance Theme", order = 0)]
    [HelpURL(XRHelpURLConstants.k_FloatAffordanceThemeDatum)]
    public class FloatAffordanceThemeDatum : Datum<FloatAffordanceTheme>
    {
    }
}