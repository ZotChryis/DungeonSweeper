using Schemas;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.ClassSelection
{
    public class ClassSelectionScreen : BaseScreen
    {
        public GameObject LevelSelectGroup;
        public Button[] LevelSelectButtons;

        protected void Start()
        {
            ServiceLocator.Instance.AchievementSystem.OnAchievementCompleted += RefreshItems;
            RefreshItems(null);
        }

        protected override void OnHide()
        {
            base.OnHide();
            ServiceLocator.Instance.ChallengeSystem.CurrentChallenge = null;
        }

        protected override void OnShow()
        {
            base.OnShow();

            List<int> levelsUnlocked = GetLevelsUnlocked();
            for (int i = 0; i < LevelSelectButtons.Length; i++)
            {
                LevelSelectButtons[i].interactable = false;
                LevelSelectButtons[i].image.color = Color.white;
            }
            for (int i = 0; i < levelsUnlocked.Count; i++)
            {
                LevelSelectButtons[levelsUnlocked[i]].interactable = true;
            }

            int lastStartingLevel = FBPP.GetInt("StartingLevel", 0);
            ServiceLocator.Instance.LevelManager.StartingLevel = lastStartingLevel;
            Debug.Log("Last starting level: " + lastStartingLevel + ", length: " + LevelSelectButtons.Length);
            ColorUtility.TryParseHtmlString("#00FF5D", out Color greenFromHex);
            LevelSelectButtons[lastStartingLevel].image.color = greenFromHex;
            LevelSelectButtons[lastStartingLevel].Select();
        }

        private void OnDestroy()
        {
            ServiceLocator.Instance.AchievementSystem.OnAchievementCompleted -= RefreshItems;
        }

        private void RefreshItems(AchievementSchema _)
        {
            var unlockedClasses = ServiceLocator.Instance.AchievementSystem.GetUnlockedClasses();
            var items = GetComponentsInChildren<ClassSelectionItem>(includeInactive: true);
            foreach (var classSelectionItem in items)
            {
                classSelectionItem.SetLocked(!unlockedClasses.Contains(classSelectionItem.GetClassId()));
            }
        }


        /// <summary>
        /// Select level 0, 1, 2, etc.
        /// </summary>
        /// <param name="option"></param>
        public void OnLevelSelected(int option)
        {
            ServiceLocator.Instance.LevelManager.StartingLevel = option;
            FBPP.SetInt("StartingLevel", option);
            foreach (var button in LevelSelectButtons)
            {
                button.image.color = Color.white;
            }
            ColorUtility.TryParseHtmlString("#00FF5D", out Color greenFromHex);
            LevelSelectButtons[option].image.color = greenFromHex;
        }

        /// <summary>
        /// Gets a list of levels the player has unlocked. By default level 0 is unlocked.
        /// </summary>
        /// <returns>List of levels that have been unlocked by level id</returns>
        private List<int> GetLevelsUnlocked()
        {
            List<int> unlockedLevels = new List<int>(5);
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
    }
}