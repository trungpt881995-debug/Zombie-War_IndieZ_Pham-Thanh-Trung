using System; using System.Collections.Generic; using ZombieWar.Features.VFX.Domain;
namespace ZombieWar.Features.VFX.Catalog
{
    public sealed class VFXCatalog:IVFXCatalog
    {
        private readonly Dictionary<VFXId,VFXDefinition> _items;
        public int Count=>_items.Count;
        public VFXCatalog(IReadOnlyList<VFXDefinition> definitions)
        {
            if(definitions==null)throw new ArgumentNullException(nameof(definitions)); _items=new Dictionary<VFXId,VFXDefinition>(definitions.Count);
            for(int i=0;i<definitions.Count;i++){var d=definitions[i];if(_items.ContainsKey(d.Id))throw new ArgumentException("Duplicate VFXId: "+d.Id);_items.Add(d.Id,d);}
        }
        public bool TryGet(VFXId id,out VFXDefinition definition)=>_items.TryGetValue(id,out definition);
    }
}
