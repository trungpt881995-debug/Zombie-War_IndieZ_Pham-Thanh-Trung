namespace ZombieWar.Features.VFX.Domain
{
    public readonly struct VFXPose
    {
        public VFXPoint Position{get;} public VFXDirection Forward{get;}
        public VFXPose(in VFXPoint position,in VFXDirection forward){Position=position;Forward=forward;}
        public static VFXPose At(in VFXPoint point){var f=VFXDirection.Forward;return new VFXPose(in point,in f);}
    }
}
