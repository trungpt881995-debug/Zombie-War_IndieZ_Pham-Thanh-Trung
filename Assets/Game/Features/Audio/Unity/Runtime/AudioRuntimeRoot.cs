using System;
using UnityEngine;
using ZombieWar.Features.Audio.Services;
using ZombieWar.Features.Audio.Unity.Config;
using ZombieWar.Features.Audio.Unity.Music;
using ZombieWar.Features.Audio.Unity.Pool;

namespace ZombieWar.Features.Audio.Unity.Runtime
{
    public sealed class AudioRuntimeRoot :
        MonoBehaviour
    {
        [SerializeField]
        private AudioCatalogConfig catalogConfig;

        [SerializeField]
        private UnityAudioSourcePool voicePool;

        [SerializeField]
        private UnityMusicPlayer musicPlayer;

        [SerializeField]
        private AudioSimulationDriver simulationDriver;

        private IAudioRuntimeConfigurator _configurator;

        public bool IsBound { get; private set; }
        public IAudioRuntime Runtime { get; private set; }

        public void Bind(
            IAudioRuntime runtime,
            IAudioRuntimeConfigurator configurator,
            IAudioPreferences preferences)
        {
            if (IsBound)
            {
                return;
            }

            Runtime = runtime ??
                throw new ArgumentNullException(nameof(runtime));

            _configurator = configurator ??
                throw new ArgumentNullException(nameof(configurator));

            if (catalogConfig == null ||
                voicePool == null ||
                musicPlayer == null ||
                simulationDriver == null)
            {
                throw new InvalidOperationException(
                    "AudioRuntimeRoot is missing catalog, pool, " +
                    "music player, or simulation driver.");
            }

            voicePool.Bind(
                catalogConfig,
                preferences ??
                throw new ArgumentNullException(nameof(preferences)));

            musicPlayer.Bind(catalogConfig);

            _configurator.Initialize(
                catalogConfig.BuildCatalog(),
                voicePool,
                musicPlayer);

            IAudioRuntimeDriver driver =
                runtime as IAudioRuntimeDriver;

            if (driver == null)
            {
                throw new InvalidOperationException(
                    "Resolved IAudioRuntime must also implement " +
                    "IAudioRuntimeDriver.");
            }

            simulationDriver.Bind(driver);
            IsBound = true;
        }

        public void Unbind()
        {
            if (!IsBound)
            {
                return;
            }

            simulationDriver?.Unbind();
            _configurator?.Shutdown();

            _configurator = null;
            Runtime = null;
            IsBound = false;
        }

#if UNITY_EDITOR
        public void ConfigureForEditor(
            AudioCatalogConfig catalog,
            UnityAudioSourcePool pool,
            UnityMusicPlayer music,
            AudioSimulationDriver driver)
        {
            catalogConfig = catalog;
            voicePool = pool;
            musicPlayer = music;
            simulationDriver = driver;
        }
#endif

        private void OnDestroy()
        {
            Unbind();
        }
    }

    public sealed class AudioSimulationDriver :
        MonoBehaviour
    {
        private IAudioRuntimeDriver _driver;

        public void Bind(
            IAudioRuntimeDriver driver)
        {
            _driver = driver;
        }

        public void Unbind()
        {
            _driver = null;
        }

        private void Update()
        {
            _driver?.Tick(
                Time.unscaledDeltaTime);
        }
    }
}
