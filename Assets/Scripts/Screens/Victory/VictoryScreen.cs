using GameAnalyticsSDK;
using Schemas;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.Victory
{
    public class VictoryScreen : BaseScreen
    {
        [SerializeField] Button MainMenu;
        [SerializeField] Button NextLevel;
        [SerializeField] Button Shop;

        private void Start()
        {
            MainMenu.onClick.AddListener(OnMainMenuClicked);
            NextLevel.onClick.AddListener(OnNextLevelClicked);
            Shop.onClick.AddListener(OnShopClicked);
        }

        private void OnShopClicked()
        {
            // Hide the victory screen, then show the shop screen
            ServiceLocator.Instance.OverlayScreenManager.HideAllScreens();
            ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.Shop);
        }

        private void OnMainMenuClicked()
        {
            // TODO: We need a real 'reset' of the game, right now this just reloads the whole scene lmao
            CheatManager.Instance.Restart();
        }

        private void OnNextLevelClicked()
        {
            ServiceLocator.Instance.Player.ResetPlayer();
            ServiceLocator.Instance.LevelManager.NextLevel();
            ServiceLocator.Instance.OverlayScreenManager.HideAllScreens();
            ServiceLocator.Instance.SaveSystem.SaveGame();
        }

        protected override void OnShow()
        {
            bool isFinalLevel = ServiceLocator.Instance.LevelManager.IsCurrentLevelFinalLevel();
            MainMenu.gameObject.SetActive(isFinalLevel);
            
            bool hasChallenge = ServiceLocator.Instance.ChallengeSystem.CurrentChallenge != null;
            
            // Check if the challenge blocks the shop. If so, disable the shop button and enable the next level button
            bool shopBlockedByChallenge = hasChallenge && ServiceLocator.Instance.ChallengeSystem.CurrentChallenge.BlockShopping;
            Shop.gameObject.SetActive(!isFinalLevel && !shopBlockedByChallenge);
            NextLevel.gameObject.SetActive(!isFinalLevel && shopBlockedByChallenge);
            
            // Check for challenge completion
            if (isFinalLevel && hasChallenge)
            {
                ServiceLocator.Instance.ChallengeSystem.Complete(ServiceLocator.Instance.ChallengeSystem.CurrentChallenge);
            }

            if (isFinalLevel)
            {
                ServiceLocator.Instance.SaveSystem.WipeRun();
            }
            
            ServiceLocator.Instance.AudioManager.PlaySfx("Victory");
            
            // TODO: Find better spot for this
            ServiceLocator.Instance.LevelManager.TryGrantAnnihilatorBonus();
            
            // TODO: Reverse the listener approach. AchievementSystem should listen for an event to check itself instead
            //  of being told to check
            ServiceLocator.Instance.AchievementSystem.CheckAchievements(AchievementSchema.TriggerType.Victory);
            ServiceLocator.Instance.AchievementSystem.CheckAchievements(AchievementSchema.TriggerType.Pacifist);
            ServiceLocator.Instance.AchievementSystem.CheckAchievements(AchievementSchema.TriggerType.Killer);
            ServiceLocator.Instance.AchievementSystem.CheckAchievements(AchievementSchema.TriggerType.FullBoardClear);

            // Send game analytics that the player has won. progression01=LevelName. progression02=Class.ToString. Score=PlayerLevel.
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, ServiceLocator.Instance.LevelManager.CurrentLevelName, ServiceLocator.Instance.Player.Class.ToString(), ServiceLocator.Instance.Player.CurrentPlayerLevel);
        }
    }
}