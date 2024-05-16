using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Utility methods for working with UnityEngine <see cref="Object"/> types.
    /// </summary>
    public static class UnityObjectUtils
    {
        /// <summary>
        /// Calls the proper Destroy method on an object based on if application is playing.
        /// </summary>
        /// <remarks>
        /// In Play mode or when running your built application, this function calls <see cref="Object.Destroy(UnityObject)"/>.
        ///
        /// In the Editor, outside of Play mode, this function calls <see cref="Object.DestroyImmediate(UnityObject)"/>.
        /// </remarks>
        /// <param name="obj">Object to be destroyed.</param>
        /// <param name="withUndo">Whether to record and undo operation for the destroy action.</param>
        public static void Destroy(UnityObject obj, bool withUndo = false)
        {
            if (Application.isPlaying)
            {
                UnityObject.Destroy(obj);
            }
#if UNITY_EDITOR
            else
            {
                if (withUndo)
                    Undo.DestroyObjectImmediate(obj);
                else
                    UnityObject.DestroyImmediate(obj);
            }
#endif
        }

        /// <summary>
        /// Returns a component of the specified type that is associated with an object, if possible.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="objectIn"/> is the requested type, then this function casts it to
        /// type T and returns it.
        ///
        /// If <paramref name="objectIn"/> is a <see cref="GameObject"/>, then this function returns
        /// the first component of the requested type, if one exists.
        ///
        /// If <paramref name="objectIn"/> is a different type of component, this function returns
        /// the first component of the requested type on the same GameObject, if one exists.
        /// </remarks>
        /// <param name="objectIn">The Unity Object reference to convert.</param>
        /// <typeparam name="T"> The type to convert to.</typeparam>
        /// <returns>A component of type `T`, if found on the object. Otherwise returns `null`.</returns>
        public static T ConvertUnityObjectToType<T>(UnityObject objectIn) where T : class
        {
            var interfaceOut = objectIn as T;
            if (interfaceOut == null && objectIn != null)
            {
                var go = objectIn as GameObject;
                if (go != null)
                {
                    interfaceOut = go.GetComponent<T>();
                    return interfaceOut;
                }

                var comp = objectIn as Component;
                if (comp != null)
                    interfaceOut = comp.GetComponent<T>();
            }

            return interfaceOut;
        }

        /// <summary>
        /// Removes any destroyed UnityObjects from a list.
        /// </summary>
        /// <typeparam name="T">The specific type of UnityObject in the dictionary.</typeparam>
        /// <param name="list">A list of UnityObjects that may contain destroyed objects.</param>
        public static void RemoveDestroyedObjects<T>(List<T> list) where T : UnityObject
        {
            var removeList = CollectionPool<List<T>, T>.GetCollection();
            foreach (var component in list)
            {
                if (component == null)
                    removeList.Add(component);
            }

            foreach (var entry in removeList)
            {
                list.Remove(entry);
            }

            CollectionPool<List<T>, T>.RecycleCollection(removeList);
        }

        /// <summary>
        /// Removes any destroyed keys from a dictionary that uses UnityObjects as its key type.
        /// </summary>
        /// <typeparam name="TKey">The specific type of UnityObject serving as keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The value type of the dictionary.</typeparam>
        /// <param name="dictionary">A dictionary of UnityObjects that may contain destroyed objects.</param>
        public static void RemoveDestroyedKeys<TKey, TValue>(Dictionary<TKey, TValue> dictionary) where TKey : UnityObject
        {
            var removeList = CollectionPool<List<TKey>, TKey>.GetCollection();
            foreach (var kvp in dictionary)
            {
                var key = kvp.Key;
                if (key == null)
                    removeList.Add(key);
            }

            foreach (var key in removeList)
            {
                dictionary.Remove(key);
            }

            CollectionPool<List<TKey>, TKey>.RecycleCollection(removeList);
        }
    }
}
