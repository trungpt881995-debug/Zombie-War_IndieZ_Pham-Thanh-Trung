#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ZombieWar.Features.Feedback.Domain;
using ZombieWar.Features.Feedback.Unity.Config;
using ZombieWar.Features.Feedback.Unity.Runtime;
using ZombieWar.Features.Feedback.Unity.View;

namespace ZombieWar.Features.Feedback.Editor
{
    public static class ZombieWarFeedbackPlaceholderBuilder
    {
        private const string RootFolder = "Assets/GameGenerated/Feedback";
        private const string ConfigFolder = RootFolder + "/Config";

        [MenuItem("Tools/Zombie War/Feedback/Create Placeholder Feedback Setup")]
        public static void CreatePlaceholderFeedbackSetup()
        {
            EnsureFolders();

            FeedbackCatalogConfig catalog = CreateCatalog();
            CreateSceneRoot(catalog);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static FeedbackCatalogConfig CreateCatalog()
        {
            var configs = new List<FeedbackConfig>
            {
                CreateConfig(
                    FeedbackId.PistolShot,
                    FeedbackPriority.Low,
                    false,
                    true,
                    FeedbackCameraCue.LightWeapon,
                    0.08f,
                    true,
                    HapticFeedbackStrength.Light,
                    0.12f,
                    false,
                    ScreenFeedbackKind.Impact,
                    0f,
                    0f,
                    true,
                    0.15f,
                    0.08f),

                CreateConfig(
                    FeedbackId.AKShot,
                    FeedbackPriority.Low,
                    false,
                    true,
                    FeedbackCameraCue.LightWeapon,
                    0.05f,
                    true,
                    HapticFeedbackStrength.Light,
                    0.20f,
                    false,
                    ScreenFeedbackKind.Impact,
                    0f,
                    0f,
                    true,
                    0.10f,
                    0.05f),

                CreateConfig(
                    FeedbackId.ShotgunShot,
                    FeedbackPriority.High,
                    false,
                    true,
                    FeedbackCameraCue.HeavyWeapon,
                    0.14f,
                    true,
                    HapticFeedbackStrength.Medium,
                    0.18f,
                    false,
                    ScreenFeedbackKind.Impact,
                    0f,
                    0f,
                    true,
                    0.80f,
                    0.12f),

                CreateConfig(
                    FeedbackId.SniperShot,
                    FeedbackPriority.High,
                    false,
                    true,
                    FeedbackCameraCue.HeavyWeapon,
                    0.16f,
                    true,
                    HapticFeedbackStrength.Heavy,
                    0.25f,
                    false,
                    ScreenFeedbackKind.Impact,
                    0f,
                    0f,
                    true,
                    1.00f,
                    0.14f),

                CreateConfig(
                    FeedbackId.GrenadeShot,
                    FeedbackPriority.Normal,
                    false,
                    true,
                    FeedbackCameraCue.HeavyWeapon,
                    0.12f,
                    true,
                    HapticFeedbackStrength.Medium,
                    0.20f,
                    false,
                    ScreenFeedbackKind.Impact,
                    0f,
                    0f,
                    true,
                    0.70f,
                    0.12f),

                CreateConfig(
                    FeedbackId.FlamethrowerStart,
                    FeedbackPriority.Low,
                    false,
                    false,
                    FeedbackCameraCue.LightWeapon,
                    0f,
                    true,
                    HapticFeedbackStrength.Light,
                    0.50f,
                    false,
                    ScreenFeedbackKind.Impact,
                    0f,
                    0f,
                    false,
                    0f,
                    0f),

                CreateConfig(
                    FeedbackId.SoldierDamaged,
                    FeedbackPriority.Normal,
                    true,
                    true,
                    FeedbackCameraCue.SoldierDamage,
                    0.12f,
                    true,
                    HapticFeedbackStrength.Medium,
                    0.12f,
                    true,
                    ScreenFeedbackKind.Damage,
                    0.28f,
                    0.18f,
                    false,
                    0f,
                    0f),

                CreateConfig(
                    FeedbackId.SoldierCriticalDamage,
                    FeedbackPriority.High,
                    true,
                    true,
                    FeedbackCameraCue.SoldierDamage,
                    0.18f,
                    true,
                    HapticFeedbackStrength.Heavy,
                    0.18f,
                    true,
                    ScreenFeedbackKind.Damage,
                    0.45f,
                    0.24f,
                    false,
                    0f,
                    0f),

                CreateConfig(
                    FeedbackId.GrenadeExplosion,
                    FeedbackPriority.High,
                    true,
                    true,
                    FeedbackCameraCue.Explosion,
                    0.25f,
                    true,
                    HapticFeedbackStrength.Heavy,
                    0.20f,
                    true,
                    ScreenFeedbackKind.Impact,
                    0.12f,
                    0.15f,
                    false,
                    0f,
                    0f),

                CreateConfig(
                    FeedbackId.BossHit,
                    FeedbackPriority.Normal,
                    false,
                    true,
                    FeedbackCameraCue.BossImpact,
                    0.10f,
                    false,
                    HapticFeedbackStrength.Light,
                    0f,
                    false,
                    ScreenFeedbackKind.Impact,
                    0f,
                    0f,
                    false,
                    0f,
                    0f),

                CreateConfig(
                    FeedbackId.BossDefeated,
                    FeedbackPriority.Critical,
                    true,
                    true,
                    FeedbackCameraCue.BossImpact,
                    0.50f,
                    true,
                    HapticFeedbackStrength.Heavy,
                    0.35f,
                    true,
                    ScreenFeedbackKind.Positive,
                    0.22f,
                    0.30f,
                    false,
                    0f,
                    0f),

                CreateConfig(
                    FeedbackId.SoldierGroupLevelUp,
                    FeedbackPriority.High,
                    false,
                    true,
                    FeedbackCameraCue.LightWeapon,
                    0.12f,
                    true,
                    HapticFeedbackStrength.Medium,
                    0.20f,
                    true,
                    ScreenFeedbackKind.Positive,
                    0.18f,
                    0.24f,
                    false,
                    0f,
                    0f),

                CreateConfig(
                    FeedbackId.GameOver,
                    FeedbackPriority.Critical,
                    true,
                    true,
                    FeedbackCameraCue.SoldierDamage,
                    0.40f,
                    true,
                    HapticFeedbackStrength.Heavy,
                    0.35f,
                    true,
                    ScreenFeedbackKind.Damage,
                    0.42f,
                    0.35f,
                    false,
                    0f,
                    0f),

                CreateConfig(
                    FeedbackId.LevelComplete,
                    FeedbackPriority.Critical,
                    true,
                    true,
                    FeedbackCameraCue.BossImpact,
                    0.30f,
                    true,
                    HapticFeedbackStrength.Medium,
                    0.25f,
                    true,
                    ScreenFeedbackKind.Positive,
                    0.20f,
                    0.28f,
                    false,
                    0f,
                    0f),

                CreateConfig(
                    FeedbackId.EndGame,
                    FeedbackPriority.Critical,
                    true,
                    true,
                    FeedbackCameraCue.BossImpact,
                    0.40f,
                    true,
                    HapticFeedbackStrength.Heavy,
                    0.35f,
                    true,
                    ScreenFeedbackKind.Positive,
                    0.30f,
                    0.40f,
                    false,
                    0f,
                    0f)
            };

            string path = ConfigFolder + "/FeedbackCatalog_Game.asset";
            FeedbackCatalogConfig catalog =
                AssetDatabase.LoadAssetAtPath<FeedbackCatalogConfig>(path);

            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<FeedbackCatalogConfig>();
                AssetDatabase.CreateAsset(
                    catalog,
                    path);
            }

            catalog.EditorSetConfigs(configs.ToArray());

            EditorUtility.SetDirty(catalog);
            Selection.activeObject = catalog;

            return catalog;
        }

