using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }

    private static MainThreadDispatcher _instance = null;
    public static MainThreadDispatcher Instance()
    {
        if (_instance == null)
        {
            _instance = new GameObject("MainThreadDispatcher").AddComponent<MainThreadDispatcher>();
            DontDestroyOnLoad(_instance.gameObject);
        }
        return _instance;
    }
}
