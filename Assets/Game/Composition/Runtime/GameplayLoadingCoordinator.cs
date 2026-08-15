using System;
using System.Collections;
using UnityEngine;
using VContainer;
using ZombieWar.Bootstrap;
using ZombieWar.Features.Map.Domain;
using ZombieWar.Features.Map.Unity.Runtime;
using ZombieWar.GameFlow.Controller;
using ZombieWar.GameFlow.Domain;
using ZombieWar.GameFlow.Model;

namespace ZombieWar.Composition
{
    [DisallowMultipleComponent]
    public sealed class GameplayLoadingCoordinator : MonoBehaviour
    {
        [Header("Gameplay Scene")]
        [SerializeField]
        private GameplaySceneComposition gameplaySceneComposition;

        [Header("Map")]
        [SerializeField]
        private MapRuntimeRoot mapRuntimeRoot;

        [SerializeField]
        private MapId initialMap = MapId.Map01;

        private GameFlowModel _flowModel;
        private GameFlowController _flowController;
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

            _flowModel.StateChanged += OnGameFlowStateChanged;

            // Handles both cases:
            // 1. this scene exists before Loading begins;
            // 2. this scene is loaded while GameFlow is already Loading.
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

        private void OnGameFlowStateChanged(
            GameFlowStateId state)
        {
            if (state != GameFlowStateId.Loading)
            {
                return;
            }

            if (_waitRoutine != null)
            {
                return;
            }

            _waitRoutine = StartCoroutine(
                WaitForGameplayReady());
        }

        private IEnumerator WaitForGameplayReady()
        {
            // GameplaySceneComposition owns scene-runtime binding. Do not request
            // map loading until all scene RuntimeRoots have been initialized.
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

            Debug.Log(
                $"[Loading] Loading map: {initialMap}",
                this);

            var loadTask = mapRuntimeRoot.LoadMapAsync(initialMap);

            while (!loadTask.IsCompleted)
            {
                if (!IsStillLoading())
                {
                    // MapRuntimeRoot owns the map-load lifetime token. We stop this
                    // orchestration routine here and must not advance GameFlow.
                    FinishRoutine();
                    yield break;
                }

                yield return null;
            }

            if (loadTask.IsCanceled)
            {
                Debug.LogError(
                    $"[Loading] Loading {initialMap} was cancelled.",
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
                        $"[Loading] Loading {initialMap} failed with an unknown exception.",
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

            // Let the instantiated MapView and dependent Unity components complete
            // their current frame before gameplay gates are opened.
            yield return null;

            if (IsStillLoading())
            {
                _flowController.BeginGameplay();
            }

            FinishRoutine();
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

            if (initialMap == MapId.None)
            {
                throw new InvalidOperationException(
                    "GameplayLoadingCoordinator initialMap cannot be MapId.None.");
            }
        }
    }
}
