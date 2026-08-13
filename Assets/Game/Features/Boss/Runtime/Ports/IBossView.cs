using GeneralCore.Architecture; using ZombieWar.Features.Boss.Domain;
namespace ZombieWar.Features.Boss.Ports
{
    public interface IBossView:IView
    {
        BossPoint Position{get;} void ResetForReuse(); void SetActive(bool active); void SetScale(float scale); void SetLocomotionSpeed(float normalizedSpeed); void SetGameplayCollisionEnabled(bool enabled);
        void FaceTarget(in BossPoint target,float rotationSpeed,float deltaTime); void PlaySpawn(); void PlayAttack(); void PlayHit(); void PlayDeath(); void SetAnimationPaused(bool paused);
    }
}
