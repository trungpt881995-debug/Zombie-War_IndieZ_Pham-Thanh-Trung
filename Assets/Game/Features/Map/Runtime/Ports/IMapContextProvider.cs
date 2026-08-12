using ZombieWar.Features.Map.Domain;

namespace ZombieWar.Features.Map.Ports
{
    public interface IMapContextProvider
    {
        bool TryGetCurrentContext(out MapRuntimeContext context);
    }
}
