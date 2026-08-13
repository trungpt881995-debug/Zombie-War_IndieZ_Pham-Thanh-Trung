using System; using System.Collections.Generic; using UnityEngine; using ZombieWar.Features.VFX.Catalog; using ZombieWar.Features.VFX.Domain;
namespace ZombieWar.Features.VFX.Unity.Config
{
    [CreateAssetMenu(menuName="Zombie War/VFX/VFX Catalog",fileName="VFXCatalog_Game")]
    public sealed class VFXCatalogConfig:ScriptableObject
    {
        public VFXConfig[] effects=Array.Empty<VFXConfig>();
        public IVFXCatalog CreateCatalog(){var defs=new List<VFXDefinition>(effects?.Length??0);if(effects==null)throw new InvalidOperationException("VFX effects array is null.");for(int i=0;i<effects.Length;i++){if(effects[i]==null)throw new InvalidOperationException("VFX Config is null at index "+i);defs.Add(effects[i].CreateDefinition());}return new VFXCatalog(defs);}
        public VFXConfig Find(VFXId id){if(effects==null)return null;for(int i=0;i<effects.Length;i++)if(effects[i]!=null&&effects[i].id==id)return effects[i];return null;}
    }
}
