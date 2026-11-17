using UnityEngine;
using UnityEngine.UI;
using IslandSurvival.Core;

namespace IslandSurvival.UI
{
    public class MainMenuController : MonoBehaviour
    {
        void Start()
        {
            if (GameManager.Instance == null)
            {
                var gm = new GameObject("GameManager");
                gm.AddComponent<GameManager>();
            }
            GameManager.Instance.SetState(GameState.MainMenu);
            BuildMenu();
        }

        void BuildMenu()
        {
            var canvas = new GameObject("MenuCanvas").AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvas.gameObject.AddComponent<GraphicRaycaster>();
            canvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);

            var bg = new GameObject("BG");
            bg.transform.SetParent(canvas.transform, false);
            var bgRt = bg.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
            bg.AddComponent<Image>().color = new Color(0.05f, 0.12f, 0.18f, 1f);

            var title = CreateText(canvas.transform, "ISLAND SURVIVAL", 64, new Vector2(0.5f, 0.78f), FontStyle.Bold);
            CreateText(canvas.transform, "Explore · Gather · Build · Survive", 22, new Vector2(0.5f, 0.7f), FontStyle.Normal);

            CreateButton(canvas.transform, "New Game", new Vector2(0.5f, 0.52f), () => GameManager.Instance.StartNewGame());
            CreateButton(canvas.transform, "Continue", new Vector2(0.5f, 0.42f), () =>
            {
                if (SaveSystem.SaveSystem.HasSave()) GameManager.Instance.ContinueGame();
                else GameManager.Instance.StartNewGame();
            });
            CreateButton(canvas.transform, "Quit", new Vector2(0.5f, 0.32f), () => GameManager.Instance.Quit());

            CreateText(canvas.transform, "Unity 2022.3 LTS · URP · v1.0", 14, new Vector2(0.5f, 0.08f), FontStyle.Normal);
        }

        Text CreateText(Transform parent, string msg, int size, Vector2 anchor, FontStyle style)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.sizeDelta = new Vector2(900, 80);
            var t = go.AddComponent<Text>();
            t.text = msg;
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = size;
            t.alignment = TextAnchor.MiddleCenter;
            t.fontStyle = style;
            t.color = Color.white;
            return t;
        }

        void CreateButton(Transform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.sizeDelta = new Vector2(280, 52);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.45f, 0.35f, 0.95f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(action);
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.2f, 0.6f, 0.45f);
            colors.pressedColor = new Color(0.1f, 0.35f, 0.28f);
            btn.colors = colors;

            var tg = new GameObject("Label");
            tg.transform.SetParent(go.transform, false);
            var trt = tg.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            var text = tg.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
        }
    }
}
