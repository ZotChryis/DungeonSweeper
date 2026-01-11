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
        [SerializeField] private AchievementItem ChainedAchievementPrefab;
        [SerializeField] private TMP_Text Title;
        [SerializeField] private Transform ContentRoot;

        private List<AchievementItem> Items;
        
        // The key is the ID of the first schema in the chain, the value is the entire chain (including the first)
        private readonly Dictionary<AchievementSchema.Id, List<AchievementSchema>> _chainedAchievements = new Dictionary<AchievementSchema.Id, List<AchievementSchema>>();
        
        private void Start()
        {
            // Build a cache of the chains for future use
            List<List<AchievementSchema.Id>> chainedAchievements = AchievementSchema.ChainedAchievements;
            foreach (var chainedAchievement in chainedAchievements)
            {
                List<AchievementSchema> achievements = new List<AchievementSchema>();
                foreach (var item in chainedAchievement)
                {
                    var achievement = ServiceLocator.Instance.Schemas.AchievementSchemas.Find(a => a.AchievementId == item);
                    achievements.Add(achievement);
                }
                
                _chainedAchievements.Add(chainedAchievement[0], achievements);
            }
            
            RefreshItems();

            ServiceLocator.Instance.AchievementSystem.OnAchievementCompleted += OnAchievementCompleted;
        }

        private void OnDestroy()
        {
            ServiceLocator.Instance.AchievementSystem.OnAchievementCompleted -= OnAchievementCompleted;
        }
        
        private void RefreshItems()
        {
            HashSet<AchievementSchema.Id> servicedIds = new HashSet<AchievementSchema.Id>();
            
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
                // If the two achievements share the same class completion requirement, go by ID
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
                // We already did this one (prob a chained achievement)
                if (servicedIds.Contains(schema.AchievementId))
                {
                    continue;
                }
                
                // Check for any chain starting with this achievement. If not, just add it and move on.
                if (!_chainedAchievements.ContainsKey(schema.AchievementId))
                {
                    AchievementItem item = Instantiate<AchievementItem>(AchievementPrefab, ContentRoot);
                    item.SetSchema(schema);
                    Items.Add(item);
                    servicedIds.Add(schema.AchievementId);
                    continue;
                }
                
                // Otherwise, we add a special version of the achievement and control it there
                AchievementItem chainedItem = Instantiate<AchievementItem>(ChainedAchievementPrefab, ContentRoot);
                chainedItem.SetSchemas(_chainedAchievements[schema.AchievementId]);
                foreach (var achievementSchema in _chainedAchievements[schema.AchievementId])
                {
                    servicedIds.Add(achievementSchema.AchievementId);
                }
            }
            
            int completed = ServiceLocator.Instance.AchievementSystem.GetFinishedAchievementCount();
            string text = $"ACHIEVEMENTS ({completed}/{servicedIds.Count})";
            Title.SetText(text);
        }

        private void OnAchievementCompleted(AchievementSchema obj)
        {
            RefreshItems();
        }
    }
}
