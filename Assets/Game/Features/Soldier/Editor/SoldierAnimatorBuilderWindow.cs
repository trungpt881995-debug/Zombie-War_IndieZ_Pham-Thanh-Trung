#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using ZombieWar.Features.Soldier.Animation;

namespace ZombieWar.Features.Soldier.EditorTools
{
    public sealed class SoldierAnimatorBuilderWindow : EditorWindow
    {
        private const string DefaultOutputFolder =
            "Assets/Game/Generated/SoldierAnimation";

        private const string ControllerFileName =
            "ZombieWar_Soldier.controller";

        private const string MaskFileName =
            "ZombieWar_Soldier_UpperBody.mask";

        [SerializeField]
        private AnimationClip idleClip;

        [SerializeField]
        private AnimationClip walkClip;

        [SerializeField]
        private AnimationClip runClip;

        [SerializeField]
        private AnimationClip aimCenterClip;

        [SerializeField]
        private AnimationClip aimForwardClip;

        [SerializeField]
        private AnimationClip aimBackwardClip;

        [SerializeField]
        private AnimationClip aimLeftClip;

        [SerializeField]
        private AnimationClip aimRightClip;

        [SerializeField]
        private AnimationClip shootClip;

        [SerializeField]
        private bool includeHeadInUpperBodyMask;

        [SerializeField]
        private string outputFolder =
            DefaultOutputFolder;

        [MenuItem("Tools/Zombie War/Soldier/Open Animator Builder")]
        private static void Open()
        {
            SoldierAnimatorBuilderWindow window =
                GetWindow<SoldierAnimatorBuilderWindow>();

            window.titleContent =
                new GUIContent("Soldier Animator Builder");

            window.minSize =
                new Vector2(430f, 620f);

            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(
                "Zombie War Soldier Animator",
                EditorStyles.boldLabel);

            EditorGUILayout.Space(4f);

            EditorGUILayout.HelpBox(
                "Builds the production Animator contract used by SoldierView: " +
                "Locomotion 1D Blend Tree, UpperBody layer, Humanoid AvatarMask, " +
                "Aim state and actual-shot Shoot trigger.",
                MessageType.Info);

            EditorGUILayout.Space(8f);

            DrawLocomotionSection();
            EditorGUILayout.Space(8f);
            DrawAimSection();
            EditorGUILayout.Space(8f);
            DrawShootSection();
            EditorGUILayout.Space(8f);
            DrawOutputSection();

            EditorGUILayout.Space(14f);

            using (new EditorGUI.DisabledScope(!HasRequiredClips()))
            {
                if (GUILayout.Button(
                        "Build / Rebuild Soldier Animator",
                        GUILayout.Height(36f)))
                {
                    Build();
                }
            }

            if (!HasRequiredClips())
            {
                EditorGUILayout.HelpBox(
                    "Required clips: Idle, Walk, Run, Aim Center / Single Aim, Shoot.",
                    MessageType.Warning);
            }
        }

        private void DrawLocomotionSection()
        {
            EditorGUILayout.LabelField(
                "Locomotion - Required",
                EditorStyles.boldLabel);

            idleClip =
                DrawClip("Idle", idleClip);

            walkClip =
                DrawClip("Walk", walkClip);

            runClip =
                DrawClip("Run", runClip);
        }

        private void DrawAimSection()
        {
            EditorGUILayout.LabelField(
                "Upper Body Aim",
                EditorStyles.boldLabel);

            aimCenterClip =
                DrawClip(
                    "Aim Center / Single Aim",
                    aimCenterClip);

            EditorGUILayout.HelpBox(
                "Optional directional clips enable a 2D Simple Directional Aim Blend Tree. " +
                "If any directional clip is missing, the builder uses Aim Center as a single Aim state.",
                MessageType.None);

            aimForwardClip =
                DrawClip("Aim Forward", aimForwardClip);

            aimBackwardClip =
                DrawClip("Aim Backward", aimBackwardClip);

            aimLeftClip =
                DrawClip("Aim Left", aimLeftClip);

            aimRightClip =
                DrawClip("Aim Right", aimRightClip);
        }

