#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using Schemas;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

namespace Gameplay
{
    [System.Serializable]
    public class RunData
    {
        public int dungeonLevel;
        public Class.Id playerClass;
        public List<ItemSchema.Id> items = new List<ItemSchema.Id>();
        public int shopXp;
        public bool canGetAchievements;
        public bool isHardcore;
        public ChallengeSchema.Id currentChallenge;
    }

    public class SaveSystem
    {
        private const string c_saveFilePath = "DungeonSweeperRunSave.txt";

        private string _saveFilePath;

        public static string GetSaveFilePath()
        {
            string path = Application.persistentDataPath;

#if PLATFORM_WEBGL
            path = "idbfs/DungeonSweeper";
#endif

#if !DISABLESTEAMWORKS
            string steamPath;
            try
            {
                steamPath = SteamUser.GetSteamID().ToString(); // {64BitSteamID}
                path += "/" + steamPath;
            }
            catch
            {
                // Do nothing. Error usually caused by windows standalone not run from steam.
            }
#endif

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }
    
        public SaveSystem()
        {
            string path = GetSaveFilePath();
            Debug.Log("Saving player save/load to : " + path + " filename: player_data.json");
            _saveFilePath = Path.Combine(path, "player_data.json");
        }

        /// <summary>
        /// Called when starting a dungeon level.
        /// </summary>
        public void SaveGame()
        {
            RunData data = new RunData();

            data.dungeonLevel = ServiceLocator.Instance.LevelManager.CurrentLevel;
            data.playerClass = ServiceLocator.Instance.Player.Class;
            data.shopXp = ServiceLocator.Instance.Player.ShopXp;
            data.canGetAchievements = ServiceLocator.Instance.AchievementSystem.AllowAchievementsToBeCompleted;
            data.isHardcore = ServiceLocator.Instance.Player.IsHardcore;
            data.currentChallenge = ServiceLocator.Instance.ChallengeSystem.CurrentChallenge 
                ? ServiceLocator.Instance.ChallengeSystem.CurrentChallenge.ChallengeId 
                : ChallengeSchema.Id.None;
            
            foreach (ItemInstance itemInstance in ServiceLocator.Instance.Player.Inventory.GetAllItems())
            {
                for (int i = 0; i < itemInstance.StackCount; i++)
                {
                    data.items.Add(itemInstance.Schema.ItemId);
                }
            }

            string json = JsonUtility.ToJson(data);
            File.WriteAllText(_saveFilePath, json);
        }

        /// <summary>
        /// HACK ALERT !!!
        /// This tries to recreate the state of the dungeon at a specific level with player items 're-added' back.
        /// This means we are limited to what we can and cannot do mid-rounds.
        /// Anything an item does will always be re-done when added back, so rely on Purchase trigger and not much else.
        /// </summary>
        public void LoadGame()
        {
            if (File.Exists(_saveFilePath))
            {
                string json = File.ReadAllText(_saveFilePath);
                RunData data = JsonUtility.FromJson<RunData>(json);

                // Set the challenge if it was there
                if (data.currentChallenge != ChallengeSchema.Id.None)
                {
                    ServiceLocator.Instance.ChallengeSystem.SelectedChallenge = ServiceLocator.Instance.Schemas.ChallengeSchemas.Find(c => c.ChallengeId == data.currentChallenge);
                    ServiceLocator.Instance.ChallengeSystem.Commit();
                }
                
                // First set the level -- this is important to do first so that we set spawn settings
                ServiceLocator.Instance.LevelManager.SetLevel(data.dungeonLevel);
                
                // Reset the player to be as close as vanilla as possible
                ServiceLocator.Instance.Player.ResetPlayer();
                
                // Sets the class info but importantly DO NOT grant the item, because...
                ServiceLocator.Instance.Player.TEMP_SetClass(data.playerClass, false);
                
                // We clear all player items that might be lingering, and re-add the serialized items
                // HACK: SuppressAllToasts because I hated seeing all the items being added on a load
                ServiceLocator.Instance.ToastManager.SuppressAllToasts = true;
                ServiceLocator.Instance.Player.SuppressAllCoins = true;
                ServiceLocator.Instance.Player.Inventory.Clear();
                foreach (var item in data.items)
                {
                    ServiceLocator.Instance.Player.Inventory.AddItem(item);
                }
                ServiceLocator.Instance.ToastManager.SuppressAllToasts = false;
                ServiceLocator.Instance.Player.SuppressAllCoins = false;
                
                // Regenerate grid because items may effect generation
                ServiceLocator.Instance.Grid.GenerateGrid();
                
                // Set player money AFTER items have been added -- this should handle cash generation items
                ServiceLocator.Instance.Player.ShopXp = data.shopXp;
                
                // Set important booleans after all is said and done
                ServiceLocator.Instance.Player.IsHardcore = data.isHardcore;
                ServiceLocator.Instance.AchievementSystem.AllowAchievementsToBeCompleted = data.canGetAchievements;
            }
            else
            {
                Debug.LogWarning("No save file found!");
            }
        }

        public void WipeRun()
        {
            File.Delete(_saveFilePath);
        }

        public bool HasSave()
        {
            return File.Exists(_saveFilePath);
        }
    }
}