using UnityEngine;
using IslandSurvival.Core;

namespace IslandSurvival.Environment
{
    public class DayNightCycle : MonoBehaviour
    {
        public static DayNightCycle Instance { get; private set; }

        public Light sun;
        public Light moon;
        public float dayLengthSeconds = 600f;
        [Range(0, 1)] public float timeOfDay = 0.28f;

        Gradient _sunColor;
        AnimationCurve _sunIntensity;
        AnimationCurve _ambientIntensity;
        Camera _cam;

        void Awake()
        {
            Instance = this;
            BuildCurves();
        }

        void Start()
        {
            _cam = Camera.main;
            Apply();
        }

        void BuildCurves()
        {
            _sunColor = new Gradient();
            _sunColor.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.08f, 0.1f, 0.22f), 0.00f),
                    new GradientColorKey(new Color(0.15f, 0.15f, 0.3f), 0.18f),
                    new GradientColorKey(new Color(1.0f, 0.45f, 0.25f), 0.24f),
                    new GradientColorKey(new Color(1.0f, 0.85f, 0.55f), 0.30f),
                    new GradientColorKey(new Color(1.0f, 0.96f, 0.88f), 0.50f),
                    new GradientColorKey(new Color(1.0f, 0.9f, 0.7f), 0.65f),
                    new GradientColorKey(new Color(1.0f, 0.4f, 0.2f), 0.76f),
                    new GradientColorKey(new Color(0.25f, 0.15f, 0.35f), 0.85f),
                    new GradientColorKey(new Color(0.08f, 0.1f, 0.22f), 1.00f)
                },
                new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });

            _sunIntensity = new AnimationCurve(
                new Keyframe(0.00f, 0.02f),
                new Keyframe(0.20f, 0.05f),
                new Keyframe(0.25f, 0.7f),
                new Keyframe(0.35f, 1.15f),
                new Keyframe(0.50f, 1.35f),
                new Keyframe(0.70f, 1.0f),
                new Keyframe(0.78f, 0.45f),
                new Keyframe(0.85f, 0.05f),
                new Keyframe(1.00f, 0.02f));

            _ambientIntensity = new AnimationCurve(
                new Keyframe(0.0f, 0.08f),
                new Keyframe(0.25f, 0.25f),
                new Keyframe(0.5f, 0.55f),
                new Keyframe(0.75f, 0.3f),
                new Keyframe(1.0f, 0.08f));
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
            sun.transform.rotation = Quaternion.Euler(sunAngle, 35f, 0f);
            sun.color = _sunColor.Evaluate(timeOfDay);
            sun.intensity = _sunIntensity.Evaluate(timeOfDay);
            sun.shadowStrength = Mathf.Clamp01(sun.intensity * 0.7f);

            if (moon != null)
            {
                moon.transform.rotation = Quaternion.Euler(sunAngle + 180f, 35f, 0f);
                float night = Mathf.Clamp01(1f - sun.intensity * 2f);
                moon.intensity = night * 0.25f;
                moon.color = new Color(0.55f, 0.65f, 0.95f);
            }

            float amb = _ambientIntensity.Evaluate(timeOfDay);
            Color ambCol = Color.Lerp(new Color(0.05f, 0.06f, 0.12f), new Color(0.55f, 0.62f, 0.7f), amb);
            RenderSettings.ambientLight = ambCol;

            Color fogDay = new Color(0.55f, 0.72f, 0.88f);
            Color fogSunset = new Color(0.7f, 0.4f, 0.3f);
            Color fogNight = new Color(0.03f, 0.04f, 0.08f);
            Color fog;
            if (timeOfDay > 0.22f && timeOfDay < 0.32f)
                fog = Color.Lerp(fogNight, fogSunset, (timeOfDay - 0.22f) / 0.1f);
            else if (timeOfDay > 0.72f && timeOfDay < 0.85f)
                fog = Color.Lerp(fogDay, fogSunset, (timeOfDay - 0.72f) / 0.08f);
            else if (timeOfDay >= 0.85f || timeOfDay < 0.22f)
                fog = fogNight;
            else
                fog = fogDay;
            RenderSettings.fogColor = fog;
            RenderSettings.fogDensity = timeOfDay > 0.85f || timeOfDay < 0.2f ? 0.008f : 0.0042f;

            if (_cam == null) _cam = Camera.main;
            if (_cam != null)
            {
                _cam.backgroundColor = Color.Lerp(fog, sun.color, 0.25f);
                // mild exposure feel via ambient only
            }
        }

        public string GetPhaseName()
        {
            if (timeOfDay < 0.2f) return "Night";
            if (timeOfDay < 0.3f) return "Sunrise";
            if (timeOfDay < 0.45f) return "Morning";
            if (timeOfDay < 0.55f) return "Noon";
            if (timeOfDay < 0.7f) return "Afternoon";
            if (timeOfDay < 0.82f) return "Sunset";
            return "Night";
        }
    }
}
