using System;
using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Impact
{
    public sealed class ProjectileImpactPolicyProvider : IProjectileImpactPolicyProvider
    {
        private readonly StopOnHitPolicy _stopOnHit = new StopOnHitPolicy();
        private readonly PiercingImpactPolicy _pierce = new PiercingImpactPolicy();
        private readonly ExplodeOnGroundPolicy _explode = new ExplodeOnGroundPolicy();

        public IProjectileImpactPolicy Get(ProjectileImpactMode mode)
        {
            switch (mode)
            {
                case ProjectileImpactMode.StopOnHit: 
                    return _stopOnHit;

                case ProjectileImpactMode.Pierce: 
                    return _pierce;

                case ProjectileImpactMode.ExplodeOnGround: 
                    return _explode;
                    
                default: throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }
    }
}
