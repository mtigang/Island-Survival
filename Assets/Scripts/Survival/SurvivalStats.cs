using UnityEngine;
using IslandSurvival.Core;

namespace IslandSurvival.Survival
{
    public class SurvivalStats : MonoBehaviour
    {
        public static SurvivalStats Instance { get; private set; }

        [Header("Values 0-100")]
        public float hunger = 85f;
        public float thirst = 85f;
        public float energy = 100f;

        [Header("Drain per second")]
        public float hungerDrain = 0.35f;
        public float thirstDrain = 0.55f;
        public float energyRegen = 2.5f;
        public float energyRegenDelay = 2f;

        float _lastEnergyUse;
        bool _warnedHunger, _warnedThirst, _warnedEnergy;

        void Awake() => Instance = this;

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            hunger = Mathf.Max(0f, hunger - hungerDrain * Time.deltaTime);
            thirst = Mathf.Max(0f, thirst - thirstDrain * Time.deltaTime);

            if (Time.time - _lastEnergyUse > energyRegenDelay)
                energy = Mathf.Min(100f, energy + energyRegen * Time.deltaTime);

            if (hunger < 20f && !_warnedHunger) { _warnedHunger = true; GameEvents.Notify("You are getting hungry..."); }
            if (thirst < 20f && !_warnedThirst) { _warnedThirst = true; GameEvents.Notify("You are getting thirsty..."); }
            if (energy < 15f && !_warnedEnergy) { _warnedEnergy = true; GameEvents.Notify("You feel exhausted..."); }
            if (hunger > 25f) _warnedHunger = false;
            if (thirst > 25f) _warnedThirst = false;
            if (energy > 20f) _warnedEnergy = false;

            if (hunger <= 0f || thirst <= 0f)
            {
                // Soft fail: drain energy hard rather than instant death
                energy = Mathf.Max(0f, energy - 5f * Time.deltaTime);
            }

            GameEvents.SurvivalChanged();
        }

        public void DrainEnergy(float amount)
        {
            energy = Mathf.Max(0f, energy - amount);
            _lastEnergyUse = Time.time;
        }

        public void Restore(float h, float t, float e)
        {
            hunger = Mathf.Clamp(hunger + h, 0f, 100f);
            thirst = Mathf.Clamp(thirst + t, 0f, 100f);
            energy = Mathf.Clamp(energy + e, 0f, 100f);
            GameEvents.SurvivalChanged();
        }
    }
}
