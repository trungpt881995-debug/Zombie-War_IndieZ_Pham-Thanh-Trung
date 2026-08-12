using GeneralCore.Architecture;
using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Ports
{
    public interface IProjectileView : IView
    {
        ProjectilePoint Position { get; }
        void Activate(in ProjectileViewLaunchData data);
        void Deactivate();
    }
}
