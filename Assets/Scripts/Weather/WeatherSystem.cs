using UnityEngine;
using IslandSurvival.Core;

namespace IslandSurvival.Weather
{
    public class WeatherSystem : MonoBehaviour
    {
        public static WeatherSystem Instance { get; private set; }

        public string current = "Clear";
        public float changeInterval = 180f;
        public ParticleSystem rain;

        float _timer;
        readonly string[] _types = { "Clear", "Cloudy", "LightRain", "HeavyRain" };

        void Awake() => Instance = this;

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.Paused) return;
            _timer += Time.deltaTime;
            if (_timer >= changeInterval)
            {
                _timer = 0f;
                SetWeather(_types[Random.Range(0, _types.Length)]);
            }
        }

        public void SetWeather(string w)
        {
            current = w;
            if (rain != null)
            {
                var em = rain.emission;
                if (w == "Clear" || w == "Cloudy")
                {
                    em.rateOverTime = 0;
                    if (rain.isPlaying) rain.Stop();
                }
                else
                {
                    em.rateOverTime = w == "HeavyRain" ? 600f : 180f;
                    if (!rain.isPlaying) rain.Play();
                }
            }
            RenderSettings.fogDensity = w == "HeavyRain" ? 0.012f : w == "LightRain" ? 0.008f : w == "Cloudy" ? 0.006f : 0.004f;
            GameEvents.WeatherChanged(w);
        }
    }
}
