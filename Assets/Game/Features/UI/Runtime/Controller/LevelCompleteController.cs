using System;
using ZombieWar.Features.UI.Ports;
using ZombieWar.Features.UI.Utilities;
using ZombieWar.Features.UI.View;

namespace ZombieWar.Features.UI.Controller
{
    public sealed class LevelCompleteController
    {
        private readonly IUIFlowActionPort _flow;
        private readonly SingleExecutionGuard _guard = new SingleExecutionGuard();
        private ILevelCompleteView _view;
        public LevelCompleteController(IUIFlowActionPort flow) => _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        public void Bind(ILevelCompleteView v)
        {
            Unbind();
            _view = v ?? throw new ArgumentNullException(nameof(v));
            _view.ReplayClicked += Replay;
            _view.NextClicked += Next;
            _view.MenuClicked += Menu;
        }
        public void Unbind()
        {
            if (_view != null)
            {
                _view.ReplayClicked -= Replay;
                _view.NextClicked -= Next;
                _view.MenuClicked -= Menu;
            }
            _view = null;
        }
        public void Reset() => _guard.Reset();
        private void Replay()
        {
            if (_guard.TryEnter()) _flow.Replay();
        }
        private void Next()
        {
            if (_guard.TryEnter()) _flow.Next();
        }
        private void Menu()
        {
            if (_guard.TryEnter()) _flow.Menu();
        }
    }
}
