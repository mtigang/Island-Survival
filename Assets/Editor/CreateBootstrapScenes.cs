#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using IslandSurvival.Bootstrap;
using IslandSurvival.UI;

namespace IslandSurvival.EditorTools
{
    public static class CreateBootstrapScenes
    {
        [MenuItem("Island Survival/Fix GameWorld Bootstrap")]
        public static void FixGameWorld()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("GameWorldBootstrap");
            go.AddComponent<GameWorldBootstrap>();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameWorld.unity");
            AssetDatabase.Refresh();
            Debug.Log("GameWorld bootstrap scene saved.");
        }

        [MenuItem("Island Survival/Fix MainMenu Bootstrap")]
        public static void FixMainMenu()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("MainMenuBootstrap");
            go.AddComponent<MainMenuBootstrap>();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
            AssetDatabase.Refresh();
            Debug.Log("MainMenu bootstrap scene saved.");
        }
    }
}
#endif
