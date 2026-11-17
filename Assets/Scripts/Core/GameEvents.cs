using System;
using UnityEngine;

namespace IslandSurvival.Core
{
    public static class GameEvents
    {
        public static event Action<string, int> OnItemCollected;
        public static event Action<string> OnNotification;
        public static event Action OnInventoryChanged;
        public static event Action OnSurvivalChanged;
        public static event Action<float> OnTimeOfDayChanged;
        public static event Action<string> OnWeatherChanged;
        public static event Action OnPlayerDied;

        public static void ItemCollected(string id, int amount) => OnItemCollected?.Invoke(id, amount);
        public static void Notify(string msg) => OnNotification?.Invoke(msg);
        public static void InventoryChanged() => OnInventoryChanged?.Invoke();
        public static void SurvivalChanged() => OnSurvivalChanged?.Invoke();
        public static void TimeOfDayChanged(float t) => OnTimeOfDayChanged?.Invoke(t);
        public static void WeatherChanged(string w) => OnWeatherChanged?.Invoke(w);
        public static void PlayerDied() => OnPlayerDied?.Invoke();
    }
}
