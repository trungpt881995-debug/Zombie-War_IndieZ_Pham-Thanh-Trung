using GameplayCore.Damage;

namespace ZombieWar.Integration.Zombie
{
    public interface IZombieAttackBinding
    {
        void BindSharedSoldierGroup(IDamageable damageable);
        void Unbind();
    }
}
