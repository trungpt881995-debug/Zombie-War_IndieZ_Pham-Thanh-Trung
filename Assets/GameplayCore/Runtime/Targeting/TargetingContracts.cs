using System.Collections.Generic;
using GameplayCore.Entities;

namespace GameplayCore.Targeting
{
    public interface ITargetable
    {
        EntityId EntityId { get; }
        bool IsTargetable { get; }
    }

    public interface ITargetSelector<in TContext, TTarget> where TTarget : ITargetable
    {
        TTarget Select(TContext context, IReadOnlyList<TTarget> candidates);
    }
}
