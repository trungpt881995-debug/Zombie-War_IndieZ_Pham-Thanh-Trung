using ZombieWar.Features.VFX.Domain;
namespace ZombieWar.Features.VFX.Ports
{
    public interface IVFXView
    {
        void Activate(in VFXPose pose,float scale); void SetPose(in VFXPose pose); void Play(); void SetPaused(bool paused); void Stop(); void Deactivate();
    }
}
