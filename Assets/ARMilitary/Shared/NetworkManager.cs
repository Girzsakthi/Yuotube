using System;
using System.Collections.Generic;
using ARMilitary.Data;
using ARMilitary.Network;
using UnityEngine;

namespace ARMilitary.Shared
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        public event Action<SpawnPayload> OnSpawnReceived;
        public event Action<string> OnRemoveReceived;
        public event Action OnClearReceived;
        public event Action<bool> OnPeerConnected;

        public bool HasPeer { get; private set; }

        private UdpBroadcaster _broadcaster;
        private AppMode _role;
        private float _peerTimeout = 5f;
        private float _peerTimer;
        private float _heartbeatInterval = 2f;
        private float _heartbeatTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _broadcaster = new UdpBroadcaster();
            _broadcaster.StartListening();
        }

        public void SetRole(AppMode role) => _role = role;

        private void Update()
        {
            while (_broadcaster.TryDequeueRaw(out var json))
                ProcessRaw(json);

            _heartbeatTimer += Time.deltaTime;
            if (_heartbeatTimer >= _heartbeatInterval)
            {
                _heartbeatTimer = 0f;
                SendEnvelope("HEARTBEAT", null);
            }

            if (HasPeer)
            {
                _peerTimer += Time.deltaTime;
                if (_peerTimer > _peerTimeout)
                {
                    HasPeer = false;
                    OnPeerConnected?.Invoke(false);
                }
            }
        }

        private void ProcessRaw(string json)
        {
            try
            {
                var envelope = JsonUtility.FromJson<UdpEnvelope>(json);
                if (envelope == null || envelope.senderId == _broadcaster.DeviceId) return;

                _peerTimer = 0f;
                if (!HasPeer)
                {
                    HasPeer = true;
                    OnPeerConnected?.Invoke(true);
                }

                switch (envelope.messageType)
                {
                    case "SPAWN":
                        var payload = JsonUtility.FromJson<SpawnPayload>(envelope.payload);
                        if (payload != null) OnSpawnReceived?.Invoke(payload);
                        break;
                    case "REMOVE":
                        OnRemoveReceived?.Invoke(envelope.payload);
                        break;
                    case "CLEAR":
                        OnClearReceived?.Invoke();
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ARMilitary] Bad UDP packet: " + e.Message);
            }
        }

        public void SendSpawnCommands(List<SpawnPayload> payloads)
        {
            foreach (var p in payloads)
                SendEnvelope("SPAWN", JsonUtility.ToJson(p));
        }

        public void SendRemove(string commandId) => SendEnvelope("REMOVE", commandId);
        public void SendClear() => SendEnvelope("CLEAR", null);

        private void SendEnvelope(string type, string payload)
        {
            var envelope = new UdpEnvelope
            {
                messageType = type,
                senderId = _broadcaster.DeviceId,
                senderRole = _role.ToString(),
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                payload = payload ?? ""
            };
            _broadcaster.Send(JsonUtility.ToJson(envelope));
        }

        private void OnDestroy()
        {
            _broadcaster?.Dispose();
        }
    }
}
