namespace GeneralCore.Architecture
{
    public interface ICommand { }

    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        void Handle(TCommand command);
    }

    public interface ICommandBus
    {
        void Send<TCommand>(TCommand command) where TCommand : ICommand;
    }

    public interface ICommandRegistry
    {
        void Register<TCommand>(ICommandHandler<TCommand> handler) where TCommand : ICommand;
        void Unregister<TCommand>(ICommandHandler<TCommand> handler) where TCommand : ICommand;
    }
}
