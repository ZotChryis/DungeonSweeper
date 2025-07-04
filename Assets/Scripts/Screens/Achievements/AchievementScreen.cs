using System.Collections.Generic;
using Schemas;
using UnityEngine;

namespace Screens.Achievements
{
    public class AchievementScreen : BaseScreen
    {
        [SerializeField] private AchievementItem AchievementPrefab;
        [SerializeField] private Transform ContentRoot;

        private List<AchievementItem> Items;
        
        private void Start()
        {
            RefreshItems();

            ServiceLocator.Instance.AchievementSystem.OnAchievementCompleted += OnAchievementCompleted;
        }

        private void RefreshItems()
        {
            if (Items != null)
            {
                foreach (var achievementItem in Items)
                {
                    Destroy(achievementItem.gameObject);
                }
                Items.Clear();
            }
            
            var schemas = ServiceLocator.Instance.Schemas.AchievementSchemas;
            Items = new List<AchievementItem>(schemas.Count);
            
            foreach (var schema in schemas)
            {
                AchievementItem item = Instantiate<AchievementItem>(AchievementPrefab, ContentRoot);
                item.SetSchema(schema);
                Items.Add(item);
            }
        }

        private void OnAchievementCompleted(AchievementSchema obj)
        {
            RefreshItems();
        }
    }
}
