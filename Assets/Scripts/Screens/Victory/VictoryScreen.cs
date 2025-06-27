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
            bool nextLevelAvailable = ServiceLocator.Instance.LevelManager.CurrentLevel < 2;
            MainMenu.gameObject.SetActive(!nextLevelAvailable);
            Shop.gameObject.SetActive(nextLevelAvailable);
        }
    }
}