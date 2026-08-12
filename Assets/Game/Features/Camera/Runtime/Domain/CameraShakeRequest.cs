namespace ZombieWar.Features.Camera.Domain
{
    public readonly struct CameraShakeRequest
    {
        public CameraShakeId Id { get; }
        public float Amplitude { get; }
        public float Frequency { get; }
        public float Duration { get; }

        public CameraShakeRequest(in CameraShakeDefinition definition)
        {
            Id = definition.Id;
            Amplitude = definition.Amplitude;
            Frequency = definition.Frequency;
            Duration = definition.Duration;
        }
    }
}
