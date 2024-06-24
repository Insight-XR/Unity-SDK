using System;
using UnityEngine;

namespace UnityEditor.XR.Interaction.Toolkit.Utilities.Internal
{
    /// <summary>
    /// Utility methods for Handles.
    /// </summary>
    /// <seealso cref="Handles"/>
    static class PositionHandleUtility
    {
        // Most of the private methods (with the exception of the CapFunction) were directly copied from Handles source code.

        // When axis is looking away from camera, fade it out along 25 -> 15 degrees range
        // ReSharper disable InconsistentNaming -- Constant values from Handles
        static readonly float k_CameraViewLerpStart1 = Mathf.Cos(Mathf.Deg2Rad * 25f);
        static readonly float k_CameraViewLerpEnd1 = Mathf.Cos(Mathf.Deg2Rad * 15f);
        // When axis is looking towards the camera, fade it out along 170 -> 175 degrees range
        static readonly float k_CameraViewLerpStart2 = Mathf.Cos(Mathf.Deg2Rad * 170f);
        static readonly float k_CameraViewLerpEnd2 = Mathf.Cos(Mathf.Deg2Rad * 175f);
        // ReSharper restore InconsistentNaming

        // Hide & disable axis if they have faded out more than 60%
        const float k_CameraViewThreshold = 0.6f;

        static readonly float[] s_CameraViewLerp = new float[3];
        static readonly int[] s_AxisDrawOrder = { 0, 1, 2 };

        /// <summary>
        /// Draws a position handle without the arrow caps or plane squares.
        /// </summary>
        /// <param name="position">Center of the handle in 3D space.</param>
        /// <param name="rotation">Orientation of the handle in 3D space.</param>
        /// <param name="axisSize">Relative size of the handle to a regular position handle. Scales each axis of the constant screen-size handle.</param>
        /// <remarks>
        /// The handle looks like the built-in move tool in Unity but with only the axis lines and is not interactable.
        /// </remarks>
        public static void DrawLineOnlyPositionHandle(Vector3 position, Quaternion rotation, Vector3 axisSize)
        {
            var originalHandlesColor = Handles.color;

            // Calculate the camera view vector in Handle draw space
            // this handle the case where the matrix is skewed
            var matrix = Handles.matrix;
            var handlePosition = matrix.MultiplyPoint3x4(position);
            var drawToWorldMatrix = matrix * Matrix4x4.TRS(position, rotation, Vector3.one);
            var invDrawToWorldMatrix = drawToWorldMatrix.inverse;
            var viewVectorDrawSpace = GetCameraViewFrom(handlePosition, invDrawToWorldMatrix);

            var size = HandleUtility.GetHandleSize(position);

            // Calculate per axis camera lerp
            for (var i = 0; i < 3; ++i)
            {
                s_CameraViewLerp[i] = GetCameraViewLerpForWorldAxis(viewVectorDrawSpace, GetAxisVector(i));
            }

            // Calculate back-to-front order to draw the axes
            CalcDrawOrder(viewVectorDrawSpace, s_AxisDrawOrder);

            for (var ii = 0; ii < 3; ++ii)
            {
                var i = s_AxisDrawOrder[ii];

                var cameraLerp = s_CameraViewLerp[i];
                if (cameraLerp <= k_CameraViewThreshold)
                {
                    Handles.color = GetColorByAxis(i);
                    Handles.color = GetFadedAxisColor(Handles.color, cameraLerp);
                    Handles.color = ToActiveColorSpace(Handles.color);

                    var axisVector = GetAxisVector(i);
                    var dir = rotation * axisVector;

                    position = Handles.Slider(position, dir, size * axisSize[i], LineOnlyCapFunction, EditorSnapSettings.move[i]);
                }
            }

            Handles.color = originalHandlesColor;
        }

        static void LineOnlyCapFunction(int controlId, Vector3 position, Quaternion rotation, float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.Layout:
                case EventType.MouseMove:
                {
                    var direction = rotation * Vector3.forward;
                    var linePos = position + direction * size;
                    HandleUtility.AddControl(controlId, HandleUtility.DistanceToLine(position, linePos));
                    break;
                }
                case EventType.Repaint:
                {
                    var direction = rotation * Vector3.forward;
                    var linePos = position + direction * size;
#if UNITY_2020_3_OR_NEWER
                    Handles.DrawLine(position, linePos, Handles.lineThickness);
#else
                    Handles.DrawLine(position, linePos);
#endif
                    break;
                }
            }
        }

        static float GetCameraViewLerpForWorldAxis(Vector3 viewVector, Vector3 axis)
        {
            var dot = Vector3.Dot(viewVector, axis);
            var l1 = Mathf.InverseLerp(k_CameraViewLerpStart1, k_CameraViewLerpEnd1, dot);
            var l2 = Mathf.InverseLerp(k_CameraViewLerpStart2, k_CameraViewLerpEnd2, dot);
            return Mathf.Max(l1, l2);
        }

        static Vector3 GetAxisVector(int axis)
        {
            switch (axis)
            {
                case 0:
                    return Vector3.right;
                case 1:
                    return Vector3.up;
                case 2:
                    return Vector3.forward;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis));
            }
        }

        static Color GetColorByAxis(int axis)
        {
            switch (axis)
            {
                case 0:
                    return Handles.xAxisColor;
                case 1:
                    return Handles.yAxisColor;
                case 2:
                    return Handles.zAxisColor;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis));
            }
        }

        static Color GetFadedAxisColor(Color col, float fade)
        {
            return Color.Lerp(col, Color.clear, fade);
        }

        static Color ToActiveColorSpace(Color color)
        {
            return QualitySettings.activeColorSpace == ColorSpace.Linear ? color.linear : color;
        }

        static Vector3 GetCameraViewFrom(Vector3 position, Matrix4x4 matrix)
        {
            var camera = Camera.current;
            return camera.orthographic
                ? matrix.MultiplyVector(camera.transform.forward).normalized
                : matrix.MultiplyVector(position - camera.transform.position).normalized;
        }

        static void Swap(ref Vector3 v, int[] indices, int a, int b)
        {
            (v[a], v[b]) = (v[b], v[a]);

            (indices[a], indices[b]) = (indices[b], indices[a]);
        }

        // Given view direction in handle space, calculate
        // back-to-front order in which handle axes should be drawn.
        // The array should be [3] size, and will contain axis indices
        // from (0,1,2) set.
        static void CalcDrawOrder(Vector3 viewDir, int[] ordering)
        {
            ordering[0] = 0;
            ordering[1] = 1;
            ordering[2] = 2;
            // Essentially an unrolled bubble sort for 3 elements
            if (viewDir.y > viewDir.x) Swap(ref viewDir, ordering, 1, 0);
            if (viewDir.z > viewDir.y) Swap(ref viewDir, ordering, 2, 1);
            if (viewDir.y > viewDir.x) Swap(ref viewDir, ordering, 1, 0);
        }
    }
}
