using ARMilitary.AR;
using ARMilitary.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace ARMilitary.Player
{
    public class MilitaryHUD : MonoBehaviour
    {
        private Text    _coordsText;
        private Text    _signalText;
        private Text    _objectCountText;
        private Text    _statusText;
        private Image   _reticle;
        private Image   _signalIcon;
        private Canvas  _canvas;

        private float   _coordUpdateTimer;
        private const float CoordInterval = 0.5f;

        public static MilitaryHUD Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Build(Transform canvasParent)
        {
            _canvas = canvasParent.GetComponent<Canvas>();

            // ── Coordinates (top-right) ──────────────────────────────────────
            _coordsText = MakeText(canvasParent, "CoordsText", "00/00M",
                new Vector2(-10, -10), new Vector2(200, 40),
                TextAnchor.UpperRight, 18, new Color(0.2f, 1f, 0.2f));

            // ── Signal (top-left) ────────────────────────────────────────────
            _signalText = MakeText(canvasParent, "SignalText", "● ● ● ●",
                new Vector2(10, -10), new Vector2(120, 40),
                TextAnchor.UpperLeft, 16, new Color(0.2f, 1f, 0.2f));

            // ── Object count (bottom-left) ───────────────────────────────────
            _objectCountText = MakeText(canvasParent, "ObjectCount", "TARGETS: 0",
                new Vector2(10, 10), new Vector2(160, 36),
                TextAnchor.LowerLeft, 16, new Color(0.9f, 0.7f, 0.1f));

            // ── Status (bottom-right) ────────────────────────────────────────
            _statusText = MakeText(canvasParent, "StatusText", "SCANNING...",
                new Vector2(-10, 10), new Vector2(200, 36),
                TextAnchor.LowerRight, 14, new Color(0.2f, 1f, 0.2f));

            // ── Targeting reticle (center) ────────────────────────────────────
            _reticle = MakeReticle(canvasParent);
        }

        private void Update()
        {
            _coordUpdateTimer += Time.deltaTime;
            if (_coordUpdateTimer >= CoordInterval)
            {
                _coordUpdateTimer = 0f;
                UpdateCoordinates();
            }

            if (ARSessionController.Instance != null && _statusText != null)
            {
                bool tracking = ARSessionController.Instance.IsTracking;
                _statusText.text  = tracking ? "AR ACTIVE" : "ACQUIRING...";
                _statusText.color = tracking ? new Color(0.2f, 1f, 0.2f) : Color.yellow;
            }

            if (PlaneAnchorManager.Instance != null && _objectCountText != null)
                _objectCountText.text = "TARGETS: " + PlaneAnchorManager.Instance.SpawnedCount;
        }

        public void SetPeerConnected(bool connected)
        {
            if (_signalText == null) return;
            _signalText.text  = connected ? "● ● ● ●" : "○ ○ ○ ○";
            _signalText.color = connected ? new Color(0.2f, 1f, 0.2f) : Color.red;
        }

        private void UpdateCoordinates()
        {
            if (_coordsText == null) return;
            if (Camera.main != null)
            {
                var pos = Camera.main.transform.position;
                _coordsText.text = $"{pos.x:F1}/{pos.z:F1}M";
            }
        }

        // ─── Factory helpers ──────────────────────────────────────────────────

        private static Text MakeText(Transform parent, string name, string text,
                                     Vector2 offset, Vector2 size,
                                     TextAnchor alignment, int fontSize, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();

            // Anchor to correct corner based on alignment
            bool left   = alignment == TextAnchor.UpperLeft  || alignment == TextAnchor.LowerLeft;
            bool top    = alignment == TextAnchor.UpperLeft  || alignment == TextAnchor.UpperRight;
            var anchor  = new Vector2(left ? 0 : 1, top ? 1 : 0);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot     = anchor;
            rt.anchoredPosition = offset;
            rt.sizeDelta = size;

            var t = go.AddComponent<Text>();
            t.text      = text;
            t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize  = fontSize;
            t.color     = color;
            t.alignment = alignment;
            t.fontStyle = FontStyle.Bold;

            // Slight dark outline for readability
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.8f);
            outline.effectDistance = new Vector2(1, -1);

            return t;
        }

        private static Image MakeReticle(Transform parent)
        {
            var go = new GameObject("Reticle");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(60, 60);
            rt.anchoredPosition = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 1f, 0.2f, 0.7f);

            // Build crosshair from 4 child rects
            void MakeLine(string n, Vector2 size, Vector2 pos)
            {
                var lineGO = new GameObject(n);
                lineGO.transform.SetParent(go.transform, false);
                var lrt = lineGO.AddComponent<RectTransform>();
                lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 0.5f);
                lrt.sizeDelta = size;
                lrt.anchoredPosition = pos;
                lineGO.AddComponent<Image>().color = new Color(0.2f, 1f, 0.2f, 0.9f);
            }

            MakeLine("H", new Vector2(48, 2), Vector2.zero);
            MakeLine("V", new Vector2(2, 48), Vector2.zero);
            MakeLine("CenterDot", new Vector2(4, 4), Vector2.zero);
            img.color = Color.clear; // outer image transparent; lines form the cross

            return img;
        }
    }
}
