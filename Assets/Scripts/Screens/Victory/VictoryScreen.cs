using Schemas;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.Victory
{
    public class VictoryScreen : BaseScreen
    {
        [SerializeField] Button MainMenu;
        [SerializeField] Button Shop;

        private void Start()
        {
            MainMenu.onClick.AddListener(OnMainMenuClicked);
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
            ServiceLocator.Instance.CheatManager.Restart();
        }

        protected override void OnShow()
        {
            bool isFinalLevel = ServiceLocator.Instance.LevelManager.CurrentLevel == ServiceLocator.Instance.LevelManager.Levels.Length - 1;
            MainMenu.gameObject.SetActive(isFinalLevel);
            Shop.gameObject.SetActive(!isFinalLevel);
            
            ServiceLocator.Instance.AudioManager.PlaySfx("Victory");
            
            // TODO: Reverse the listener approach. AchievementSystem should listen for an event to check itself instead
            //  of being told to check
            ServiceLocator.Instance.AchievementSystem.CheckAchievements(AchievementSchema.TriggerType.Victory);
            ServiceLocator.Instance.AchievementSystem.CheckAchievements(AchievementSchema.TriggerType.Pacifist);
            ServiceLocator.Instance.AchievementSystem.CheckAchievements(AchievementSchema.TriggerType.FullBoardClear);
        }
    }
}