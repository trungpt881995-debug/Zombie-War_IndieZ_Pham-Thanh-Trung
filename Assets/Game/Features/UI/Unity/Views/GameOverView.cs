using System;
using UnityEngine;
using UnityEngine.UI;
using ZombieWar.Features.UI.Domain;
using ZombieWar.Features.UI.Unity.Components;
using ZombieWar.Features.UI.View;

namespace ZombieWar.Features.UI.Unity.Views
{
    public sealed class GameOverView : CanvasGroupScreenView, IGameOverView
    {
        [SerializeField] Button replayButton, menuButton;
        public override UIScreenId ScreenId => UIScreenId.GameOver;
        public event Action ReplayClicked, MenuClicked;
        private void Awake()
        {
            if (replayButton != null) replayButton.onClick.AddListener(() => ReplayClicked?.Invoke());
            if (menuButton != null) menuButton.onClick.AddListener(() => MenuClicked?.Invoke());
        }
    }
}
