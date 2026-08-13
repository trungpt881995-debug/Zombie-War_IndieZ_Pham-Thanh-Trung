using System;
using ZombieWar.Features.Camera.Domain;
using ZombieWar.Features.Camera.Services;
using ZombieWar.Features.Feedback.Domain;
using ZombieWar.Features.Feedback.Ports;

namespace ZombieWar.Integration.Feedback.Camera
{
    public sealed class CameraFeedbackAdapter : ICameraFeedbackPort
    {
        private readonly ICameraRuntime _camera;

        public CameraFeedbackAdapter(ICameraRuntime camera)
        {
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        }

        public bool TryPlay(FeedbackCameraCue cue)
        {
            CameraShakeId id = Map(cue);

            return id != CameraShakeId.None &&
                   _camera.TryRequestShake(id);
        }

        public void CancelAll()
        {
            // Current Camera Feature exposes one-shot request-based shakes only.
            // No stop API is invented here.
        }

        private static CameraShakeId Map(FeedbackCameraCue cue)
        {
            switch (cue)
            {
                case FeedbackCameraCue.LightWeapon:
                    return CameraShakeId.LightWeapon;

                case FeedbackCameraCue.HeavyWeapon:
                    return CameraShakeId.HeavyWeapon;

                case FeedbackCameraCue.Explosion:
                    return CameraShakeId.Explosion;

                case FeedbackCameraCue.BossImpact:
                    return CameraShakeId.BossImpact;

                case FeedbackCameraCue.SoldierDamage:
                    return CameraShakeId.SoldierDamage;

                default:
                    return CameraShakeId.None;
            }
        }
    }
}
