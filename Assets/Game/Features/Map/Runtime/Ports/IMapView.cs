using GeneralCore.Architecture;
using ZombieWar.Features.Map.Domain;

namespace ZombieWar.Features.Map.Ports
{
    public interface IMapView : IView
    {
        MapId Id { get; }
        bool TryBuildContext(out MapRuntimeContext context, out string error);
    }
}