        private static FeedbackConfig CreateConfig(
            FeedbackId id,
            FeedbackPriority priority,
            bool terminalSafe,
            bool camera,
            FeedbackCameraCue cameraCue,
            float cameraDuration,
            bool haptic,
            HapticFeedbackStrength hapticStrength,
            float hapticCooldown,
            bool screen,
            ScreenFeedbackKind screenKind,
            float screenIntensity,
            float screenDuration,
            bool recoil,
            float recoilStrength,
            float recoilDuration)
        {
            string path = ConfigFolder + "/Feedback_" + id + ".asset";
            FeedbackConfig config =
                AssetDatabase.LoadAssetAtPath<FeedbackConfig>(path);

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<FeedbackConfig>();
                AssetDatabase.CreateAsset(
                    config,
                    path);
            }

            config.EditorConfigure(
                id,
                priority,
                terminalSafe,
                camera,
                cameraCue,
                cameraDuration,
                haptic,
                hapticStrength,
                hapticCooldown,
                screen,
                screenKind,
                screenIntensity,
                screenDuration,
                recoil,
                recoilStrength,
                recoilDuration);

            EditorUtility.SetDirty(config);

            return config;
        }

        private static void CreateSceneRoot(FeedbackCatalogConfig catalog)
        {
            GameObject old = GameObject.Find("ZombieWar_FeedbackRuntime");

            if (old != null)
            {
                Object.DestroyImmediate(old);
            }

            var root = new GameObject("ZombieWar_FeedbackRuntime");

            var canvasObject = new GameObject(
                "FeedbackCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler));

            canvasObject.transform.SetParent(
                root.transform,
                false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(
                1080f,
                1920f);

            var flashObject = new GameObject(
                "ScreenFlash",
                typeof(RectTransform),
                typeof(Image),
                typeof(ScreenFlashView));

            flashObject.transform.SetParent(
                canvasObject.transform,
                false);

            RectTransform flashRect =
                flashObject.GetComponent<RectTransform>();

            Stretch(flashRect);

            Image flashImage = flashObject.GetComponent<Image>();
            flashImage.raycastTarget = false;
            flashImage.color = new Color(
                1f,
                0f,
                0f,
                0f);

            flashImage.enabled = false;

            FeedbackSimulationDriver driver =
                root.AddComponent<FeedbackSimulationDriver>();

            FeedbackRuntimeRoot runtimeRoot =
                root.AddComponent<FeedbackRuntimeRoot>();

            FeedbackDebugView debugView =
                root.AddComponent<FeedbackDebugView>();

            var rootSerialized = new SerializedObject(runtimeRoot);

            rootSerialized
                .FindProperty("catalogConfig")
                .objectReferenceValue = catalog;

            rootSerialized
                .FindProperty("screenFlashView")
                .objectReferenceValue =
                    flashObject.GetComponent<ScreenFlashView>();

            rootSerialized
                .FindProperty("simulationDriver")
                .objectReferenceValue = driver;

            rootSerialized.ApplyModifiedPropertiesWithoutUndo();

            var debugSerialized = new SerializedObject(debugView);

            debugSerialized
                .FindProperty("runtimeRoot")
                .objectReferenceValue = runtimeRoot;

            debugSerialized.ApplyModifiedPropertiesWithoutUndo();

            Undo.RegisterCreatedObjectUndo(
                root,
                "Create Zombie War Feedback Runtime");

            Selection.activeGameObject = root;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureFolders()
        {
            EnsureFolder(
                "Assets",
                "GameGenerated");

            EnsureFolder(
                "Assets/GameGenerated",
                "Feedback");

            EnsureFolder(
                RootFolder,
                "Config");
        }

        private static void EnsureFolder(
            string parent,
            string child)
        {
            string path = parent + "/" + child;

            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(
                    parent,
                    child);
            }
        }
    }
}
#endif
