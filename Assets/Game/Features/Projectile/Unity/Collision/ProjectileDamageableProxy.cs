using GameplayCore.Damage;
using UnityEngine;

namespace ZombieWar.Features.Projectile.Unity.Collision
{
    [DisallowMultipleComponent]
    public sealed class ProjectileDamageableProxy : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour damageableBehaviour;
        private IDamageable _runtimeDamageable;

        public IDamageable Damageable =>
            _runtimeDamageable ?? damageableBehaviour as IDamageable;

        public void Initialize(IDamageable damageable)
        {
            _runtimeDamageable = damageable;
        }

        private void OnValidate()
        {
            if (damageableBehaviour != null && !(damageableBehaviour is IDamageable))
                damageableBehaviour = null;
        }
    }
}
