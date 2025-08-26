using DG.Tweening;
using System;
using UI;
using UnityEngine;

namespace Screens
{
    public class GameOverScreen : BaseScreen
    {
        [SerializeField] private DSButton Retry;
        [SerializeField] private DSButton MainMenu;

        protected override void Awake()
        {
            base.Awake();

            Retry.OnConfirmed += OnRetryConfrimed;
            MainMenu.OnConfirmed += OnMainMenuConfirmed;
        }

        protected void OnDestroy()
        {
            Retry.OnConfirmed -= OnRetryConfrimed;
            MainMenu.OnConfirmed -= OnMainMenuConfirmed;
        }

        private void OnRetryConfrimed()
        {
            TransitionManager.Instance.DoTransition(TransitionManager.TransitionType.Goop, HandleRetry);
        }

        private void OnMainMenuConfirmed()
        {
            TransitionManager.Instance.DoTransition(TransitionManager.TransitionType.Goop, CheatManager.Instance.Restart);
        }

        private void HandleRetry()
        {
            // Once they confirmed it once, they don't need to again this run
            Retry.RequireConfirmation = false;
            ServiceLocator.Instance.LevelManager.RetryCurrentLevel();
            ServiceLocator.Instance.OverlayScreenManager.HideAllScreens();
        }

        public void ToggleShowGameOver()
        {
            MainContentRoot.SetActive(!MainContentRoot.activeSelf);
        }
    }
}
