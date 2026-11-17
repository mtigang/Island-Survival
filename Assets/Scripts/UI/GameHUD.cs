using UnityEngine;
using UnityEngine.UI;
using IslandSurvival.Core;
using IslandSurvival.Interaction;
using IslandSurvival.Survival;
using IslandSurvival.Inventory;
using IslandSurvival.Environment;
using IslandSurvival.Weather;
using IslandSurvival.Crafting;
using IslandSurvival.Building;

namespace IslandSurvival.UI
{
    public class GameHUD : MonoBehaviour
    {
        Canvas _canvas;
        Text _prompt, _notify, _timeTxt, _weatherTxt;
        Image _hunger, _thirst, _energy;
        GameObject _invPanel, _craftPanel, _pausePanel, _buildPanel;
        Text _invText, _craftText;
        float _notifyTimer;
        bool _invOpen, _craftOpen, _pauseOpen, _buildOpen;

        void Start() => BuildUI();

        void BuildUI()
        {
            _canvas = new GameObject("HUD_Canvas").AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvas.gameObject.AddComponent<GraphicRaycaster>();
            var scaler = _canvas.GetComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(1920, 1080);

            _prompt = MakeText("Prompt", new Vector2(0.5f, 0.35f), 22, TextAnchor.MiddleCenter, FontStyle.Bold);
            _notify = MakeText("Notify", new Vector2(0.5f, 0.75f), 20, TextAnchor.MiddleCenter, FontStyle.Normal);
            _timeTxt = MakeText("Time", new Vector2(0.9f, 0.95f), 18, TextAnchor.MiddleRight, FontStyle.Normal);
            _weatherTxt = MakeText("Weather", new Vector2(0.9f, 0.91f), 16, TextAnchor.MiddleRight, FontStyle.Normal);

            _hunger = MakeBar("Hunger", new Vector2(0.08f, 0.95f), new Color(0.9f, 0.45f, 0.15f));
            _thirst = MakeBar("Thirst", new Vector2(0.08f, 0.91f), new Color(0.25f, 0.55f, 0.95f));
            _energy = MakeBar("Energy", new Vector2(0.08f, 0.87f), new Color(0.95f, 0.85f, 0.2f));

            // Crosshair
            var cross = MakePanel("Crosshair", new Vector2(0.5f, 0.5f), new Vector2(6, 6), new Color(1, 1, 1, 0.7f));

            _invPanel = MakePanel("Inventory", new Vector2(0.5f, 0.5f), new Vector2(520, 420), new Color(0.08f, 0.1f, 0.12f, 0.92f));
            _invText = MakeChildText(_invPanel.transform, "InvBody", 16);
            _invPanel.SetActive(false);

            _craftPanel = MakePanel("Craft", new Vector2(0.5f, 0.5f), new Vector2(560, 480), new Color(0.08f, 0.1f, 0.12f, 0.92f));
            _craftText = MakeChildText(_craftPanel.transform, "CraftBody", 15);
            _craftPanel.SetActive(false);

            _buildPanel = MakePanel("Build", new Vector2(0.5f, 0.5f), new Vector2(480, 360), new Color(0.08f, 0.1f, 0.12f, 0.92f));
            var buildTxt = MakeChildText(_buildPanel.transform, "BuildBody", 16);
            buildTxt.text = "BUILD MENU\n\n1 Foundation\n2 Wall\n3 Floor\n4 Roof\n5 Doorway\n6 Storage Box\n7 Campfire\n\nPress number to place\nQ cancel  R rotate  LMB confirm";
            _buildPanel.SetActive(false);

            _pausePanel = MakePanel("Pause", new Vector2(0.5f, 0.5f), new Vector2(360, 320), new Color(0.05f, 0.07f, 0.09f, 0.95f));
            var pauseTxt = MakeChildText(_pausePanel.transform, "PauseBody", 20);
            pauseTxt.text = "PAUSED\n\nEsc Resume\nF5 Save\nM Main Menu";
            _pausePanel.SetActive(false);

            GameEvents.OnNotification += OnNotify;
        }

        void OnDestroy() => GameEvents.OnNotification -= OnNotify;

        void OnNotify(string msg)
        {
            if (_notify != null) { _notify.text = msg; _notifyTimer = 2.5f; }
        }

        void Update()
        {
            if (GameManager.Instance == null) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_invOpen || _craftOpen || _buildOpen) { CloseAllMenus(); return; }
                TogglePause();
            }
            if (GameManager.Instance.State == GameState.Paused)
            {
                if (Input.GetKeyDown(KeyCode.M)) GameManager.Instance.LoadMainMenu();
                return;
            }
            if (GameManager.Instance.State != GameState.Playing) return;

            if (Input.GetKeyDown(KeyCode.Tab)) ToggleInv();
            if (Input.GetKeyDown(KeyCode.C)) ToggleCraft();
            if (Input.GetKeyDown(KeyCode.B)) ToggleBuild();
            if (Input.GetKeyDown(KeyCode.F5)) SaveSystem.SaveSystem.Instance?.SaveGame();

