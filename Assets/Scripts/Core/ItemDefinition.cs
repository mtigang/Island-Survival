using UnityEngine;

namespace IslandSurvival.Core
{
    public enum ItemCategory { Resource, Tool, Food, Drink, Building, Misc }

    [CreateAssetMenu(menuName = "Island Survival/Item Definition", fileName = "Item_")]
    public class ItemDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public ItemCategory category;
        public int maxStack = 64;
        public Sprite icon;
        public bool isConsumable;
        public float hungerRestore;
        public float thirstRestore;
        public float energyRestore;
        public Color gizmoColor = Color.white;
    }
}
