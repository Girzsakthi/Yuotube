using ARMilitary.AR;
using ARMilitary.Shared;
using UnityEngine;

namespace ARMilitary.Player
{
    public class PlayerController : MonoBehaviour
    {
        private MilitaryHUD _hud;

        private void Start()
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnSpawnReceived  += OnSpawnReceived;
                NetworkManager.Instance.OnRemoveReceived += OnRemoveReceived;
                NetworkManager.Instance.OnClearReceived  += OnClearReceived;
                NetworkManager.Instance.OnPeerConnected  += OnPeerConnected;
                NetworkManager.Instance.SetRole(AppMode.Player);
            }

            _hud = MilitaryHUD.Instance;
        }

        private void OnSpawnReceived(Data.SpawnPayload payload)
        {
            if (PlaneAnchorManager.Instance != null)
                PlaneAnchorManager.Instance.PlaceObject(payload);
        }

        private void OnRemoveReceived(string commandId)
        {
            if (PlaneAnchorManager.Instance != null)
                PlaneAnchorManager.Instance.RemoveObject(commandId);
        }

        private void OnClearReceived()
        {
            if (PlaneAnchorManager.Instance != null)
                PlaneAnchorManager.Instance.ClearAll();
        }

        private void OnPeerConnected(bool connected)
        {
            _hud?.SetPeerConnected(connected);
            Debug.Log("[ARMilitary] Instructor " + (connected ? "connected" : "disconnected"));
        }

        private void OnDestroy()
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnSpawnReceived  -= OnSpawnReceived;
                NetworkManager.Instance.OnRemoveReceived -= OnRemoveReceived;
                NetworkManager.Instance.OnClearReceived  -= OnClearReceived;
                NetworkManager.Instance.OnPeerConnected  -= OnPeerConnected;
            }
        }
    }
}
