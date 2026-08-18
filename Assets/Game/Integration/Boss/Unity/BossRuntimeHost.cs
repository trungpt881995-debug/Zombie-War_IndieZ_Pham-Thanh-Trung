using UnityEngine;
using ZombieWar.Features.Projectile.Unity.Collision;
using ZombieWar.Features.Boss.Controller;
using ZombieWar.Features.Boss.Domain;
using ZombieWar.Features.Boss.Unity.Movement;
using ZombieWar.Features.Boss.Unity.View;

namespace ZombieWar.Integration.Boss.Unity
{
    [DisallowMultipleComponent]
    public sealed class BossRuntimeHost : MonoBehaviour
    {
        [Header("Runtime")]
        [SerializeField] private BossView bossView;
        [SerializeField] private NavMeshBossMotor motor;
        [SerializeField] private ProjectileDamageableProxy projectileDamageableProxy;

        [Header("Soldier Target Aim Point")]
        [Tooltip(
            "Optional world target point used by Soldier Targeting/Weapon. " +
            "Create a direct child named AimPoint around the Boss chest/upper torso " +
            "and assign it here. If empty, Awake tries to find child 'AimPoint'.")]
        [SerializeField] private Transform aimPoint;

        [Tooltip(
            "Fallback LOCAL offset used only when AimPoint is not assigned/found. " +
            "Because TransformPoint is used, Boss root scale is respected.")]
        [SerializeField] private Vector3 fallbackAimLocalOffset =
            new Vector3(0f, 1.25f, 0f);

        public BossView View => bossView;
        public NavMeshBossMotor Motor => motor;
        public ProjectileDamageableProxy DamageableProxy =>
            projectileDamageableProxy;
        public Transform AimPoint => aimPoint;
        public BossController Controller { get; private set; }
        public BossCombatBridge CombatBridge { get; private set; }

        private void Awake()
        {
            if (bossView == null)
            {
                bossView = GetComponent<BossView>();
            }

            if (motor == null)
            {
                motor = GetComponent<NavMeshBossMotor>();
            }

            if (motor == null)
            {
                throw new MissingComponentException(
                    $"{nameof(BossRuntimeHost)} on '{name}' requires {nameof(NavMeshBossMotor)}. " +
                    "Add NavMeshBossMotor + NavMeshAgent to the Boss prefab before Play Mode.");
            }

            if (projectileDamageableProxy == null)
            {
                projectileDamageableProxy =
                    GetComponent<ProjectileDamageableProxy>();
            }

            if (aimPoint == null)
            {
                aimPoint = transform.Find("AimPoint");
            }
        }

        /// <summary>
        /// Target position exposed to Soldier Targeting and therefore Weapon.
        /// This intentionally does NOT replace BossController.Position used by
        /// Boss AI/movement/range logic.
        /// </summary>
        public BossPoint GetTargetPosition()
        {
            Vector3 worldPosition = aimPoint != null
                ? aimPoint.position
                : transform.TransformPoint(fallbackAimLocalOffset);

            return new BossPoint(
                worldPosition.x,
                worldPosition.y,
                worldPosition.z);
        }

        public void Bind(
            BossController controller,
            BossCombatBridge bridge)
        {
            Controller = controller;
            CombatBridge = bridge;

            if (projectileDamageableProxy != null)
            {
                projectileDamageableProxy.Initialize(bridge);
            }

            if (bossView != null)
            {
                bossView.AttackImpact += controller.NotifyAttackImpact;
                bossView.AttackFinished +=
                    controller.NotifyAttackAnimationFinished;
                bossView.HitFinished +=
                    controller.NotifyHitAnimationFinished;
                bossView.DeathFinished +=
                    controller.NotifyDeathAnimationFinished;
            }
        }

        private void OnDestroy()
        {
            if (bossView != null && Controller != null)
            {
                bossView.AttackImpact -= Controller.NotifyAttackImpact;
                bossView.AttackFinished -=
                    Controller.NotifyAttackAnimationFinished;
                bossView.HitFinished -=
                    Controller.NotifyHitAnimationFinished;
                bossView.DeathFinished -=
                    Controller.NotifyDeathAnimationFinished;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 position = aimPoint != null
                ? aimPoint.position
                : transform.TransformPoint(fallbackAimLocalOffset);

            Gizmos.DrawWireSphere(position, 0.15f);
            Gizmos.DrawLine(transform.position, position);
        }
#endif
    }
}
