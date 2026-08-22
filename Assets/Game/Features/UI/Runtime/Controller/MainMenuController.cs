using System;
using ZombieWar.Features.UI.Ports;
using ZombieWar.Features.UI.Utilities;
using ZombieWar.Features.UI.View;

namespace ZombieWar.Features.UI.Controller
{
    public sealed class MainMenuController
    {
        private readonly IUIFlowActionPort _flow;
        private readonly SingleExecutionGuard _guard = new SingleExecutionGuard();
        private IMainMenuView _view;
        public MainMenuController(IUIFlowActionPort flow) => _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        public void Bind(IMainMenuView v)
        {
            Unbind();
            _view = v ?? throw new ArgumentNullException(nameof(v));
            _view.PlayClicked += OnPlay;
        }
        public void Unbind()
        {
            if (_view != null) _view.PlayClicked -= OnPlay;
            _view = null;
        }
        public void Reset() => _guard.Reset();
        private void OnPlay()
        {
            if (_guard.TryEnter()) _flow.Play();
        }
    }
}
