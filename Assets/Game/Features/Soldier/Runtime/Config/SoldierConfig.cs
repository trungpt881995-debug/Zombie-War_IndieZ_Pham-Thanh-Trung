using UnityEngine;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Config
{
    [CreateAssetMenu(
        fileName = "SoldierConfig",
        menuName = "Zombie War/Soldier/Soldier Config")]
    public sealed class SoldierConfig : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField]
        [Min(0.01f)]
        private float moveSpeed = 5f;

        [Tooltip(
            "How quickly each Soldier body turns toward its desired facing " +
            "direction. 0 = snap instantly.")]
        [SerializeField]
        [Min(0f)]
        private float moveRotationDegreesPerSecond = 720f;

        [Header("Aiming")]
        [Tooltip(
            "Upper-body aim smoothing speed. Animator-layer aiming can ignore " +
            "this when procedural aiming is disabled.")]
        [SerializeField]
        [Min(0f)]
        private float aimRotationDegreesPerSecond = 720f;

        [Tooltip(
            "When the target is farther than this angle from the movement/body " +
            "forward reference, the whole Soldier turns to face the target.")]
        [SerializeField]
        [Range(0f, 180f)]
        private float bodyTurnEnterAimAngleDegrees = 100f;

        [Tooltip(
            "Hysteresis release angle. After target-facing begins, return to " +
            "movement-facing only when movement-to-target is at/below this value.")]
        [SerializeField]
        [Range(0f, 180f)]
        private float bodyTurnReleaseAimAngleDegrees = 80f;

        public SoldierSettings CreateSettings()
        {
            return new SoldierSettings(
                moveSpeed,
                moveRotationDegreesPerSecond,
                aimRotationDegreesPerSecond,
                bodyTurnEnterAimAngleDegrees,
                bodyTurnReleaseAimAngleDegrees);
        }

        private void OnValidate()
        {
            if (bodyTurnReleaseAimAngleDegrees > bodyTurnEnterAimAngleDegrees)
            {
                bodyTurnReleaseAimAngleDegrees = bodyTurnEnterAimAngleDegrees;
            }
        }
    }
}
