namespace GeneralCore.UIInput
{
    public interface IInputGate
    {
        bool GameplayInputEnabled { get; }
        void SetGameplayInputEnabled(bool enabled);
    }

    public interface IUiBlockQuery
    {
        bool IsPointerOverInteractiveUi(int pointerId);
    }
}
