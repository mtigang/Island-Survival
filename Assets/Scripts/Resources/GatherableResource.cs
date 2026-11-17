using UnityEngine;
using IslandSurvival.Interaction;
using IslandSurvival.Inventory;
using IslandSurvival.Core;

namespace IslandSurvival.Resources
{
    public class GatherableResource : MonoBehaviour, IInteractable
    {
        public string itemId = "wood";
        public int amountMin = 1;
        public int amountMax = 3;
        public float cooldown = 90f;
        public bool destroyOnGather;
        public string promptOverride;

        float _readyAt;
        bool _depleted;
        Renderer[] _renderers;
        Color[] _original;

        void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _original = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
                if (_renderers[i].material.HasProperty("_Color"))
                    _original[i] = _renderers[i].material.color;
        }

        public string GetPrompt()
        {
            if (_depleted) return "Depleted";
            if (!string.IsNullOrEmpty(promptOverride)) return "[E] " + promptOverride;
            var def = ItemDatabase.Instance?.Get(itemId);
            return "[E] Gather " + (def != null ? def.displayName : itemId);
        }

        public bool CanInteract() => !_depleted && Time.time >= _readyAt;

        public void Interact(GameObject interactor)
        {
            if (!CanInteract()) return;
            int amt = Random.Range(amountMin, amountMax + 1);
            var inv = interactor.GetComponentInParent<InventorySystem>() ?? InventorySystem.Instance;
            if (inv == null) return;
            if (!inv.AddItem(itemId, amt)) return;

            if (destroyOnGather)
            {
                Destroy(gameObject);
                return;
            }

            _depleted = true;
            _readyAt = Time.time + cooldown;
            SetTint(new Color(0.5f, 0.5f, 0.5f, 0.7f));
            Invoke(nameof(Regrow), cooldown);
        }

        void Regrow()
        {
            _depleted = false;
            RestoreTint();
        }

        void SetTint(Color c)
        {
            if (_renderers == null) return;
            foreach (var r in _renderers)
                if (r && r.material.HasProperty("_Color"))
                    r.material.color = c;
        }

        void RestoreTint()
        {
            if (_renderers == null) return;
            for (int i = 0; i < _renderers.Length; i++)
                if (_renderers[i] && _renderers[i].material.HasProperty("_Color"))
                    _renderers[i].material.color = _original[i];
        }
    }
}
