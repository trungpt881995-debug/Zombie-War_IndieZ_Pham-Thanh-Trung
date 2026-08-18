using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZombieWar.Composition
{
    [DisallowMultipleComponent]
    public sealed class BootSceneLoader : MonoBehaviour
    {
        private const string DefaultGameplaySceneName = "ZombieWar_Gameplay";

        [Header("Dependencies")]
        [SerializeField]
        private GlobalPresentationComposition globalPresentationComposition;

        [Header("Gameplay Scene")]
        [SerializeField]
        private string gameplaySceneName = DefaultGameplaySceneName;

        [SerializeField]
        private bool loadSceneOnStart = true;

        [SerializeField]
        private bool setGameplaySceneActive = true;

        [SerializeField]
        private bool unloadBootSceneAfterLoad = true;

        private Coroutine _loadRoutine;
        private bool _isCompleted;

        public bool IsLoading => _loadRoutine != null;

        public bool IsCompleted => _isCompleted;

        public string GameplaySceneName => gameplaySceneName;

        private void Start()
        {
            if (loadSceneOnStart)
            {
                LoadGameplayScene();
            }
        }

        public void LoadGameplayScene()
        {
            if (_isCompleted || _loadRoutine != null)
            {
                return;
            }

            _loadRoutine = StartCoroutine(LoadGameplaySceneRoutine());
        }

        private IEnumerator LoadGameplaySceneRoutine()
        {
            Scene bootScene = gameObject.scene;

            if (!ValidateConfiguration(bootScene))
            {
                _loadRoutine = null;
                yield break;
            }

            if (!TryBindGlobalPresentation())
            {
                _loadRoutine = null;
                yield break;
            }

            Scene gameplayScene = SceneManager.GetSceneByName(gameplaySceneName);

            if (!gameplayScene.IsValid() || !gameplayScene.isLoaded)
            {
                AsyncOperation loadOperation = SceneManager.LoadSceneAsync(gameplaySceneName, LoadSceneMode.Additive);

                if (loadOperation == null)
                {
                    Debug.LogError(
                        $"[{nameof(BootSceneLoader)}] Unity failed to create a load operation for " +
                        $"scene '{gameplaySceneName}'.",
                        this);

                    _loadRoutine = null;
                    yield break;
                }

                while (!loadOperation.isDone)
                {
                    yield return null;
                }

                gameplayScene = SceneManager.GetSceneByName(
                    gameplaySceneName);
            }

            if (!gameplayScene.IsValid() || !gameplayScene.isLoaded)
            {
                Debug.LogError(
                    $"[{nameof(BootSceneLoader)}] Gameplay scene '{gameplaySceneName}' was not loaded successfully.",
                    this);

                _loadRoutine = null;
                yield break;
            }

            if (setGameplaySceneActive && !SceneManager.SetActiveScene(gameplayScene))
            {
                Debug.LogError(
                    $"[{nameof(BootSceneLoader)}] Could not set gameplay scene '{gameplaySceneName}' as active.",
                    this);

                _loadRoutine = null;
                yield break;
            }

            _isCompleted = true;

            if (unloadBootSceneAfterLoad && bootScene.IsValid() && bootScene.isLoaded && bootScene != gameplayScene)
            {
                AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(bootScene);

                if (unloadOperation != null)
                {
                    while (!unloadOperation.isDone)
                    {
                        yield return null;
                    }
                }
            }

            _loadRoutine = null;
        }

        private bool ValidateConfiguration(Scene bootScene)
        {
            if (globalPresentationComposition == null)
            {
                Debug.LogError(
                    $"[{nameof(BootSceneLoader)}] GlobalPresentationComposition is not assigned.",
                    this);

                return false;
            }

            if (string.IsNullOrWhiteSpace(gameplaySceneName))
            {
                Debug.LogError(
                    $"[{nameof(BootSceneLoader)}] Gameplay Scene Name is empty.",
                    this);

                return false;
            }

            string trimmedSceneName = gameplaySceneName.Trim();

            if (!string.Equals(trimmedSceneName, gameplaySceneName, StringComparison.Ordinal))
            {
                gameplaySceneName = trimmedSceneName;
            }

            if (string.Equals(bootScene.name, gameplaySceneName, StringComparison.Ordinal))
            {
                Debug.LogError(
                    $"[{nameof(BootSceneLoader)}] Gameplay scene cannot be the same as the boot scene " +
                    $"('{bootScene.name}').",
                    this);

                return false;
            }

            Scene alreadyLoaded = SceneManager.GetSceneByName(gameplaySceneName);

            if ((!alreadyLoaded.IsValid() || !alreadyLoaded.isLoaded) && !Application.CanStreamedLevelBeLoaded(gameplaySceneName))
            {
                Debug.LogError(
                    $"[{nameof(BootSceneLoader)}] Scene '{gameplaySceneName}' cannot be loaded. " +
                    "Add it to the active Build Profile / Scenes In Build.",
                    this);

                return false;
            }

            return true;
        }

        private bool TryBindGlobalPresentation()
        {
            try
            {
                globalPresentationComposition.Bind();
                return globalPresentationComposition.IsBound;
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception,
                    this);

                return false;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(gameplaySceneName))
            {
                gameplaySceneName = DefaultGameplaySceneName;
            }
            else
            {
                gameplaySceneName = gameplaySceneName.Trim();
            }
        }
#endif
    }
}
