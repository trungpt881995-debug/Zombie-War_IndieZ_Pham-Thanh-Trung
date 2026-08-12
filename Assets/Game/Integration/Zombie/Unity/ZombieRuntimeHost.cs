using UnityEngine;
using ZombieWar.Features.Projectile.Unity.Collision;
using ZombieWar.Features.Zombie.Controller;
using ZombieWar.Features.Zombie.Unity.Movement;
using ZombieWar.Features.Zombie.Unity.View;

namespace ZombieWar.Integration.Zombie.Unity
{
    [DisallowMultipleComponent]
    public sealed class ZombieRuntimeHost : MonoBehaviour
    {
        [SerializeField] private ZombieView zombieView;
        [SerializeField] private CharacterControllerZombieMotor motor;
        [SerializeField] private ProjectileDamageableProxy projectileDamageableProxy;
        public ZombieView View => zombieView;
        public CharacterControllerZombieMotor Motor => motor;
        public ProjectileDamageableProxy DamageableProxy => projectileDamageableProxy;
        public ZombieController Controller { get; private set; }
        public ZombieCombatBridge CombatBridge { get; private set; }

        private void Awake()
        {
            if (zombieView == null) zombieView = GetComponent<ZombieView>();
            if (motor == null) motor = GetComponent<CharacterControllerZombieMotor>();
            if (projectileDamageableProxy == null) projectileDamageableProxy = GetComponent<ProjectileDamageableProxy>();
        }

        public void Bind(ZombieController controller, ZombieCombatBridge bridge)
        {
            Controller = controller;
            CombatBridge = bridge;
            if (projectileDamageableProxy != null) projectileDamageableProxy.Initialize(bridge);
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
            if (zombieView != null && Controller != null)
            {
                zombieView.AttackImpact -= Controller.NotifyAttackImpact;
                zombieView.AttackFinished -= Controller.NotifyAttackAnimationFinished;
                zombieView.HitFinished -= Controller.NotifyHitAnimationFinished;
                zombieView.DeathFinished -= Controller.NotifyDeathAnimationFinished;
            }
        }
    }
}
