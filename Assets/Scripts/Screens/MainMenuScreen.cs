using NUnit.Framework;
using Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
    public class MainMenuScreen : BaseScreen
    {
        [SerializeField] private Button NewGameButton;
        [SerializeField] private Button LoadGameButton;
        [SerializeField] private Button ChallengeButton;
        [SerializeField] private Button AchievementButton;
        [SerializeField] private Button SettingsButton;
        [SerializeField] private Button DiscordButton;
        [SerializeField] private Button QuitGameButton;


        private void Start()
        {
            // TODO: find a better place for this?
            ServiceLocator.Instance.ChallengeSystem.TryUnlockChallengesForLegacyPlayers();
            
            NewGameButton.onClick.AddListener(OnNewGamePressed);
            ChallengeButton.onClick.AddListener(OnChallengesPressed);
            QuitGameButton.onClick.AddListener(OnQuitGamePressed);

            NewGameButton.gameObject.SetActive(true);

            LoadGameButton.onClick.AddListener(OnLoadGamePressed);
            AchievementButton.onClick.AddListener(OnAchievementsPressed);
            SettingsButton.onClick.AddListener(OnSettingsPressed);
            DiscordButton.onClick.AddListener(OnDiscordPressed);

            // Only show the challenges button if the feature is unlocked
            bool challengesUnlocked = ServiceLocator.Instance.ChallengeSystem.AreChallengesUnlocked();
            ChallengeButton.gameObject.SetActive(challengesUnlocked);
            
            // Start the game with the main menu screen as the only active screen
            ServiceLocator.Instance.OverlayScreenManager.HideAllScreens();
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

        private void OnChallengesPressed()
        {
            ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.Challenges);
        }
        
        private void OnQuitGamePressed()
        {
            Application.Quit();
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
            BaseScreen screen = ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.Settings);
            if(screen is SettingsScreen settingsScreen)
            {
                // Don't show main menu button from main menu.
                settingsScreen.SetMainMenuActive(false);
            }
        }
    }
}