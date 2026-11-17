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
using IslandSurvival.Visual;

namespace IslandSurvival.UI
{
    public class GameHUD : MonoBehaviour
    {
        Canvas _canvas;
        Text _prompt, _notify, _timeTxt, _weatherTxt, _hotbarHint;
        Image _hunger, _thirst, _energy;
        Image[] _hotbarSlots = new Image[6];
        Image[] _hotbarIcons = new Image[6];
        Text[] _hotbarCounts = new Text[6];
        GameObject _invPanel, _craftPanel, _pausePanel, _buildPanel, _settingsPanel;
        Text _invText, _craftText;
        float _notifyTimer;
        bool _invOpen, _craftOpen, _pauseOpen, _buildOpen, _settingsOpen;
        CanvasGroup _notifyCg;

        void Start()
        {
            VisualAssetFactory.Ensure();
            BuildUI();
            GameEvents.OnNotification += OnNotify;
            GameEvents.OnInventoryChanged += RefreshHotbar;
        }

        void OnDestroy()
        {
            GameEvents.OnNotification -= OnNotify;
            GameEvents.OnInventoryChanged -= RefreshHotbar;
        }

        void BuildUI()
        {
            _canvas = UIFactory.CreateCanvas("HUD_Canvas", 10);

            // Crosshair
            var cross = UIFactory.Anchored(_canvas.transform, "Crosshair", new Vector2(0.5f, 0.5f), new Vector2(8, 8));
            var cimg = cross.gameObject.AddComponent<Image>();
            cimg.sprite = VisualAssetFactory.Circle;
            cimg.color = new Color(1, 1, 1, 0.55f);

            // Prompt
            var promptRt = UIFactory.Panel(_canvas.transform, "PromptPanel", new Vector2(0.5f, 0.28f), new Vector2(420, 44), VisualAssetFactory.PanelDark);
            _prompt = UIFactory.Label(promptRt, "Prompt", "", 20, TextAnchor.MiddleCenter, FontStyle.Bold);
            promptRt.gameObject.SetActive(false);
            _prompt.raycastTarget = false;

            // Notification
            var notifyRt = UIFactory.Panel(_canvas.transform, "Notify", new Vector2(0.5f, 0.82f), new Vector2(480, 48), VisualAssetFactory.Panel);
            _notify = UIFactory.Label(notifyRt, "NotifyText", "", 18);
            _notifyCg = notifyRt.gameObject.AddComponent<CanvasGroup>();
            _notifyCg.alpha = 0;

            // Survival bars (top-left)
            var barsRoot = UIFactory.Anchored(_canvas.transform, "Bars", new Vector2(0.02f, 0.97f), new Vector2(260, 90));
            barsRoot.pivot = new Vector2(0, 1);
            barsRoot.anchorMin = new Vector2(0.02f, 0.97f);
            barsRoot.anchorMax = new Vector2(0.02f, 0.97f);
            _hunger = UIFactory.Bar(barsRoot, "Hunger", new Vector2(0, 0.7f), new Vector2(1, 0.95f), new Color(0.92f, 0.48f, 0.18f));
            _thirst = UIFactory.Bar(barsRoot, "Thirst", new Vector2(0, 0.38f), new Vector2(1, 0.63f), new Color(0.28f, 0.6f, 0.95f));
            _energy = UIFactory.Bar(barsRoot, "Energy", new Vector2(0, 0.06f), new Vector2(1, 0.31f), new Color(0.95f, 0.82f, 0.2f));

            // Time / weather top-right
            var tr = UIFactory.Panel(_canvas.transform, "TimePanel", new Vector2(0.92f, 0.95f), new Vector2(200, 64), VisualAssetFactory.PanelDark);
            _timeTxt = UIFactory.Label(tr, "Time", "Morning", 18, TextAnchor.UpperCenter, FontStyle.Bold);
            _weatherTxt = UIFactory.Label(tr, "Weather", "Clear", 15, TextAnchor.LowerCenter);

            // Hotbar bottom center
            var hotbar = UIFactory.Panel(_canvas.transform, "Hotbar", new Vector2(0.5f, 0.06f), new Vector2(420, 72), VisualAssetFactory.PanelDark);
            for (int i = 0; i < 6; i++)
            {
                float x = (i - 2.5f) / 6f;
                var slot = UIFactory.Anchored(hotbar, "Slot" + i, new Vector2(0.5f + x, 0.5f), new Vector2(58, 58));
                var img = slot.gameObject.AddComponent<Image>();
                img.sprite = VisualAssetFactory.Slot;
                img.type = Image.Type.Sliced;
                _hotbarSlots[i] = img;

                var iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(slot, false);
                var irt = iconGo.AddComponent<RectTransform>();
                irt.anchorMin = new Vector2(0.15f, 0.2f);
                irt.anchorMax = new Vector2(0.85f, 0.9f);
                irt.offsetMin = irt.offsetMax = Vector2.zero;
                _hotbarIcons[i] = iconGo.AddComponent<Image>();
                _hotbarIcons[i].preserveAspect = true;
                _hotbarIcons[i].enabled = false;

                var cnt = new GameObject("Count");
                cnt.transform.SetParent(slot, false);
                var crt = cnt.AddComponent<RectTransform>();
                crt.anchorMin = new Vector2(0.5f, 0f);
                crt.anchorMax = new Vector2(1f, 0.4f);
                crt.offsetMin = crt.offsetMax = Vector2.zero;
                _hotbarCounts[i] = cnt.AddComponent<Text>();
                _hotbarCounts[i].font = UIFactory.Font;
                _hotbarCounts[i].fontSize = 12;
                _hotbarCounts[i].alignment = TextAnchor.MiddleRight;
                _hotbarCounts[i].color = Color.white;
                _hotbarCounts[i].text = "";
            }
            _hotbarHint = UIFactory.Label(hotbar, "Hint", "1-6 select · Tab inventory · C craft · B build", 11, TextAnchor.LowerCenter);
            var hrt = _hotbarHint.GetComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0, -0.45f);
            hrt.anchorMax = new Vector2(1, 0);
            _hotbarHint.color = new Color(0.8f, 0.85f, 0.82f, 0.7f);
            _hotbarHint.fontSize = 12;

