using UnityEngine;
using ZombieWar.Features.Feedback.Services;

namespace ZombieWar.Features.Feedback.Unity.Runtime
{
    public sealed class FeedbackSimulationDriver : MonoBehaviour
    {
        private IFeedbackRuntime _runtime;

        public void Bind(IFeedbackRuntime runtime)
        {
            _runtime = runtime;
        }

        public void Unbind()
        {
            _runtime = null;
        }

        private void Update()
        {
            _runtime?.Tick(Time.unscaledDeltaTime);
        }
    }
}
