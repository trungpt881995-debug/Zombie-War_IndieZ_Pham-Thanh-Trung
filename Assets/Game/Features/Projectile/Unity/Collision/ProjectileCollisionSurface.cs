using UnityEngine;
using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Unity.Collision
{
    [DisallowMultipleComponent]
    public sealed class ProjectileCollisionSurface : MonoBehaviour
    {
        [SerializeField] private ProjectileCollisionKind kind = ProjectileCollisionKind.Environment;
        public ProjectileCollisionKind Kind => kind;

        private void OnValidate()
        {
            if (kind == ProjectileCollisionKind.Damageable)
                kind = ProjectileCollisionKind.Environment;
        }
    }
}
