using UnityEngine;
using Unity.Cinemachine;
using ZombieWar.Features.Camera.Domain;
using ZombieWar.Features.Camera.Unity.Runtime;

namespace ZombieWar.Infrastructure.Camera.Cinemachine
{
    public sealed class CinemachineCameraShakeDriver : CameraShakeDriverBehaviour
    {
        [SerializeField] private CinemachineBasicMultiChannelPerlin noise;

        private float _remaining;

        public override bool TryPlay(in CameraShakeRequest request)
        {
            if (noise == null || request.Duration <= 0f) return false;
            noise.AmplitudeGain = request.Amplitude;
            noise.FrequencyGain = request.Frequency;
            _remaining = request.Duration;
            return true;
        }

        public override void StopAll()
        {
            _remaining = 0f;
            if (noise == null) return;
            noise.AmplitudeGain = 0f;
            noise.FrequencyGain = 0f;
        }

        private void Update()
        {
            if (_remaining <= 0f) return;
            _remaining -= Time.deltaTime;
            if (_remaining <= 0f) StopAll();
        }

        private void OnDisable() => StopAll();
    }
}
