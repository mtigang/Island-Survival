using UnityEngine;
using IslandSurvival.Core;

namespace IslandSurvival.Interaction
{
    public class InteractionSystem : MonoBehaviour
    {
        public static InteractionSystem Instance { get; private set; }

        public float range = 3.2f;
        public LayerMask mask = ~0;
        public Camera playerCamera;

        public IInteractable Current { get; private set; }
        public string CurrentPrompt { get; private set; }

        void Awake()
        {
            Instance = this;
            if (playerCamera == null) playerCamera = Camera.main;
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            {
                Current = null; CurrentPrompt = "";
                return;
            }

            Current = null;
            CurrentPrompt = "";
            if (playerCamera == null) playerCamera = Camera.main;
            if (playerCamera == null) return;

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range, mask, QueryTriggerInteraction.Collide))
            {
                var interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null && interactable.CanInteract())
                {
                    Current = interactable;
                    CurrentPrompt = interactable.GetPrompt();
                    if (Input.GetKeyDown(KeyCode.E))
                        interactable.Interact(gameObject);
                }
            }
        }
    }
}
