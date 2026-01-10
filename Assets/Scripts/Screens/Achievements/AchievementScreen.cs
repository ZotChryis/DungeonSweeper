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
            
            if (!ServiceLocator.Instance.IsPaidVersion())
            {
                schemas.RemoveAll(a => a.PaidExclusive);
            }

            schemas.Sort((a1, a2) =>
            {
                // First and foremost, use the sort group logic. Allow us to inform sort groups by the data.
                var sortGroupResult = a1.SortGroup.CompareTo(a2.SortGroup);
                if (sortGroupResult != 0)
                {
                    return sortGroupResult;
                }

                // If there was a tie, then...
                // If both give a reward class, prefer the one where the class comes first
                if (a1.RewardClass != Class.Id.None && a2.RewardClass != Class.Id.None)
                {
                    return a1.RewardClass > a2.RewardClass ? 1 : -1;
                }
                
                // If the two achievements share the same class, go by ID
                if (a1.Class == a2.Class && a1.Class != Class.Id.None)
                {
                    return a1.AchievementId > a2.AchievementId ? 1 : -1;
                }

                // Otherwise, prefer one if it is using a class vs the other doesn't have a class
                if (a1.Class != Class.Id.None && a2.Class == Class.Id.None)
                {
                    return -1;
                }
                if (a1.Class == Class.Id.None && a2.Class != Class.Id.None)
                {
                    return 1;
                }

                // If both have a class, prefer the one that appears first by classID
                if (a1.Class != Class.Id.None && a2.Class != Class.Id.None)
                {
                    return a1.Class > a2.Class ? 1 : -1;
                }

                // Last resort is sort by the order the achievement was made
                return a1.AchievementId > a2.AchievementId ? 1 : -1;
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
