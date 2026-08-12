using UnityEngine;
using ZombieWar.Features.Projectile.Controller;
using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Unity.Collision
{
    [DisallowMultipleComponent]
    public sealed class ProjectileCollisionRelay : MonoBehaviour
    {
        private ProjectileController _controller;

        public void Bind(ProjectileController controller) => _controller = controller;

        private void OnCollisionEnter(UnityEngine.Collision collision)
        {
            if (_controller == null || !_controller.IsFlying || collision == null) return;
            Vector3 point = collision.contactCount > 0 ? collision.GetContact(0).point : collision.collider.ClosestPoint(transform.position);
            Dispatch(collision.collider, point);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_controller == null || !_controller.IsFlying || other == null) return;
            Vector3 point = other.ClosestPoint(transform.position);
            Dispatch(other, point);
        }

        private void Dispatch(Collider other, Vector3 worldPoint)
        {
            var point = new ProjectilePoint(worldPoint.x, worldPoint.y, worldPoint.z);

            ProjectileDamageableProxy damageableProxy = other.GetComponentInParent<ProjectileDamageableProxy>();
            if (damageableProxy != null && damageableProxy.Damageable != null)
            {
                ProjectileCollision hit = ProjectileCollision.ForDamageable(damageableProxy.Damageable, in point);
                _controller.HandleCollision(in hit);
                return;
            }

            ProjectileCollisionSurface surface = other.GetComponentInParent<ProjectileCollisionSurface>();
            ProjectileCollisionKind kind = surface != null ? surface.Kind : ProjectileCollisionKind.Unknown;
            ProjectileCollision collision = ProjectileCollision.ForSurface(kind, in point);
            _controller.HandleCollision(in collision);
        }
    }
}
