using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Boss.Domain;
using ZombieWar.Features.Boss.Services;

namespace ZombieWar.Features.Boss.Commands
{
    public sealed class SpawnLevelBossesCommandHandler : ICommandHandler < SpawnLevelBossesCommand >
    {
        private readonly IBossRuntime _runtime;
        public SpawnLevelBossesCommandHandler(IBossRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }
        public void Handle(SpawnLevelBossesCommand c)
        {
            BossSpawnSelection s = c.Selection;
            BossPoint p = c.Anchor;
            _runtime.TrySpawn(in s, in p);
        }
    }
    public sealed class SetBossGameplayEnabledCommandHandler : ICommandHandler < SetBossGameplayEnabledCommand >
    {
        private readonly IBossRuntime _runtime;
        public SetBossGameplayEnabledCommandHandler(IBossRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }
        public void Handle(SetBossGameplayEnabledCommand c) => _runtime.SetGameplayEnabled(c.Enabled);
    }
    public sealed class CancelAllBossesCommandHandler : ICommandHandler < CancelAllBossesCommand >
    {
        private readonly IBossRuntime _runtime;
        public CancelAllBossesCommandHandler(IBossRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }
        public void Handle(CancelAllBossesCommand c) => _runtime.CancelAll();
    }
}
