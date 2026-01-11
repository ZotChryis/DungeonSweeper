using System.Collections.Generic;
using Gameplay;
using Schemas;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.Achievements
{
    public class AchievementItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text Title;
        [SerializeField] private TMP_Text Description;
        [SerializeField] private TMP_Text Reward;
        [SerializeField] private TMP_Text Date;
        [SerializeField] private GameObject Check;
        [SerializeField] private Image AchievementIcon;
        [SerializeField] private Material GrayscaleMaterial;

        [SerializeField] private Transform TabContainer;
        [SerializeField] private AchievementTabButton TabButtonPrefab;

        private AchievementSchema _currentSchema;
        
        // Only used if servicing a chain of achievements
        private List<AchievementSchema> _allSchemas;
        private List<AchievementTabButton> _tabButtons;
        private AchievementTabButton _currentTab;
        
        public void SetSchema(AchievementSchema schema)
        {
            _currentSchema = schema;
            
            Title.SetText(schema.Title);
            Description.SetText(schema.Description);
            Reward.SetText(schema.Reward);
            AchievementIcon.sprite = schema.AchievementIcon;

            bool achieved = schema.AchievementId.IsAchieved();
            if (achieved)
            {
                AchievementIcon.material = null;
            }
            else
            {
                AchievementIcon.material = GrayscaleMaterial;
            }
            string dateAchieved = FBPP.GetString("Achievement" + schema.AchievementId, "");

            Check.SetActive(achieved);

            Date.gameObject.SetActive(achieved);
            Date.SetText("Achieved: " + dateAchieved);
        }

        /// <summary>
        /// This version is to be used when multiple achievements are to be tracked by one element.
        /// Make sure it is spawned using the version with the tab containers!
        /// Im adding this in a semi-hacky way (no inheritence) so be warned!
        /// </summary>
        public void SetSchemas(List<AchievementSchema> schemas)
        {
            _allSchemas = schemas;
            
            // For every instance, we need to make a tab button
            int toShow = 0;
            _tabButtons =  new List<AchievementTabButton>(_allSchemas.Count);
            for (int i = 0; i < _allSchemas.Count; i++)
            {
                var schema = _allSchemas[i];
                bool achieved = schema.AchievementId.IsAchieved();
                
                AchievementTabButton tab =  Instantiate<AchievementTabButton>(TabButtonPrefab, TabContainer);
                tab.SetAchivedState(achieved);
                tab.SetSelected(false);
                _tabButtons.Add(tab);

                if (achieved)
                {
                    toShow = i;
                }
                
                int capturedIndex = i;
                tab.GetComponent<Button>().onClick.AddListener(() => OnTabClicked(capturedIndex));
            }
            
            OnTabClicked(toShow);
        }

        private void OnTabClicked(int tabIndex)
        {
            SetSchema(_allSchemas[tabIndex]);
            
            // If there are tabs, make sure they are toggled properly
            if (_currentTab != null)
            {
                _currentTab.SetSelected(false);
            }
            
            _currentTab =  _tabButtons[tabIndex];
            _currentTab.SetSelected(true);
        }
    }
}
