using UnityEngine;
using UnityEngine.UI;
using IslandSurvival.Visual;

namespace IslandSurvival.UI
{
    public static class UIFactory
    {
        public static Font Font
        {
            get
            {
                var f = Resources.GetBuiltinResource<Font>("Arial.ttf");
                return f;
            }
        }

        public static Canvas CreateCanvas(string name, int sortOrder = 0)
        {
            VisualAssetFactory.Ensure();
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static RectTransform Panel(Transform parent, string name, Vector2 anchor, Vector2 size, Sprite sprite = null)
        {
            VisualAssetFactory.Ensure();
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.sprite = sprite ?? VisualAssetFactory.Panel;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            return rt;
        }

        public static Text Label(Transform parent, string name, string text, int size, TextAnchor align = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(8, 4);
            rt.offsetMax = new Vector2(-8, -4);
            var t = go.AddComponent<Text>();
            t.font = Font;
            t.fontSize = size;
            t.alignment = align;
            t.fontStyle = style;
            t.color = new Color(0.92f, 0.95f, 0.94f);
            t.text = text;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        public static Button Button(Transform parent, string label, Vector2 anchor, Vector2 size, UnityEngine.Events.UnityAction onClick)
        {
            VisualAssetFactory.Ensure();
            var go = new GameObject("Btn_" + label);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.sprite = VisualAssetFactory.Button;
            img.type = Image.Type.Sliced;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var cols = btn.colors;
            cols.normalColor = Color.white;
            cols.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            cols.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            cols.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            cols.fadeDuration = 0.12f;
            btn.colors = cols;
            if (onClick != null) btn.onClick.AddListener(onClick);

            var tg = new GameObject("Label");
            tg.transform.SetParent(go.transform, false);
            var trt = tg.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            var text = tg.AddComponent<Text>();
            text.text = label;
            text.font = Font;
            text.fontSize = Mathf.RoundToInt(size.y * 0.42f);
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            return btn;
        }

        public static Image Bar(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color fillColor)
        {
            VisualAssetFactory.Ensure();
            var bg = new GameObject(name + "_BG");
            bg.transform.SetParent(parent, false);
            var bgRt = bg.AddComponent<RectTransform>();
            bgRt.anchorMin = anchorMin;
            bgRt.anchorMax = anchorMax;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.sprite = VisualAssetFactory.BarBg;
            bgImg.type = Image.Type.Sliced;
            bgImg.color = new Color(0, 0, 0, 0.55f);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(bg.transform, false);
            var frt = fill.AddComponent<RectTransform>();
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.offsetMin = new Vector2(3, 3);
            frt.offsetMax = new Vector2(-3, -3);
            var fimg = fill.AddComponent<Image>();
            fimg.color = fillColor;
            fimg.type = Image.Type.Filled;
            fimg.fillMethod = Image.FillMethod.Horizontal;
            fimg.fillOrigin = 0;
            return fimg;
        }

        public static RectTransform Anchored(Transform parent, string name, Vector2 anchor, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.sizeDelta = size;
            return rt;
        }
    }
}
