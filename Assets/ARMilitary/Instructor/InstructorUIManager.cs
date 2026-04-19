using System.Collections.Generic;
using ARMilitary.Data;
using ARMilitary.Primitives;
using ARMilitary.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace ARMilitary.Instructor
{
    public class InstructorUIManager : MonoBehaviour
    {
        private InstructorController _controller;
        private Transform _canvasRoot;
        private GridLayoutGroup _grid;
        private readonly List<ObjectTileButton> _tiles = new List<ObjectTileButton>();
        private Text _statusText;
        private Text _peerText;

        private void Awake()
        {
            _controller = GetComponent<InstructorController>();
            if (_controller == null) _controller = gameObject.AddComponent<InstructorController>();
        }

        private void Start()
        {
            if (NetworkManager.Instance != null)
                NetworkManager.Instance.OnPeerConnected += OnPeerConnected;
        }

        public void BuildUI(Canvas canvas)
        {
            _canvasRoot = canvas.transform;
            BuildBackground();
            BuildTopBar();
            BuildCategoryRow();
            BuildObjectGrid();
            BuildSendBar();
        }

        // ── Dark panel background ────────────────────────────────────────────
        private void BuildBackground()
        {
            var bg = new GameObject("Background");
            bg.transform.SetParent(_canvasRoot, false);
            var rt = bg.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = bg.AddComponent<Image>();
            img.color = new Color(0.05f, 0.08f, 0.05f, 0.92f);
        }

        // ── Top bar: title + WiFi indicator ─────────────────────────────────
        private void BuildTopBar()
        {
            var bar = MakeRect("TopBar", _canvasRoot,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -60), new Vector2(0, 0));
            bar.GetComponent<Image>().color = new Color(0.1f, 0.2f, 0.1f, 1f);

            MakeLabelIn(bar, "INSTRUCTOR CONTROL", new Vector2(0.5f, 0.5f),
                        new Vector2(-60, 0), new Vector2(300, 50), 22, TextAnchor.MiddleCenter,
                        new Color(0.3f, 1f, 0.3f));

            _peerText = MakeLabelIn(bar, "NO SIGNAL", new Vector2(1f, 0.5f),
                                    new Vector2(-10, 0), new Vector2(120, 40), 14, TextAnchor.MiddleRight,
                                    Color.red);
        }

        // ── Category filter row ──────────────────────────────────────────────
        private void BuildCategoryRow()
        {
            var row = MakeRect("CategoryRow", _canvasRoot,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -110), new Vector2(0, -60));
            row.GetComponent<Image>().color = new Color(0.08f, 0.13f, 0.08f, 1f);

            var hlg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 5, 5);
            hlg.spacing = 8;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            var categories = new[] { Category.All, Category.Ground, Category.Air, Category.Show };
            foreach (var cat in categories)
            {
                var btn = MakeButton(row, cat.ToString().ToUpper(),
                                     new Color(0.15f, 0.3f, 0.15f), new Color(0.2f, 1f, 0.2f));
                var captured = cat;
                btn.onClick.AddListener(() =>
                {
                    _controller.SetCategory(captured);
                    RefreshGrid();
                });
            }
        }

        // ── Object grid ──────────────────────────────────────────────────────
        private void BuildObjectGrid()
        {
            var panel = MakeRect("GridPanel", _canvasRoot,
                new Vector2(0, 0.25f), new Vector2(1, 1),
                new Vector2(0, -110), new Vector2(0, -110));
            panel.GetComponent<Image>().color = new Color(0.07f, 0.1f, 0.07f, 1f);

            var glg = panel.gameObject.AddComponent<GridLayoutGroup>();
            glg.padding = new RectOffset(16, 16, 16, 16);
            glg.cellSize = new Vector2(140, 140);
            glg.spacing = new Vector2(16, 16);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;

            _grid = glg;
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            foreach (var t in _tiles) if (t != null) Destroy(t.gameObject);
            _tiles.Clear();

            var visible = _controller.Catalogue.Filter(_controller.ActiveCategory);
            foreach (var entry in visible)
            {
                var tile = ObjectTileButton.Create(_grid.transform, entry, _controller);
                _tiles.Add(tile);
            }
        }

        // ── Send bar ─────────────────────────────────────────────────────────
        private void BuildSendBar()
        {
            var bar = MakeRect("SendBar", _canvasRoot,
                Vector2.zero, new Vector2(1, 0.25f),
                Vector2.zero, Vector2.zero);
            bar.GetComponent<Image>().color = new Color(0.05f, 0.1f, 0.05f, 1f);

            var hlg = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 12, 12);
            hlg.spacing = 12;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = true;

            // CLEAR button
            var clearBtn = MakeButton(bar, "CLEAR", new Color(0.4f, 0.1f, 0.1f), Color.white);
            clearBtn.GetComponent<LayoutElement>().preferredWidth = 120;
            clearBtn.onClick.AddListener(() => _controller.ClearAll());

            // SEND button (prominent green)
            var sendBtn = MakeButton(bar, "SEND", new Color(0.1f, 0.5f, 0.1f), Color.white, 26);
            var le = sendBtn.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            sendBtn.onClick.AddListener(() =>
            {
                _controller.SendSelected();
                foreach (var tile in _tiles) tile.RefreshVisual();
            });

            _statusText = MakeLabelIn(bar, "Select targets above", new Vector2(0f, 0.5f),
                                      new Vector2(0, 0), new Vector2(0, 30), 13, TextAnchor.MiddleLeft,
                                      new Color(0.6f, 0.9f, 0.6f));
        }

        private void OnPeerConnected(bool connected)
        {
            if (_peerText == null) return;
            _peerText.text  = connected ? "● LINKED" : "NO SIGNAL";
            _peerText.color = connected ? new Color(0.2f, 1f, 0.2f) : Color.red;
        }

        private void OnDestroy()
        {
            if (NetworkManager.Instance != null)
                NetworkManager.Instance.OnPeerConnected -= OnPeerConnected;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static RectTransform MakeRect(string name, Transform parent,
                                               Vector2 anchorMin, Vector2 anchorMax,
                                               Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
            go.AddComponent<Image>(); // placeholder; caller sets color
            return rt;
        }

        private static Button MakeButton(Transform parent, string label,
                                          Color bgColor, Color textColor, int fontSize = 18)
        {
            var go = new GameObject("Btn_" + label);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = bgColor * 1.3f;
            colors.pressedColor     = bgColor * 0.7f;
            btn.colors = colors;

            var txtGO = new GameObject("Text");
            txtGO.transform.SetParent(go.transform, false);
            var trt = txtGO.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            var t = txtGO.AddComponent<Text>();
            t.text = label;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize  = fontSize;
            t.color     = textColor;
            t.alignment = TextAnchor.MiddleCenter;
            t.fontStyle = FontStyle.Bold;

            return btn;
        }

        private static Text MakeLabelIn(RectTransform parent, string text,
                                         Vector2 anchor, Vector2 anchoredPos, Vector2 size,
                                         int fontSize, TextAnchor alignment, Color color)
        {
            var go = new GameObject("Lbl_" + text);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var t = go.AddComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = alignment;
            t.fontStyle = FontStyle.Bold;
            return t;
        }
    }
}
