using System;
using System.Collections.Generic;
using Gameplay;
using Schemas;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

namespace Screens.Achievements
{
    public class AchievementScreen : BaseScreen
    {
        [SerializeField] private AchievementItem AchievementPrefab;
        [SerializeField] private TMP_Text Title;
        [SerializeField] private Transform ContentRoot;

        private List<AchievementItem> Items;
        
        private void Start()
        {
            RefreshItems();

            ServiceLocator.Instance.AchievementSystem.OnAchievementCompleted += OnAchievementCompleted;
        }

        private void OnDestroy()
        {
            ServiceLocator.Instance.AchievementSystem.OnAchievementCompleted -= OnAchievementCompleted;
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
            schemas.Sort((a1, a2) =>
            {
                if (a1.Class == a2.Class)
                {
                    return a1.AchievementId > a2.AchievementId ? 1 : -1;
                }

                if (a1.Class != Class.Id.None && a2.Class == Class.Id.None)
                {
                    return -1;
                }
                else if (a1.Class == Class.Id.None && a2.Class != Class.Id.None)
                {
                    return 1;
                }
                
                return a1.Class > a2.Class ? 1 : -1;
            });
            Items = new List<AchievementItem>(schemas.Count);
            
            foreach (var schema in schemas)
            {
                AchievementItem item = Instantiate<AchievementItem>(AchievementPrefab, ContentRoot);
                item.SetSchema(schema);
                Items.Add(item);
            }
            
            int completed = ServiceLocator.Instance.AchievementSystem.GetFinishedAchievementCount();
            string text = $"ACHIEVEMENTS ({completed}/{Items.Count})";
            Title.SetText(text);
        }

        private void OnAchievementCompleted(AchievementSchema obj)
        {
            RefreshItems();
        }
    }
}
