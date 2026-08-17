using System;
using System.Collections;
using GeneralCore.Architecture;
using UnityEngine;
using VContainer;
using ZombieWar.Bootstrap;
using ZombieWar.Features.Level.Commands;
using ZombieWar.Features.Level.Domain;
using ZombieWar.Features.Level.Services;
using ZombieWar.Features.Map.Domain;
using ZombieWar.Features.Map.Unity.Runtime;
using ZombieWar.Features.UI.Domain;
using ZombieWar.GameFlow.Controller;
using ZombieWar.GameFlow.Domain;
using ZombieWar.GameFlow.Model;
using ZombieWar.Integration.UI.GameFlow;

namespace ZombieWar.Composition
{
    /// <summary>
    /// Orchestrates a level load at the Composition boundary.
    ///
    /// Ownership remains explicit:
    /// - UI only records Play / Replay / Next intent.
    /// - LevelRuntime remains the source of truth for the current GameLevel.
    /// - MapRuntime owns map instance lifecycle.
    /// - GameFlow owns Loading -> Gameplay transition.
    /// - GameLevelStartedEvent fans out reset/difficulty/presentation work.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameplayLoadingCoordinator : MonoBehaviour
    {
        // Temporary game-specific hard-coded Map02 spawn.
        // No Unity Terrain dependency is used here because the Composition assembly
        // does not reference UnityEngine.TerrainModule in the current project.
        // Tune X/Y/Z directly to a safe point above the Map02 ground.
        private const float Map02SoldierSpawnX = 0f;
        private const float Map02SoldierSpawnY = 20f;
        private const float Map02SoldierSpawnZ = 0f;

        [Header("Gameplay Scene")]
        [SerializeField]
        private GameplaySceneComposition gameplaySceneComposition;

        [Header("Map")]
        [SerializeField]
        private MapRuntimeRoot mapRuntimeRoot;

        private GameFlowModel _flowModel;
        private GameFlowController _flowController;
        private IGameFlowUIActionContext _flowActionContext;
        private ILevelRuntime _levelRuntime;
        private ICommandBus _commands;
        private SoldierRuntimeRoot _soldierRuntimeRoot;
        private Coroutine _waitRoutine;

        private void Start()
        {
            ValidateReferences();

            GameLifetimeScope lifetimeScope =
                FindFirstObjectByType<GameLifetimeScope>();

            if (lifetimeScope == null)
            {
                throw new InvalidOperationException(
                    "GameplayLoadingCoordinator could not find GameLifetimeScope.");
            }

            IObjectResolver resolver = lifetimeScope.Container;

            if (resolver == null)
            {
                throw new InvalidOperationException(
                    "GameLifetimeScope container has not been built.");
            }

            _flowModel = resolver.Resolve<GameFlowModel>();
            _flowController = resolver.Resolve<GameFlowController>();
            _flowActionContext = resolver.Resolve<IGameFlowUIActionContext>();
            _levelRuntime = resolver.Resolve<ILevelRuntime>();
            _commands = resolver.Resolve<ICommandBus>();

            ResolveSingleSoldierRuntimeRoot();

            _flowModel.StateChanged += OnGameFlowStateChanged;

            // Supports both scene-order cases:
            // 1. Gameplay scene already exists when Loading begins.
            // 2. Gameplay scene is loaded while GameFlow is already Loading.
            OnGameFlowStateChanged(_flowModel.CurrentState);
        }

        private void OnDestroy()
        {
            if (_flowModel != null)
            {
                _flowModel.StateChanged -= OnGameFlowStateChanged;
            }

            if (_waitRoutine != null)
            {
                StopCoroutine(_waitRoutine);
                _waitRoutine = null;
            }
        }

        private void OnGameFlowStateChanged(GameFlowStateId state)
        {
            if (state != GameFlowStateId.Loading)
            {
                return;
            }

            if (_waitRoutine != null)
            {
                return;
            }

            _waitRoutine = StartCoroutine(WaitForGameplayReady());
        }

        private IEnumerator WaitForGameplayReady()
        {
            // Scene-owned RuntimeRoots must be initialized before map/level orchestration.
            while (!gameplaySceneComposition.IsBound)
            {
                if (!IsStillLoading())
                {
                    FinishRoutine();
                    yield break;
                }

                yield return null;
            }

            if (!mapRuntimeRoot.IsInitialized)
            {
                Debug.LogError(
                    "[Loading] GameplaySceneComposition is bound, but MapRuntimeRoot " +
                    "is not initialized.",
                    this);

                FinishRoutine();
                yield break;
            }

            if (_levelRuntime == null || !_levelRuntime.IsInitialized)
            {
                Debug.LogError(
                    "[Loading] GameplaySceneComposition is bound, but LevelRuntime " +
                    "is not initialized.",
                    this);

                FinishRoutine();
                yield break;
            }

            UIFlowAction action =
                _flowActionContext != null
                    ? _flowActionContext.Consume()
                    : UIFlowAction.None;

            if (!TryResolveTargetLevel(
                    action,
                    _levelRuntime.GameLevel,
                    out GameLevelId targetLevel))
            {
                Debug.LogError(
                    $"[Loading] Cannot resolve target Game Level. " +
                    $"Action={action}, Current={_levelRuntime.GameLevel}.",
                    this);

                FinishRoutine();
                yield break;
            }

            MapId targetMap;
            try
            {
                targetMap = ResolveMap(targetLevel);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                Debug.LogException(exception, this);
                FinishRoutine();
                yield break;
            }

            // Close/cancel scene-owned transient combat state from the previous run.
            gameplaySceneComposition.PrepareForLevelTransition();

            Debug.Log(
                $"[Loading] Action={action}, Level={targetLevel}, Map={targetMap}",
                this);

            var loadTask = mapRuntimeRoot.LoadMapAsync(targetMap);

            while (!loadTask.IsCompleted)
            {
                if (!IsStillLoading())
                {
                    // MapRuntime owns its async load lifetime. Stop orchestration only.
                    FinishRoutine();
                    yield break;
                }

                yield return null;
            }

            if (loadTask.IsCanceled)
            {
                Debug.LogError(
                    $"[Loading] Loading {targetMap} was cancelled.",
                    this);

                FinishRoutine();
                yield break;
            }

            if (loadTask.IsFaulted)
            {
                Exception exception =
                    loadTask.Exception?.GetBaseException();

                if (exception != null)
                {
                    Debug.LogException(exception, this);
                }
                else
                {
                    Debug.LogError(
                        $"[Loading] Loading {targetMap} failed with an unknown exception.",
                        this);
                }

                FinishRoutine();
                yield break;
            }

            MapLoadResult result = loadTask.Result;

            if (!result.Success)
            {
                Debug.LogError(
                    $"[Loading] Failed to load {result.MapId}. " +
                    $"Reason={result.FailureReason}, Message={result.Message}",
                    this);

                FinishRoutine();
                yield break;
            }

            Debug.Log(
                result.AlreadyLoaded
                    ? $"[Loading] Map already ready: {result.MapId}"
                    : $"[Loading] Map ready: {result.MapId}",
                this);

            // Allow newly loaded MapView/Nav/anchors to finish the current Unity frame.
            yield return null;

            if (!IsStillLoading())
            {
                FinishRoutine();
                yield break;
            }

            // Map02 currently has a different world layout from Map01.
            // Keep this intentionally simple: hard-code a safe Map02 world position
            // before gameplay/gravity are enabled again. Replay on Map02 uses the same reset.
            if (targetMap == MapId.Map02 && !TeleportSoldierGroupForMap02())
            {
                FinishRoutine();
                yield break;
            }

            // IMPORTANT ORDER:
            // Begin Level first. GameLevelStartedEvent resets Soldier progression and
            // configures Spawn difficulty before GameFlow enters Gameplay and starts spawning.
            _commands.Send(new BeginGameLevelCommand(targetLevel));

            LevelProgressSnapshot snapshot = _levelRuntime.Snapshot();
            if (snapshot.State != LevelState.Running ||
                snapshot.GameLevel != targetLevel)
            {
                Debug.LogError(
                    $"[Loading] BeginGameLevel failed. " +
                    $"Requested={targetLevel}, Runtime={snapshot.GameLevel}, " +
                    $"State={snapshot.State}.",
                    this);

                FinishRoutine();
                yield break;
            }

            Debug.Log(
                $"[Loading] Level started: {snapshot.GameLevel}, " +
                $"SoldierGroup={snapshot.SoldierGroupLevel}, " +
                $"Kills={snapshot.NormalZombieKillCount}.",
                this);

            _flowController.BeginGameplay();
            FinishRoutine();
        }

        private static bool TryResolveTargetLevel(
            UIFlowAction action,
            GameLevelId currentLevel,
            out GameLevelId targetLevel)
        {
            switch (action)
            {
                case UIFlowAction.Play:
                    targetLevel = GameLevelId.GameLevel01;
                    return true;

                case UIFlowAction.Replay:
                    targetLevel = currentLevel != GameLevelId.None
                        ? currentLevel
                        : GameLevelId.GameLevel01;
                    return true;

                case UIFlowAction.Next:
                    if (currentLevel == GameLevelId.GameLevel01)
                    {
                        targetLevel = GameLevelId.GameLevel02;
                        return true;
                    }

                    targetLevel = GameLevelId.None;
                    return false;

                case UIFlowAction.None:
                    // Supports non-UI callers of GameFlowController.BeginLoading().
                    targetLevel = currentLevel != GameLevelId.None
                        ? currentLevel
                        : GameLevelId.GameLevel01;
                    return true;

                case UIFlowAction.Menu:
                default:
                    targetLevel = GameLevelId.None;
                    return false;
            }
        }

        private static MapId ResolveMap(GameLevelId gameLevel)
        {
            switch (gameLevel)
            {
                case GameLevelId.GameLevel01:
                    return MapId.Map01;

                case GameLevelId.GameLevel02:
                    return MapId.Map02;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(gameLevel),
                        gameLevel,
                        "No Map mapping exists for this Game Level.");
            }
        }

