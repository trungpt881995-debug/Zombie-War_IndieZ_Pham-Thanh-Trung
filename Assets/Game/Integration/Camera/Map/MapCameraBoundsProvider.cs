using System;
using ZombieWar.Features.Camera.Domain;
using ZombieWar.Features.Camera.Ports;
using ZombieWar.Features.Map.Services;

namespace ZombieWar.Integration.Camera.Map
{
    public sealed class MapCameraBoundsProvider : ICameraBoundsProvider
    {
        private readonly IMapRuntime _mapRuntime;

        public MapCameraBoundsProvider(IMapRuntime mapRuntime) =>
            _mapRuntime = mapRuntime ?? throw new ArgumentNullException(nameof(mapRuntime));

        public bool TryGetBounds(out CameraBounds bounds)
        {
            if (!_mapRuntime.TryGetCurrentContext(out ZombieWar.Features.Map.Domain.MapRuntimeContext context))
            {
                bounds = default;
                return false;
            }

            ZombieWar.Features.Map.Domain.MapBounds source = context.CameraBounds;
            if (!source.IsValid)
            {
                bounds = default;
                return false;
            }

            bounds = new CameraBounds(source.MinX, source.MaxX, source.MinZ, source.MaxZ);
            return true;
        }
    }
}
