using System; using UnityEngine; using ZombieWar.Features.VFX.Services; using ZombieWar.Features.VFX.Unity.Config; using ZombieWar.Features.VFX.Unity.Pool;
namespace ZombieWar.Features.VFX.Unity.Runtime
{
    public sealed class VFXRuntimeRoot:MonoBehaviour
    {
        [SerializeField]private VFXCatalogConfig catalogConfig; [SerializeField]private UnityVFXPoolRegistry poolRegistry; [SerializeField]private VFXSimulationDriver simulationDriver;
        private IVFXRuntime _runtime; private IVFXRuntimeConfigurator _configurator; public bool IsBound=>_runtime!=null; public IVFXRuntime Runtime=>_runtime;
        public void Bind(IVFXRuntime runtime,IVFXRuntimeConfigurator configurator){if(runtime==null)throw new ArgumentNullException(nameof(runtime));if(configurator==null)throw new ArgumentNullException(nameof(configurator));if(catalogConfig==null||poolRegistry==null||simulationDriver==null)throw new InvalidOperationException("VFXRuntimeRoot references are incomplete.");Unbind();_runtime=runtime;_configurator=configurator;poolRegistry.Initialize(catalogConfig);_configurator.Initialize(catalogConfig.CreateCatalog(),poolRegistry);simulationDriver.Bind(_runtime);}
        public void Unbind(){if(simulationDriver!=null)simulationDriver.Unbind();if(_configurator!=null)_configurator.Shutdown();if(poolRegistry!=null)poolRegistry.Shutdown();_runtime=null;_configurator=null;}
        private void OnDestroy()=>Unbind();
    }
}
