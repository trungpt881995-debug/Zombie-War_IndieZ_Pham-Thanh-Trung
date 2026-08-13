using ZombieWar.Features.Boss.Domain; namespace ZombieWar.Integration.Boss { public interface IBossTargetSource { BossPoint Position{get;} bool IsActive{get;} } }
