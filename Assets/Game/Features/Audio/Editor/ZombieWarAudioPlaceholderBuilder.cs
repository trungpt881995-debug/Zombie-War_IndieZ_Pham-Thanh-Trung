using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Unity.Config;
using ZombieWar.Features.Audio.Unity.Debugging;
using ZombieWar.Features.Audio.Unity.Music;
using ZombieWar.Features.Audio.Unity.Pool;
using ZombieWar.Features.Audio.Unity.Runtime;

namespace ZombieWar.Features.Audio.Editor
{
    public static class ZombieWarAudioPlaceholderBuilder
    {
        private const string GeneratedRoot =
            "Assets/GameGenerated/Audio";

        private const string ConfigFolder =
            GeneratedRoot + "/Config";

        private const string ClipFolder =
            "Assets/Game/Features/Audio/Placeholder/Clips";

        [MenuItem(
            "Tools/Zombie War/Audio/Create Placeholder Audio Setup",
            priority = 1900)]
        public static void CreatePlaceholderAudioSetup()
        {
            EnsureFolder(GeneratedRoot);
            EnsureFolder(ConfigFolder);

            AssetDatabase.Refresh();

            AudioConfig[] configs =
                CreateAudioConfigs();

            AudioCatalogConfig catalog =
                CreateCatalog(configs);

            CreateSceneRuntime(catalog);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject =
                GameObject.Find("ZombieWar_AudioRuntime");

            Debug.Log(
                "Zombie War placeholder Audio setup created. " +
                "Replace AudioClip references in the generated configs " +
                "when production audio is available.");
        }

        private static AudioConfig[] CreateAudioConfigs()
        {
            var configs =
                new List<AudioConfig>();

            Add(
                configs,
                AudioId.PistolFire,
                AudioCategory.SFX,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.ThreeD,
                AudioPriority.Low,
                8,
                0.90f,
                0.98f,
                1.02f,
                1f,
                24f,
                false);

            Add(
                configs,
                AudioId.AKFire,
                AudioCategory.SFX,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.ThreeD,
                AudioPriority.Low,
                10,
                0.72f,
                0.98f,
                1.02f,
                1f,
                24f,
                false);

            Add(
                configs,
                AudioId.ShotgunFire,
                AudioCategory.SFX,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.ThreeD,
                AudioPriority.Normal,
                6,
                1f,
                0.98f,
                1.02f,
                1f,
                28f,
                false);

            Add(
                configs,
                AudioId.SniperFire,
                AudioCategory.SFX,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.ThreeD,
                AudioPriority.High,
                6,
                1f,
                0.98f,
                1.02f,
                1f,
                32f,
                false);

            Add(
                configs,
                AudioId.GrenadeFire,
                AudioCategory.SFX,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.ThreeD,
                AudioPriority.Normal,
                6,
                0.95f,
                0.98f,
                1.02f,
                1f,
                28f,
                false);

            Add(
                configs,
                AudioId.FlamethrowerLoop,
                AudioCategory.SFX,
                AudioLifetimeMode.Looping,
                AudioSpatialMode.ThreeD,
                AudioPriority.Normal,
                4,
                0.65f,
                1f,
                1f,
                1f,
                24f,
                false);

            Add(
                configs,
                AudioId.BulletImpact,
                AudioCategory.SFX,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.ThreeD,
                AudioPriority.Low,
                8,
                0.55f,
                0.92f,
                1.08f,
                0.5f,
                18f,
                false);

            Add(
                configs,
                AudioId.GrenadeExplosion,
                AudioCategory.SFX,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.ThreeD,
                AudioPriority.High,
                4,
                1f,
                0.96f,
                1.04f,
                2f,
                40f,
                true);

            Add(
                configs,
                AudioId.ZombieHit,
                AudioCategory.SFX,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.ThreeD,
                AudioPriority.Low,
                6,
                0.50f,
                0.92f,
                1.08f,
                0.5f,
                16f,
                false);

            Add(
                configs,
                AudioId.ZombieAttack,
                AudioCategory.SFX,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.ThreeD,
                AudioPriority.Low,
                4,
                0.58f,
                0.92f,
                1.08f,
                0.5f,
                18f,
                false);

            Add(
                configs,
                AudioId.ZombieDeath,
                AudioCategory.SFX,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.ThreeD,
                AudioPriority.Low,
                4,
                0.62f,
                0.90f,
                1.10f,
                1f,
                20f,
                false);

            Add(
                configs,
                AudioId.BossSpawn,
                AudioCategory.SFX,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.ThreeD,
                AudioPriority.High,
                2,
                1f,
                0.98f,
                1.02f,
                2f,
                42f,
                true);

            Add(
                configs,
                AudioId.BossAttack,
                AudioCategory.SFX,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.ThreeD,
                AudioPriority.High,
                2,
                0.88f,
                0.98f,
                1.02f,
                1f,
                32f,
                false);

            Add(
                configs,
                AudioId.BossHit,
                AudioCategory.SFX,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.ThreeD,
                AudioPriority.Normal,
                4,
                0.72f,
                0.95f,
                1.05f,
                1f,
                28f,
                false);

            Add(
                configs,
                AudioId.BossDeath,
                AudioCategory.SFX,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.ThreeD,
                AudioPriority.Critical,
                1,
                1f,
                1f,
                1f,
                2f,
                48f,
                true);

            Add(
                configs,
                AudioId.SoldierDamage,
                AudioCategory.SFX,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.TwoD,
                AudioPriority.High,
                3,
                0.85f,
                0.98f,
                1.02f,
                0f,
                0f,
                true);

            Add(
                configs,
                AudioId.SoldierGroupLevelUp,
                AudioCategory.SFX,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.TwoD,
                AudioPriority.High,
                1,
                0.90f,
                1f,
                1f,
                0f,
                0f,
                false);

            Add(
                configs,
                AudioId.UIButtonClick,
                AudioCategory.UI,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.TwoD,
                AudioPriority.Normal,
                4,
                0.55f,
                1f,
                1f,
                0f,
                0f,
                true);

            Add(
                configs,
                AudioId.WeaponSelected,
                AudioCategory.UI,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.TwoD,
                AudioPriority.Normal,
                2,
                0.65f,
                1f,
                1f,
                0f,
                0f,
                true);

            Add(
                configs,
                AudioId.GameOver,
                AudioCategory.UI,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.TwoD,
                AudioPriority.Critical,
                1,
                1f,
                1f,
                1f,
                0f,
                0f,
                true);

            Add(
                configs,
                AudioId.LevelComplete,
                AudioCategory.UI,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.TwoD,
                AudioPriority.Critical,
                1,
                1f,
                1f,
                1f,
                0f,
                0f,
                true);

            Add(
                configs,
                AudioId.EndGame,
                AudioCategory.UI,
                AudioLifetimeMode.OneShot,
                AudioSpatialMode.TwoD,
                AudioPriority.Critical,
                1,
                1f,
                1f,
                1f,
                0f,
                0f,
                true);

            Add(
                configs,
                AudioId.MainMenuMusic,
                AudioCategory.Music,
                AudioLifetimeMode.Looping,
                AudioSpatialMode.TwoD,
                AudioPriority.Normal,
                1,
                0.60f,
                1f,
                1f,
                0f,
                0f,
                true);

            Add(
                configs,
                AudioId.GameplayMusic,
                AudioCategory.Music,
                AudioLifetimeMode.Looping,
                AudioSpatialMode.TwoD,
                AudioPriority.Normal,
                1,
                0.55f,
                1f,
                1f,
                0f,
                0f,
                true);

            return configs.ToArray();
        }