        private void DrawShootSection()
        {
            EditorGUILayout.LabelField(
                "Upper Body Shoot - Required",
                EditorStyles.boldLabel);

            shootClip =
                DrawClip("Shoot", shootClip);

            includeHeadInUpperBodyMask =
                EditorGUILayout.Toggle(
                    "Include Head In Mask",
                    includeHeadInUpperBodyMask);
        }

        private void DrawOutputSection()
        {
            EditorGUILayout.LabelField(
                "Output",
                EditorStyles.boldLabel);

            outputFolder =
                EditorGUILayout.TextField(
                    "Folder",
                    outputFolder);

            EditorGUILayout.LabelField(
                "Controller",
                ControllerFileName);

            EditorGUILayout.LabelField(
                "AvatarMask",
                MaskFileName);
        }

        private static AnimationClip DrawClip(
            string label,
            AnimationClip current)
        {
            return (AnimationClip)EditorGUILayout.ObjectField(
                label,
                current,
                typeof(AnimationClip),
                false);
        }

        private bool HasRequiredClips()
        {
            return idleClip != null &&
                   walkClip != null &&
                   runClip != null &&
                   aimCenterClip != null &&
                   shootClip != null;
        }

        private bool HasCompleteDirectionalAimSet()
        {
            return aimForwardClip != null &&
                   aimBackwardClip != null &&
                   aimLeftClip != null &&
                   aimRightClip != null;
        }

        private void Build()
        {
            if (!HasRequiredClips())
            {
                EditorUtility.DisplayDialog(
                    "Soldier Animator Builder",
                    "Assign all required clips before building.",
                    "OK");

                return;
            }

            string normalizedFolder =
                NormalizeAssetFolder(outputFolder);

            EnsureAssetFolder(normalizedFolder);

            string controllerPath =
                normalizedFolder + "/" + ControllerFileName;

            string maskPath =
                normalizedFolder + "/" + MaskFileName;

            DeleteAssetIfExists(controllerPath);
            DeleteAssetIfExists(maskPath);

            AvatarMask upperBodyMask =
                CreateUpperBodyMask(
                    maskPath,
                    includeHeadInUpperBodyMask);

            AnimatorController controller =
                AnimatorController.CreateAnimatorControllerAtPath(
                    controllerPath);

            ConfigureParameters(controller);
            ConfigureBaseLocomotion(controller);
            ConfigureUpperBodyLayer(
                controller,
                upperBodyMask);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = controller;
            EditorGUIUtility.PingObject(controller);

            Debug.Log(
                "[ZombieWar] Soldier Animator built successfully.\n" +
                controllerPath + "\n" +
                maskPath);
        }

        private static string NormalizeAssetFolder(
            string value)
        {
            string normalized =
                string.IsNullOrWhiteSpace(value)
                    ? DefaultOutputFolder
                    : value.Trim().Replace('\\', '/');

            normalized = normalized.TrimEnd('/');

            if (!normalized.StartsWith(
                    "Assets",
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Output folder must be inside Assets/.");
            }

            return normalized;
        }

        private static void EnsureAssetFolder(
            string assetFolder)
        {
            string[] parts =
                assetFolder.Split('/');

            if (parts.Length == 0 ||
                parts[0] != "Assets")
            {
                throw new InvalidOperationException(
                    "Output folder must start with Assets.");
            }

            string current = "Assets";

            for (int i = 1; i < parts.Length; i++)
            {
                string next =
                    current + "/" + parts[i];

                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(
                        current,
                        parts[i]);
                }

                current = next;
            }
        }

