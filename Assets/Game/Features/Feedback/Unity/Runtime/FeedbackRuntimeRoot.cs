using System;
using UnityEngine;
using ZombieWar.Features.Feedback.Ports;
using ZombieWar.Features.Feedback.Services;
using ZombieWar.Features.Feedback.Unity.Adapters;
using ZombieWar.Features.Feedback.Unity.Config;
using ZombieWar.Features.Feedback.Unity.View;

namespace ZombieWar.Features.Feedback.Unity.Runtime
{
    public sealed class FeedbackRuntimeRoot : MonoBehaviour
    {
        [SerializeField] private FeedbackCatalogConfig catalogConfig;
        [SerializeField] private ScreenFlashView screenFlashView;
        [SerializeField] private FeedbackSimulationDriver simulationDriver;

        private IFeedbackRuntime _runtime;
        private IFeedbackRuntimeConfigurator _configurator;

        public bool IsBound => _runtime != null;
        public IFeedbackRuntime Runtime => _runtime;

        public void Bind(
            IFeedbackRuntime runtime,
            IFeedbackRuntimeConfigurator configurator,
            ICameraFeedbackPort camera,
            IRecoilFeedbackPort recoil)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime));
            }

            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            if (recoil == null)
            {
                throw new ArgumentNullException(nameof(recoil));
            }

            if (catalogConfig == null ||
                screenFlashView == null ||
                simulationDriver == null)
            {
                throw new InvalidOperationException(
                    "FeedbackRuntimeRoot references are incomplete.");
            }

            Unbind();

            _runtime = runtime;
            _configurator = configurator;

            var haptic = new UnityHapticFeedbackPort();
            var screen = new UnityScreenFeedbackPort(screenFlashView);

            _configurator.Initialize(
                catalogConfig.CreateCatalog(),
                camera,
                haptic,
                screen,
                recoil);

            simulationDriver.Bind(_runtime);
        }

        public void Unbind()
        {
            if (simulationDriver != null)
            {
                simulationDriver.Unbind();
            }

            _configurator?.Shutdown();

            _runtime = null;
            _configurator = null;
        }

        private void OnDestroy()
        {
            Unbind();
        }
    }
}
