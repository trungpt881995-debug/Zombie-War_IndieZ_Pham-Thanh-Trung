using ZombieWar.Features.Boss.Domain;
namespace ZombieWar.Features.Boss.Ports { public interface IBossMotor { BossPoint Position{get;} float NormalizedSpeed{get;} void Warp(in BossPoint position); void SetEnabled(bool enabled); void MoveTowards(in BossPoint target,float speed,float deltaTime); void Stop(); } }
