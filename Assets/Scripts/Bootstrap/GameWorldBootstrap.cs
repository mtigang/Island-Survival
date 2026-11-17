using UnityEngine;
using IslandSurvival.Core;
using IslandSurvival.Player;
using IslandSurvival.Interaction;
using IslandSurvival.Inventory;
using IslandSurvival.Crafting;
using IslandSurvival.Building;
using IslandSurvival.Survival;
using IslandSurvival.Environment;
using IslandSurvival.Weather;
using IslandSurvival.UI;
using IslandSurvival.SaveSystem;
using IslandSurvival.Audio;

namespace IslandSurvival.Bootstrap
{
    /// <summary>
    /// Entry point for GameWorld scene. Builds the full playable experience at runtime
    /// so the project opens cleanly without missing scene references.
    /// </summary>
    public class GameWorldBootstrap : MonoBehaviour
    {
        [Header("Generation")]
        public int islandSeed = 42;
        public float dayLengthSeconds = 600f;

        void Start()
        {
            EnsureCore();
            var island = BuildIsland();
            var player = BuildPlayer(island);
            BuildSystems(player);
            BuildLightingAndAtmosphere();
            BuildUI();
            BuildAudio();

            if (GameManager.Instance != null && !GameManager.Instance.IsNewGame)
                SaveSystem.SaveSystem.Instance?.LoadGame();
            else
                GameEvents.Notify("Welcome to the island. Gather resources. Survive.");

            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Playing);
        }

        void EnsureCore()
        {
            if (GameManager.Instance == null)
                new GameObject("GameManager").AddComponent<GameManager>();
            if (ItemDatabase.Instance == null)
                new GameObject("ItemDatabase").AddComponent<ItemDatabase>();
            if (SaveSystem.SaveSystem.Instance == null)
                new GameObject("SaveSystem").AddComponent<SaveSystem.SaveSystem>();
        }

        IslandGenerator BuildIsland()
        {
            var go = new GameObject("Island");
            var gen = go.AddComponent<IslandGenerator>();
            gen.seed = islandSeed;
            gen.Generate();
            return gen;
        }

        GameObject BuildPlayer(IslandGenerator island)
        {
            var player = new GameObject("Player");
            var spawn = island.PlayerSpawn != null
                ? island.PlayerSpawn.position
                : new Vector3(0f, 5f, -80f);
            player.transform.position = spawn + Vector3.up * 0.5f;
            if (island.PlayerSpawn != null)
                player.transform.rotation = island.PlayerSpawn.rotation;

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.stepOffset = 0.35f;
            cc.slopeLimit = 50f;

            var camGo = new GameObject("PlayerCamera");
            camGo.transform.SetParent(player.transform);
            camGo.transform.localPosition = new Vector3(0f, 1.65f, 0f);
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 75f;
            cam.nearClipPlane = 0.08f;
            cam.farClipPlane = 400f;
            camGo.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();

            var fpc = player.AddComponent<FirstPersonController>();
            fpc.cameraPivot = camGo.transform;

            player.AddComponent<SurvivalStats>();
            player.AddComponent<InventorySystem>();
            var interaction = player.AddComponent<InteractionSystem>();
            interaction.playerCamera = cam;

            return player;
        }

        void BuildSystems(GameObject player)
        {
            new GameObject("CraftingSystem").AddComponent<CraftingSystem>();
            new GameObject("BuildingSystem").AddComponent<BuildingSystem>();
        }

        void BuildLightingAndAtmosphere()
        {
            // Sun
            var sunGo = new GameObject("Sun");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.85f;
            sun.intensity = 1.1f;
            sun.color = new Color(1f, 0.96f, 0.88f);
            sunGo.transform.rotation = Quaternion.Euler(50f, 40f, 0f);

            var day = sunGo.AddComponent<DayNightCycle>();
            day.sun = sun;
            day.dayLengthSeconds = dayLengthSeconds;
            day.timeOfDay = 0.28f;
            day.Apply();

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0045f;
            RenderSettings.fogColor = new Color(0.55f, 0.72f, 0.85f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.4f, 0.45f, 0.5f);

            // Sky-ish solid background
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(0.45f, 0.7f, 0.95f);

            // Weather + rain particles
            var weatherGo = new GameObject("Weather");
            var weather = weatherGo.AddComponent<WeatherSystem>();
            var rainGo = new GameObject("Rain");
            rainGo.transform.SetParent(weatherGo.transform);
            var ps = rainGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startSpeed = 12f;
            main.startSize = 0.05f;
            main.startLifetime = 1.2f;
            main.maxParticles = 2000;
            main.startColor = new Color(0.7f, 0.8f, 0.95f, 0.6f);
            var emission = ps.emission;
            emission.rateOverTime = 0;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(40f, 1f, 40f);
            rainGo.transform.position = new Vector3(0f, 25f, 0f);
            // Follow player roughly
            rainGo.AddComponent<RainFollow>();
            weather.rain = ps;
        }

        void BuildUI()
        {
            new GameObject("GameHUD").AddComponent<GameHUD>();
        }

        void BuildAudio()
        {
            new GameObject("AudioManager").AddComponent<AudioManager>();
        }
    }

    public class RainFollow : MonoBehaviour
    {
        void LateUpdate()
        {
            var p = FirstPersonController.Instance;
            if (p == null) return;
            var pos = p.transform.position;
            pos.y += 20f;
            transform.position = pos;
        }
    }
}
