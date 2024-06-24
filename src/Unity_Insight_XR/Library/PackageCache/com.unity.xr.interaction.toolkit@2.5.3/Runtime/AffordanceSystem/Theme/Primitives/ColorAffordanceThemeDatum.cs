using System;
using Unity.XR.CoreUtils.Datums;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives
{
    /// <summary>
    /// Blend mode used by the color affordance receiver when applying the new color.
    /// </summary>
    public enum ColorBlendMode : byte
    {
        /// <summary>
        /// Solid replaces existing colors.
        /// </summary>
        Solid = 0,

        /// <summary>
        /// Add adds the color to the initial color captured on start, using the blend amount value.
        /// </summary>
        Add = 1,

        /// <summary>
        /// Mix uses the blend amount to interpolate between the initial color captured on start and the target value.
        /// </summary>
        Mix = 2,
    }

    /// <summary>
    /// Affordance state theme data structure for for Color affordances. 
    /// </summary>
    [Serializable]
    public class ColorAffordanceTheme : BaseAffordanceTheme<Color>
    {
        [Header("Color Blend Configuration")]
        [SerializeField]
        [Tooltip("- Solid: Replaces the target value directly." +
            "\n- Add: Adds initial color to target color." +
            "\n- Mix: Blends initial and target color.")]
        ColorBlendMode m_ColorBlendMode = ColorBlendMode.Solid;

        /// <summary>
        /// Blend mode used by the color affordance receiver when applying the new color.
        /// </summary>
        public ColorBlendMode colorBlendMode
        {
            get => m_ColorBlendMode;
            set => m_ColorBlendMode = value;
        }

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Value between 0 and 1 used to compute color blend modes.")]
        float m_BlendAmount = 1f;

        /// <summary>
        /// Value between 0 and 1 used to compute color blend modes.
        /// </summary>
        public float blendAmount
        {
            get => m_BlendAmount;
            set => m_BlendAmount = value;
        }

        /// <summary>
        /// Makes this theme's settings match the settings of another theme.
        /// </summary>
        /// <param name="other">The <seealso cref="ColorAffordanceTheme"/> to deep copy values from. It will not be modified.</param>
        public override void CopyFrom(BaseAffordanceTheme<Color> other)
        {
            base.CopyFrom(other);
            var otherColorTheme = (ColorAffordanceTheme)other;
            colorBlendMode = otherColorTheme.colorBlendMode;
            blendAmount = otherColorTheme.blendAmount;
        }
    }

    /// <summary>
    /// Serializable container class that holds a Color affordance theme value or container asset reference.
    /// </summary>
    /// <seealso cref="ColorAffordanceThemeDatum"/>
    [Serializable]
    public class ColorAffordanceThemeDatumProperty : DatumProperty<ColorAffordanceTheme, ColorAffordanceThemeDatum>
    {
        /// <inheritdoc/>
        public ColorAffordanceThemeDatumProperty(ColorAffordanceTheme value) : base(value)
        {
        }

        /// <inheritdoc/>
        public ColorAffordanceThemeDatumProperty(ColorAffordanceThemeDatum datum) : base(datum)
        {
        }
    }

    /// <summary>
    /// <see cref="ScriptableObject"/> container class that holds a Color affordance theme value.
    /// </summary>
    [CreateAssetMenu(fileName = "ColorAffordanceTheme", menuName = "Affordance Theme/Color Affordance Theme", order = 0)]
    [HelpURL(XRHelpURLConstants.k_ColorAffordanceThemeDatum)]
    public class ColorAffordanceThemeDatum : Datum<ColorAffordanceTheme>
    {
    }
}