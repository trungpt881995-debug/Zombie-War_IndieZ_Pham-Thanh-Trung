using System;

namespace GameplayCore.Entities
{
    public enum EntityLifecycleState { Created, Active, Inactive, Destroyed }

    public interface IEntityLifecycle
    {
        EntityLifecycleState State { get; }
        event Action<EntityLifecycleState> StateChanged;
        void Activate();
        void Deactivate();
        void Destroy();
    }

    public sealed class EntityLifecycle : IEntityLifecycle
    {
        public EntityLifecycleState State { get; private set; } = EntityLifecycleState.Created;
        public event Action<EntityLifecycleState> StateChanged;

        public void Activate()
        {
            if (State != EntityLifecycleState.Created && State != EntityLifecycleState.Inactive)
                throw new InvalidOperationException($"Cannot activate entity from {State}.");
            Set(EntityLifecycleState.Active);
        }

        public void Deactivate()
        {
            if (State != EntityLifecycleState.Active)
                throw new InvalidOperationException($"Cannot deactivate entity from {State}.");
            Set(EntityLifecycleState.Inactive);
        }

        public void Destroy()
        {
            if (State == EntityLifecycleState.Destroyed) return;
            Set(EntityLifecycleState.Destroyed);
        }

        private void Set(EntityLifecycleState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
