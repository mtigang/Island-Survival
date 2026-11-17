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
using IslandSurvival.Visual;

namespace IslandSurvival.Bootstrap
{
    public class GameWorldBootstrap : MonoBehaviour
    {
        public int islandSeed = 42;
        public float dayLengthSeconds = 600f;

        void Start()
        {
            VisualAssetFactory.Ensure();
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
                GameEvents.Notify("Welcome to the island. Gather resources. Survive the night.");

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
            gen.resolution = 160;
            gen.size = 240f;
            gen.height = 32f;
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
            cam.fieldOfView = 72f;
            cam.nearClipPlane = 0.07f;
            cam.farClipPlane = 320f;
            cam.allowHDR = true;
            cam.allowMSAA = true;
            camGo.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<AtmosphereController>();

            var fpc = player.AddComponent<FirstPersonController>();
            fpc.cameraPivot = camGo.transform;
            fpc.mouseSensitivity = 1.55f;
            fpc.walkSpeed = 4.3f;
            fpc.sprintSpeed = 7.4f;

            player.AddComponent<SurvivalStats>();
            player.AddComponent<InventorySystem>();
            var interaction = player.AddComponent<InteractionSystem>();
            interaction.playerCamera = cam;
            interaction.range = 3.4f;

            return player;
        }

        void BuildSystems(GameObject player)
        {
            new GameObject("CraftingSystem").AddComponent<CraftingSystem>();
            new GameObject("BuildingSystem").AddComponent<BuildingSystem>();
        }

        void BuildLightingAndAtmosphere()
        {
            var sunGo = new GameObject("Sun");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.8f;
            sun.shadowBias = 0.04f;
            sun.intensity = 1.2f;
            sun.color = new Color(1f, 0.96f, 0.88f);
            sunGo.transform.rotation = Quaternion.Euler(50f, 35f, 0f);

            var moonGo = new GameObject("Moon");
            var moon = moonGo.AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.shadows = LightShadows.None;
            moon.intensity = 0.05f;
            moon.color = new Color(0.55f, 0.65f, 0.95f);

            var day = sunGo.AddComponent<DayNightCycle>();
            day.sun = sun;
            day.moon = moon;
            day.dayLengthSeconds = dayLengthSeconds;
            day.timeOfDay = 0.28f;
            day.Apply();

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0042f;
            RenderSettings.fogColor = new Color(0.55f, 0.72f, 0.88f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.42f, 0.48f, 0.52f);

            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = new Color(0.45f, 0.72f, 0.92f);
            }

            var weatherGo = new GameObject("Weather");
            var weather = weatherGo.AddComponent<WeatherSystem>();
            var rainGo = new GameObject("Rain");
            rainGo.transform.SetParent(weatherGo.transform);
            var ps = rainGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startSpeed = 14f;
            main.startSize = 0.04f;
            main.startLifetime = 1.1f;
            main.maxParticles = 2500;
            main.startColor = new Color(0.75f, 0.85f, 0.95f, 0.55f);
            var emission = ps.emission;
            emission.rateOverTime = 0;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(50f, 1f, 50f);
            rainGo.transform.position = new Vector3(0f, 25f, 0f);
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
            pos.y += 22f;
            transform.position = pos;
        }
    }
}
