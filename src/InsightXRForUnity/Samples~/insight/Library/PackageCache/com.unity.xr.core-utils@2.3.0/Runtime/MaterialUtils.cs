using System;
using System.Globalization;
using UnityEngine;
using UnityObject = UnityEngine.Object;

#if INCLUDE_UGUI
using UnityEngine.UI;
#endif

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Runtime Material utilities.
    /// </summary>
    public static class MaterialUtils
    {
        /// <summary>
        /// Clones and replaces the material assigned to a <see cref="Renderer"/>.
        /// </summary>
        /// <remarks>
        /// > [!WARNING]
        /// > You must call <see cref="UnityObjectUtils.Destroy(UnityObject, bool)"/> on this material object when done.
        /// </remarks>
        /// <seealso cref="Renderer.material"/>
        /// <param name="renderer">The renderer assigned the material to clone.</param>
        /// <returns>The cloned material.</returns>
        public static Material GetMaterialClone(Renderer renderer)
        {
            // The following is equivalent to renderer.material, but gets rid of the error messages in edit mode
            return renderer.material = UnityObject.Instantiate(renderer.sharedMaterial);
        }

#if INCLUDE_UGUI
        /// <summary>
        /// Clones and replaces the material assigned to a <see cref="Graphic"/>.
        /// </summary>
        /// <remarks>
        /// To use this function, your project must contain the
        /// [Unity UI package (com.unity.ugui)](https://docs.unity3d.com/Manual/com.unity.ugui.html).
        /// 
        /// > [!WARNING]
        /// > You must call <see cref="UnityObjectUtils.Destroy(UnityObject, bool)"/> on this material object when done.
        /// </remarks>
        /// <seealso cref="Graphic.material"/>
        /// <param name="graphic">The Graphic object assigned the material to clone.</param>
        /// <returns>Cloned material</returns>
        public static Material GetMaterialClone(Graphic graphic)
        {
            // The following is equivalent to graphic.material, but gets rid of the error messages in edit mode
            return graphic.material = UnityObject.Instantiate(graphic.material);
        }
#endif

        /// <summary>
        /// Clones and replaces all materials assigned to a <see cref="Renderer"/>
        /// </summary>
        /// <remarks>
        /// > [!WARNING]
        /// > You must call <see cref="UnityObjectUtils.Destroy(UnityObject, bool)"/> on each cloned material object in the array when done.
        /// </remarks>
        /// <seealso cref="Renderer.materials"/>
        /// <param name="renderer">Renderer assigned the materials to clone and replace.</param>
        /// <returns>Cloned materials</returns>
        public static Material[] CloneMaterials(Renderer renderer)
        {
            var sharedMaterials = renderer.sharedMaterials;
            for (var i = 0; i < sharedMaterials.Length; i++)
            {
                sharedMaterials[i] = UnityObject.Instantiate(sharedMaterials[i]);
            }

            renderer.sharedMaterials = sharedMaterials;
            return sharedMaterials;
        }

        /// <summary>
        /// Converts an RGB or RGBA formatted hex string to a <see cref="Color"/> object.
        /// </summary>
        /// <param name="hex">The formatted string, with an optional "0x" or "#" prefix.</param>
        /// <returns>The color value represented by the formatted string.</returns>
        public static Color HexToColor(string hex)
        {
            hex = hex.Replace("0x", "").Replace("#", "");
            var r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            var a = hex.Length == 8 ? byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber) : (byte)255;

            return new Color32(r, g, b, a);
        }

        /// <summary>
        /// Shift the hue of a color by a given amount.
        /// </summary>
        /// <remarks>The hue value wraps around to 0 if the shifted hue exceeds 1.0.</remarks>
        /// <param name="color">The input color.</param>
        /// <param name="shift">The amount of shift.</param>
        /// <returns>The output color.</returns>
        public static Color HueShift(Color color, float shift)
        {
            Vector3 hsv;
            Color.RGBToHSV(color, out hsv.x, out hsv.y, out hsv.z);
            hsv.x = Mathf.Repeat(hsv.x + shift, 1f);
            return Color.HSVToRGB(hsv.x, hsv.y, hsv.z);
        }

        /// <summary>
        /// Adds a material to this renderer's array of shared materials.
        /// </summary>
        /// <param name="renderer">The renderer on which to add the material.</param>
        /// <param name="material">The material to add.</param>
        public static void AddMaterial(this Renderer renderer, Material material)
        {
            var materials = renderer.sharedMaterials;
            var length = materials.Length;
            var newMaterials = new Material[length + 1];
            Array.Copy(materials, newMaterials, length);
            newMaterials[length] = material;
            renderer.sharedMaterials = newMaterials;
        }
    }
}
