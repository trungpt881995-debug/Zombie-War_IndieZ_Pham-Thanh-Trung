using System;

namespace GameplayCore.Levels
{
    public enum LevelLifecycleState { None, Preparing, Ready, Running, Paused, Completed, Failed, Unloading }

    public interface ILevelLifecycle
    {
        LevelLifecycleState State { get; }
        event Action<LevelLifecycleState> StateChanged;
        void Prepare();
        void Ready();
        void Start();
        void Pause();
        void Resume();
        void Complete();
        void Fail();
        void BeginUnload();
        void FinishUnload();
    }

    public sealed class LevelLifecycle : ILevelLifecycle
    {
        public LevelLifecycleState State { get; private set; } = LevelLifecycleState.None;
        public event Action<LevelLifecycleState> StateChanged;

        public void Prepare() => RequireAndSet(LevelLifecycleState.None, LevelLifecycleState.Preparing);
        public void Ready() => RequireAndSet(LevelLifecycleState.Preparing, LevelLifecycleState.Ready);
        public void Start() => RequireAndSet(LevelLifecycleState.Ready, LevelLifecycleState.Running);
        public void Pause() => RequireAndSet(LevelLifecycleState.Running, LevelLifecycleState.Paused);
        public void Resume() => RequireAndSet(LevelLifecycleState.Paused, LevelLifecycleState.Running);
        public void Complete() => RequireOneAndSet(LevelLifecycleState.Completed, LevelLifecycleState.Running, LevelLifecycleState.Paused);
        public void Fail() => RequireOneAndSet(LevelLifecycleState.Failed, LevelLifecycleState.Running, LevelLifecycleState.Paused);
        public void BeginUnload() => RequireOneAndSet(LevelLifecycleState.Unloading, LevelLifecycleState.Completed, LevelLifecycleState.Failed, LevelLifecycleState.Ready);
        public void FinishUnload() => RequireAndSet(LevelLifecycleState.Unloading, LevelLifecycleState.None);

        private void RequireAndSet(LevelLifecycleState expected, LevelLifecycleState next)
        {
            if (State != expected) throw new InvalidOperationException($"Invalid level transition: {State} -> {next}.");
            Set(next);
        }

        private void RequireOneAndSet(LevelLifecycleState next, params LevelLifecycleState[] expected)
        {
            for (var i = 0; i < expected.Length; i++)
                if (State == expected[i]) { Set(next); return; }
            throw new InvalidOperationException($"Invalid level transition: {State} -> {next}.");
        }

        private void Set(LevelLifecycleState next) { State = next; StateChanged?.Invoke(next); }
    }
}
