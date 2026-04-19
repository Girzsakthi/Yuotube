using UnityEngine;
using UnityEngine.UI;

namespace ARMilitary.Shared
{
    public class ModeSelectUI : MonoBehaviour
    {
        public System.Action<AppMode> OnModeSelected;

        public void Build(Canvas canvas)
        {
            var root = canvas.transform;

            // Full-screen dark background
            var bg = new GameObject("BG");
            bg.transform.SetParent(root, false);
            var bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.03f, 0.06f, 0.03f, 1f);

            // Title
            MakeLabel(root, "AR MILITARY TRAINER",
                new Vector2(0.5f, 0.75f), new Vector2(400, 60), 32,
                new Color(0.3f, 1f, 0.3f));

            MakeLabel(root, "v1.0 — Select Device Role",
                new Vector2(0.5f, 0.65f), new Vector2(300, 35), 16,
                new Color(0.5f, 0.8f, 0.5f));

            // Instructor button
            MakeRoleButton(root, "INSTRUCTOR", "Select & send AR objects to player",
                new Vector2(0.5f, 0.45f), new Color(0.1f, 0.35f, 0.1f),
                () => OnModeSelected?.Invoke(AppMode.Instructor));

            // Player button
            MakeRoleButton(root, "PLAYER", "View AR objects in the real world",
                new Vector2(0.5f, 0.28f), new Color(0.15f, 0.25f, 0.4f),
                () => OnModeSelected?.Invoke(AppMode.Player));

            // Footer note
            MakeLabel(root, "Both devices must be on the same WiFi network",
                new Vector2(0.5f, 0.08f), new Vector2(380, 30), 13,
                new Color(0.4f, 0.6f, 0.4f));
        }

        private static void MakeRoleButton(Transform parent, string title, string subtitle,
                                            Vector2 anchor, Color bgColor, System.Action onClick)
        {
            var go = new GameObject("Btn_" + title);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(280, 80);
            rt.anchoredPosition = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = bgColor;
            var btn = go.AddComponent<Button>();
            var cols = btn.colors;
            cols.highlightedColor = bgColor * 1.4f;
            cols.pressedColor     = bgColor * 0.6f;
            btn.colors = cols;
            btn.onClick.AddListener(() => onClick());

            MakeLabelChild(go.transform, title,    new Vector2(0.5f, 0.65f), new Vector2(260, 36), 24, Color.white, FontStyle.Bold);
            MakeLabelChild(go.transform, subtitle, new Vector2(0.5f, 0.2f),  new Vector2(260, 24), 13, new Color(0.8f, 0.9f, 0.8f), FontStyle.Normal);
        }

        private static void MakeLabelChild(Transform parent, string text, Vector2 anchor,
                                            Vector2 size, int fontSize, Color color, FontStyle style)
        {
            var go = new GameObject("Lbl");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
            var t = go.AddComponent<Text>();
            t.text      = text;
            t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize  = fontSize;
            t.color     = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.fontStyle = style;
        }

        private static void MakeLabel(Transform parent, string text, Vector2 anchor,
                                       Vector2 size, int fontSize, Color color)
        {
            var go = new GameObject("Lbl_" + text);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
            var t = go.AddComponent<Text>();
            t.text      = text;
            t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize  = fontSize;
            t.color     = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.fontStyle = FontStyle.Bold;
        }
    }
}
