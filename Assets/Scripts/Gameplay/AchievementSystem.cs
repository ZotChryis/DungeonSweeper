using System;
using System.Collections.Generic;
using System.Linq;
using Schemas;

namespace Gameplay
{
    public class AchievementSystem
    {
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
        
        public void CheckAchievements(AchievementSchema.TriggerType trigger)
        {
            var achievements = ServiceLocator.Instance.Schemas.AchievementSchemas
                .FindAll(a => a.Trigger == trigger);

            // TODO FIX HACK: Fix this later -- the crown is still on the board when we transition dungeons, but we shouldnt care about it
            //  in out calculations
            var tileObjects = ServiceLocator.Instance.Grid.GetAllTileObjects()
                .FindAll(t => t.TileId != TileSchema.Id.Crown);
            
            foreach (var schema in achievements)
            {
                if (schema.Level != -1 && schema.Level != ServiceLocator.Instance.LevelManager.CurrentLevel)
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
        }
    }
}