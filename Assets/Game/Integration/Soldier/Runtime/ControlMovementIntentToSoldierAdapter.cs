using System;
using ZombieWar.Features.Control.Domain;
using ZombieWar.Features.Control.Ports;
using ZombieWar.Features.Soldier.Domain;
using ZombieWar.Features.Soldier.Ports;

namespace ZombieWar.Integration.Soldier
{
    /// <summary>
    /// Adapter Pattern: translates Control Feature movement intent into the
    /// Soldier Feature input contract without creating a Soldier -> Control dependency.
    /// </summary>
    public sealed class ControlMovementIntentToSoldierAdapter :
        IMovementIntentSink
    {
        private readonly ISoldierGroupInputBuffer _buffer;

        public ControlMovementIntentToSoldierAdapter(
            ISoldierGroupInputBuffer buffer)
        {
            _buffer = buffer ??
                throw new ArgumentNullException(nameof(buffer));
        }

        public void Set(
            in MovementIntent intent)
        {
            var input =
                new SoldierMoveInput(
                    intent.X,
                    intent.Y,
                    intent.Magnitude);

            _buffer.Set(
                in input);
        }
    }
}
