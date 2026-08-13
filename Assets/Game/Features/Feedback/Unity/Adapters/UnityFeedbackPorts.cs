using System;
using UnityEngine;
using ZombieWar.Features.Feedback.Domain;
using ZombieWar.Features.Feedback.Ports;
using ZombieWar.Features.Feedback.Unity.View;

namespace ZombieWar.Features.Feedback.Unity.Adapters
{
    public sealed class UnityScreenFeedbackPort : IScreenFeedbackPort
    {
        private readonly ScreenFlashView _view;

        public UnityScreenFeedbackPort(ScreenFlashView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public bool TryFlash(
            ScreenFeedbackKind kind,
            float intensity,
            float duration)
        {
            _view.Flash(
                kind,
                intensity,
                duration);

            return true;
        }

        public void SetSuspended(bool suspended)
        {
            _view.SetSuspended(suspended);
        }

        public void Clear()
        {
            _view.Clear();
        }
    }

    public sealed class UnityHapticFeedbackPort : IHapticFeedbackPort
    {
        public bool TryPlay(HapticFeedbackStrength strength)
        {
            if (strength == HapticFeedbackStrength.None)
            {
                return false;
            }

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
            return true;
#else
            return false;
#endif
        }

        public void CancelAll()
        {
        }
    }
}
