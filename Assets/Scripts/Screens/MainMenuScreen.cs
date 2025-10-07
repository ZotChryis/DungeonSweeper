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
        [SerializeField] private Button AchievementButton;
        [SerializeField] private Button SettingsButton;
        [SerializeField] private Button DiscordButton;
        [SerializeField] private Button QuitGameButton;


        private void Start()
        {
            NewGameButton.onClick.AddListener(OnNewGamePressed);

            QuitGameButton.onClick.AddListener(OnQuitGamePressed);

            NewGameButton.gameObject.SetActive(true);

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