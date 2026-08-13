using System;
using GameplayCore.Entities; using ZombieWar.Features.Health.Domain;
namespace ZombieWar.Integration.UI.Health { public interface IUIHealthBinding : IDisposable { bool IsBound{get;} EntityId OwnerId{get;} void Start(); void Bind(EntityId ownerId,IReadOnlyHealth health); void Unbind(EntityId ownerId); } }
