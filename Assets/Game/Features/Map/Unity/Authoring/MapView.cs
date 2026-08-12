using UnityEngine;
using ZombieWar.Features.Map.Domain;
using ZombieWar.Features.Map.Ports;

namespace ZombieWar.Features.Map.Unity.Authoring
{
    [DisallowMultipleComponent]
    public sealed class MapView : MonoBehaviour, IMapView
    {
        [SerializeField] private MapId mapId = MapId.Map01;
        [SerializeField] private MapBoundsVolume gameplayBounds;
        [SerializeField] private MapBoundsVolume cameraBounds;
        [SerializeField] private MapSpawnSectorVolume[] spawnSectors = new MapSpawnSectorVolume[4];
        [SerializeField] private MapBossSpawnPoint bossSpawnPoint;
        [SerializeField] private MapNavigationReference navigationReference;

        public MapId Id => mapId;

        public bool TryBuildContext(out MapRuntimeContext context, out string error)
        {
            context = null;
            error = string.Empty;

            if (mapId == MapId.None) { error = "MapView MapId cannot be None."; return false; }
            if (gameplayBounds == null) { error = "GameplayBounds is not assigned."; return false; }
            if (cameraBounds == null) { error = "CameraBounds is not assigned."; return false; }
            if (bossSpawnPoint == null) { error = "BossSpawnPoint is not assigned."; return false; }
            if (spawnSectors == null || spawnSectors.Length != 4) { error = "Exactly four spawn sector volumes are required."; return false; }

            MapBounds gameplay = gameplayBounds.BuildBounds();
            MapBounds camera = cameraBounds.BuildBounds();
            if (!gameplay.IsValid) { error = "GameplayBounds is invalid."; return false; }
            if (!camera.IsValid) { error = "CameraBounds is invalid."; return false; }

            var sectors = new MapSpawnSector[4];
            var seen = new bool[4];
            for (int i = 0; i < spawnSectors.Length; i++)
            {
                MapSpawnSectorVolume volume = spawnSectors[i];
                if (volume == null) { error = $"Spawn sector volume at index {i} is not assigned."; return false; }
                int sectorIndex = (int)volume.SectorId;
                if (sectorIndex < 0 || sectorIndex >= seen.Length) { error = "Unknown spawn sector id."; return false; }
                if (seen[sectorIndex]) { error = $"Duplicate spawn sector id: {volume.SectorId}."; return false; }
                seen[sectorIndex] = true;
                sectors[i] = volume.BuildSector();
            }

            for (int i = 0; i < seen.Length; i++)
            {
                if (seen[i]) continue;
                error = $"Missing spawn sector id: {(MapSpawnSectorId)i}.";
                return false;
            }

            MapPoint bossPoint = bossSpawnPoint.Position;
            context = new MapRuntimeContext(
                mapId,
                in gameplay,
                in camera,
                sectors,
                in bossPoint,
                navigationReference != null && navigationReference.IsAssigned);
            return true;
        }
    }
}
