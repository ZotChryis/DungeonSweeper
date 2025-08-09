using System;
using System.Collections.Generic;
using System.Linq;
using Schemas;
using Sirenix.Utilities;

namespace Gameplay
{
    public class AchievementSystem
    {
        public bool AllowAchievementsToBeCompleted = true;
        
        public Action<AchievementSchema> OnAchievementCompleted;

        public List<Class.Id> GetUnlockedClasses()
        {
            HashSet<Class.Id> classes = new HashSet<Class.Id>();
            
            // Adventurer is unlocked by default
            classes.Add(Class.Id.Adventurer);
            
            var achievements = ServiceLocator.Instance.Schemas.AchievementSchemas
                .FindAll(a => a.RewardClass != Class.Id.None);

            foreach (var schema in achievements)
            {
                string key = "Achievement" + schema.AchievementId;

                // Not completed
                if (!FBPP.GetBool(key))
                {
                    continue;
                }
                
                classes.Add(schema.RewardClass);
            }

            return classes.ToList();
        }

        public List<ItemSchema.Id> GetLockedItems()
        {
            List<ItemSchema.Id> items = new List<ItemSchema.Id>();
            
            // Find all the achievements with an item reward
            var achievements = ServiceLocator.Instance.Schemas.AchievementSchemas
                .FindAll(a => a.RewardItem != ItemSchema.Id.None);
            
            // The item is locked if the achievement is not complete
            foreach (var achievementSchema in achievements)
            {
                string key = "Achievement" + achievementSchema.AchievementId;
                if (!FBPP.GetBool(key))
                {
                    items.Add(achievementSchema.RewardItem);
                }
            }

            return items;
        }

        public int GetFinishedAchievementCount()
        {
            var achievements = ServiceLocator.Instance.Schemas.AchievementSchemas
                .FindAll(schema =>
                {
                    string key = "Achievement" + schema.AchievementId;
                    return FBPP.GetBool(key);
                });
            
            return achievements.Count;
        }
        
        public void CheckAchievements(AchievementSchema.TriggerType trigger)
        {
            bool allowAchievement = AllowAchievementsToBeCompleted;
            
#if UNITY_EDITOR
            // Shitty hack to allow achievement testing with cheats
            allowAchievement = true;
#endif
            if (!allowAchievement)
            {
                return;
            }
            
            var achievements = ServiceLocator.Instance.Schemas.AchievementSchemas
                .FindAll(a => a.Trigger == trigger);

            // TODO FIX HACK: Fix this later -- the crown is still on the board when we transition dungeons, but we shouldnt care about it
            //  in out calculations.
            // TODO FIX HACK: We also can't kill Balrog so we must remove it from this so that annihilator works
            var tileObjects = ServiceLocator.Instance.Grid.GetAllTileObjects()
                .FindAll(t => t.TileId != TileSchema.Id.Crown && t.TileId != TileSchema.Id.Balrog);
            
            foreach (var schema in achievements)
            {
                if (schema.Level != -1 && schema.Level != ServiceLocator.Instance.LevelManager.CurrentLevel)
                {
                    continue;
                }

                if (schema.RequiresHardcore && !ServiceLocator.Instance.Player.IsHardcore)
                {
                    continue;
                }
                
                switch (trigger)
                {
                    case AchievementSchema.TriggerType.Victory:
                        if (schema.Class != Class.Id.None && schema.Class != ServiceLocator.Instance.Player.Class)
                        {
                            continue;
                        }

                        if (schema.ItemAcquiredMaxCount > -1 && schema.ItemAcquiredMaxCount < ServiceLocator.Instance.Player.Inventory.GetAllItems().Count)
                        {
                            continue;
                        }

                        if (schema.ItemAcquiredMinCount > -1 && schema.ItemAcquiredMinCount > ServiceLocator.Instance.Player.Inventory.GetAllItems().Count)
                        {
                            continue;
                        }

                        Complete(schema);
                        break;
                    
                    case AchievementSchema.TriggerType.Pacifist:
                        int kills = 0;
                        foreach (var id in schema.TileData)
                        {
                            kills += ServiceLocator.Instance.Player.GetKillCount(id);
                        }

                        if (kills > 0)
                        {
                            continue;
                        }
                        
                        Complete(schema);
                        break;
                    
                    case AchievementSchema.TriggerType.FullBoardClear:
                        if (tileObjects.Count > 0)
                        {
                            continue;
                        }
                        
                        Complete(schema);
                        break;
                    
                    // This is bespoke sent by Tile.cs when the tileId == Balrog
                    case AchievementSchema.TriggerType.DemonLord:
                        Complete(schema);
                        break;
                }
            }
        }

        private void Complete(AchievementSchema schema)
        {
            string key = "Achievement" + schema.AchievementId;

            // Already completed
            if (FBPP.GetBool(key))
            {
                return;
            }
            
            FBPP.SetBool(key, true);
            FBPP.SetString(key, DateTime.Today.ToString("d"));
            
            OnAchievementCompleted?.Invoke(schema);
            ServiceLocator.Instance.AudioManager.PlaySfx("Achievement");
        }
    }
}