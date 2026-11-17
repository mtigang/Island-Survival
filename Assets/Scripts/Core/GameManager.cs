using UnityEngine;
using UnityEngine.SceneManagement;

namespace IslandSurvival.Core
{
    public enum GameState { Boot, MainMenu, Playing, Paused, GameOver }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Boot;
        public bool IsNewGame { get; set; } = true;

        [Header("Timing")]
        public float dayLengthSeconds = 600f;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;
        }

        public void SetState(GameState s)
        {
            State = s;
            Time.timeScale = s == GameState.Paused ? 0f : 1f;
            Cursor.lockState = s == GameState.Playing ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = s != GameState.Playing;
        }

        public void StartNewGame()
        {
            IsNewGame = true;
            SceneManager.LoadScene("GameWorld");
        }

        public void ContinueGame()
        {
            IsNewGame = false;
            SceneManager.LoadScene("GameWorld");
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
            SetState(GameState.MainMenu);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