        private static void DeleteAssetIfExists(
            string assetPath)
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        private static AvatarMask CreateUpperBodyMask(
            string assetPath,
            bool includeHead)
        {
            var mask =
                new AvatarMask
                {
                    name = "ZombieWar_Soldier_UpperBody"
                };

            mask.SetHumanoidBodyPartActive(
                AvatarMaskBodyPart.Root,
                false);

            mask.SetHumanoidBodyPartActive(
                AvatarMaskBodyPart.Body,
                true);

            mask.SetHumanoidBodyPartActive(
                AvatarMaskBodyPart.Head,
                includeHead);

            mask.SetHumanoidBodyPartActive(
                AvatarMaskBodyPart.LeftLeg,
                false);

            mask.SetHumanoidBodyPartActive(
                AvatarMaskBodyPart.RightLeg,
                false);

            mask.SetHumanoidBodyPartActive(
                AvatarMaskBodyPart.LeftArm,
                true);

            mask.SetHumanoidBodyPartActive(
                AvatarMaskBodyPart.RightArm,
                true);

            mask.SetHumanoidBodyPartActive(
                AvatarMaskBodyPart.LeftFingers,
                true);

            mask.SetHumanoidBodyPartActive(
                AvatarMaskBodyPart.RightFingers,
                true);

            mask.SetHumanoidBodyPartActive(
                AvatarMaskBodyPart.LeftFootIK,
                false);

            mask.SetHumanoidBodyPartActive(
                AvatarMaskBodyPart.RightFootIK,
                false);

            mask.SetHumanoidBodyPartActive(
                AvatarMaskBodyPart.LeftHandIK,
                false);

            mask.SetHumanoidBodyPartActive(
                AvatarMaskBodyPart.RightHandIK,
                false);

            AssetDatabase.CreateAsset(
                mask,
                assetPath);

            return mask;
        }

        private static void ConfigureParameters(
            AnimatorController controller)
        {
            controller.AddParameter(
                SoldierAnimatorContract.MovementSpeed,
                AnimatorControllerParameterType.Float);

            controller.AddParameter(
                SoldierAnimatorContract.AimX,
                AnimatorControllerParameterType.Float);

            controller.AddParameter(
                SoldierAnimatorContract.AimY,
                AnimatorControllerParameterType.Float);

            controller.AddParameter(
                SoldierAnimatorContract.HasTarget,
                AnimatorControllerParameterType.Bool);

            controller.AddParameter(
                SoldierAnimatorContract.Shoot,
                AnimatorControllerParameterType.Trigger);
        }

        private void ConfigureBaseLocomotion(
            AnimatorController controller)
        {
            AnimatorControllerLayer baseLayer =
                controller.layers[0];

            AnimatorStateMachine stateMachine =
                baseLayer.stateMachine;

            AnimatorState locomotionState =
                stateMachine.AddState(
                    SoldierAnimatorContract.LocomotionState);

            locomotionState.writeDefaultValues = false;

            stateMachine.defaultState =
                locomotionState;

            var blendTree =
                new BlendTree
                {
                    name = SoldierAnimatorContract.LocomotionBlendTree,
                    blendType = BlendTreeType.Simple1D,
                    blendParameter = SoldierAnimatorContract.MovementSpeed,
                    useAutomaticThresholds = false
                };

            AssetDatabase.AddObjectToAsset(
                blendTree,
                controller);

            blendTree.AddChild(
                idleClip,
                0f);

            blendTree.AddChild(
                walkClip,
                0.5f);

            blendTree.AddChild(
                runClip,
                1f);

            locomotionState.motion =
                blendTree;
        }

