using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// <see langword="bool"/> Unity Event implementation to serialize in the editor.
    /// </summary>
    [Serializable]
    public class BoolUnityEvent : UnityEvent<bool>
    {
    }

    /// <summary>
    /// <see langword="float"/> Unity Event implementation to serialize in the editor.
    /// </summary>
    [Serializable]
    public class FloatUnityEvent : UnityEvent<float>
    {
    }

    /// <summary>
    /// <see cref="Vector2"/> Unity Event implementation to serialize in the editor.
    /// </summary>
    [Serializable]
    public class Vector2UnityEvent : UnityEvent<Vector2>
    {
    }

    /// <summary>
    /// <see cref="Vector3"/> Unity Event implementation to serialize in the editor.
    /// </summary>
    [Serializable]
    public class Vector3UnityEvent : UnityEvent<Vector3>
    {
    }

    /// <summary>
    /// <see cref="Vector4"/> Unity Event implementation to serialize in the editor.
    /// </summary>
    [Serializable]
    public class Vector4UnityEvent : UnityEvent<Vector4>
    {
    }

    /// <summary>
    /// <see cref="Quaternion"/> Unity Event Implementation to serialize in the editor.
    /// </summary>
    [Serializable]
    public class QuaternionUnityEvent : UnityEvent<Quaternion>
    {
    }

    /// <summary>
    /// <see langword="int"/> Unity Event implementation to serialize in the editor.
    /// </summary>
    [Serializable]
    public class IntUnityEvent : UnityEvent<int>
    {
    }

    /// <summary>
    /// <see cref="Color"/> Unity Event implementation to serialize in the editor.
    /// </summary>
    [Serializable]
    public class ColorUnityEvent : UnityEvent<Color>
    {
    }

    /// <summary>
    /// <see langword="string"/> Unity Event Implementation to serialize in the editor.
    /// </summary>
    [Serializable]
    public class StringUnityEvent : UnityEvent<string>
    {
    }
}
