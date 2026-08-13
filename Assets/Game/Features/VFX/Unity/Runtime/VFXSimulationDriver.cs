using UnityEngine; using ZombieWar.Features.VFX.Services;
namespace ZombieWar.Features.VFX.Unity.Runtime
{
    public sealed class VFXSimulationDriver:MonoBehaviour
    {
        private IVFXRuntime _runtime; public void Bind(IVFXRuntime runtime)=>_runtime=runtime; public void Unbind()=>_runtime=null;
        private void Update(){_runtime?.Tick(Time.unscaledDeltaTime);}
    }
}
