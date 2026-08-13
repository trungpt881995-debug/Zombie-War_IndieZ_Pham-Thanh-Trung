using System; using System.Collections.Generic; using ZombieWar.Features.Spawn.Domain;
namespace ZombieWar.Features.Spawn.Catalog
{
    public sealed class SpawnTuningCatalog : ISpawnTuningCatalog
    {
        private readonly Dictionary<SpawnDifficultyKey,SpawnTuning> _items;
        public SpawnTuningCatalog(IReadOnlyList<SpawnTuningEntry> entries)
        {
            if(entries==null) throw new ArgumentNullException(nameof(entries));
            _items=new Dictionary<SpawnDifficultyKey,SpawnTuning>(entries.Count);
            for(int i=0;i<entries.Count;i++)
            {
                SpawnTuningEntry e=entries[i];
                if(_items.ContainsKey(e.Key)) throw new ArgumentException($"Duplicate spawn tuning key: {e.Key}.",nameof(entries));
                _items.Add(e.Key,e.Tuning);
            }
        }
        public bool TryGet(in SpawnDifficultyKey key,out SpawnTuning tuning) { tuning=default; return _items.TryGetValue(key,out tuning); }
    }
}
