using System.Collections.Generic;
using UnityEngine;

namespace IslandSurvival.Core
{
    /// <summary>
    /// Runtime item catalog. Built procedurally so the project has no missing ScriptableObject refs.
    /// </summary>
    public class ItemDatabase : MonoBehaviour
    {
        public static ItemDatabase Instance { get; private set; }
        readonly Dictionary<string, ItemDefinition> _map = new Dictionary<string, ItemDefinition>();
        public IEnumerable<ItemDefinition> All => _map.Values;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildDefaults();
        }

        void BuildDefaults()
        {
            Add("wood", "Wood", "Solid timber from trees.", ItemCategory.Resource, 64, new Color(0.45f, 0.28f, 0.12f));
            Add("stone", "Stone", "Hard rock useful for tools.", ItemCategory.Resource, 64, new Color(0.55f, 0.55f, 0.55f));
            Add("leaves", "Leaves", "Soft foliage.", ItemCategory.Resource, 64, new Color(0.2f, 0.55f, 0.15f));
            Add("fiber", "Fiber", "Plant fibers for cordage.", ItemCategory.Resource, 64, new Color(0.7f, 0.65f, 0.3f));
            Add("fruit", "Fruit", "Sweet tropical fruit.", ItemCategory.Food, 16, new Color(0.9f, 0.2f, 0.2f), true, 25f, 5f, 5f);
            Add("freshwater", "Fresh Water", "Clean drinking water.", ItemCategory.Drink, 8, new Color(0.3f, 0.55f, 0.95f), true, 0f, 35f, 5f);
            Add("wooden_tool", "Wooden Tool", "A crude multipurpose tool.", ItemCategory.Tool, 1, new Color(0.5f, 0.35f, 0.15f));
            Add("water_container", "Water Container", "Stores freshwater.", ItemCategory.Misc, 1, new Color(0.4f, 0.5f, 0.7f));
            Add("campfire", "Campfire Kit", "Place a campfire.", ItemCategory.Building, 4, new Color(0.8f, 0.35f, 0.1f));
            Add("foundation", "Wooden Foundation", "Base for a shelter.", ItemCategory.Building, 8, new Color(0.48f, 0.3f, 0.14f));
            Add("wall", "Wooden Wall", "Shelter wall panel.", ItemCategory.Building, 16, new Color(0.5f, 0.32f, 0.16f));
            Add("floor", "Wooden Floor", "Shelter floor panel.", ItemCategory.Building, 16, new Color(0.46f, 0.28f, 0.12f));
            Add("roof", "Wooden Roof", "Shelter roof panel.", ItemCategory.Building, 12, new Color(0.42f, 0.25f, 0.1f));
            Add("doorway", "Wooden Doorway", "Open doorway frame.", ItemCategory.Building, 4, new Color(0.5f, 0.3f, 0.15f));
            Add("storage_box", "Storage Box", "Store excess items.", ItemCategory.Building, 4, new Color(0.4f, 0.25f, 0.1f));
        }

        void Add(string id, string name, string desc, ItemCategory cat, int stack, Color col,
            bool consumable = false, float hunger = 0, float thirst = 0, float energy = 0)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.id = id;
            item.displayName = name;
            item.description = desc;
            item.category = cat;
            item.maxStack = stack;
            item.gizmoColor = col;
            item.isConsumable = consumable;
            item.hungerRestore = hunger;
            item.thirstRestore = thirst;
            item.energyRestore = energy;
            item.icon = MakeIcon(col);
            _map[id] = item;
        }

        Sprite MakeIcon(Color c)
        {
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            var pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }

        public ItemDefinition Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            _map.TryGetValue(id, out var item);
            return item;
        }
    }
}
