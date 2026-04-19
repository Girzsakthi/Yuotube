using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ARMilitary.AR
{
    public class ARSessionController : MonoBehaviour
    {
        public static ARSessionController Instance { get; private set; }

        public bool IsTracking { get; private set; }
        public bool IsAvailable { get; private set; }
        public event Action<bool> OnTrackingChanged;

        private ARSession _session;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Initialize(ARSession session)
        {
            _session = session;
            ARSession.stateChanged += OnStateChanged;
            StartCoroutine(ARSession.CheckAvailability());
        }

        private void OnStateChanged(ARSessionStateChangedEventArgs args)
        {
            IsAvailable = args.state != ARSessionState.Unsupported &&
                          args.state != ARSessionState.NeedsInstall;

            bool tracking = args.state == ARSessionState.SessionTracking;
            if (tracking != IsTracking)
            {
                IsTracking = tracking;
                OnTrackingChanged?.Invoke(IsTracking);
                Debug.Log("[ARMilitary] Tracking: " + IsTracking);
            }
        }

        private void OnDestroy()
        {
            ARSession.stateChanged -= OnStateChanged;
        }
    }
}
