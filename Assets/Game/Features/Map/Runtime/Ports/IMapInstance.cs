using ZombieWar.Features.Map.Domain;

namespace ZombieWar.Features.Map.Ports
{
    public interface IMapInstance
    {
        MapId MapId { get; }
        IMapView View { get; }
    }
}
