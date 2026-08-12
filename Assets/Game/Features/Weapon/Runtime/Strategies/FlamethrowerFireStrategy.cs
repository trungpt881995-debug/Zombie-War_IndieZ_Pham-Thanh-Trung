using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Ports;

namespace ZombieWar.Features.Weapon.Strategies
{
    public sealed class FlamethrowerFireStrategy : IWeaponFireStrategy
    {
        private readonly IWeaponFlamePort _flame;
        private readonly IWeaponFeedbackPort _feedback;

        public FlamethrowerFireStrategy(IWeaponFlamePort flame, IWeaponFeedbackPort feedback)
        { _flame = flame; _feedback = feedback; }

        public void OnTargetAcquired(in WeaponDefinition weapon, in WeaponFireContext context)
        {
            if (!TryCreateRequest(in weapon, in context, out WeaponFlameRequest request)) return;
            _flame.Begin(in request);
            _feedback.OnFlameStarted(context.OwnerId);
        }

        public bool Fire(in WeaponDefinition weapon, in WeaponFireContext context)
        {
            if (!TryCreateRequest(in weapon, in context, out WeaponFlameRequest request)) return false;
            _flame.ApplyTick(in request);
            return true;
        }

        public void OnTargetCleared(EntityId ownerId)
        {
            _flame.End(ownerId);
            _feedback.OnFlameStopped(ownerId);
        }

        private static bool TryCreateRequest(
            in WeaponDefinition weapon,
            in WeaponFireContext context,
            out WeaponFlameRequest request)
        {
            WeaponPoint origin = context.Muzzle.Position;
            WeaponPoint target = context.Target.Position;
            if (!WeaponDirection.TryFromTo(in origin, in target, out WeaponDirection direction))
            { request = default; return false; }
            request = new WeaponFlameRequest(
                context.OwnerId, context.Target.TargetId,
                in origin, in direction,
                weapon.TargetRange, weapon.FlameRadius, weapon.Damage);
            return true;
        }
    }
}
