using System; using System.Collections.Generic; using UnityEngine; using ZombieWar.Features.VFX.Domain; using ZombieWar.Features.VFX.Ports; using ZombieWar.Features.VFX.Unity.Config; using ZombieWar.Features.VFX.Unity.View;
namespace ZombieWar.Features.VFX.Unity.Pool
{
    public sealed class UnityVFXPoolRegistry:MonoBehaviour,IVFXPoolRegistry
    {
        [SerializeField]private Transform poolRoot; private readonly Dictionary<VFXId,Pool> _pools=new Dictionary<VFXId,Pool>();
        public void Initialize(VFXCatalogConfig catalog){Shutdown();if(catalog==null)throw new ArgumentNullException(nameof(catalog));if(poolRoot==null){var go=new GameObject("VFX_PoolRoot");go.transform.SetParent(transform,false);poolRoot=go.transform;}var items=catalog.effects??Array.Empty<VFXConfig>();for(int i=0;i<items.Length;i++){var c=items[i];if(c==null||c.prefab==null)throw new InvalidOperationException("VFX prefab missing at catalog index "+i);if(_pools.ContainsKey(c.id))throw new InvalidOperationException("Duplicate VFX pool: "+c.id);var pool=new Pool(c,poolRoot);_pools.Add(c.id,pool);pool.Prewarm();}}
        public bool TryAcquire(VFXId id,out IVFXLease lease){if(_pools.TryGetValue(id,out Pool p))return p.TryAcquire(out lease);lease=null;return false;}
        public void ReleaseAll(){foreach(var pair in _pools)pair.Value.ReleaseAllActive();}
        public void Shutdown(){foreach(var pair in _pools)pair.Value.DestroyAll();_pools.Clear();}
        private sealed class Pool
        {
            private readonly VFXConfig _config; private readonly Transform _root; private readonly Stack<Item> _available=new Stack<Item>(); private readonly List<Item> _all=new List<Item>();
            public Pool(VFXConfig config,Transform root){_config=config;_root=root;}
            public void Prewarm(){for(int i=0;i<_config.prewarmCount;i++)_available.Push(Create());}
            public bool TryAcquire(out IVFXLease lease){Item item=_available.Count>0?_available.Pop():null;if(item==null){if(!_config.allowGrowth||_all.Count>=_config.maxCapacity){lease=null;return false;}item=Create();}item.Acquire();lease=item;return true;}
            private Item Create(){var v=UnityEngine.Object.Instantiate(_config.prefab,_root);v.name=_config.prefab.name+"_Pooled";v.gameObject.SetActive(false);var item=new Item(this,v);_all.Add(item);return item;}
            private void Return(Item item){if(item==null)return;_available.Push(item);}
            public void ReleaseAllActive(){for(int i=0;i<_all.Count;i++)if(!_all[i].IsReleased){_all[i].ViewComponent.Stop();_all[i].ViewComponent.Deactivate();_all[i].Release();}}
            public void DestroyAll(){for(int i=0;i<_all.Count;i++)if(_all[i].ViewComponent!=null)UnityEngine.Object.Destroy(_all[i].ViewComponent.gameObject);_all.Clear();_available.Clear();}
            private sealed class Item:IVFXLease
            {
                private readonly Pool _owner; public ParticleVFXView ViewComponent{get;} public IVFXView View=>ViewComponent; public bool IsReleased{get;private set;}=true;
                public Item(Pool owner,ParticleVFXView view){_owner=owner;ViewComponent=view;} public void Acquire(){IsReleased=false;}
                public void Release(){if(IsReleased)return;IsReleased=true;_owner.Return(this);}
            }
        }
    }
}
