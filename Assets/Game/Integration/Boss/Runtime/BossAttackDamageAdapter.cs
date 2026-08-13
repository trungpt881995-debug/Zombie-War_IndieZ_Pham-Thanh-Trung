using GameplayCore.Damage; using ZombieWar.Features.Boss.Domain; using ZombieWar.Features.Boss.Ports;
namespace ZombieWar.Integration.Boss
{
    public sealed class BossAttackDamageAdapter:IBossAttackPort,IBossAttackBinding
    {
        private readonly IDamageService _damage;private IDamageable _shared;public BossAttackDamageAdapter(IDamageService damage){_damage=damage;}
        public void BindSharedSoldierGroup(IDamageable d)=>_shared=d;public void Unbind()=>_shared=null;
        public bool TryAttack(in BossAttackRequest request){if(_shared==null||!_shared.IsAlive||request.Damage<=0f)return false;var info=new DamageInfo(request.AttackerId,request.Damage,"BossAttack");return _damage.TryApply(_shared,info);}
    }
}
