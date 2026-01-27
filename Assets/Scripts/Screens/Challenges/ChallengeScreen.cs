using System;
using System.Collections.Generic;
using Gameplay;
using Schemas;
using Screens.ClassSelection;
using Screens.Inventory;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Screens.Challenges
{
    public class ChallengeScreen : BaseScreen
    {
        [SerializeField] private ChallengeItem challengePrefab;
        [SerializeField] private InventoryItem itemPrefab;
        
        [SerializeField] private Transform challengeContent;
        [SerializeField] private Transform itemContent;
        
        [SerializeField] private Button playButton;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text startingClassLabel;
        [SerializeField] private TMP_Text moreContextLabel;

        private List<ChallengeItem> _challenges = new();
        private List<InventoryItem> _items = new();
        
        private void Start()
        {
            playButton.interactable = false;
            playButton.onClick.AddListener(OnPlayButtonClicked);

            RefreshChallenges();
        }

        protected override void OnShow()
        {
            // Choose the first challenge when showing the screen
            ChallengeSchema first = ServiceLocator.Instance.Schemas.ChallengeSchemas[0];
            SelectChallenge(first);
        }

        protected override void OnHide()
        {
            base.OnHide();
            
            ServiceLocator.Instance.ChallengeSystem.SelectedChallenge = null;
        }

        private void RefreshChallenges()
        {
            foreach (var c in _challenges)
            {
                Destroy(c.gameObject);
            }
            _challenges.Clear();

            var challenges = ServiceLocator.Instance.Schemas.ChallengeSchemas;
            challenges.Sort(((s1, s2) => String.Compare(s1.Title, s2.Title, StringComparison.Ordinal)));
            
            int completedAmount = 0;
            foreach (var schema in ServiceLocator.Instance.Schemas.ChallengeSchemas)
            {
                ChallengeSchema capturedSchema = schema;
                ChallengeItem c = Instantiate<ChallengeItem>(challengePrefab, challengeContent);
                c.SetData(schema, () => { SelectChallenge(capturedSchema); });
                _challenges.Add(c);

                if (ServiceLocator.Instance.ChallengeSystem.IsCompleted(schema))
                {
                    completedAmount += 1;
                }
            }

            string titleString = $"CHALLENGES {completedAmount}/{ServiceLocator.Instance.Schemas.ChallengeSchemas.Count}";
            title.SetText(titleString);
        }

        private void SelectChallenge(ChallengeSchema schema)
        {
            ServiceLocator.Instance.ChallengeSystem.SelectedChallenge = schema;
            
            playButton.interactable = true;
            startingClassLabel.SetText(schema.StartingClass == Class.Id.None ? "You Choose" : schema.StartingClass.ToString());
            moreContextLabel.SetText(schema.Context);

            foreach (var item in _items)
            {
                Destroy(item.gameObject);
            }
            _items.Clear();
            
            // If there is a starting class, add that item to the list of items for clarity
            if (schema.StartingClass != Class.Id.None)
            {
                ClassSchema classSchema = ServiceLocator.Instance.Schemas.ClassSchemas.Find(c => c.Id == schema.StartingClass);
                ItemSchema classItem = ServiceLocator.Instance.Schemas.ItemSchemas.Find(i => i.ItemId == classSchema.StartingItem);
                InventoryItem item = Instantiate(itemPrefab, itemContent);
                item.Initialize(classItem);
                _items.Add(item);
            }
            
            // Add the extra items
            if (schema.StartingItems != null && schema.StartingItems.Length > 0)
            {
                foreach (var itemSchema in schema.StartingItems)
                {
                    InventoryItem item = Instantiate(itemPrefab, itemContent);
                    item.Initialize(itemSchema);
                    _items.Add(item);
                }
            }
        }

        private void OnPlayButtonClicked()
        {
            // just in case
            if (ServiceLocator.Instance.ChallengeSystem.SelectedChallenge == null)
            {
                return;
            }
            
            // If a specific class is used, then do that and start the game
            if (ServiceLocator.Instance.ChallengeSystem.SelectedChallenge.StartingClass != Class.Id.None)
            {
                // At this point we commit to the challenge
                ServiceLocator.Instance.ChallengeSystem.Commit();
                ServiceLocator.Instance.LevelManager.StartingLevel = 0;
                
                ServiceLocator.Instance.SaveSystem.WipeRun();
                ServiceLocator.Instance.AchievementSystem.AllowAchievementsToBeCompleted = true;
                ServiceLocator.Instance.Player.TEMP_SetClass(ServiceLocator.Instance.ChallengeSystem.CurrentChallenge.StartingClass);
                ServiceLocator.Instance.LevelManager.SetToStartingLevel();
                ServiceLocator.Instance.Grid.GenerateGrid();
                ServiceLocator.Instance.Player.ChangeBountyTarget();
                ServiceLocator.Instance.Player.ChangeMenuTarget();
                ServiceLocator.Instance.OverlayScreenManager.HideAllScreens();
            }
            else
            {
                // Otherwise, we just open the class select with the challenge option
                ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.ClassSelection);
            }
        }
    }

    public class ChallengeSystem
    {
        private static string saveKeyPrefix = "Challenge_";
        private static string featureSaveKey = "ChallengesUnlocked";
        
        public ChallengeSchema SelectedChallenge { get; set; }
        public ChallengeSchema CurrentChallenge { get; set; }
        
        public void TryUnlockChallengesForLegacyPlayers()
        {
            // On creation, check to see if we need to auto-unlock challenges for legacy players
            // (they will unlock based off achievements)
            if (!AreChallengesUnlocked() && ServiceLocator.Instance.AchievementSystem.IsAnyClassLevel0AchievementCompleted())
            {
                UnlockChallenges();
                ServiceLocator.Instance.ToastManager.RequestToast(null, "Challenges Unlocked!", "Access special challenge runs from the main menu!", 3.0f);
            }

            CheckChallengeAchievements();
        }

        public bool AreChallengesUnlocked()
        {
            return FBPP.GetBool(featureSaveKey, false);
        }

        public void UnlockChallenges()
        {
            FBPP.SetBool(featureSaveKey, true);
            FBPP.Save();
        }
        
        public bool IsCompleted(ChallengeSchema schema)
        {
            return FBPP.GetBool(saveKeyPrefix + schema.ChallengeId, false);
        }
        
        public void Complete(ChallengeSchema schema)
        {
            // Don't recomplete challenges
            if (IsCompleted(schema))
            {
                return;
            }
            
            ServiceLocator.Instance.ToastManager.RequestToast(null, "Challenge Completed!", schema.Title, 3.0f);
            FBPP.SetBool(saveKeyPrefix + schema.ChallengeId, true);
            FBPP.Save();
            
            CheckChallengeAchievements();
        }

        private void CheckChallengeAchievements()
        {
            ServiceLocator.Instance.AchievementSystem.CheckAchievements(AchievementSchema.TriggerType.Challenges);
        }
        
        public void Commit()
        {
            CurrentChallenge = SelectedChallenge;
            if (CurrentChallenge == null)
            {
                ServiceLocator.Instance.LevelManager.UseDefaultLevels();
                ServiceLocator.Instance.Player.UseDefaultLevelProgression();
                return;
            }
            
            // Override the levels if we need to
            if (CurrentChallenge.OverrideLevels)
            {
                ServiceLocator.Instance.LevelManager.OverrideLevels(ServiceLocator.Instance.ChallengeSystem.SelectedChallenge.Levels);
            }
            else
            {
                ServiceLocator.Instance.LevelManager.UseDefaultLevels();
            }
            
            // Override level progression schema if we need to
            if (CurrentChallenge.OverrideLevelProgression && CurrentChallenge.LevelProgression != null) 
            {
                ServiceLocator.Instance.Player.OverrideLevelProgression(ServiceLocator.Instance.ChallengeSystem.SelectedChallenge.LevelProgression);
            }
            else
            {
                ServiceLocator.Instance.Player.UseDefaultLevelProgression();
            }
        }

        public int GetCompletedChallengeCount()
        {
            int completed = 0;
            foreach (var schema in ServiceLocator.Instance.Schemas.ChallengeSchemas)
            {
                completed += IsCompleted(schema) ? 1 : 0;
            }
            return completed;
        }
    }
}
