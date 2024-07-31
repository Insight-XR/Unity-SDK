using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsightDesk
{
    public class InsightObjectPool<T>
    {
        private readonly Stack<T> _objects;
        private readonly Func<T> _objectGenerator;
        private int _numOutstandingObjects;

        public InsightObjectPool(Func<T> objectGenerator, int initSize = 0)
        {
            _objects = new Stack<T>(
                Mathf.CeilToInt(initSize * 1.1f)); // make slightly larger than initSize so pool has some room to grow before doubling stack size
            _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));

            for (int i = 0; i < initSize; i++)
            {
                Return(objectGenerator());
            }

            _numOutstandingObjects = 0;
        }

        public T Get()
        {
            _numOutstandingObjects++;
            var obj = _objects.Count > 0 ? _objects.Pop() : _objectGenerator();

            // Log when an object is retrieved from the pool
            //Debug.Log($"Object retrieved from pool. Outstanding objects: {_numOutstandingObjects}, Pool size: {_objects.Count}");

            return obj;
        }

        public void Return(T item)
        {
            _numOutstandingObjects--;
            _objects.Push(item);

            // Log when an object is returned to the pool
            //Debug.Log($"Object returned to pool. Outstanding objects: {_numOutstandingObjects}, Pool size: {_objects.Count}");
        }

        public int PoolSize()
        {
            return _objects.Count;
        }

        public int NumOutstandingObjects()
        {
            return _numOutstandingObjects;
        }
    }

}