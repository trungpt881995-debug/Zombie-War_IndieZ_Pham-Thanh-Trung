using UnityEngine; using ZombieWar.Features.VFX.Domain; using ZombieWar.Features.VFX.Unity.View;
namespace ZombieWar.Features.VFX.Unity.Config
{
    [CreateAssetMenu(menuName="Zombie War/VFX/VFX Config",fileName="VFXConfig")]
    public sealed class VFXConfig:ScriptableObject
    {
        public VFXId id=VFXId.BulletImpact; public ParticleVFXView prefab; public VFXLifetimeMode lifetime=VFXLifetimeMode.OneShot; [Min(0.01f)]public float duration=.5f;
        public bool allowDuringTerminalDrain=true; [Min(0)]public int prewarmCount=4; [Min(1)]public int maxCapacity=32; public bool allowGrowth=true; [Min(.01f)]public float defaultScale=1f;
        public VFXDefinition CreateDefinition()=>new VFXDefinition(id,lifetime,duration,allowDuringTerminalDrain,prewarmCount,maxCapacity,allowGrowth,defaultScale);
    }
}