        private static void Add(
            List<AudioConfig> configs,
            AudioId id,
            AudioCategory category,
            AudioLifetimeMode lifetime,
            AudioSpatialMode spatial,
            AudioPriority priority,
            int maxConcurrent,
            float volume,
            float pitchMin,
            float pitchMax,
            float minDistance,
            float maxDistance,
            bool terminalSafe)
        {
            string path =
                $"{ConfigFolder}/Audio_{id}.asset";

            AudioConfig config =
                AssetDatabase.LoadAssetAtPath<AudioConfig>(path);

            if (config == null)
            {
                config =
                    ScriptableObject.CreateInstance<AudioConfig>();

                AssetDatabase.CreateAsset(
                    config,
                    path);
            }

            AudioClip clip =
                AssetDatabase.LoadAssetAtPath<AudioClip>(
                    $"{ClipFolder}/{id}.wav");

            AudioClip[] clips =
                clip != null
                    ? new[] { clip }
                    : new AudioClip[0];

            config.ConfigureForEditor(
                id,
                clips,
                category,
                lifetime,
                spatial,
                priority,
                maxConcurrent,
                volume,
                pitchMin,
                pitchMax,
                minDistance,
                maxDistance,
                terminalSafe);

            EditorUtility.SetDirty(config);
            configs.Add(config);
        }

        private static AudioCatalogConfig CreateCatalog(
            AudioConfig[] configs)
        {
            string path =
                $"{ConfigFolder}/AudioCatalog_Game.asset";

            AudioCatalogConfig catalog =
                AssetDatabase.LoadAssetAtPath<AudioCatalogConfig>(path);

            if (catalog == null)
            {
                catalog =
                    ScriptableObject.CreateInstance<AudioCatalogConfig>();

                AssetDatabase.CreateAsset(
                    catalog,
                    path);
            }

            catalog.ConfigureForEditor(configs);
            EditorUtility.SetDirty(catalog);

            return catalog;
        }

        private static void CreateSceneRuntime(
            AudioCatalogConfig catalog)
        {
            GameObject existing =
                GameObject.Find("ZombieWar_AudioRuntime");

            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }

            var rootObject =
                new GameObject("ZombieWar_AudioRuntime");

            Undo.RegisterCreatedObjectUndo(
                rootObject,
                "Create Zombie War Audio Runtime");

            UnityAudioSourcePool pool =
                rootObject.AddComponent<UnityAudioSourcePool>();

            UnityMusicPlayer music =
                rootObject.AddComponent<UnityMusicPlayer>();

            AudioSimulationDriver driver =
                rootObject.AddComponent<AudioSimulationDriver>();

            AudioRuntimeRoot runtimeRoot =
                rootObject.AddComponent<AudioRuntimeRoot>();

            AudioDebugView debug =
                rootObject.AddComponent<AudioDebugView>();

            runtimeRoot.ConfigureForEditor(
                catalog,
                pool,
                music,
                driver);

            var debugObject =
                new SerializedObject(debug);

            debugObject.FindProperty("runtimeRoot")
                .objectReferenceValue = runtimeRoot;

            debugObject.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = rootObject;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }
    }
}
