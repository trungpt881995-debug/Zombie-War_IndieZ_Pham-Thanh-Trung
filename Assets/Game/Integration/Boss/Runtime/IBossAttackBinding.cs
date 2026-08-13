using GameplayCore.Damage; namespace ZombieWar.Integration.Boss { public interface IBossAttackBinding { void BindSharedSoldierGroup(IDamageable damageable); void Unbind(); } }
