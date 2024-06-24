using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for the <see cref="Vector3"/> type.
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Returns the component-wise inverse of this vector [1/x,1/y,1/z].
        /// </summary>
        /// <param name="vector">The vector to invert.</param>
        /// <returns>The inverted vector</returns>
        public static Vector3 Inverse(this Vector3 vector)
        {
            return new Vector3(1.0f / vector.x, 1.0f / vector.y, 1.0f / vector.z);
        }

        /// <summary>
        /// Returns the smallest component of this vector.
        /// </summary>
        /// <param name="vector">The vector whose minimum component will be returned.</param>
        /// <returns>The minimum value.</returns>
        public static float MinComponent(this Vector3 vector)
        {
            return Mathf.Min(Mathf.Min(vector.x, vector.y), vector.z);
        }

        /// <summary>
        /// Returns the largest component of this vector.
        /// </summary>
        /// <param name="vector">The vector whose maximum component will be returned.</param>
        /// <returns>The maximum value.</returns>
        public static float MaxComponent(this Vector3 vector)
        {
            return Mathf.Max(Mathf.Max(vector.x, vector.y), vector.z);
        }

        /// <summary>
        /// Returns the component-wise absolute value of this vector [abs(x), abs(y), abs(z)].
        /// </summary>
        /// <param name="vector">The vector whose absolute value will be returned</param>
        /// <returns>A vector containing the component-wise absolute values of this vector.</returns>
        public static Vector3 Abs(this Vector3 vector)
        {
            vector.x = Mathf.Abs(vector.x);
            vector.y = Mathf.Abs(vector.y);
            vector.z = Mathf.Abs(vector.z);
            return vector;
        }

        /// <summary>
        /// Returns a new vector3 that multiplies each component of both input vectors together.
        /// </summary>
        /// <param name="value">Input value to scale.</param>
        /// <param name="scale">Vector3 used to scale components of input value.</param>
        /// <returns>Scaled input value.</returns>
        public static Vector3 Multiply(this Vector3 value, Vector3 scale)
        {
            return new Vector3(value.x * scale.x, value.y * scale.y, value.z * scale.z);
        }

        /// <summary>
        /// Returns a new `Vector3` that divides each component of the input value by each component of the scale value.
        /// </summary>
        /// <param name="value">Input value to scale.</param>
        /// <param name="scale">`Vector3` used to scale components of input value.</param>
        /// <returns>Scaled input value.</returns>
        /// <exception cref="System.DivideByZeroException">Thrown if scale parameter has any 0 values. Consider using <see cref="SafeDivide"/>.</exception>
        public static Vector3 Divide(this Vector3 value, Vector3 scale)
        {
            return new Vector3(value.x / scale.x, value.y / scale.y, value.z / scale.z);
        }

        /// <summary>
        /// Returns a new `Vector3` that divides each component of the input value by each component of the scale value.
        /// If any divisor is 0 or the output of the division is a `NaN`, then the output of that component will be zero.
        /// </summary>
        /// <param name="value">Input value to scale.</param>
        /// <param name="scale">`Vector3` used to scale components of input value.</param>
        /// <returns>Scaled input value.</returns>
        public static Vector3 SafeDivide(this Vector3 value, Vector3 scale)
        {
            float x = Mathf.Approximately(scale.x, 0f) ? 0f : value.x / scale.x;
            if (float.IsNaN(x))
            {
                x = 0f;
            }

            float y = Mathf.Approximately(scale.y, 0f) ? 0f : value.y / scale.y;
            if (float.IsNaN(y))
            {
                y = 0f;
            }

            float z = Mathf.Approximately(scale.z, 0f) ? 0f : value.z / scale.z;
            if (float.IsNaN(z))
            {
                z = 0f;
            }
            return new Vector3(x, y, z);
        }
    }
}
