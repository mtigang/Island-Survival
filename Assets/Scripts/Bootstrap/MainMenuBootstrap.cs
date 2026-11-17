using UnityEngine;
using IslandSurvival.UI;
using IslandSurvival.Core;

namespace IslandSurvival.Bootstrap
{
    public class MainMenuBootstrap : MonoBehaviour
    {
        void Start()
        {
            if (GameManager.Instance == null)
                new GameObject("GameManager").AddComponent<GameManager>();
            if (FindObjectOfType<MainMenuController>() == null)
                new GameObject("MainMenu").AddComponent<MainMenuController>();
        }
    }
}
