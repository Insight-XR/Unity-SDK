using System.Collections.Generic;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Provides a generic object pool implementation.
    /// </summary>
    /// <typeparam name="T">The <see cref="System.Type"/> of objects in this pool.</typeparam>
    public class ObjectPool<T> where T: class, new()
    {
        /// <summary>
        /// All objects currently in this pool.
        /// </summary>
        protected readonly Queue<T> PooledQueue = new Queue<T>();

        /// <summary>
        /// Gets an object instance from the pool. Creates a new instance if the pool is empty.
        /// </summary>
        /// <returns>The object instance</returns>
        public virtual T Get()
        {
            return PooledQueue.Count == 0 ? new T() : PooledQueue.Dequeue();
        }

        /// <summary>
        /// Returns an object instance to the pool.
        /// </summary>
        /// <remarks>
        /// Object values can be cleared automatically if <see cref="ClearInstance(T)"/> is implemented.
        /// The base `ObjectPool` class does not clear objects to the pool.
        /// </remarks>
        /// <param name="instance">The instance to return.</param>
        public void Recycle(T instance)
        {
            ClearInstance(instance);
            PooledQueue.Enqueue(instance);
        }

        /// <summary>
        /// Implement this function in a derived class to
        /// automatically clear an instance when <see cref="Recycle"/> is called.
        /// </summary>
        /// <param name="instance">The object to clear.</param>
        protected virtual void ClearInstance(T instance) { }
    }
}