        private void ConfigureUpperBodyLayer(
            AnimatorController controller,
            AvatarMask upperBodyMask)
        {
            controller.AddLayer(
                SoldierAnimatorContract.UpperBodyLayer);

            AnimatorControllerLayer[] layers =
                controller.layers;

            int layerIndex =
                layers.Length - 1;

            AnimatorControllerLayer upperLayer =
                layers[layerIndex];

            upperLayer.defaultWeight = 1f;
            upperLayer.blendingMode =
                AnimatorLayerBlendingMode.Override;
            upperLayer.avatarMask =
                upperBodyMask;

            layers[layerIndex] =
                upperLayer;

            controller.layers =
                layers;

            AnimatorStateMachine stateMachine =
                controller.layers[layerIndex].stateMachine;

            AnimatorState idleState =
                stateMachine.AddState(
                    SoldierAnimatorContract.UpperBodyIdleState);

            AnimatorState aimState =
                stateMachine.AddState(
                    SoldierAnimatorContract.AimState);

            AnimatorState shootState =
                stateMachine.AddState(
                    SoldierAnimatorContract.ShootState);

            idleState.writeDefaultValues = false;
            aimState.writeDefaultValues = false;
            shootState.writeDefaultValues = false;

            stateMachine.defaultState =
                idleState;

            aimState.motion =
                CreateAimMotion(controller);

            shootState.motion =
                shootClip;

            AddImmediateBoolTransition(
                idleState,
                aimState,
                SoldierAnimatorContract.HasTarget,
                true,
                0.08f);

            AddImmediateBoolTransition(
                aimState,
                idleState,
                SoldierAnimatorContract.HasTarget,
                false,
                0.08f);

            AnimatorStateTransition shootTransition =
                stateMachine.AddAnyStateTransition(
                    shootState);

            shootTransition.hasExitTime = false;
            shootTransition.hasFixedDuration = true;
            shootTransition.duration = 0.02f;
            shootTransition.canTransitionToSelf = true;
            shootTransition.AddCondition(
                AnimatorConditionMode.If,
                0f,
                SoldierAnimatorContract.Shoot);

            AddExitTransition(
                shootState,
                aimState,
                SoldierAnimatorContract.HasTarget,
                true);

            AddExitTransition(
                shootState,
                idleState,
                SoldierAnimatorContract.HasTarget,
                false);
        }

        private Motion CreateAimMotion(
            AnimatorController controller)
        {
            if (!HasCompleteDirectionalAimSet())
            {
                return aimCenterClip;
            }

            var blendTree =
                new BlendTree
                {
                    name = SoldierAnimatorContract.AimBlendTree,
                    blendType = BlendTreeType.SimpleDirectional2D,
                    blendParameter = SoldierAnimatorContract.AimX,
                    blendParameterY = SoldierAnimatorContract.AimY,
                    useAutomaticThresholds = false
                };

            AssetDatabase.AddObjectToAsset(
                blendTree,
                controller);

            blendTree.AddChild(
                aimCenterClip,
                Vector2.zero);

            blendTree.AddChild(
                aimForwardClip,
                new Vector2(0f, 1f));

            blendTree.AddChild(
                aimBackwardClip,
                new Vector2(0f, -1f));

            blendTree.AddChild(
                aimLeftClip,
                new Vector2(-1f, 0f));

            blendTree.AddChild(
                aimRightClip,
                new Vector2(1f, 0f));

            return blendTree;
        }

        private static void AddImmediateBoolTransition(
            AnimatorState source,
            AnimatorState destination,
            string parameter,
            bool expected,
            float duration)
        {
            AnimatorStateTransition transition =
                source.AddTransition(destination);

            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = duration;

            transition.AddCondition(
                expected
                    ? AnimatorConditionMode.If
                    : AnimatorConditionMode.IfNot,
                0f,
                parameter);
        }

        private static void AddExitTransition(
            AnimatorState source,
            AnimatorState destination,
            string parameter,
            bool expected)
        {
            AnimatorStateTransition transition =
                source.AddTransition(destination);

            transition.hasExitTime = true;
            transition.exitTime = 0.9f;
            transition.hasFixedDuration = true;
            transition.duration = 0.04f;

            transition.AddCondition(
                expected
                    ? AnimatorConditionMode.If
                    : AnimatorConditionMode.IfNot,
                0f,
                parameter);
        }
    }
}
#endif
