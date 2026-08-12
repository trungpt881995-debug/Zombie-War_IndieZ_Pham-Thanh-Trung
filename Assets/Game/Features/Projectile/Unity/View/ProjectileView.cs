using UnityEngine;
using ZombieWar.Features.Projectile.Domain;
using ZombieWar.Features.Projectile.Ports;

namespace ZombieWar.Features.Projectile.Unity.View
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public sealed class ProjectileView : MonoBehaviour, IProjectileView
    {
        [SerializeField] private CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        private Rigidbody _body;
        private Collider _collider;
        private TrailRenderer[] _trails;

        public ProjectilePoint Position
        {
            get
            {
                Vector3 p = transform.position;
                return new ProjectilePoint(p.x, p.y, p.z);
            }
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _trails = GetComponentsInChildren<TrailRenderer>(true);
            PrepareInactive();
        }

        public void Activate(in ProjectileViewLaunchData data)
        {
            Vector3 position = new Vector3(data.Origin.X, data.Origin.Y, data.Origin.Z);
            Vector3 velocity = new Vector3(data.InitialVelocity.X, data.InitialVelocity.Y, data.InitialVelocity.Z);

            transform.position = position;
            if (velocity.sqrMagnitude > 0.000001f)
                transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);

            ClearTrails();
            _body.isKinematic = false;
            _body.useGravity = data.UseGravity;
            _body.detectCollisions = true;
            _body.collisionDetectionMode = collisionDetectionMode;
            _body.linearVelocity = velocity;
            _body.angularVelocity = Vector3.zero;
            _collider.enabled = true;
            gameObject.SetActive(true);
            _body.WakeUp();
        }

        public void Deactivate()
        {
            if (_body != null)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
                _body.detectCollisions = false;
                _body.useGravity = false;
                _body.isKinematic = true;
            }
            if (_collider != null) _collider.enabled = false;
            gameObject.SetActive(false);
        }

        private void PrepareInactive()
        {
            if (_body != null)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
                _body.detectCollisions = false;
                _body.useGravity = false;
                _body.isKinematic = true;
            }
            if (_collider != null) _collider.enabled = false;
            ClearTrails();
        }

        private void ClearTrails()
        {
            if (_trails == null) return;
            for (int i = 0; i < _trails.Length; i++)
            {
                if (_trails[i] != null) 
                    _trails[i].Clear();
            }
                
        }
    }
}
