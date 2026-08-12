using GameplayCore.Entities;
using ZombieWar.Features.Soldier.Domain;
using ZombieWar.Features.Soldier.Ports;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Services;

namespace ZombieWar.Integration.Weapon
{
    public sealed class WeaponToSoldierAttackAdapter : ISoldierAttackPort
    {
        private readonly IWeaponAttackService _attack;
        public WeaponToSoldierAttackAdapter(IWeaponAttackService attack) => _attack = attack;

        public void Update(EntityId soldierId, in SoldierTargetInfo target, float deltaTime)
        {
            if (!target.HasTarget) { _attack.ClearTarget(soldierId); return; }
            SoldierPoint position = target.Position;
            var weaponPoint = new WeaponPoint(position.X, position.Y, position.Z);
            var weaponTarget = new WeaponTarget(target.TargetId, in weaponPoint);
            _attack.Update(soldierId, in weaponTarget, deltaTime);
        }

        public void ClearTarget(EntityId soldierId) => _attack.ClearTarget(soldierId);
    }
}
