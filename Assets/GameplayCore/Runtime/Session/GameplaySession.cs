using System;

namespace GameplayCore.Session
{
    public enum GameplaySessionState
    {
        Uninitialized,
        Preparing,
        Running,
        Paused,
        Completed,
        Failed,
        Stopped
    }

    public interface IGameplaySession
    {
        GameplaySessionState State { get; }
        event Action<GameplaySessionState> StateChanged;
        void Prepare();
        void Start();
        void Pause();
        void Resume();
        void Complete();
        void Fail();
        void Stop();
    }

    public sealed class GameplaySession : IGameplaySession
    {
        public GameplaySessionState State { get; private set; } = GameplaySessionState.Uninitialized;
        public event Action<GameplaySessionState> StateChanged;

        public void Prepare() => Transition(GameplaySessionState.Preparing, GameplaySessionState.Uninitialized, GameplaySessionState.Stopped, GameplaySessionState.Completed, GameplaySessionState.Failed);
        public void Start() => Transition(GameplaySessionState.Running, GameplaySessionState.Preparing);
        public void Pause() => Transition(GameplaySessionState.Paused, GameplaySessionState.Running);
        public void Resume() => Transition(GameplaySessionState.Running, GameplaySessionState.Paused);
        public void Complete() => Transition(GameplaySessionState.Completed, GameplaySessionState.Running, GameplaySessionState.Paused);
        public void Fail() => Transition(GameplaySessionState.Failed, GameplaySessionState.Running, GameplaySessionState.Paused);
        public void Stop() => Transition(GameplaySessionState.Stopped, State);

        private void Transition(GameplaySessionState next, params GameplaySessionState[] allowedFrom)
        {
            var allowed = false;
            for (var i = 0; i < allowedFrom.Length; i++)
                if (State == allowedFrom[i]) { allowed = true; break; }
            if (!allowed) throw new InvalidOperationException($"Invalid gameplay session transition: {State} -> {next}.");
            State = next;
            StateChanged?.Invoke(State);
        }
    }
}
