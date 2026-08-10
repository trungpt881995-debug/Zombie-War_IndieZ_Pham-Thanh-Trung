using System;
using System.Collections.Generic;
using GeneralCore.AnalyticsDiagnostics;
using ZombieWar.GameFlow.Domain;
using ZombieWar.GameFlow.Model;

namespace ZombieWar.GameFlow.StateMachine
{
    public sealed class GameFlowStateMachine
    {
        private readonly Dictionary<GameFlowStateId, IGameFlowState> _states = new Dictionary<GameFlowStateId, IGameFlowState>();
        private readonly GameFlowModel _model;
        private readonly IGameLogger _logger;
        private IGameFlowState _current;

        public GameFlowStateMachine(IReadOnlyList<IGameFlowState> states, GameFlowModel model, IGameLogger logger)
        {
            _model = model;
            _logger = logger;
            for (var i = 0; i < states.Count; i++)
            {
                var state = states[i];
                if (_states.ContainsKey(state.Id)) throw new InvalidOperationException($"Duplicate Game Flow state: {state.Id}.");
                _states.Add(state.Id, state);
            }
        }

        public void ChangeState(GameFlowStateId next)
        {
            if (_current != null && _current.Id == next) return;
            if (!_states.TryGetValue(next, out var target)) throw new InvalidOperationException($"Game Flow state not registered: {next}.");
            var previous = _current?.Id ?? GameFlowStateId.None;
            _current?.Exit();
            _current = target;
            _current.Enter();
            _model.SetState(next);
            _logger.Info($"[GameFlow] {previous} -> {next}");
        }
    }
}