            // Inventory panel
            _invPanel = UIFactory.Panel(_canvas.transform, "Inventory", new Vector2(0.5f, 0.52f), new Vector2(560, 480), VisualAssetFactory.Panel).gameObject;
            UIFactory.Label(_invPanel.transform, "Title", "INVENTORY", 26, TextAnchor.UpperCenter, FontStyle.Bold);
            _invText = UIFactory.Label(_invPanel.transform, "Body", "", 15, TextAnchor.UpperLeft);
            var ibt = _invText.GetComponent<RectTransform>();
            ibt.offsetMin = new Vector2(24, 24);
            ibt.offsetMax = new Vector2(-24, -48);
            _invPanel.SetActive(false);

            // Craft panel
            _craftPanel = UIFactory.Panel(_canvas.transform, "Craft", new Vector2(0.5f, 0.52f), new Vector2(600, 520), VisualAssetFactory.Panel).gameObject;
            UIFactory.Label(_craftPanel.transform, "Title", "CRAFTING", 26, TextAnchor.UpperCenter, FontStyle.Bold);
            _craftText = UIFactory.Label(_craftPanel.transform, "Body", "", 15, TextAnchor.UpperLeft);
            var cbt = _craftText.GetComponent<RectTransform>();
            cbt.offsetMin = new Vector2(24, 24);
            cbt.offsetMax = new Vector2(-24, -48);
            _craftPanel.SetActive(false);

            // Build panel
            _buildPanel = UIFactory.Panel(_canvas.transform, "Build", new Vector2(0.5f, 0.52f), new Vector2(480, 400), VisualAssetFactory.Panel).gameObject;
            var buildBody = UIFactory.Label(_buildPanel.transform, "Body", "", 17, TextAnchor.UpperLeft);
            buildBody.text = "BUILDING\n\n1  Foundation\n2  Wall\n3  Floor\n4  Roof\n5  Doorway\n6  Storage Box\n7  Campfire\n\nLMB place · R rotate · Q cancel\nCraft pieces first in Crafting (C)";
            var bbt = buildBody.GetComponent<RectTransform>();
            bbt.offsetMin = new Vector2(28, 20);
            bbt.offsetMax = new Vector2(-28, -20);
            _buildPanel.SetActive(false);

            // Pause
            _pausePanel = UIFactory.Panel(_canvas.transform, "Pause", new Vector2(0.5f, 0.5f), new Vector2(400, 360), VisualAssetFactory.PanelDark).gameObject;
            UIFactory.Label(_pausePanel.transform, "Title", "PAUSED", 32, TextAnchor.UpperCenter, FontStyle.Bold);
            UIFactory.Button(_pausePanel.transform, "Resume", new Vector2(0.5f, 0.55f), new Vector2(240, 48), () => TogglePause());
            UIFactory.Button(_pausePanel.transform, "Save Game", new Vector2(0.5f, 0.38f), new Vector2(240, 48), () => SaveSystem.SaveSystem.Instance?.SaveGame());
            UIFactory.Button(_pausePanel.transform, "Main Menu", new Vector2(0.5f, 0.21f), new Vector2(240, 48), () => GameManager.Instance.LoadMainMenu());
            _pausePanel.SetActive(false);
        }

        void OnNotify(string msg)
        {
            if (_notify != null)
            {
                _notify.text = msg;
                _notifyTimer = 2.8f;
                if (_notifyCg != null) _notifyCg.alpha = 1f;
            }
        }

