using NUnit.Framework;
using Schemas;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
    public class MainMenuScreen : BaseScreen
    {
        [SerializeField] private Button NewGameButton;
        [SerializeField] private Button NewGameLevelSelectButton;
        [SerializeField] private Button LevelSelectButton;
        [SerializeField] private GameObject LevelSelectGrid;
        [SerializeField] private Button[] LevelSelectButtons;
        [SerializeField] private int[] StartingLevelStartingXp;
        [SerializeField] private Button LoadGameButton;
        [SerializeField] private Button AchievementButton;
        [SerializeField] private Button SettingsButton;
        [SerializeField] private Button DiscordButton;

        private void Start()
        {
            NewGameButton.onClick.AddListener(OnNewGamePressed);
            NewGameLevelSelectButton.onClick.AddListener(OnNewGamePressed);
            LevelSelectButton.onClick.AddListener(OnLevelSelectPressed);

            int lastSetStartingLevel = FBPP.GetInt("StartingLevel", 0);
            ServiceLocator.Instance.LevelManager.StartingLevel = lastSetStartingLevel;

            List<int> levelsUnlocked = GetLevelsUnlocked();
            if (levelsUnlocked.Count == 1)
            {
                NewGameButton.gameObject.SetActive(true);

                NewGameLevelSelectButton.gameObject.SetActive(false);
                LevelSelectButton.gameObject.SetActive(false);
                ServiceLocator.Instance.LevelManager.StartingLevel = 0;
            }
            else
            {
                NewGameButton.gameObject.SetActive(false);

                NewGameLevelSelectButton.gameObject.SetActive(true);
                LevelSelectButton.gameObject.SetActive(true);
                for (int i = 0; i < levelsUnlocked.Count; i++)
                {
                    LevelSelectButtons[levelsUnlocked[i]].gameObject.SetActive(true);
                }

                LevelSelectButton.GetComponentInChildren<TextMeshProUGUI>().text = (lastSetStartingLevel + 1).ToString();
            }

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
            ServiceLocator.Instance.Player.ShopXp = StartingLevelStartingXp[ServiceLocator.Instance.LevelManager.StartingLevel];
            ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.ClassSelection);
        }

        /// <summary>
        /// Popup when player selects which level to start at.
        /// </summary>
        /// <param name="option"></param>
        public void OnLevelSelected(int option)
        {
            ServiceLocator.Instance.LevelManager.StartingLevel = option;
            LevelSelectButton.GetComponentInChildren<TextMeshProUGUI>().text = (option + 1).ToString();
            FBPP.SetInt("StartingLevel", option);
            LevelSelectGrid.SetActive(false);
        }

        /// <summary>
        /// Toggles popup which shows levels the player can start at.
        /// </summary>
        private void OnLevelSelectPressed()
        {
            LevelSelectGrid.SetActive(!LevelSelectGrid.activeSelf);
        }

        /// <summary>
        /// Gets a list of levels the player has unlocked. By default level 0 is unlocked.
        /// </summary>
        /// <returns>List of levels that have been unlocked by level id</returns>
        private List<int> GetLevelsUnlocked()
        {
            List<int> unlockedLevels = new List<int>();
            unlockedLevels.Add(0);
            if (AchievementSchema.Id.Hardcore0.IsAchieved())
            {
                unlockedLevels.Add(1);
            }
            if (AchievementSchema.Id.Hardcore1.IsAchieved())
            {
                unlockedLevels.Add(2);
            }
            if (AchievementSchema.Id.Hardcore2.IsAchieved())
            {
                unlockedLevels.Add(3);
            }
            if (AchievementSchema.Id.Hardcore3.IsAchieved())
            {
                unlockedLevels.Add(4);
            }
            return unlockedLevels;
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