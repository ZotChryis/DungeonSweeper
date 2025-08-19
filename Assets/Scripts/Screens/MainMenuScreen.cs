using System;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
    public class MainMenuScreen : BaseScreen
    {
        [SerializeField] private Button NewGameButton;
        [SerializeField] private Button LoadGameButton;
        [SerializeField] private Button AchievementButton;
        [SerializeField] private Button SettingsButton;
        [SerializeField] private Button DiscordButton;

        private void Start()
        {
            NewGameButton.onClick.AddListener(OnNewGamePressed);
            LoadGameButton.onClick.AddListener(OnLoadGamePressed);
            AchievementButton.onClick.AddListener(OnAchievementsPressed);
            SettingsButton.onClick.AddListener(OnSettingsPressed);
            DiscordButton.onClick.AddListener(OnDiscordPressed);

            // The game is authored to start with this overlay active
            // TODO: We should probably make a proper screen management cycle 
            ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.MainMenu);
        }

        private void OnEnable()
        {
            LoadGameButton.gameObject.SetActive(ServiceLocator.Instance.SaveSystem.HasSave());
        }

        private void OnDiscordPressed()
        {
            Application.OpenURL("https://discord.gg/fx2u78DHGR");
        }

        private void OnNewGamePressed()
        {
            // Open the class selection screen
            // That screen has logic to start a game when something is selected
            ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.ClassSelection);
        }

        private void OnLoadGamePressed()
        {
            ServiceLocator.Instance.SaveSystem.LoadGame();
            TransitionManager.Instance.DoTransition(TransitionManager.TransitionType.Goop, ServiceLocator.Instance.OverlayScreenManager.HideAllScreens);
        }
        
        // TODO: Show achievements screen
        public void OnAchievementsPressed()
        {
            ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.Achievements);
        }

        // TODO: Show settings screen
        public void OnSettingsPressed()
        {
            ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.Settings);
        }
    }
}