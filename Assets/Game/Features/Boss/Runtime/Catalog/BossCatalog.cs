using System; using System.Collections.Generic; using ZombieWar.Features.Boss.Domain;
namespace ZombieWar.Features.Boss.Catalog
{
    public sealed class BossCatalog:IBossCatalog
    {
        private readonly Dictionary<BossId,BossDefinition> _definitions=new Dictionary<BossId,BossDefinition>(3);
        public BossCatalog(IReadOnlyList<BossDefinition> definitions){if(definitions==null)throw new ArgumentNullException(nameof(definitions));for(int i=0;i<definitions.Count;i++){BossDefinition d=definitions[i];if(d.Id==BossId.None)throw new ArgumentException("Boss None is invalid.");if(_definitions.ContainsKey(d.Id))throw new ArgumentException($"Duplicate Boss definition: {d.Id}");_definitions.Add(d.Id,d);}if(_definitions.Count!=3||!_definitions.ContainsKey(BossId.BossA)||!_definitions.ContainsKey(BossId.BossB)||!_definitions.ContainsKey(BossId.BossC))throw new ArgumentException("BossCatalog requires exactly BossA, BossB and BossC.");}
        public bool TryGet(BossId id,out BossDefinition definition){definition=default;if(id==BossId.None)return false;return _definitions.TryGetValue(id,out definition);}
    }
}
