using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
    public class MainMenuScreen : BaseScreen
    {
        [SerializeField] private Button NewGameButton;
        [SerializeField] private Button AchievementButton;
        [SerializeField] private Button SettingsButton;

        private void Start()
        {
            NewGameButton.onClick.AddListener(OnNewGamePressed);
            AchievementButton.onClick.AddListener(OnAchievementsPressed);
            SettingsButton.onClick.AddListener(OnSettingsPressed);

            // The game is authored to start with this overlay active
            // TODO: We should probably make a proper screen management cycle 
            ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.MainMenu);
        }

        public void OnNewGamePressed()
        {
            // Open the class selection screen
            // That screen has logic to start a game when something is selected
            ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.ClassSelection);
        }

        // TODO: Show achievements screen
        public void OnAchievementsPressed()
        {
            
        }

        // TODO: Show settings screen
        public void OnSettingsPressed()
        {
            
        }
    }
}