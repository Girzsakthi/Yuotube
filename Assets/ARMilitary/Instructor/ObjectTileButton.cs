using ARMilitary.Data;
using ARMilitary.Primitives;
using UnityEngine;
using UnityEngine.UI;

namespace ARMilitary.Instructor
{
    public class ObjectTileButton : MonoBehaviour
    {
        private CatalogueEntry    _entry;
        private InstructorController _controller;
        private Image  _bg;
        private Image  _previewImg;
        private Text   _label;

        private static readonly Color SelectedColor   = new Color(0.1f, 0.5f, 0.1f);
        private static readonly Color UnselectedColor = new Color(0.12f, 0.18f, 0.12f);

        public static ObjectTileButton Create(Transform parent, CatalogueEntry entry,
                                              InstructorController controller)
        {
            var go = new GameObject("Tile_" + entry.displayName);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = Vector2.zero;

            var tile = go.AddComponent<ObjectTileButton>();
            tile._entry = entry;
            tile._controller = controller;
            tile.Build();
            return tile;
        }

        private void Build()
        {
            _bg = gameObject.AddComponent<Image>();
            _bg.color = UnselectedColor;

            var btn = gameObject.AddComponent<Button>();
            btn.targetGraphic = _bg;
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.2f, 0.6f, 0.2f);
            colors.pressedColor     = new Color(0.05f, 0.35f, 0.05f);
            btn.colors = colors;
            btn.onClick.AddListener(OnClick);

            // Border
            var border = new GameObject("Border");
            border.transform.SetParent(transform, false);
            var brt = border.AddComponent<RectTransform>();
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = new Vector2(-2, -2); brt.offsetMax = new Vector2(2, 2);
            var borderImg = border.AddComponent<Image>();
            borderImg.color = _entry.tintColor * 0.6f;

            // Preview area (top 65%)
            var previewGO = new GameObject("Preview");
            previewGO.transform.SetParent(transform, false);
            var prt = previewGO.AddComponent<RectTransform>();
            prt.anchorMin = new Vector2(0, 0.35f); prt.anchorMax = Vector2.one;
            prt.offsetMin = new Vector2(8, 0); prt.offsetMax = new Vector2(-8, -8);
            _previewImg = previewGO.AddComponent<Image>();
            _previewImg.color = _entry.tintColor * 0.3f;

            // Label (bottom 30%)
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(transform, false);
            var lrt = labelGO.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = new Vector2(1, 0.35f);
            lrt.offsetMin = new Vector2(4, 4); lrt.offsetMax = new Vector2(-4, 0);
            _label = labelGO.AddComponent<Text>();
            _label.text = _entry.displayName;
            _label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _label.fontSize  = 16;
            _label.color     = _entry.tintColor;
            _label.alignment = TextAnchor.MiddleCenter;
            _label.fontStyle = FontStyle.Bold;

            // Render thumbnail asynchronously
            StartCoroutine(RenderThumbnail());
        }

        private System.Collections.IEnumerator RenderThumbnail()
        {
            yield return new WaitForEndOfFrame();
            var tex = ThumbnailRenderer.Render(_entry.objectType, 128, 128);
            if (tex != null && _previewImg != null)
                _previewImg.sprite = Sprite.Create(tex,
                    new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
        }

        private void OnClick()
        {
            _controller.ToggleSelect(_entry.objectType);
            RefreshVisual();
        }

        public void RefreshVisual()
        {
            bool sel = _controller.IsSelected(_entry.objectType);
            _bg.color    = sel ? SelectedColor : UnselectedColor;
            _label.color = sel ? Color.white   : _entry.tintColor;
        }
    }
}