            if (_buildOpen)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) BuildingSystem.Instance?.StartBuild("foundation");
                if (Input.GetKeyDown(KeyCode.Alpha2)) BuildingSystem.Instance?.StartBuild("wall");
                if (Input.GetKeyDown(KeyCode.Alpha3)) BuildingSystem.Instance?.StartBuild("floor");
                if (Input.GetKeyDown(KeyCode.Alpha4)) BuildingSystem.Instance?.StartBuild("roof");
                if (Input.GetKeyDown(KeyCode.Alpha5)) BuildingSystem.Instance?.StartBuild("doorway");
                if (Input.GetKeyDown(KeyCode.Alpha6)) BuildingSystem.Instance?.StartBuild("storage_box");
                if (Input.GetKeyDown(KeyCode.Alpha7)) BuildingSystem.Instance?.StartBuild("campfire");
            }

            if (_craftOpen && CraftingSystem.Instance != null)
            {
                for (int i = 0; i < Mathf.Min(9, CraftingSystem.Instance.recipes.Count); i++)
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                        CraftingSystem.Instance.Craft(CraftingSystem.Instance.recipes[i]);
            }

            // HUD values
            if (InteractionSystem.Instance != null && _prompt != null)
                _prompt.text = InteractionSystem.Instance.CurrentPrompt ?? "";

            if (_notifyTimer > 0f) { _notifyTimer -= Time.unscaledDeltaTime; if (_notifyTimer <= 0f && _notify) _notify.text = ""; }

            var stats = SurvivalStats.Instance;
            if (stats != null)
            {
                SetBar(_hunger, stats.hunger / 100f);
                SetBar(_thirst, stats.thirst / 100f);
                SetBar(_energy, stats.energy / 100f);
            }

            if (DayNightCycle.Instance != null && _timeTxt != null)
                _timeTxt.text = DayNightCycle.Instance.GetPhaseName();
            if (WeatherSystem.Instance != null && _weatherTxt != null)
                _weatherTxt.text = WeatherSystem.Instance.current;

            if (_invOpen) RefreshInv();
            if (_craftOpen) RefreshCraft();
        }

        void TogglePause()
        {
            _pauseOpen = !_pauseOpen;
            _pausePanel.SetActive(_pauseOpen);
            GameManager.Instance.SetState(_pauseOpen ? GameState.Paused : GameState.Playing);
        }

        void ToggleInv()
        {
            _invOpen = !_invOpen;
            _invPanel.SetActive(_invOpen);
            if (_invOpen) { _craftOpen = false; _craftPanel.SetActive(false); _buildOpen = false; _buildPanel.SetActive(false); }
            Cursor.lockState = _invOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = _invOpen;
        }

        void ToggleCraft()
        {
            _craftOpen = !_craftOpen;
            _craftPanel.SetActive(_craftOpen);
            if (_craftOpen) { _invOpen = false; _invPanel.SetActive(false); _buildOpen = false; _buildPanel.SetActive(false); RefreshCraft(); }
            Cursor.lockState = _craftOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = _craftOpen;
        }

        void ToggleBuild()
        {
            _buildOpen = !_buildOpen;
            _buildPanel.SetActive(_buildOpen);
            if (_buildOpen) { _invOpen = false; _invPanel.SetActive(false); _craftOpen = false; _craftPanel.SetActive(false); }
            Cursor.lockState = _buildOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = _buildOpen;
        }

        void CloseAllMenus()
        {
            _invOpen = _craftOpen = _buildOpen = false;
            _invPanel.SetActive(false); _craftPanel.SetActive(false); _buildPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        }

        void RefreshInv()
        {
            if (_invText == null || InventorySystem.Instance == null) return;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("INVENTORY  (Tab close | RMB use hotbar item)");
            sb.AppendLine("Hotbar select: 1-6");
            sb.AppendLine("----------------------------");
            var slots = InventorySystem.Instance.slots;
            for (int i = 0; i < slots.Length; i++)
            {
                string mark = i == InventorySystem.Instance.selectedHotbar ? ">" : " ";
                if (slots[i].IsEmpty) sb.AppendLine($"{mark}[{i + 1}] —");
                else
                {
                    var def = ItemDatabase.Instance?.Get(slots[i].itemId);
                    string n = def != null ? def.displayName : slots[i].itemId;
                    sb.AppendLine($"{mark}[{i + 1}] {n} x{slots[i].count}");
                }
            }
            _invText.text = sb.ToString();
        }

        void RefreshCraft()
        {
            if (_craftText == null || CraftingSystem.Instance == null) return;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("CRAFTING  (C close | press 1-9 to craft)");
            sb.AppendLine("----------------------------");
            int idx = 1;
            foreach (var r in CraftingSystem.Instance.recipes)
            {
                bool ok = CraftingSystem.Instance.CanCraft(r);
                sb.Append(ok ? "✓ " : "✗ ");
                sb.Append(idx++).Append(") ").Append(r.displayName).Append("  needs: ");
                foreach (var ing in r.ingredients)
                    sb.Append(ing.itemId).Append("x").Append(ing.amount).Append(" ");
                sb.AppendLine();
            }
            _craftText.text = sb.ToString();
        }

        Text MakeText(string name, Vector2 anchor, int size, TextAnchor align, FontStyle style)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_canvas.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.sizeDelta = new Vector2(700, 40);
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = size;
            t.alignment = align;
            t.fontStyle = style;
            t.color = Color.white;
            t.text = "";
            return t;
        }

        Image MakeBar(string name, Vector2 anchor, Color color)
        {
            var bg = MakePanel(name + "BG", anchor, new Vector2(180, 16), new Color(0, 0, 0, 0.5f));
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(bg.transform, false);
            var rt = fillGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(2, 2); rt.offsetMax = new Vector2(-2, -2);
            var img = fillGo.AddComponent<Image>();
            img.color = color;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            return img;
        }

        void SetBar(Image img, float t)
        {
            if (img) img.fillAmount = Mathf.Clamp01(t);
        }

        GameObject MakePanel(string name, Vector2 anchor, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_canvas.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        Text MakeChildText(Transform parent, string name, int size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(16, 16); rt.offsetMax = new Vector2(-16, -16);
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = size;
            t.alignment = TextAnchor.UpperLeft;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }
    }
}
