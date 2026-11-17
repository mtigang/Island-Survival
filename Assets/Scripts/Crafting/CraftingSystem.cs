using System;
using System.Collections.Generic;
using UnityEngine;
using IslandSurvival.Core;
using IslandSurvival.Inventory;

namespace IslandSurvival.Crafting
{
    [Serializable]
    public class CraftIngredient
    {
        public string itemId;
        public int amount;
    }

    [Serializable]
    public class CraftRecipe
    {
        public string id;
        public string displayName;
        public string resultItemId;
        public int resultAmount = 1;
        public List<CraftIngredient> ingredients = new List<CraftIngredient>();
    }

    public class CraftingSystem : MonoBehaviour
    {
        public static CraftingSystem Instance { get; private set; }
        public List<CraftRecipe> recipes = new List<CraftRecipe>();

        void Awake()
        {
            Instance = this;
            if (recipes.Count == 0) BuildDefaults();
        }

        void BuildDefaults()
        {
            recipes.Add(R("wooden_tool", "Wooden Tool", "wooden_tool", 1,
                ("wood", 5), ("stone", 2), ("fiber", 2)));
            recipes.Add(R("campfire", "Campfire", "campfire", 1,
                ("wood", 6), ("stone", 4)));
            recipes.Add(R("water_container", "Water Container", "water_container", 1,
                ("leaves", 4), ("fiber", 3), ("wood", 2)));
            recipes.Add(R("foundation", "Wooden Foundation", "foundation", 1,
                ("wood", 8), ("fiber", 2)));
            recipes.Add(R("wall", "Wooden Wall", "wall", 1,
                ("wood", 6), ("fiber", 1)));
            recipes.Add(R("floor", "Wooden Floor", "floor", 1,
                ("wood", 5)));
            recipes.Add(R("roof", "Wooden Roof", "roof", 1,
                ("wood", 6), ("leaves", 4)));
            recipes.Add(R("doorway", "Wooden Doorway", "doorway", 1,
                ("wood", 5), ("fiber", 2)));
            recipes.Add(R("storage_box", "Storage Box", "storage_box", 1,
                ("wood", 8), ("fiber", 3)));
        }

        CraftRecipe R(string id, string name, string result, int amt, params (string, int)[] ings)
        {
            var r = new CraftRecipe { id = id, displayName = name, resultItemId = result, resultAmount = amt };
            foreach (var i in ings) r.ingredients.Add(new CraftIngredient { itemId = i.Item1, amount = i.Item2 });
            return r;
        }

        public bool CanCraft(CraftRecipe recipe)
        {
            var inv = InventorySystem.Instance;
            if (inv == null || recipe == null) return false;
            foreach (var ing in recipe.ingredients)
                if (!inv.HasItems(ing.itemId, ing.amount)) return false;
            return true;
        }

        public bool Craft(CraftRecipe recipe)
        {
            var inv = InventorySystem.Instance;
            if (inv == null || !CanCraft(recipe)) return false;
            foreach (var ing in recipe.ingredients)
                inv.Consume(ing.itemId, ing.amount);
            inv.AddItem(recipe.resultItemId, recipe.resultAmount);
            GameEvents.Notify("Crafted " + recipe.displayName);
            return true;
        }
    }
}
