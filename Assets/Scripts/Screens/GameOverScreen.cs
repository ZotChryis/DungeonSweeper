using System;
using UI;
using UnityEngine;

namespace Screens
{
    public class GameOverScreen : BaseScreen
    {
        [SerializeField] private DSButton Retry;
        
        protected override void Awake()
        {
            base.Awake();

            Retry.OnConfirmed += OnRetryConfrimed;
        }

        protected void OnDestroy()
        {
            Retry.OnConfirmed -= OnRetryConfrimed;
        }

        private void OnRetryConfrimed()
        {
            // Once they confirmed it once, they don't need to again this run
            Retry.RequireConfirmation = false;
            ServiceLocator.Instance.LevelManager.RetryCurrentLevel();
            ServiceLocator.Instance.OverlayScreenManager.HideAllScreens();
        }
    }
}
