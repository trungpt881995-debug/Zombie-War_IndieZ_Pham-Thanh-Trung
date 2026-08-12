using ZombieWar.Features.Map.Domain;

namespace ZombieWar.Features.Map.Model
{
    public sealed class MapModel
    {
        public MapState State { get; private set; } = MapState.Unloaded;
        public MapId CurrentMapId { get; private set; } = MapId.None;
        public MapRuntimeContext Context { get; private set; }
        public int Generation { get; private set; }

        public int BeginOperation()
        {
            Generation++;
            return Generation;
        }

        public void BeginLoading()
        {
            State = MapState.Loading;
            CurrentMapId = MapId.None;
            Context = null;
        }

        public void SetLoaded(MapId mapId, MapRuntimeContext context)
        {
            CurrentMapId = mapId;
            Context = context;
            State = MapState.Loaded;
        }

        public void BeginUnloading() => State = MapState.Unloading;

        public void SetUnloaded()
        {
            State = MapState.Unloaded;
            CurrentMapId = MapId.None;
            Context = null;
        }

        public void SetFailed()
        {
            State = MapState.Failed;
            CurrentMapId = MapId.None;
            Context = null;
        }
    }
}
