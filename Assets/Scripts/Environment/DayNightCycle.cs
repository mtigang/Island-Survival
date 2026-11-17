using UnityEngine;
using IslandSurvival.Core;

namespace IslandSurvival.Environment
{
    public class DayNightCycle : MonoBehaviour
    {
        public static DayNightCycle Instance { get; private set; }

        public Light sun;
        public float dayLengthSeconds = 600f;
        [Range(0, 1)] public float timeOfDay = 0.3f; // 0=midnight, 0.25=sunrise, 0.5=noon

        public Gradient sunColor;
        public AnimationCurve sunIntensity;

        void Awake()
        {
            Instance = this;
            if (sunColor == null || sunColor.colorKeys.Length == 0)
            {
                sunColor = new Gradient();
                sunColor.SetKeys(
                    new[] {
                        new GradientColorKey(new Color(0.1f, 0.12f, 0.25f), 0f),
                        new GradientColorKey(new Color(1f, 0.55f, 0.3f), 0.25f),
                        new GradientColorKey(new Color(1f, 0.95f, 0.85f), 0.5f),
                        new GradientColorKey(new Color(1f, 0.5f, 0.25f), 0.75f),
                        new GradientColorKey(new Color(0.1f, 0.12f, 0.25f), 1f)
                    },
                    new[] {
                        new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1)
                    });
            }
            if (sunIntensity == null || sunIntensity.length == 0)
            {
                sunIntensity = new AnimationCurve(
                    new Keyframe(0f, 0.05f),
                    new Keyframe(0.25f, 0.8f),
                    new Keyframe(0.5f, 1.2f),
                    new Keyframe(0.75f, 0.6f),
                    new Keyframe(1f, 0.05f));
            }
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.Paused) return;
            timeOfDay += Time.deltaTime / Mathf.Max(60f, dayLengthSeconds);
            if (timeOfDay > 1f) timeOfDay -= 1f;
            Apply();
            GameEvents.TimeOfDayChanged(timeOfDay);
        }

        public void Apply()
        {
            if (sun == null) return;
            float sunAngle = timeOfDay * 360f - 90f;
            sun.transform.rotation = Quaternion.Euler(sunAngle, 40f, 0f);
            sun.color = sunColor.Evaluate(timeOfDay);
            sun.intensity = sunIntensity.Evaluate(timeOfDay);
            RenderSettings.ambientLight = Color.Lerp(new Color(0.05f, 0.06f, 0.1f), new Color(0.45f, 0.5f, 0.55f), sun.intensity);
            RenderSettings.fogColor = Color.Lerp(new Color(0.02f, 0.03f, 0.06f), new Color(0.55f, 0.7f, 0.85f), Mathf.Clamp01(sun.intensity));
        }

        public string GetPhaseName()
        {
            if (timeOfDay < 0.2f) return "Night";
            if (timeOfDay < 0.3f) return "Sunrise";
            if (timeOfDay < 0.45f) return "Morning";
            if (timeOfDay < 0.55f) return "Noon";
            if (timeOfDay < 0.7f) return "Afternoon";
            if (timeOfDay < 0.8f) return "Sunset";
            return "Night";
        }
    }
}
