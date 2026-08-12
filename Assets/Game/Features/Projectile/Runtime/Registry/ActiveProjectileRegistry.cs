using System;
using System.Collections.Generic;
using ZombieWar.Features.Projectile.Controller;

namespace ZombieWar.Features.Projectile.Registry
{
    public sealed class ActiveProjectileRegistry : IActiveProjectileRegistry
    {
        private readonly List<ProjectileController> _active = new List<ProjectileController>(64);
        public int Count => _active.Count;
        public ProjectileController GetAt(int index) => _active[index];

        public bool Add(ProjectileController projectile)
        {
            if (projectile == null) throw new ArgumentNullException(nameof(projectile));
            if (_active.Contains(projectile)) return false;
            _active.Add(projectile);
            return true;
        }

        public bool Remove(ProjectileController projectile)
        {
            int index = _active.IndexOf(projectile);
            if (index < 0) return false;
            int last = _active.Count - 1;
            _active[index] = _active[last];
            _active.RemoveAt(last);
            return true;
        }

        public void Clear() => _active.Clear();
    }
}
