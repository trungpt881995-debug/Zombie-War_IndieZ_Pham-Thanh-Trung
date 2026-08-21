#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using ZombieWar.Features.Soldier.Animation;

namespace ZombieWar.Features.Soldier.Editor
{
    public sealed class SoldierBackwardLocomotionSetupWindow : EditorWindow
    {
        private const string LocomotionStateName = "Locomotion";

        // Mirror the current forward 1D locomotion thresholds:
        // +0.5 Walk Forward, +1.0 Run Forward
        // -0.5 Walk Backward, -1.0 Run Backward
        private const float WalkBackwardThreshold = -0.5f;
        private const float RunBackwardThreshold = -1f;

        [SerializeField]
        private AnimatorController controller;

        [SerializeField]
        private AnimationClip walkBackwardClip;

        [SerializeField]
        private AnimationClip runBackwardClip;

        [MenuItem("Tools/Zombie War/Soldier/Add Backward Locomotion")]
        private static void Open()
        {
            var window = GetWindow<SoldierBackwardLocomotionSetupWindow>();
            window.titleContent = new GUIContent("Soldier Backward");
            window.minSize = new Vector2(460f, 225f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(
                "Add Walk + Run Backward To Existing Soldier Controller",
                EditorStyles.boldLabel);

            EditorGUILayout.Space(4f);

            controller = (AnimatorController)EditorGUILayout.ObjectField(
                "Soldier Controller",
                controller,
                typeof(AnimatorController),
                false);

            EditorGUILayout.Space(4f);

            walkBackwardClip = (AnimationClip)EditorGUILayout.ObjectField(
                "Walk Backward Clip",
                walkBackwardClip,
                typeof(AnimationClip),
                false);

            runBackwardClip = (AnimationClip)EditorGUILayout.ObjectField(
                "Run Backward Clip",
                runBackwardClip,
                typeof(AnimationClip),
                false);

            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(
                "The tool preserves the current UpperBody layer and every existing " +
                "non-negative locomotion child. Existing negative MovementSpeed children " +
                "are replaced with Walk Backward at -0.5 and Run Backward at -1.0.",
                MessageType.Info);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Resulting signed MovementSpeed layout:");
            EditorGUILayout.LabelField("-1.0  -> Run Backward");
            EditorGUILayout.LabelField("-0.5  -> Walk Backward");
            EditorGUILayout.LabelField(" 0.0  -> Idle");
            EditorGUILayout.LabelField("+0.5  -> Walk Forward (existing)");
            EditorGUILayout.LabelField("+1.0  -> Run Forward (existing)");

            EditorGUILayout.Space(8f);

            bool missingRequiredReference =
                controller == null ||
                walkBackwardClip == null ||
                runBackwardClip == null;

            using (new EditorGUI.DisabledScope(missingRequiredReference))
            {
                if (GUILayout.Button("Apply Walk + Run Backward Locomotion"))
                    Apply();
            }
        }

        private void Apply()
        {
            try
            {
                EnsureMovementSpeedParameter(controller);

                BlendTree locomotion =
                    FindLocomotionBlendTree(controller);

                ConfigureBackwardChildren(
                    locomotion,
                    walkBackwardClip,
                    runBackwardClip);

                EditorUtility.SetDirty(locomotion);
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log(
                    "[ZombieWar][SoldierAnimator] Backward locomotion configured: " +
                    $"controller='{controller.name}', " +
                    $"walkBackward='{walkBackwardClip.name}' @ {WalkBackwardThreshold}, " +
                    $"runBackward='{runBackwardClip.name}' @ {RunBackwardThreshold}.");

                EditorUtility.DisplayDialog(
                    "Soldier Backward Locomotion",
                    "Walk Backward and Run Backward were added successfully.\n\n" +
                    "MovementSpeed thresholds:\n" +
                    "-1.0 = Run Backward\n" +
                    "-0.5 = Walk Backward\n" +
                    " 0.0 = Idle\n" +
                    "+0.5 = Walk Forward\n" +
                    "+1.0 = Run Forward",
                    "OK");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "Soldier Backward Locomotion",
                    exception.Message,
                    "OK");
            }
        }

        private static void EnsureMovementSpeedParameter(
            AnimatorController animatorController)
        {
            AnimatorControllerParameter[] parameters =
                animatorController.parameters;

            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];

                if (!string.Equals(
                        parameter.name,
                        SoldierAnimatorContract.MovementSpeed,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                if (parameter.type != AnimatorControllerParameterType.Float)
                {
                    throw new InvalidOperationException(
                        $"Animator parameter '{SoldierAnimatorContract.MovementSpeed}' " +
                        "must be a Float.");
                }

                return;
            }

            animatorController.AddParameter(
                SoldierAnimatorContract.MovementSpeed,
                AnimatorControllerParameterType.Float);
        }

        private static BlendTree FindLocomotionBlendTree(
            AnimatorController animatorController)
        {
            AnimatorControllerLayer[] layers = animatorController.layers;

            for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
            {
                AnimatorStateMachine stateMachine =
                    layers[layerIndex].stateMachine;

                ChildAnimatorState[] states = stateMachine.states;

                for (int stateIndex = 0; stateIndex < states.Length; stateIndex++)
                {
                    AnimatorState state = states[stateIndex].state;

                    if (!string.Equals(
                            state.name,
                            LocomotionStateName,
                            StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (!(state.motion is BlendTree blendTree))
                    {
                        throw new InvalidOperationException(
                            $"State '{LocomotionStateName}' is not using a BlendTree.");
                    }

                    if (blendTree.blendType != BlendTreeType.Simple1D)
                    {
                        throw new InvalidOperationException(
                            "Locomotion BlendTree must use 1D blending.");
                    }

                    return blendTree;
                }
            }

            throw new InvalidOperationException(
                $"Could not find Animator state '{LocomotionStateName}'.");
        }

        private static void ConfigureBackwardChildren(
            BlendTree blendTree,
            AnimationClip walkBackward,
            AnimationClip runBackward)
        {
            if (walkBackward == null)
                throw new ArgumentNullException(nameof(walkBackward));

            if (runBackward == null)
                throw new ArgumentNullException(nameof(runBackward));

            blendTree.blendParameter =
                SoldierAnimatorContract.MovementSpeed;

            blendTree.useAutomaticThresholds = false;

            ChildMotion[] existing = blendTree.children;
            var children = new List<ChildMotion>(existing.Length + 2);

            // Preserve Idle / Walk Forward / Run Forward and any other authored
            // non-negative children. Remove old negative children so running the tool
            // repeatedly is deterministic and never creates duplicates.
            for (int i = 0; i < existing.Length; i++)
            {
                if (existing[i].threshold >= 0f)
                    children.Add(existing[i]);
            }

            children.Add(CreateChild(
                runBackward,
                RunBackwardThreshold));

            children.Add(CreateChild(
                walkBackward,
                WalkBackwardThreshold));

            children.Sort((a, b) =>
                a.threshold.CompareTo(b.threshold));

            blendTree.children = children.ToArray();
        }

        private static ChildMotion CreateChild(
            Motion motion,
            float threshold)
        {
            return new ChildMotion
            {
                motion = motion,
                threshold = threshold,
                position = Vector2.zero,
                timeScale = 1f,
                cycleOffset = 0f,
                mirror = false,
                directBlendParameter = string.Empty
            };
        }
    }
}
#endif
