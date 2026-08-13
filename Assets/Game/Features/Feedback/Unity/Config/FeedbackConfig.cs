using UnityEngine;
using ZombieWar.Features.Feedback.Domain;

namespace ZombieWar.Features.Feedback.Unity.Config
{
    [CreateAssetMenu(
        fileName = "Feedback_",
        menuName = "Zombie War/Feedback/Feedback Config")]
    public sealed class FeedbackConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private FeedbackId id;
        [SerializeField] private FeedbackPriority priority = FeedbackPriority.Normal;
        [SerializeField] private bool allowDuringTerminalDrain;

        [Header("Camera")]
        [SerializeField] private bool cameraEnabled;
        [SerializeField] private FeedbackCameraCue cameraCue = FeedbackCameraCue.LightWeapon;
        [SerializeField, Min(0f)] private float cameraOccupancyDuration = 0.1f;

        [Header("Haptic")]
        [SerializeField] private bool hapticEnabled;
        [SerializeField] private HapticFeedbackStrength hapticStrength = HapticFeedbackStrength.Light;
        [SerializeField, Min(0f)] private float hapticCooldown = 0.15f;

        [Header("Screen")]
        [SerializeField] private bool screenEnabled;
        [SerializeField] private ScreenFeedbackKind screenKind = ScreenFeedbackKind.Impact;
        [SerializeField, Range(0f, 1f)] private float screenIntensity = 0.2f;
        [SerializeField, Min(0f)] private float screenDuration = 0.15f;

        [Header("Recoil")]
        [SerializeField] private bool recoilEnabled;
        [SerializeField, Min(0f)] private float recoilStrength = 0.1f;
        [SerializeField, Min(0f)] private float recoilDuration = 0.1f;

        public FeedbackId Id => id;

        public FeedbackRecipe CreateRecipe()
        {
            var camera = new CameraFeedbackDefinition(
                cameraEnabled,
                cameraCue,
                cameraOccupancyDuration);

            var haptic = new HapticFeedbackDefinition(
                hapticEnabled,
                hapticStrength,
                hapticCooldown);

            var screen = new ScreenFeedbackDefinition(
                screenEnabled,
                screenKind,
                screenIntensity,
                screenDuration);

            var recoil = new RecoilFeedbackDefinition(
                recoilEnabled,
                recoilStrength,
                recoilDuration);

            return new FeedbackRecipe(
                id,
                priority,
                allowDuringTerminalDrain,
                in camera,
                in haptic,
                in screen,
                in recoil);
        }

#if UNITY_EDITOR
        public void EditorConfigure(
            FeedbackId newId,
            FeedbackPriority newPriority,
            bool terminalSafe,
            bool enableCamera,
            FeedbackCameraCue newCameraCue,
            float cameraDuration,
            bool enableHaptic,
            HapticFeedbackStrength newHapticStrength,
            float newHapticCooldown,
            bool enableScreen,
            ScreenFeedbackKind newScreenKind,
            float newScreenIntensity,
            float newScreenDuration,
            bool enableRecoil,
            float newRecoilStrength,
            float newRecoilDuration)
        {
            id = newId;
            priority = newPriority;
            allowDuringTerminalDrain = terminalSafe;

            cameraEnabled = enableCamera;
            cameraCue = newCameraCue;
            cameraOccupancyDuration = cameraDuration;

            hapticEnabled = enableHaptic;
            hapticStrength = newHapticStrength;
            hapticCooldown = newHapticCooldown;

            screenEnabled = enableScreen;
            screenKind = newScreenKind;
            screenIntensity = newScreenIntensity;
            screenDuration = newScreenDuration;

            recoilEnabled = enableRecoil;
            recoilStrength = newRecoilStrength;
            recoilDuration = newRecoilDuration;
        }
#endif
    }
}
