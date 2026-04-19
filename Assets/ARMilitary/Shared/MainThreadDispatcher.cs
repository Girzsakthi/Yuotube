using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace ARMilitary.Shared
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        public static MainThreadDispatcher Instance { get; private set; }

        private readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Enqueue(Action action)
        {
            if (action != null)
                _queue.Enqueue(action);
        }

        private void Update()
        {
            while (_queue.TryDequeue(out var action))
            {
                try { action(); }
                catch (Exception e) { Debug.LogError($"[ARMilitary] Dispatch error: {e}"); }
            }
        }
    }
}
