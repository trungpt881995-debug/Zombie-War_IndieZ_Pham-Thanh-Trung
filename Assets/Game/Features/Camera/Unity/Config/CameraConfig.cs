using System;
using UnityEngine;
using ZombieWar.Features.Camera.Catalog;
using ZombieWar.Features.Camera.Domain;

namespace ZombieWar.Features.Camera.Unity.Config
{
    [CreateAssetMenu(fileName = "CameraConfig", menuName = "Zombie War/Camera/Camera Config")]
    public sealed class CameraConfig : ScriptableObject
    {
        [Header("Projection")]
        [SerializeField] private CameraProjectionMode projectionMode = CameraProjectionMode.Perspective;
        [SerializeField, Range(1f, 179f)] private float fieldOfView = 50f;
        [SerializeField, Min(0.01f)] private float orthographicSize = 10f;
        [SerializeField, Min(0.001f)] private float nearClip = 0.1f;
        [SerializeField, Min(0.01f)] private float farClip = 300f;

        [Header("Angled Top-Down Orientation")]
        [SerializeField] private float pitch = 55f;
        [SerializeField] private float yaw = 45f;
        [SerializeField] private float roll;

        [Header("Follow Offset")]
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 12f, -10f);

        [Header("Follow Damping")]
        [SerializeField] private Vector3 positionDamping = new Vector3(0.3f, 0.3f, 0.3f);

        [Header("Lifecycle")]
        [SerializeField] private bool startGameplayEnabled = true;
        [SerializeField] private bool snapOnInitialize = true;

        [Header("Shake Profiles")]
        [SerializeField] private ShakeEntry[] shakes = Array.Empty<ShakeEntry>();

        public bool StartGameplayEnabled => startGameplayEnabled;
        public bool SnapOnInitialize => snapOnInitialize;

        public CameraProfile BuildProfile()
        {
            return new CameraProfile(
                projectionMode,
                fieldOfView,
                orthographicSize,
                nearClip,
                farClip,
                pitch,
                yaw,
                roll,
                followOffset.x,
                followOffset.y,
                followOffset.z,
                positionDamping.x,
                positionDamping.y,
                positionDamping.z);
        }

        public ICameraShakeCatalog BuildShakeCatalog()
        {
            var definitions = new CameraShakeDefinition[shakes == null ? 0 : shakes.Length];
            for (int i = 0; i < definitions.Length; i++) definitions[i] = shakes[i].Build();
            return new CameraShakeCatalog(definitions);
        }

        private void OnValidate()
        {
            fieldOfView = Mathf.Clamp(fieldOfView, 1f, 179f);
            orthographicSize = Mathf.Max(0.01f, orthographicSize);
            nearClip = Mathf.Max(0.001f, nearClip);
            farClip = Mathf.Max(nearClip + 0.01f, farClip);
            positionDamping.x = Mathf.Max(0f, positionDamping.x);
            positionDamping.y = Mathf.Max(0f, positionDamping.y);
            positionDamping.z = Mathf.Max(0f, positionDamping.z);
        }

        [Serializable]
        private struct ShakeEntry
        {
            [SerializeField] private CameraShakeId id;
            [SerializeField, Min(0f)] private float amplitude;
            [SerializeField, Min(0f)] private float frequency;
            [SerializeField, Min(0.01f)] private float duration;

            public CameraShakeDefinition Build() =>
                new CameraShakeDefinition(id, amplitude, frequency, duration);
        }
    }
}
