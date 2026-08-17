using System;
using UnityEngine;
using ZombieWar.Features.Projectile.Services;

namespace ZombieWar.Features.Projectile.Unity.Runtime
{
    [DisallowMultipleComponent]
    public sealed class ProjectileRuntimeRoot : MonoBehaviour
    {
        public IProjectileLauncher Launcher { get; private set; }
        public bool IsInitialized => Launcher != null;

        public void Initialize(IProjectileLauncher launcher)
        {
            if (IsInitialized)
            {
                return;
            }

            Launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
        }

        /// <summary>
        /// Hitscan shots are resolved immediately, so there are no flying projectiles to cancel.
        /// Kept for compatibility with the previous runtime contract.
        /// </summary>
        public void CancelAll()
        {
        }

        private void OnDestroy()
        {
            Launcher = null;
        }
    }
}
