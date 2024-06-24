using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for the <see cref="Vector2"/> type.
    /// </summary>
    public static class Vector2Extensions
    {
        /// <summary>
        ///  Returns the component-wise inverse of this vector [1/x, 1/y].
        /// </summary>
        /// <param name="vector">The vector to invert.</param>
        /// <returns>The inverted vector.</returns>
        public static Vector2 Inverse(this Vector2 vector)
        {
            return new Vector2(1.0f / vector.x, 1.0f / vector.y);
        }

        /// <summary>
        /// Returns the smallest component of this vector.
        /// </summary>
        /// <param name="vector">The vector whose minimum component will be returned.</param>
        /// <returns>The minimum value.</returns>
        public static float MinComponent(this Vector2 vector)
        {
            return Mathf.Min(vector.x, vector.y);
        }

        /// <summary>
        /// Returns the largest component of this vector.
        /// </summary>
        /// <param name="vector">The vector whose maximum component will be returned.</param>
        /// <returns>The maximum value.</returns>
        public static float MaxComponent(this Vector2 vector)
        {
            return Mathf.Max(vector.x, vector.y);
        }

        /// <summary>
        /// Returns the component-wise absolute value of this vector [abs(x), abs(y)].
        /// </summary>
        /// <param name="vector">The vector whose absolute value will be returned.</param>
        /// <returns>The component-wise absolute value of this vector.</returns>
        public static Vector2 Abs(this Vector2 vector)
        {
            vector.x = Mathf.Abs(vector.x);
            vector.y = Mathf.Abs(vector.y);
            return vector;
        }
    }
}
