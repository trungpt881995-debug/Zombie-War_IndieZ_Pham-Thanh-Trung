using System;
using UnityEngine;
using ZombieWar.Features.UI.Services;
using ZombieWar.Features.UI.Unity.Config;
using ZombieWar.Features.UI.Unity.Views;
using ZombieWar.Features.UI.View;

namespace ZombieWar.Features.UI.Unity.Root
{
    public sealed class UIRuntimeRoot : MonoBehaviour, IUIRootView
    {
        [SerializeField] private UIConfig uiConfig;
        [SerializeField] private WeaponUIConfig weaponUIConfig;
        [SerializeField] private MainMenuView mainMenu;
        [SerializeField] private GameplayHudView gameplay;
        [SerializeField] private PauseView pause;
        [SerializeField] private LevelCompleteView levelComplete;
        [SerializeField] private GameOverView gameOver;
        [SerializeField] private EndGameView endGame;
        private IUIRuntime _runtime;
        public IMainMenuView MainMenu => mainMenu;
        public IGameplayHudView Gameplay => gameplay;
        public IPauseView Pause => pause;
        public ILevelCompleteView LevelComplete => levelComplete;
        public IGameOverView GameOver => gameOver;
        public IEndGameView EndGame => endGame;
        public bool IsBound => _runtime != null;
        public void Bind(IUIRuntime runtime)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            if (!ValidateViews()) throw new InvalidOperationException("UIRuntimeRoot is missing one or more required screen views.");
            _runtime = runtime;
            ApplyConfig();
            _runtime.Bind(this);
        }
        public void Unbind()
        {
            if (_runtime == null) return;
            _runtime.Unbind(this);
            _runtime = null;
        }
        private void OnDestroy() => Unbind();
        private bool ValidateViews() => mainMenu != null && gameplay != null && pause != null && levelComplete != null && gameOver != null && endGame != null;
        private void ApplyConfig()
        {
            if (uiConfig != null)
            {
                mainMenu.SetTitle(uiConfig.gameTitle);
                endGame.SetReplayVisible(uiConfig.showEndGameReplay);
            }
            gameplay.ApplyWeaponConfig(weaponUIConfig);
        }
    }
}
