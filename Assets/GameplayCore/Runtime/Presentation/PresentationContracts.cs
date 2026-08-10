namespace GameplayCore.Presentation
{
    public interface IPresentationSource<out TPresentation>
    {
        TPresentation CreatePresentation();
    }
}
