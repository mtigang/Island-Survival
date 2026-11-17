using UnityEngine;
using UnityEngine.UI;
using IslandSurvival.Core;
using IslandSurvival.Visual;

namespace IslandSurvival.UI
{
    public class MainMenuController : MonoBehaviour
    {
        void Start()
        {
            VisualAssetFactory.Ensure();
            if (GameManager.Instance == null)
                new GameObject("GameManager").AddComponent<GameManager>();
            GameManager.Instance.SetState(GameState.MainMenu);
            BuildMenu();
        }

        void BuildMenu()
        {
            var canvas = UIFactory.CreateCanvas("MenuCanvas", 0);

            // Background gradient simulation
            var bg = new GameObject("BG");
            bg.transform.SetParent(canvas.transform, false);
            var bgRt = bg.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.12f, 0.16f, 1f);

            // Decorative horizon band
            var band = new GameObject("Horizon");
            band.transform.SetParent(canvas.transform, false);
            var bandRt = band.AddComponent<RectTransform>();
            bandRt.anchorMin = new Vector2(0, 0.35f);
            bandRt.anchorMax = new Vector2(1, 0.62f);
            bandRt.offsetMin = bandRt.offsetMax = Vector2.zero;
            band.AddComponent<Image>().color = new Color(0.08f, 0.28f, 0.32f, 0.5f);

            // Title card
            var titleCard = UIFactory.Panel(canvas.transform, "TitleCard", new Vector2(0.5f, 0.78f), new Vector2(720, 130), VisualAssetFactory.PanelDark);
            var title = UIFactory.Label(titleCard, "Title", "ISLAND SURVIVAL", 52, TextAnchor.MiddleCenter, FontStyle.Bold);
            title.color = new Color(0.85f, 0.95f, 0.9f);
            var sub = UIFactory.Label(titleCard, "Sub", "Explore  ·  Gather  ·  Build  ·  Survive", 18, TextAnchor.LowerCenter);
            sub.color = new Color(0.55f, 0.8f, 0.7f);
            var srt = sub.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.1f, 0.05f);
            srt.anchorMax = new Vector2(0.9f, 0.4f);

            UIFactory.Button(canvas.transform, "New Game", new Vector2(0.5f, 0.48f), new Vector2(300, 56),
                () => GameManager.Instance.StartNewGame());
            UIFactory.Button(canvas.transform, "Continue", new Vector2(0.5f, 0.38f), new Vector2(300, 56),
                () =>
                {
                    if (SaveSystem.SaveSystem.HasSave()) GameManager.Instance.ContinueGame();
                    else GameManager.Instance.StartNewGame();
                });
            UIFactory.Button(canvas.transform, "Quit", new Vector2(0.5f, 0.28f), new Vector2(300, 56),
                () => GameManager.Instance.Quit());

            var foot = UIFactory.Label(canvas.transform, "Foot", "Unity 2022.3 LTS  ·  URP  ·  v1.1 Visual Update", 14, TextAnchor.MiddleCenter);
            var frt = foot.GetComponent<RectTransform>();
            frt.anchorMin = frt.anchorMax = new Vector2(0.5f, 0.06f);
            frt.sizeDelta = new Vector2(800, 30);
            foot.color = new Color(0.5f, 0.6f, 0.58f, 0.8f);
        }
    }
}
