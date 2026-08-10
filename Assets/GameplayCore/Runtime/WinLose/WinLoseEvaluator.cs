using System.Collections.Generic;

namespace GameplayCore.WinLose
{
    public enum WinLoseResult { None, Win, Lose }

    public interface IWinCondition { bool IsMet(); }
    public interface ILoseCondition { bool IsMet(); }

    public interface IWinLoseEvaluator
    {
        WinLoseResult Evaluate();
    }

    public sealed class WinLoseEvaluator : IWinLoseEvaluator
    {
        private readonly IReadOnlyList<IWinCondition> _winConditions;
        private readonly IReadOnlyList<ILoseCondition> _loseConditions;

        public WinLoseEvaluator(IReadOnlyList<IWinCondition> winConditions, IReadOnlyList<ILoseCondition> loseConditions)
        {
            _winConditions = winConditions;
            _loseConditions = loseConditions;
        }

        public WinLoseResult Evaluate()
        {
            for (var i = 0; i < _loseConditions.Count; i++) if (_loseConditions[i].IsMet()) return WinLoseResult.Lose;
            for (var i = 0; i < _winConditions.Count; i++) if (!_winConditions[i].IsMet()) return WinLoseResult.None;
            return _winConditions.Count > 0 ? WinLoseResult.Win : WinLoseResult.None;
        }
    }
}
