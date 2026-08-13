using UnityEngine; using ZombieWar.Features.Projectile.Unity.Collision; using ZombieWar.Features.Boss.Controller; using ZombieWar.Features.Boss.Unity.Movement; using ZombieWar.Features.Boss.Unity.View;
namespace ZombieWar.Integration.Boss.Unity
{
    [DisallowMultipleComponent]
    public sealed class BossRuntimeHost:MonoBehaviour
    {
        [SerializeField] private BossView bossView;[SerializeField] private CharacterControllerBossMotor motor;[SerializeField] private ProjectileDamageableProxy projectileDamageableProxy;
        public BossView View=>bossView;public CharacterControllerBossMotor Motor=>motor;public BossController Controller{get;private set;}public BossCombatBridge CombatBridge{get;private set;}
        private void Awake(){if(bossView==null)bossView=GetComponent<BossView>();if(motor==null)motor=GetComponent<CharacterControllerBossMotor>();if(projectileDamageableProxy==null)projectileDamageableProxy=GetComponent<ProjectileDamageableProxy>();}
        public void Bind(BossController controller,BossCombatBridge bridge){Controller=controller;CombatBridge=bridge;if(projectileDamageableProxy!=null)projectileDamageableProxy.Initialize(bridge);if(bossView!=null){bossView.AttackImpact+=controller.NotifyAttackImpact;bossView.AttackFinished+=controller.NotifyAttackAnimationFinished;bossView.HitFinished+=controller.NotifyHitAnimationFinished;bossView.DeathFinished+=controller.NotifyDeathAnimationFinished;}}
        private void OnDestroy(){if(bossView!=null&&Controller!=null){bossView.AttackImpact-=Controller.NotifyAttackImpact;bossView.AttackFinished-=Controller.NotifyAttackAnimationFinished;bossView.HitFinished-=Controller.NotifyHitAnimationFinished;bossView.DeathFinished-=Controller.NotifyDeathAnimationFinished;}}
    }
}