        private bool TeleportSoldierGroupForMap02()
        {
            if (_soldierRuntimeRoot == null)
            {
                Debug.LogError(
                    "[Loading] Cannot reposition Soldier Group for Map02: " +
                    "the scene SoldierRuntimeRoot was not resolved.",
                    this);
                return false;
            }

            if (!_soldierRuntimeRoot.IsInitialized)
            {
                Debug.LogError(
                    "[Loading] Cannot reposition Soldier Group for Map02: " +
                    "SoldierRuntimeRoot is not initialized.",
                    _soldierRuntimeRoot);
                return false;
            }

            Vector3 spawnPosition =
                new Vector3(
                    Map02SoldierSpawnX,
                    Map02SoldierSpawnY,
                    Map02SoldierSpawnZ);

            _soldierRuntimeRoot.TeleportGroup(
                spawnPosition);

            Debug.Log(
                $"[Loading] Soldier Group teleported for Map02 through " +
                $"SoldierRuntimeRoot: {spawnPosition}.",
                this);

            return true;
        }

        private void ResolveSingleSoldierRuntimeRoot()
        {
            SoldierRuntimeRoot[] roots =
                FindObjectsByType<SoldierRuntimeRoot>(
                    FindObjectsSortMode.None);

            if (roots == null ||
                roots.Length == 0)
            {
                throw new InvalidOperationException(
                    "GameplayLoadingCoordinator could not find SoldierRuntimeRoot.");
            }

            if (roots.Length != 1)
            {
                throw new InvalidOperationException(
                    $"GameplayLoadingCoordinator expected exactly one active " +
                    $"SoldierRuntimeRoot but found {roots.Length}. " +
                    "Remove duplicate Soldier runtimes before loading gameplay.");
            }

            _soldierRuntimeRoot =
                roots[0];
        }

        private bool IsStillLoading()
        {
            return _flowModel != null &&
                   _flowModel.CurrentState == GameFlowStateId.Loading;
        }

        private void FinishRoutine()
        {
            _waitRoutine = null;
        }

        private void ValidateReferences()
        {
            if (gameplaySceneComposition == null)
            {
                throw new InvalidOperationException(
                    "GameplayLoadingCoordinator requires GameplaySceneComposition.");
            }

            if (mapRuntimeRoot == null)
            {
                throw new InvalidOperationException(
                    "GameplayLoadingCoordinator requires MapRuntimeRoot.");
            }
        }
    }
}
