using GeneralCore.Architecture; using ZombieWar.Features.Boss.Domain;
namespace ZombieWar.Features.Boss.Commands { public readonly struct SpawnLevelBossesCommand:ICommand { public BossSpawnSelection Selection{get;} public BossPoint Anchor{get;} public SpawnLevelBossesCommand(in BossSpawnSelection selection,in BossPoint anchor){Selection=selection;Anchor=anchor;} } }
