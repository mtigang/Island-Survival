using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using IslandSurvival.Core;
using IslandSurvival.Player;
using IslandSurvival.Inventory;
using IslandSurvival.Survival;
using IslandSurvival.Environment;
using IslandSurvival.Weather;
using IslandSurvival.Building;

namespace IslandSurvival.SaveSystem
{
    [Serializable]
    public class SaveData
    {
        public float px, py, pz, yaw;
        public float hunger, thirst, energy, timeOfDay;
        public string weather;
        public List<InventorySlot> inventory = new List<InventorySlot>();
        public List<BuiltSaveData> buildings = new List<BuiltSaveData>();
    }

    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }
        static string Path => System.IO.Path.Combine(Application.persistentDataPath, "island_survival_save.json");

        void Awake() => Instance = this;

        public static bool HasSave() => File.Exists(Path);

        public void SaveGame()
        {
            try
            {
                var data = new SaveData();
                var player = FirstPersonController.Instance;
                if (player != null)
                {
                    data.px = player.transform.position.x;
                    data.py = player.transform.position.y;
                    data.pz = player.transform.position.z;
                    data.yaw = player.transform.eulerAngles.y;
                }
                var stats = SurvivalStats.Instance;
                if (stats != null)
                {
                    data.hunger = stats.hunger;
                    data.thirst = stats.thirst;
                    data.energy = stats.energy;
                }
                if (DayNightCycle.Instance != null) data.timeOfDay = DayNightCycle.Instance.timeOfDay;
                if (WeatherSystem.Instance != null) data.weather = WeatherSystem.Instance.current;
                if (InventorySystem.Instance != null) data.inventory = InventorySystem.Instance.Export();
                if (BuildingSystem.Instance != null) data.buildings = BuildingSystem.Instance.Export();

                File.WriteAllText(Path, JsonUtility.ToJson(data, true));
                GameEvents.Notify("Game saved");
            }
            catch (Exception e)
            {
                Debug.LogError("Save failed: " + e.Message);
                GameEvents.Notify("Save failed");
            }
        }

        public void LoadGame()
        {
            if (!HasSave()) return;
            try
            {
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(Path));
                if (data == null) return;
                var player = FirstPersonController.Instance;
                if (player != null)
                    player.Teleport(new Vector3(data.px, data.py, data.pz), Quaternion.Euler(0f, data.yaw, 0f));
                var stats = SurvivalStats.Instance;
                if (stats != null)
                {
                    stats.hunger = data.hunger;
                    stats.thirst = data.thirst;
                    stats.energy = data.energy;
                }
                if (DayNightCycle.Instance != null)
                {
                    DayNightCycle.Instance.timeOfDay = data.timeOfDay;
                    DayNightCycle.Instance.Apply();
                }
                if (WeatherSystem.Instance != null && !string.IsNullOrEmpty(data.weather))
                    WeatherSystem.Instance.SetWeather(data.weather);
                if (InventorySystem.Instance != null) InventorySystem.Instance.Import(data.inventory);
                if (BuildingSystem.Instance != null) BuildingSystem.Instance.Import(data.buildings);
                GameEvents.Notify("Game loaded");
            }
            catch (Exception e)
            {
                Debug.LogError("Load failed: " + e.Message);
                GameEvents.Notify("Load failed");
            }
        }
    }
}
