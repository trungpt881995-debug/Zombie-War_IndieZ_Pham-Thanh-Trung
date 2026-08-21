using System;
using System.Collections.Generic;

namespace ZombieWar.Features.Map.Domain
{
    public sealed class MapRuntimeContext
    {
        private readonly MapSpawnSector[] _spawnSectors;

        public MapId MapId { get; }
        public MapBounds GameplayBounds { get; }
        public MapBounds CameraBounds { get; }
        public IReadOnlyList<MapSpawnSector> SpawnSectors => _spawnSectors;
        public MapPoint SoldierSpawnPoint { get; }
        public bool HasSoldierSpawnPoint { get; }
        public MapPoint BossSpawnPoint { get; }
        public bool HasNavigationReference { get; }

        /// <summary>
        /// Backward-compatible constructor for older tests/adapters that do not yet
        /// author a Soldier spawn point. Runtime MapView instances should use the
        /// explicit Soldier-spawn overload below.
        /// </summary>
        public MapRuntimeContext(
            MapId mapId,
            in MapBounds gameplayBounds,
            in MapBounds cameraBounds,
            IReadOnlyList<MapSpawnSector> spawnSectors,
            in MapPoint bossSpawnPoint,
            bool hasNavigationReference)
            : this(
                mapId,
                gameplayBounds,
                cameraBounds,
                spawnSectors,
                default,
                bossSpawnPoint,
                hasNavigationReference,
                false)
        {
        }

        public MapRuntimeContext(
            MapId mapId,
            in MapBounds gameplayBounds,
            in MapBounds cameraBounds,
            IReadOnlyList<MapSpawnSector> spawnSectors,
            in MapPoint soldierSpawnPoint,
            in MapPoint bossSpawnPoint,
            bool hasNavigationReference)
            : this(
                mapId,
                gameplayBounds,
                cameraBounds,
                spawnSectors,
                soldierSpawnPoint,
                bossSpawnPoint,
                hasNavigationReference,
                true)
        {
        }

        private MapRuntimeContext(
            MapId mapId,
            in MapBounds gameplayBounds,
            in MapBounds cameraBounds,
            IReadOnlyList<MapSpawnSector> spawnSectors,
            in MapPoint soldierSpawnPoint,
            in MapPoint bossSpawnPoint,
            bool hasNavigationReference,
            bool hasSoldierSpawnPoint)
        {
            if (mapId == MapId.None)
                throw new ArgumentOutOfRangeException(nameof(mapId));
            if (!gameplayBounds.IsValid)
                throw new ArgumentException("Gameplay bounds must be valid.", nameof(gameplayBounds));
            if (!cameraBounds.IsValid)
                throw new ArgumentException("Camera bounds must be valid.", nameof(cameraBounds));
            if (spawnSectors == null)
                throw new ArgumentNullException(nameof(spawnSectors));
            if (spawnSectors.Count != 4)
                throw new ArgumentException("Exactly four spawn sectors are required.", nameof(spawnSectors));

            _spawnSectors = new MapSpawnSector[4];
            var seen = new bool[4];

            for (int i = 0; i < spawnSectors.Count; i++)
            {
                MapSpawnSector sector = spawnSectors[i];
                int index = (int)sector.Id;

                if (index < 0 || index >= seen.Length)
                    throw new ArgumentOutOfRangeException(nameof(spawnSectors), "Unknown spawn sector id.");
                if (seen[index])
                    throw new ArgumentException($"Duplicate spawn sector: {sector.Id}.", nameof(spawnSectors));

                seen[index] = true;
                _spawnSectors[i] = sector;
            }

            for (int i = 0; i < seen.Length; i++)
            {
                if (!seen[i])
                    throw new ArgumentException(
                        $"Missing spawn sector id {(MapSpawnSectorId)i}.",
                        nameof(spawnSectors));
            }

            MapId = mapId;
            GameplayBounds = gameplayBounds;
            CameraBounds = cameraBounds;
            SoldierSpawnPoint = soldierSpawnPoint;
            HasSoldierSpawnPoint = hasSoldierSpawnPoint;
            BossSpawnPoint = bossSpawnPoint;
            HasNavigationReference = hasNavigationReference;
        }

        public bool TryGetSpawnSector(MapSpawnSectorId id, out MapSpawnSector sector)
        {
            for (int i = 0; i < _spawnSectors.Length; i++)
            {
                if (_spawnSectors[i].Id != id)
                    continue;

                sector = _spawnSectors[i];
                return true;
            }

            sector = default;
            return false;
        }
    }
}
