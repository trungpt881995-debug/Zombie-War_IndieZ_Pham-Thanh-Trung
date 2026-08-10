namespace GeneralCore.AudioHapticLocalization
{
    public interface IAudioService
    {
        void PlaySfx(string id);
        void PlayMusic(string id, bool loop = true);
        void StopMusic();
    }

    public interface IHapticService
    {
        bool Enabled { get; set; }
        void Light();
        void Medium();
        void Heavy();
    }

    public interface ILocalizationService
    {
        string CurrentLocale { get; }
        string Get(string key);
    }
}
