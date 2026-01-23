#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

using Schemas;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
                // Not completed
                if (!schema.AchievementId.IsAchieved())
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
                if (!achievementSchema.AchievementId.IsAchieved())
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
                    return schema.AchievementId.IsAchieved();
                });

            if (!ServiceLocator.Instance.IsPaidVersion())
            {
                achievements.RemoveAll(a => a.PaidExclusive);
            }

            return achievements.Count;
        }

        public void CompleteAchievementById(AchievementSchema.Id AchivementId)
        {
            var achievement = ServiceLocator.Instance.Schemas.AchievementSchemas
                .Find(a => a.AchievementId == AchivementId);

            if (achievement)
            {
                Complete(achievement);
            }
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

                int kills = 0;
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
                        kills = 0;
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

                    case AchievementSchema.TriggerType.Killer:
                        kills = 0;
                        foreach (var id in schema.TileData)
                        {
                            kills += ServiceLocator.Instance.Player.GetKillCount(id);
                        }

                        if (kills < schema.MinimumKillCount)
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
        
        private bool IsClassLevel0Achievement(AchievementSchema achievement)
        {
            return achievement.Class != Class.Id.None && achievement.Level == 0;
        }

        private void Complete(AchievementSchema schema)
        {
            // Already completed
            if (schema.AchievementId.IsAchieved())
            {
                return;
            }
            
            // Check the flag for achievements during challenges
            if (ServiceLocator.Instance.ChallengeSystem.CurrentChallenge != null && !schema.CanBeCompletedDuringChallenges)
            {
                return;
            }
            
            // Any of the "level 0" achievements should unlock challenges
            var challengeSystem = ServiceLocator.Instance.ChallengeSystem;
            if (!challengeSystem.AreChallengesUnlocked() && IsClassLevel0Achievement(schema))
            {
                challengeSystem.UnlockChallenges();
                ServiceLocator.Instance.ToastManager.RequestToast(null, "Challenges Unlocked!", "Access special challenge runs from the main menu!", 3.0f);
            }

            // Some achievements are paid exclusive achievements. You can't complete them unless you pay.
            if (schema.PaidExclusive && !ServiceLocator.Instance.IsPaidVersion())
            {
                return;
            }

            string key = "Achievement" + schema.AchievementId;

            FBPP.SetBool(key, true);
            FBPP.SetString(key, DateTime.Today.ToString("d"));

            OnAchievementCompleted?.Invoke(schema);
            ServiceLocator.Instance.AudioManager.PlaySfx("Achievement");

#if !DISABLESTEAMWORKS
            if (ServiceLocator.IsSteamDemo)
            {
                SaveSteamAchievementForLater(schema);
            }
            else
            {
                ServiceLocator.Instance.SteamStatsAndAchievements.UnlockAchievement(schema.AchievementId);
            }
#endif
        }
        
        private const string steamDemoString = "SteamDemoAchievements";
        /// <summary>
        /// get already existing saved achievementIds as a string and possibly append ","
        /// add new param schema id.
        /// Save.
        /// </summary>
        /// <param name="schema"></param>
        private void SaveSteamAchievementForLater(AchievementSchema schema)
        {
            string alreadySavedSteamAchievements = FBPP.GetString(steamDemoString, "");
            string newSteamAchievements;
            if (!string.IsNullOrEmpty(alreadySavedSteamAchievements))
            {
                newSteamAchievements = alreadySavedSteamAchievements + "," + schema.AchievementId.ToString();
            }
            else
            {
                newSteamAchievements = schema.AchievementId.ToString();
            }
            FBPP.SetString(steamDemoString, newSteamAchievements);
        }

        /// <summary>
        /// Award saved steam demo achievements.
        /// </summary>
        public void AwardSteamDemoAchievements()
        {
#if !DISABLESTEAMWORKS
            string alreadySavedSteamAchievements = FBPP.GetString(steamDemoString, "");
            if (!string.IsNullOrEmpty(alreadySavedSteamAchievements))
            {
                string[] achievements = alreadySavedSteamAchievements.Split(',');
                foreach (string achievement in achievements)
                {
                    if (Enum.TryParse<AchievementSchema.Id>(achievement, out AchievementSchema.Id result))
                    {
                        ServiceLocator.Instance.SteamStatsAndAchievements.UnlockAchievement(result);
                    }
                }
                FBPP.DeleteString(steamDemoString);
            }
#endif
        }

        public bool IsAnyClassLevel0AchievementCompleted()
        {
            foreach (var s in ServiceLocator.Instance.Schemas.AchievementSchemas)
            {
                if (IsClassLevel0Achievement(s) && s.AchievementId.IsAchieved())
                {
                    return true;
                }
            }

            return false;
        }
    }
}