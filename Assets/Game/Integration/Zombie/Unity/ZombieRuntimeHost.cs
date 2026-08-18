using UnityEngine;
using ZombieWar.Features.Projectile.Unity.Collision;
using ZombieWar.Features.Targeting.Domain;
using ZombieWar.Features.Zombie.Controller;
using ZombieWar.Features.Zombie.Unity.Movement;
using ZombieWar.Features.Zombie.Unity.View;

namespace ZombieWar.Integration.Zombie.Unity
{
    [DisallowMultipleComponent]
    public sealed class ZombieRuntimeHost : MonoBehaviour
    {
        [SerializeField] private ZombieView zombieView;
        [SerializeField] private NavMeshZombieMotor motor;
        [SerializeField] private ProjectileDamageableProxy projectileDamageableProxy;

        [Header("Soldier Targeting")]
        [Tooltip("Recommended: place this Transform around the Zombie upper chest. If null, a root-relative fallback is used.")]
        [SerializeField] private Transform aimPoint;

        [SerializeField, Min(0f)]
        private float fallbackAimHeight = 1.25f;

        public ZombieView View => zombieView;
        public NavMeshZombieMotor Motor => EnsureMotor();
        public ProjectileDamageableProxy DamageableProxy => projectileDamageableProxy;
        public ZombieController Controller { get; private set; }
        public ZombieCombatBridge CombatBridge { get; private set; }

        private void Awake()
        {
            if (zombieView == null)
                zombieView = GetComponent<ZombieView>();

            EnsureMotor();

            if (projectileDamageableProxy == null)
                projectileDamageableProxy = GetComponent<ProjectileDamageableProxy>();

            if (aimPoint == null)
                aimPoint = FindChildRecursive(transform, "AimPoint");
        }

        /// <summary>
        /// Targeting/Weapon point only. ZombieController.Position remains the root
        /// position for AI movement, attack range and other Zombie semantics.
        /// </summary>
        public TargetPoint GetCurrentTargetPoint()
        {
            Vector3 p;

            if (aimPoint != null)
            {
                p = aimPoint.position;
            }
            else
            {
                // TransformPoint also scales the fallback for giant Boss variants.
                p = transform.TransformPoint(
                    new Vector3(0f, fallbackAimHeight, 0f));
            }

            return new TargetPoint(p.x, p.y, p.z);
        }

        public void Bind(
            ZombieController controller,
            ZombieCombatBridge bridge)
        {
            Controller = controller;
            CombatBridge = bridge;

            if (projectileDamageableProxy != null)
                projectileDamageableProxy.Initialize(bridge);

            if (zombieView != null)
            {
                zombieView.AttackImpact += controller.NotifyAttackImpact;
                zombieView.AttackFinished += controller.NotifyAttackAnimationFinished;
                zombieView.HitFinished += controller.NotifyHitAnimationFinished;
                zombieView.DeathFinished += controller.NotifyDeathAnimationFinished;
            }
        }

        private void OnDestroy()
        {
            if (zombieView == null || Controller == null)
                return;

            zombieView.AttackImpact -= Controller.NotifyAttackImpact;
            zombieView.AttackFinished -= Controller.NotifyAttackAnimationFinished;
            zombieView.HitFinished -= Controller.NotifyHitAnimationFinished;
            zombieView.DeathFinished -= Controller.NotifyDeathAnimationFinished;
        }


        private NavMeshZombieMotor EnsureMotor()
        {
            if (motor == null)
                motor = GetComponent<NavMeshZombieMotor>();

            if (motor == null)
                motor = gameObject.AddComponent<NavMeshZombieMotor>();

            return motor;
        }

        private static Transform FindChildRecursive(
            Transform root,
            string childName)
        {
            if (root == null)
                return null;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);

                if (child.name == childName)
                    return child;

                Transform nested = FindChildRecursive(child, childName);
                if (nested != null)
                    return nested;
            }

            return null;
        }
    }
}
