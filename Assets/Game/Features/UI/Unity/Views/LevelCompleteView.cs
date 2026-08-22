using System;
using UnityEngine;
using UnityEngine.UI;
using ZombieWar.Features.UI.Domain;
using ZombieWar.Features.UI.Unity.Components;
using ZombieWar.Features.UI.View;

namespace ZombieWar.Features.UI.Unity.Views
{
    public sealed class LevelCompleteView : CanvasGroupScreenView, ILevelCompleteView
    {
        [SerializeField] Button replayButton, nextButton, menuButton;
        public override UIScreenId ScreenId => UIScreenId.LevelComplete;
        public event Action ReplayClicked, NextClicked, MenuClicked;
        private void Awake()
        {
            if (replayButton != null) replayButton.onClick.AddListener(() => ReplayClicked?.Invoke());
            if (nextButton != null) nextButton.onClick.AddListener(() => NextClicked?.Invoke());
            if (menuButton != null) menuButton.onClick.AddListener(() => MenuClicked?.Invoke());
        }
    }
}
