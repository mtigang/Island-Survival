namespace IslandSurvival.Interaction
{
    public interface IInteractable
    {
        string GetPrompt();
        bool CanInteract();
        void Interact(UnityEngine.GameObject interactor);
    }
}
