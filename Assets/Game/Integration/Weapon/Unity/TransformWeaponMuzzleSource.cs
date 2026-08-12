using UnityEngine;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Integration.Weapon.Unity
{
    [DisallowMultipleComponent]
    public sealed class TransformWeaponMuzzleSource : MonoBehaviour, IWeaponMuzzleSource
    {
        public WeaponMuzzle CurrentMuzzle
        {
            get
            {
                Vector3 p = transform.position;
                Vector3 f = transform.forward;
                var point = new WeaponPoint(p.x, p.y, p.z);
                var direction = new WeaponDirection(f.x, f.y, f.z);
                return new WeaponMuzzle(in point, in direction);
            }
        }
    }
}
