using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Score.Domain;
namespace ZombieWar.Features.Score.Commands { public readonly struct AwardScoreCommand : ICommand { public ScoreActionId ActionId { get; } public EntityId SourceEntityId { get; } public AwardScoreCommand(ScoreActionId actionId, EntityId sourceEntityId){ActionId=actionId;SourceEntityId=sourceEntityId;} } }
