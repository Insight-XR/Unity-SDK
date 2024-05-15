#if BURST_PRESENT
using Unity.Burst;
#endif
using Unity.Collections;
using Unity.Mathematics;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Curves
{
    /// <summary>
    /// A static class that provides utility methods for working with curves.
    /// All functions are compiled with the Burst compiler if the Burst package is present.
    /// </summary>
#if BURST_PRESENT
    [BurstCompile]
#endif
    static class CurveUtility
    {
        /// <summary>
        /// Pre-multiplied 8x applied to the Unity mathematics float epsilon value, used for float approximate equality comparisons.
        /// </summary>
        const float k_EightEpsilon = math.EPSILON * 8f;

        /// <summary>
        /// Samples a point on a quadratic Bezier curve defined by three control points and a parameter t.
        /// </summary>
        /// <param name="p0">The first control point.</param>
        /// <param name="p1">The second control point.</param>
        /// <param name="p2">The third control point.</param>
        /// <param name="t">The parameter t, ranging from 0 to 1.</param>
        /// <param name="point">The output point on the curve.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void SampleQuadraticBezierPoint(in float3 p0, in float3 p1, in float3 p2, float t, out float3 point)
        {
            var u = 1f - t;   // (1 - t)
            var uu = u * u;   // (1 - t)²
            var tt = t * t;   // t²

            // (1 - t)²P₀ + 2(1 - t)tP₁ + t²P₂ where 0 ≤ t ≤ 1
            // u²P₀ + 2utP₁ + t²P₂
            point = (uu * p0) +
                (2f * u * t * p1) +
                (tt * p2);
        }

        /// <summary>
        /// Samples a point on a cubic Bezier curve defined by four control points and a parameter t.
        /// </summary>
        /// <param name="p0">The first control point.</param>
        /// <param name="p1">The second control point.</param>
        /// <param name="p2">The third control point.</param>
        /// <param name="p3">The fourth control point.</param>
        /// <param name="t">The parameter t, ranging from 0 to 1.</param>
        /// <param name="point">The output point on the curve.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void SampleCubicBezierPoint(in float3 p0, in float3 p1, in float3 p2, in float3 p3, float t, out float3 point)
        {
            var u = 1f - t;   // (1 - t)
            var uu = u * u;   // (1 - t)²
            var uuu = uu * u; // (1 - t)³
            var tt = t * t;   // t²
            var ttt = tt * t; // t³

            // (1 - t)³P₀ + 3(1 - t)²tP₁ + 3(1 - t)t²P₂ + t³P₃ where 0 ≤ t ≤ 1
            // u³P₀ + 3u²tP₁ + 3ut²P₂ + t³P₃
            point = (uuu * p0) +
                (3f * uu * t * p1) +
                (3f * u * tt * p2) +
                (ttt * p3);
        }

        /// <summary>
        /// Elevates a quadratic Bezier curve to a cubic Bezier curve by adding an extra control point.
        /// </summary>
        /// <param name="p0">The first control point of the quadratic curve.</param>
        /// <param name="p1">The second control point of the quadratic curve.</param>
        /// <param name="p2">The third control point of the quadratic curve.</param>
        /// <param name="c0">The first control point of the cubic curve. (output)</param>
        /// <param name="c1">The second control point of the cubic curve. (output)</param>
        /// <param name="c2">The third control point of the cubic curve. (output)</param>
        /// <param name="c3">The fourth control point of the cubic curve. (output)</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void ElevateQuadraticToCubicBezier(in float3 p0, in float3 p1, in float3 p2, out float3 c0, out float3 c1, out float3 c2, out float3 c3)
        {
            // A Bezier curve of one degree can be reproduced by one of higher degree.
            // Convert quadratic Bezier curve with control points P₀, P₁, P₂
            // into a cubic Bezier curve with control points C₀, C₁, C₂, C₃.
            // The end points remain the same.
            c0 = p0;
            c1 = p0 + (2f / 3f) * (p1 - p0);
            c2 = p2 + (2f / 3f) * (p1 - p2);
            c3 = p2;
        }

        /// <summary>
        /// Generates a cubic Bezier curve from a given line segment and a curve ratio.
        /// </summary>
        /// <param name="numTargetPoints">The number of points to generate for the curve.</param>
        /// <param name="curveRatio">The ratio of the line length to use as the distance from the midpoint to the control point.</param>
        /// <param name="lineOrigin">The starting point of the line segment.</param>
        /// <param name="lineDirection">The normalized forward direction vector of the line segment.</param>
        /// <param name="endPoint">The ending point of the line segment.</param>
        /// <param name="targetPoints">A reference to a native array of <see cref="float3"/> that will store the generated curve points.</param>
#if UNITY_2022_2_OR_NEWER && BURST_PRESENT
        [BurstCompile]
#endif
        public static void GenerateCubicBezierCurve(int numTargetPoints, float curveRatio, in float3 lineOrigin, in float3 lineDirection, in float3 endPoint, ref NativeArray<float3> targetPoints)
        {
            var lineLength = math.length(endPoint - lineOrigin);
            var adjustedMidPoint = lineOrigin + lineDirection * lineLength * curveRatio;

            ElevateQuadraticToCubicBezier(lineOrigin, adjustedMidPoint, endPoint,
                out var p0, out var p1, out var p2, out var p3);

            // Set first point
            targetPoints[0] = lineOrigin;
            var interval = 1f / (numTargetPoints - 1);
            for (var i = 1; i < numTargetPoints; ++i)
            {
                var percent = i * interval;
                SampleCubicBezierPoint(p0, p1, p2, p3, percent, out var newPoint);
                targetPoints[i] = newPoint;
            }
        }

        /// <summary>
        /// Calculates the position of a projectile at a given time using constant acceleration formula.
        /// </summary>
        /// <param name="initialPosition">The initial position vector of the projectile.</param>
        /// <param name="initialVelocity">The initial velocity vector of the projectile.</param>
        /// <param name="constantAcceleration">The constant acceleration vector of the projectile, typically (0, -9.8, 0).</param>
        /// <param name="time">The time at which to calculate the position.</param>
        /// <param name="point">The output point on the curve.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void SampleProjectilePoint(in float3 initialPosition, in float3 initialVelocity, in float3 constantAcceleration, float time, out float3 point)
        {
            // Position of object in constant acceleration is:
            // x(t) = x₀ + v₀t + 0.5at²
            // where x₀ is the position at time 0,
            // v₀ is the velocity vector at time 0,
            // a is the constant acceleration vector
            point = initialPosition + initialVelocity * time + constantAcceleration * (0.5f * time * time);
        }

        /// <summary>
        /// Calculates the time of flight for a projectile launched at a given angle and initial velocity.
        /// </summary>
        /// <param name="velocityMagnitude">The magnitude of the initial velocity vector.</param>
        /// <param name="gravityAcceleration">The constant acceleration due to gravity (typically 9.8).</param>
        /// <param name="angleRad">The launch angle in radians.</param>
        /// <param name="height">The initial height of the projectile.</param>
        /// <param name="extraFlightTime">An additional time to add to the flight time.</param>
        /// <param name="flightTime">The output parameter for the calculated flight time.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void CalculateProjectileFlightTime(float velocityMagnitude, float gravityAcceleration, float angleRad, float height, float extraFlightTime, out float flightTime)
        {
            // Vertical velocity component Vy = v₀sinθ
            // When initial height = 0,
            // Time of flight = 2(initial velocity)(sine of launch angle) / (acceleration) = 2v₀sinθ/g
            // When initial height > 0,
            // Time of flight = [Vy + √(Vy² + 2gh)] / g
            // The additional flight time property is added.
            var vy = velocityMagnitude * angleRad;
            if (height < 0f)
                flightTime = 0f;
            // Does the same math roughly as Mathf.Approximately(height, 0f)
            else if (math.abs(height) < k_EightEpsilon)
                flightTime = 2f * vy / gravityAcceleration;
            else
                flightTime = (vy + math.sqrt(vy * vy + 2f * gravityAcceleration * height)) / gravityAcceleration;

            flightTime = math.max(flightTime + extraFlightTime, 0f);
        }
    }
}