using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZombieWar.Features.UI.Domain;
using ZombieWar.Features.UI.Unity.Components;
using ZombieWar.Features.UI.View;

namespace ZombieWar.Features.UI.Unity.Views
{
    public sealed class EndGameView : CanvasGroupScreenView, IEndGameView
    {
        [SerializeField] Button replayButton, menuButton;
        [SerializeField] TMP_Text finalScoreText;
        public override UIScreenId ScreenId => UIScreenId.EndGame;
        public event Action ReplayClicked, MenuClicked;
        private void Awake()
        {
            if (replayButton != null) replayButton.onClick.AddListener(() => ReplayClicked?.Invoke());
            if (menuButton != null) menuButton.onClick.AddListener(() => MenuClicked?.Invoke());
        }
        public void SetFinalScore(long score)
        {
            if (finalScoreText != null) finalScoreText.text = $"FINAL SCORE\n{score:N0}";
        }
        public void SetReplayVisible(bool visible)
        {
            if (replayButton != null) replayButton.gameObject.SetActive(visible);
        }
    }
}
