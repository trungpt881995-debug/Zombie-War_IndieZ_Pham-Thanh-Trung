using GeneralCore.Architecture;
using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Features.Zombie.Ports
{
    public interface IZombieView : IView
    {
        ZombiePoint Position { get; }
        void ResetForReuse();
        void SetActive(bool active);
        void SetLocomotionSpeed(float normalizedSpeed);
        void SetGameplayCollisionEnabled(bool enabled);
        void FaceTarget(in ZombiePoint target, float rotationSpeed, float deltaTime);
        void PlaySpawn();
        void PlayAttack();
        void PlayHit();
        void PlayDeath();
        void SetDissolveProgress(float normalizedProgress);
        void SetAnimationPaused(bool paused);
    }
}
