using System;
using UnityEngine;
using VContainer;
using ZombieWar.Bootstrap;
using ZombieWar.Features.Audio.Services;
using ZombieWar.Features.Audio.Unity.Runtime;
using ZombieWar.Features.Feedback.Ports;
using ZombieWar.Features.Feedback.Services;
using ZombieWar.Features.Feedback.Unity.Runtime;
using ZombieWar.Features.UI.Services;
using ZombieWar.Features.UI.Unity.Root;

namespace ZombieWar.Composition
{
    [DisallowMultipleComponent]
    public sealed class GlobalPresentationComposition : MonoBehaviour
    {
        [Header("DI")]
        [SerializeField]
        private GameLifetimeScope gameLifetimeScope;

        [Header("Global Presentation Roots")]
        [SerializeField]
        private UIRuntimeRoot uiRuntimeRoot;

        [SerializeField]
        private FeedbackRuntimeRoot feedbackRuntimeRoot;

        [SerializeField]
        private AudioRuntimeRoot audioRuntimeRoot;

        private bool _isBound;

        public bool IsBound => _isBound;

        public void Bind()
        {
            if (_isBound)
            {
                return;
            }

            ValidateReferences();

            IObjectResolver resolver = gameLifetimeScope.Container;

            if (resolver == null)
            {
                throw new InvalidOperationException(
                    "GameLifetimeScope container has not been built yet.");
            }

            try
            {
                BindUI(resolver);
                BindFeedback(resolver);
                BindAudio(resolver);

                _isBound = true;
            }
            catch
            {
                UnbindInternal();
                throw;
            }
        }

        public void Unbind()
        {
            if (!_isBound &&
                !HasAnyBoundRoot())
            {
                return;
            }

            UnbindInternal();
        }

        private void BindUI(
            IObjectResolver resolver)
        {
            if (uiRuntimeRoot.IsBound)
            {
                return;
            }

            uiRuntimeRoot.Bind(
                resolver.Resolve<IUIRuntime>());
        }

        private void BindFeedback(
            IObjectResolver resolver)
        {
            if (feedbackRuntimeRoot.IsBound)
            {
                return;
            }

            feedbackRuntimeRoot.Bind(
                resolver.Resolve<IFeedbackRuntime>(),
                resolver.Resolve<IFeedbackRuntimeConfigurator>(),
                resolver.Resolve<ICameraFeedbackPort>(),
                resolver.Resolve<IRecoilFeedbackPort>());
        }

        private void BindAudio(
            IObjectResolver resolver)
        {
            if (audioRuntimeRoot.IsBound)
            {
                return;
            }

            audioRuntimeRoot.Bind(
                resolver.Resolve<IAudioRuntime>(),
                resolver.Resolve<IAudioRuntimeConfigurator>(),
                resolver.Resolve<IAudioPreferences>());
        }

        private void ValidateReferences()
        {
            if (gameLifetimeScope == null)
            {
                throw new InvalidOperationException(
                    "GlobalPresentationComposition requires GameLifetimeScope.");
            }

            if (uiRuntimeRoot == null)
            {
                throw new InvalidOperationException(
                    "GlobalPresentationComposition requires UIRuntimeRoot.");
            }

            if (feedbackRuntimeRoot == null)
            {
                throw new InvalidOperationException(
                    "GlobalPresentationComposition requires FeedbackRuntimeRoot.");
            }

            if (audioRuntimeRoot == null)
            {
                throw new InvalidOperationException(
                    "GlobalPresentationComposition requires AudioRuntimeRoot.");
            }
        }

        private bool HasAnyBoundRoot()
        {
            return (uiRuntimeRoot != null && uiRuntimeRoot.IsBound) ||
                   (feedbackRuntimeRoot != null && feedbackRuntimeRoot.IsBound) ||
                   (audioRuntimeRoot != null && audioRuntimeRoot.IsBound);
        }

        private void UnbindInternal()
        {
            if (audioRuntimeRoot != null)
            {
                audioRuntimeRoot.Unbind();
            }

            if (feedbackRuntimeRoot != null)
            {
                feedbackRuntimeRoot.Unbind();
            }

            if (uiRuntimeRoot != null)
            {
                uiRuntimeRoot.Unbind();
            }

            _isBound = false;
        }

        private void OnDestroy()
        {
            Unbind();
        }
    }
}
