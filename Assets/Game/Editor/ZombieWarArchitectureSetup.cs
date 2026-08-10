#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZombieWar.Bootstrap;

namespace ZombieWar.Editor
{
    public static class ZombieWarArchitectureSetup
    {
        private const string ScenesFolder = "Assets/Game/Scenes";
        private const string BootScenePath = ScenesFolder + "/ZombieWar_Boot.unity";
        private const string GameplayScenePath = ScenesFolder + "/ZombieWar_Gameplay.unity";

        [MenuItem("Zombie War/Setup/Create Architecture Scenes")]
        public static void CreateArchitectureScenes()
        {
            Directory.CreateDirectory(ScenesFolder);
            CreateBootScene();
            CreateGameplayScene();
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Zombie War", "Architecture scenes created and added to Build Settings.", "OK");
        }

        private static void CreateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("GameLifetimeScope");
            root.AddComponent<GameLifetimeScope>();
            EditorSceneManager.SaveScene(scene, BootScenePath);
        }

        private static void CreateGameplayScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("GameplaySystems");
            new GameObject("SoldierGroup");
            new GameObject("CameraRoot");
            new GameObject("UIRoot");
            new GameObject("MapRoot");
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static void ConfigureBuildSettings()
        {
            var paths = new[] { BootScenePath, GameplayScenePath };
            var scenes = new List<EditorBuildSettingsScene>();
            for (var i = 0; i < paths.Length; i++) scenes.Add(new EditorBuildSettingsScene(paths[i], true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