        void Update()
        {
            if (GameManager.Instance == null) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_invOpen || _craftOpen || _buildOpen) { CloseMenus(); return; }
                TogglePause();
            }

            if (GameManager.Instance.State == GameState.Paused) return;
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

            // Prompt
            bool hasPrompt = InteractionSystem.Instance != null && !string.IsNullOrEmpty(InteractionSystem.Instance.CurrentPrompt);
            if (_prompt != null)
            {
                _prompt.text = hasPrompt ? InteractionSystem.Instance.CurrentPrompt : "";
                _prompt.transform.parent.gameObject.SetActive(hasPrompt);
            }

            if (_notifyTimer > 0f)
            {
                _notifyTimer -= Time.unscaledDeltaTime;
                if (_notifyCg != null)
                    _notifyCg.alpha = Mathf.Clamp01(_notifyTimer / 0.5f);
            }

            var stats = SurvivalStats.Instance;
            if (stats != null)
            {
                if (_hunger) _hunger.fillAmount = stats.hunger / 100f;
                if (_thirst) _thirst.fillAmount = stats.thirst / 100f;
                if (_energy) _energy.fillAmount = stats.energy / 100f;
            }

            if (DayNightCycle.Instance != null && _timeTxt != null)
                _timeTxt.text = DayNightCycle.Instance.GetPhaseName();
            if (WeatherSystem.Instance != null && _weatherTxt != null)
                _weatherTxt.text = WeatherSystem.Instance.current;

            if (_invOpen) RefreshInv();
            if (_craftOpen) RefreshCraft();
            RefreshHotbar();
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
            SetCursor(_invOpen);
        }

        void ToggleCraft()
        {
            _craftOpen = !_craftOpen;
            _craftPanel.SetActive(_craftOpen);
            if (_craftOpen) { _invOpen = false; _invPanel.SetActive(false); _buildOpen = false; _buildPanel.SetActive(false); RefreshCraft(); }
            SetCursor(_craftOpen);
        }

        void ToggleBuild()
        {
            _buildOpen = !_buildOpen;
            _buildPanel.SetActive(_buildOpen);
            if (_buildOpen) { _invOpen = false; _invPanel.SetActive(false); _craftOpen = false; _craftPanel.SetActive(false); }
            SetCursor(_buildOpen);
        }

        void CloseMenus()
        {
            _invOpen = _craftOpen = _buildOpen = false;
            _invPanel.SetActive(false);
            _craftPanel.SetActive(false);
            _buildPanel.SetActive(false);
            SetCursor(false);
        }

        void SetCursor(bool ui)
        {
            Cursor.lockState = ui ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = ui;
        }

        void RefreshHotbar()
        {
            var inv = InventorySystem.Instance;
            if (inv == null) return;
            for (int i = 0; i < 6; i++)
            {
                bool sel = inv.selectedHotbar == i;
                if (_hotbarSlots[i])
                    _hotbarSlots[i].sprite = sel ? VisualAssetFactory.SlotSelected : VisualAssetFactory.Slot;
                if (i < inv.slots.Length && !inv.slots[i].IsEmpty)
                {
                    _hotbarIcons[i].enabled = true;
                    _hotbarIcons[i].sprite = VisualAssetFactory.IconFor(inv.slots[i].itemId);
                    _hotbarCounts[i].text = inv.slots[i].count > 1 ? inv.slots[i].count.ToString() : "";
                }
                else
                {
                    _hotbarIcons[i].enabled = false;
                    _hotbarCounts[i].text = "";
                }
            }
        }

        void RefreshInv()
        {
            if (_invText == null || InventorySystem.Instance == null) return;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Tab close · RMB use selected hotbar item\n");
            var slots = InventorySystem.Instance.slots;
            for (int i = 0; i < slots.Length; i++)
            {
                string mark = i == InventorySystem.Instance.selectedHotbar ? "▸ " : "  ";
                if (slots[i].IsEmpty) sb.AppendLine($"{mark}[{i + 1}]  —");
                else
                {
                    var def = ItemDatabase.Instance?.Get(slots[i].itemId);
                    string n = def != null ? def.displayName : slots[i].itemId;
                    sb.AppendLine($"{mark}[{i + 1}]  {n}  ×{slots[i].count}");
                }
            }
            _invText.text = sb.ToString();
        }

        void RefreshCraft()
        {
            if (_craftText == null || CraftingSystem.Instance == null) return;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Press 1–9 to craft · C to close\n");
            int idx = 1;
            foreach (var r in CraftingSystem.Instance.recipes)
            {
                bool ok = CraftingSystem.Instance.CanCraft(r);
                sb.Append(ok ? "● " : "○ ");
                sb.Append(idx++).Append("  ").Append(r.displayName).Append("\n    ");
                foreach (var ing in r.ingredients)
                    sb.Append(ing.itemId).Append("×").Append(ing.amount).Append("  ");
                sb.AppendLine("\n");
            }
            _craftText.text = sb.ToString();
        }
    }
}
