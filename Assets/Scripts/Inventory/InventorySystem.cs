using System;
using System.Collections.Generic;
using UnityEngine;
using IslandSurvival.Core;
using IslandSurvival.Survival;

namespace IslandSurvival.Inventory
{
    [Serializable]
    public class InventorySlot
    {
        public string itemId;
        public int count;
        public bool IsEmpty => string.IsNullOrEmpty(itemId) || count <= 0;
        public void Clear() { itemId = null; count = 0; }
    }

    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem Instance { get; private set; }

        public int capacity = 24;
        public int hotbarSize = 6;
        public InventorySlot[] slots;
        public int selectedHotbar;

        void Awake()
        {
            Instance = this;
            slots = new InventorySlot[capacity];
            for (int i = 0; i < capacity; i++) slots[i] = new InventorySlot();
        }

        void Update()
        {
            for (int i = 0; i < Mathf.Min(9, hotbarSize); i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    selectedHotbar = i;
            }
            if (Input.GetMouseButtonDown(1))
                TryUseSelected();
        }

        public bool AddItem(string itemId, int amount = 1)
        {
            var def = ItemDatabase.Instance?.Get(itemId);
            if (def == null || amount <= 0) return false;

            int remaining = amount;
            // Stack into existing
            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (slots[i].itemId == itemId && slots[i].count < def.maxStack)
                {
                    int space = def.maxStack - slots[i].count;
                    int add = Mathf.Min(space, remaining);
                    slots[i].count += add;
                    remaining -= add;
                }
            }
            // Empty slots
            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (slots[i].IsEmpty)
                {
                    int add = Mathf.Min(def.maxStack, remaining);
                    slots[i].itemId = itemId;
                    slots[i].count = add;
                    remaining -= add;
                }
            }

            if (remaining < amount)
            {
                GameEvents.ItemCollected(itemId, amount - remaining);
                GameEvents.InventoryChanged();
                GameEvents.Notify("+" + (amount - remaining) + " " + def.displayName);
                return remaining == 0;
            }
            GameEvents.Notify("Inventory full");
            return false;
        }

        public bool HasItems(string itemId, int amount)
        {
            int total = 0;
            foreach (var s in slots)
                if (s.itemId == itemId) total += s.count;
            return total >= amount;
        }

        public bool Consume(string itemId, int amount)
        {
            if (!HasItems(itemId, amount)) return false;
            int left = amount;
            for (int i = 0; i < slots.Length && left > 0; i++)
            {
                if (slots[i].itemId != itemId) continue;
                int take = Mathf.Min(slots[i].count, left);
                slots[i].count -= take;
                left -= take;
                if (slots[i].count <= 0) slots[i].Clear();
            }
            GameEvents.InventoryChanged();
            return true;
        }

        public void TryUseSelected()
        {
            if (selectedHotbar < 0 || selectedHotbar >= slots.Length) return;
            var slot = slots[selectedHotbar];
            if (slot.IsEmpty) return;
            var def = ItemDatabase.Instance?.Get(slot.itemId);
            if (def == null || !def.isConsumable) return;

            var stats = GetComponent<SurvivalStats>();
            if (stats != null)
            {
                stats.Restore(def.hungerRestore, def.thirstRestore, def.energyRestore);
                Consume(slot.itemId, 1);
                GameEvents.Notify("Used " + def.displayName);
            }
        }

        public List<InventorySlot> Export()
        {
            var list = new List<InventorySlot>();
            foreach (var s in slots)
                list.Add(new InventorySlot { itemId = s.itemId, count = s.count });
            return list;
        }

        public void Import(List<InventorySlot> data)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (data != null && i < data.Count)
                {
                    slots[i].itemId = data[i].itemId;
                    slots[i].count = data[i].count;
                }
                else slots[i].Clear();
            }
            GameEvents.InventoryChanged();
        }
    }
}
