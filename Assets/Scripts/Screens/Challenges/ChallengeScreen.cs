using System.Collections.Generic;
using Gameplay;
using Schemas;
using Screens.ClassSelection;
using Screens.Inventory;
using TMPro;
using UnityEngine;
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
        private ChallengeSchema _currentChallenge;
        
        private void Start()
        {
            playButton.interactable = false;
            playButton.onClick.AddListener(OnPlayButtonClicked);

            RefreshChallenges();
            
            // Choose the first one
            ChallengeSchema first = ServiceLocator.Instance.Schemas.ChallengeSchemas[0];
            SelectChallenge(first);
        }

        private void RefreshChallenges()
        {
            foreach (var c in _challenges)
            {
                Destroy(c.gameObject);
            }
            _challenges.Clear();

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
            _currentChallenge = schema;
            
            playButton.interactable = true;
            startingClassLabel.SetText(schema.StartingClass == Class.Id.None ? "Any" : schema.StartingClass.ToString());
            moreContextLabel.SetText(schema.Context);

            foreach (var item in _items)
            {
                Destroy(item.gameObject);
            }
            _items.Clear();
            
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
            if (_currentChallenge == null)
            {
                return;
            }

            ServiceLocator.Instance.ChallengeSystem.CurrentChallenge = _currentChallenge;
            
            // Override the levels if we need to
            if (_currentChallenge.OverrideLevels)
            {
                ServiceLocator.Instance.LevelManager.OverrideLevels(_currentChallenge.Levels);
            }
            else
            {
                ServiceLocator.Instance.LevelManager.UseDefaultLevels();
            }
            
            // If a specific class is used, then do that and start the game
            if (_currentChallenge.StartingClass != Class.Id.None)
            {
                ServiceLocator.Instance.SaveSystem.Wipe();
                ServiceLocator.Instance.AchievementSystem.AllowAchievementsToBeCompleted = true;
                ServiceLocator.Instance.Player.TEMP_SetClass(_currentChallenge.StartingClass);
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
                ClassSelectionScreen classSelectionScreen =
                    ServiceLocator.Instance.OverlayScreenManager.Screens[
                        OverlayScreenManager.ScreenType.ClassSelection].GetComponent<ClassSelectionScreen>();
            }
        }
    }

    public class ChallengeSystem
    {
        private static string saveKeyPrefix = "Challenge_";
        
        public ChallengeSchema CurrentChallenge { get; set; }
        
        public bool IsCompleted(ChallengeSchema schema)
        {
            return FBPP.GetBool(saveKeyPrefix + schema.ChallengeId, false);
        }
        
        public void Complete(ChallengeSchema schema)
        {
            FBPP.SetBool(saveKeyPrefix + schema.ChallengeId, true);
            FBPP.Save();
        }
    }
}
