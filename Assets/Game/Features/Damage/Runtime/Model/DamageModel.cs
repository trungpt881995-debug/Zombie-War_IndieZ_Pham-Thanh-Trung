using GameplayCore.Damage;
using GameplayCore.Entities;
using ZombieWar.Features.Damage.Domain;

namespace ZombieWar.Features.Damage.Model
{
    /// <summary>
    /// Pure C# MVC Model. Owns damage validation and damage calculation rules.
    /// Version 1 intentionally applies no armor/critical/resistance mechanics
    /// because they are not defined by the current Zombie War specification.
    /// </summary>
    public sealed class DamageModel
    {
        public DamageResolution Resolve(IDamageable target, in DamageInfo damage)
        {
            if (target == null)
            {
                return Reject(in damage, default, DamageRejectionReason.TargetMissing);
            }

            if (!target.IsAlive)
            {
                return Reject(in damage, target.EntityId, DamageRejectionReason.TargetNotAlive);
            }

            if (damage.Amount <= 0f || float.IsNaN(damage.Amount) || float.IsInfinity(damage.Amount))
            {
                return Reject(in damage, target.EntityId, DamageRejectionReason.InvalidAmount);
            }

            // Current specification has no mitigation/critical/resistance formula.
            float finalDamage = damage.Amount;

            return new DamageResolution(damage.Source, target.EntityId, damage.Amount, finalDamage, damage.Type, true, DamageRejectionReason.None);
        }

        private static DamageResolution Reject(in DamageInfo damage, EntityId targetId, DamageRejectionReason reason)
        {
            return new DamageResolution(damage.Source,targetId,damage.Amount,0f,damage.Type,false,reason);
        }
    }
}
