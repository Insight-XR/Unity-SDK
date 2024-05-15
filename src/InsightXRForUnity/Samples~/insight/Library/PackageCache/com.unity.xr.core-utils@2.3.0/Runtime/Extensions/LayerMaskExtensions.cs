﻿using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for <see cref="LayerMask"/> structs.
    /// </summary>
    public static class LayerMaskExtensions
    {
        /// <summary>
        /// Gets the index of the first enabled layer in this layerMask.
        /// </summary>
        /// <param name="layerMask">The layer mask.</param>
        /// <returns>The index of the first enabled layer.</returns>
        public static int GetFirstLayerIndex(this LayerMask layerMask)
        {
            if (layerMask.value == 0)
                return -1;

            var layerNumber = 0;
            var mask = layerMask.value;
            while ((mask & 0x1) == 0)
            {
                mask = mask >> 1;
                layerNumber++;
            }

            return layerNumber;
        }

        /// <summary>
        /// Checks whether a layer is in a LayerMask.
        /// </summary>
        /// <param name="mask">The layer mask.</param>
        /// <param name="layer">The layer index to check for.</param>
        /// <returns>True if the specified layer is in this mask.</returns>
        public static bool Contains(this LayerMask mask, int layer)
        {
            return ((uint)(int)mask & (1 << layer)) > 0;
        }
    }
}
