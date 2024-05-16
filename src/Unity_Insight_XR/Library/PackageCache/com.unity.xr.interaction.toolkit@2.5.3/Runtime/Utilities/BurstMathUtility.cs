#if BURST_PRESENT
using Unity.Burst;
#else
using System.Runtime.CompilerServices;
#endif
using Unity.Mathematics;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities
{
    /// <summary>
    /// Provides utility functions related to vector and quaternion calculations,
    /// optimized for use with the Burst compiler when available.
    /// </summary>
#if BURST_PRESENT
    [BurstCompile]
#endif
    public static class BurstMathUtility
    {
        /// <summary>
        /// Calculates the orthogonal up vector for a given forward vector and a reference up vector.
        /// </summary>
        /// <param name="forward">The forward vector.</param>
        /// <param name="referenceUp">The reference up vector.</param>
        /// <param name="orthogonalUp">The calculated orthogonal up vector.</param>
        /// <remarks>
        /// Convenience method signature to cast output from <see cref="float3"/> to <see cref="Vector3"/>.
        /// </remarks>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void OrthogonalUpVector(in Vector3 forward, in Vector3 referenceUp, out Vector3 orthogonalUp)
        {
            OrthogonalUpVector(forward, referenceUp, out float3 float3OrthogonalUp);
            orthogonalUp = float3OrthogonalUp;
        }

        /// <summary>
        /// Calculates the orthogonal up vector for a given forward vector and a reference up vector.
        /// </summary>
        /// <param name="forward">The forward vector.</param>
        /// <param name="referenceUp">The reference up vector.</param>
        /// <param name="orthogonalUp">The calculated orthogonal up vector.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void OrthogonalUpVector(in float3 forward, in float3 referenceUp, out float3 orthogonalUp)
        {
            var right = -math.cross(forward, referenceUp);
            orthogonalUp = math.cross(forward, right);
        }

        /// <summary>
        /// Calculates a look rotation quaternion given a forward vector and a reference up vector.
        /// </summary>
        /// <param name="forward">The forward vector.</param>
        /// <param name="referenceUp">The reference up vector.</param>
        /// <param name="lookRotation">The calculated look rotation quaternion.</param>
        /// <remarks>
        /// Convenience method signature to cast output from <see cref="quaternion"/> to <see cref="Quaternion"/>.
        /// </remarks>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void OrthogonalLookRotation(in Vector3 forward, in Vector3 referenceUp, out Quaternion lookRotation)
        {
            OrthogonalLookRotation(forward, referenceUp, out quaternion lookRot);
            lookRotation = lookRot;
        }

        /// <summary>
        /// Calculates a look rotation quaternion given a forward vector and a reference up vector.
        /// </summary>
        /// <param name="forward">The forward vector.</param>
        /// <param name="referenceUp">The reference up vector.</param>
        /// <param name="lookRotation">The calculated look rotation quaternion.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void OrthogonalLookRotation(in float3 forward, in float3 referenceUp, out quaternion lookRotation)
        {
            OrthogonalUpVector(forward, referenceUp, out float3 orthogonalUp);
            lookRotation = quaternion.LookRotation(forward, orthogonalUp);
        }

        /// <summary>
        /// Projects a vector onto a plane defined by a normal orthogonal to the plane.
        /// </summary>
        /// <param name="vector">The vector to be projected.</param>
        /// <param name="planeNormal">The normal vector orthogonal to the plane.</param>
        /// <param name="projectedVector">The projected vector on the plane.</param>
#if BURST_PRESENT
        [BurstCompile]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ProjectOnPlane(in float3 vector, in float3 planeNormal, out float3 projectedVector)
        {
            var sqrMag = math.dot(planeNormal, planeNormal);
            if (sqrMag < math.EPSILON)
            {
                projectedVector = vector;
                return;
            }

            var dot = math.dot(vector, planeNormal);
            projectedVector = new float3(vector.x - planeNormal.x * dot / sqrMag,
                vector.y - planeNormal.y * dot / sqrMag,
                vector.z - planeNormal.z * dot / sqrMag);
        }

        /// <summary>
        /// Projects a vector onto a plane defined by a normal orthogonal to the plane.
        /// </summary>
        /// <param name="vector">The vector to be projected.</param>
        /// <param name="planeNormal">The normal vector orthogonal to the plane.</param>
        /// <param name="projectedVector">The projected vector on the plane.</param>
        /// <remarks>
        /// Convenience method signature to cast output from <see cref="float3"/> to <see cref="Vector3"/>.
        /// </remarks>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void ProjectOnPlane(in Vector3 vector, in Vector3 planeNormal, out Vector3 projectedVector)
        {
            ProjectOnPlane(vector, planeNormal, out float3 float3ProjectedVector);
            projectedVector = float3ProjectedVector;
        }

        /// <summary>
        /// Computes the look rotation with the forward vector projected on a plane defined by a normal orthogonal to the plane.
        /// </summary>
        /// <param name="forward">The forward vector to be projected onto the plane.</param>
        /// <param name="planeNormal">The normal vector orthogonal to the plane.</param>
        /// <param name="lookRotation">The resulting look rotation with the projected forward vector and plane normal as up direction.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void LookRotationWithForwardProjectedOnPlane(in float3 forward, in float3 planeNormal, out quaternion lookRotation)
        {
            ProjectOnPlane(forward, planeNormal, out float3 projectedForward);
            lookRotation = quaternion.LookRotation(projectedForward, planeNormal);
        }

        /// <summary>
        /// Computes the look rotation with the forward vector projected on a plane defined by a normal orthogonal to the plane.
        /// </summary>
        /// <param name="forward">The forward vector to be projected onto the plane.</param>
        /// <param name="planeNormal">The normal vector orthogonal to the plane.</param>
        /// <param name="lookRotation">The resulting look rotation with the projected forward vector and plane normal as up direction.</param>
        /// <remarks>
        /// Convenience method signature to cast output from <see cref="quaternion"/> to <see cref="Quaternion"/>.
        /// </remarks>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void LookRotationWithForwardProjectedOnPlane(in Vector3 forward, in Vector3 planeNormal, out Quaternion lookRotation)
        {
            LookRotationWithForwardProjectedOnPlane(forward, planeNormal, out quaternion lookRot);
            lookRotation = lookRot;
        }

        /// <summary>
        /// Returns the angle in degrees between two rotations <paramref name="a"/> and <paramref name="b"/>.
        /// Equivalent to <see cref="Quaternion.Angle"/>.
        /// </summary>
        /// <param name="a">The first rotation in the quaternion set.</param>
        /// <param name="b">The second rotation in the quaternion set.</param>
        /// <param name="angle">The angle in degrees between a and b.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void Angle(in quaternion a, in quaternion b, out float angle)
        {
            // See Quaternion.cs in Unity source code
            // 0.999999f = 1f - Quaternion.kEpsilon
            // 57.29578f = Mathf.Rad2Deg
            var dot = math.min(math.abs(math.dot(a, b)), 1f);
            angle = (dot > 0.999999f) ? 0f : (math.acos(dot) * 2f * 57.29578f);
        }

        /// <summary>
        /// Compares two float3s for equality with a specified level of tolerance.
        /// </summary>
        /// <param name="a">The first float3 to compare.</param>
        /// <param name="b">The second float3 to compare.</param>
        /// <param name="tolerance">The level of tolerance for the equality check. Defaults to 0.0001f.</param>
        /// <returns>Returns <see langword="true"/> if the difference between the corresponding components of the float3s is less than the specified tolerance; otherwise, <see langword="false"/>.</returns>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static bool FastVectorEquals(in float3 a, in float3 b, float tolerance = 0.0001f)
        {
            return math.abs(a.x - b.x) < tolerance && math.abs(a.y - b.y) < tolerance && math.abs(a.z - b.z) < tolerance;
        }


        /// <summary>
        /// Compares two Vector3s for equality with a specified level of tolerance.
        /// </summary>
        /// <param name="a">The first Vector3 to compare.</param>
        /// <param name="b">The second Vector3 to compare.</param>
        /// <param name="tolerance">The level of tolerance for the equality check. Defaults to 0.0001f.</param>
        /// <returns>Returns <see langword="true"/> if the difference between the corresponding components of the Vector3s is less than the specified tolerance; otherwise, <see langword="false"/>.</returns>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static bool FastVectorEquals(in Vector3 a, in Vector3 b, float tolerance = 0.0001f)
        {
            return math.abs(a.x - b.x) < tolerance && math.abs(a.y - b.y) < tolerance && math.abs(a.z - b.z) < tolerance;
        }

        /// <summary>
        /// Performs a safe division of two Vector3s. If the difference between any corresponding pair of components in the vectors exceeds a specified tolerance, the division is carried out for that component. 
        /// </summary>
        /// <param name="a">The dividend Vector3.</param>
        /// <param name="b">The divisor Vector3.</param>
        /// <param name="result">The resulting Vector3 after division. If the difference between the corresponding components of the dividend and divisor is less than the tolerance, the respective component in the result vector remains zero.</param>
        /// <param name="tolerance">The tolerance for the component-wise division operation. Defaults to 0.000001f.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void FastSafeDivide(in Vector3 a, in Vector3 b, out Vector3 result, float tolerance = 0.000001f)
        {
            FastSafeDivide(a, b, out float3 float3Result, tolerance);
            result = float3Result;
        }

        /// <summary>
        /// Performs a safe division of two float3 vectors. If the difference between any corresponding pair of components in the vectors exceeds a specified tolerance, the division is carried out for that component. 
        /// </summary>
        /// <param name="a">The dividend float3 vector.</param>
        /// <param name="b">The divisor float3 vector.</param>
        /// <param name="result">The resulting float3 vector after division. If the difference between the corresponding components of the dividend and divisor is less than the tolerance, the respective component in the result vector remains zero.</param>
        /// <param name="tolerance">The tolerance for the component-wise division operation. Defaults to 0.000001f.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void FastSafeDivide(in float3 a, in float3 b, out float3 result, float tolerance = 0.000001f)
        {
            result = new float3();
            if (math.abs(a.x - b.x) > tolerance)
                result.x = a.x / b.x;
            if (math.abs(a.y - b.y) > tolerance)
                result.y = a.y / b.y;
            if (math.abs(a.z - b.z) > tolerance)
                result.z = a.z / b.z;
        }


        /// <summary>
        /// Multiplies the corresponding elements of two float3 vectors in a fast, non-matrix multiplication.
        /// </summary>
        /// <param name="a">The first float3 vector.</param>
        /// <param name="b">The second float3 vector.</param>
        /// <param name="result">The resulting float3 vector after element-wise multiplication.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void Scale(in float3 a, in float3 b, out float3 result)
        {
            result = new float3(a.x * b.x, a.y * b.y, a.z * b.z);
        }
        
        /// <summary>
        /// Multiplies the corresponding elements of two Vector3 in a fast, non-matrix multiplication.
        /// </summary>
        /// <param name="a">The first Vector3.</param>
        /// <param name="b">The second Vector3.</param>
        /// <param name="result">The resulting Vector3 after element-wise multiplication.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void Scale(in Vector3 a, in Vector3 b, out Vector3 result)
        {
            result = new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        /// <summary>
        /// Calculates an orthogonal vector to the given vector.
        /// The method finds the smallest component of the input vector and crosses it with the corresponding basis vector.
        /// </summary>
        /// <param name="input">The input vector.</param>
        /// <returns>The resulting orthogonal vector.</returns>
        internal static Vector3 Orthogonal(Vector3 input)
        {
            Orthogonal(input, out float3 resultFloat3);
            return resultFloat3;
        }

        /// <summary>
        /// Calculates an orthogonal vector to the given vector.
        /// The method finds the smallest component of the input vector and crosses it with the corresponding basis vector.
        /// </summary>
        /// <param name="input">The input vector.</param>
        /// <param name="result">The resulting orthogonal vector.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        internal static void Orthogonal(in float3 input, out float3 result)
        {
            // Find the smallest component of v and cross it with the corresponding basis vector
            if (math.abs(input.x) < math.abs(input.y) && math.abs(input.x) < math.abs(input.z))
                result = math.cross(input, new float3(1, 0, 0)); // equivalent to Vector3.right
            else if (math.abs(input.y) < math.abs(input.z))
                result = math.cross(input, new float3(0, 1, 0)); // equivalent to Vector3.up
            else
                result = math.cross(input, new float3(0, 0, 1)); // equivalent to Vector3.forward
        }
    }
}