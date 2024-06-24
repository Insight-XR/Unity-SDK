using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Represents a series of object actions as a single undo-operation.
    /// </summary>
    /// <remarks>
    /// UndoBlock methods work in both Edit mode and Play mode. In Play mode undo-operations are disabled.
    /// This class mirrors the normal functions you find in the <see cref="Undo"/> class and collapses them into one operation
    /// when the block is complete.
    /// </remarks>
    /// <example>
    /// Proper usage of this class is:
    /// <code>
    /// using (var undoBlock = new UndoBlock("Desired Undo Message"))
    /// {
    ///     undoBlock.yourCodeToUndo()
    /// }
    /// </code>
    /// </example>
    public class UndoBlock : IDisposable
    {
        int m_UndoGroup;
        bool m_DisposedValue; // To detect redundant calls of Dispose

#if UNITY_EDITOR
        string m_UndoLabel;
        bool m_Dirty;
        bool m_TestMode;
#endif

        /// <summary>
        /// Initialize a new UndoBlock.
        /// </summary>
        /// <param name="undoLabel">The label to apply to the undo group created within this undo block.</param>
        /// <param name="testMode">Whether this is part of a test run.</param>
        public UndoBlock(string undoLabel, bool testMode = false)
        {
#if UNITY_EDITOR
            m_Dirty = false;
            m_TestMode = testMode;
            if (!Application.isPlaying && !m_TestMode)
            {
                Undo.IncrementCurrentGroup();
                m_UndoGroup = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName(undoLabel);
                m_UndoLabel = undoLabel;
            }
            else
                m_UndoGroup = -1;
#else
            m_UndoGroup = -1;
#endif
        }

        /// <summary>
        /// Register undo operations for a newly created object.
        /// </summary>
        /// <param name="objectToUndo">The object that was created.</param>
        public void RegisterCreatedObject(UnityObject objectToUndo)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && !m_TestMode)
            {
                Undo.RegisterCreatedObjectUndo(objectToUndo, m_UndoLabel);
                m_Dirty = true;
            }
#endif
        }

        /// <summary>
        /// Records any changes done on the object after the RecordObject function.
        /// </summary>
        /// <param name="objectToUndo">The reference to the object that you will be modifying.</param>
        public void RecordObject(UnityObject objectToUndo)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && !m_TestMode)
                Undo.RecordObject(objectToUndo, m_UndoLabel);
#endif
        }

        /// <summary>
        /// Sets the parent transform of an object and records an undo operation.
        /// </summary>
        /// <param name="transform">The Transform component whose parent is to be changed.</param>
        /// <param name="newParent">The parent Transform to be assigned.</param>
        public void SetTransformParent(Transform transform, Transform newParent)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && !m_TestMode)
                Undo.SetTransformParent(transform, newParent, m_UndoLabel);
            else
                transform.parent = newParent;
#else
            transform.parent = newParent;
#endif
        }

        /// <summary>
        /// Adds a component to the game object and registers an undo operation for this action.
        /// </summary>
        /// <param name="gameObject">The game object you want to add the component to.</param>
        /// <typeparam name="T">The type of component you want to add.</typeparam>
        /// <returns>The new component.</returns>
        public T AddComponent<T>(GameObject gameObject) where T : Component
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && !m_TestMode)
            {
                m_Dirty = true;
                return Undo.AddComponent<T>(gameObject);
            }
#endif

            return gameObject.AddComponent<T>();
        }

        /// <summary>
        /// Dispose of this object.
        /// </summary>
        /// <param name="disposing">Whether to cleanup this object's state.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_DisposedValue)
            {
                if (disposing && m_UndoGroup > -1)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying && !m_TestMode)
                    {
                        Undo.CollapseUndoOperations(m_UndoGroup);
                        if (m_Dirty)
                        {
                            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                        }
                    }

                    m_Dirty = false;
#endif
                }

                m_DisposedValue = true;
            }
        }

        /// <summary>
        /// This code added to correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
    }
}
