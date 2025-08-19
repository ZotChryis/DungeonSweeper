using System.Collections.Generic;
using System.IO;
using Schemas;
using UnityEngine;

namespace Gameplay
{
    [System.Serializable]
    public class RunData
    {
        public int dungeonLevel;
        public Class.Id playerClass;
        public List<ItemSchema.Id> items = new List<ItemSchema.Id>();
        public int shopXp;
    }

    public class SaveSystem
    {
        private const string c_saveFilePath = "DungeonSweeperRunSave.txt";

        private string _saveFilePath;
    
        public SaveSystem()
        {
            string path = Application.persistentDataPath;
#if PLATFORM_WEBGL
            path = "idbfs/DungeonSweeper";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
#endif
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

        public void LoadGame()
        {
            if (File.Exists(_saveFilePath))
            {
                string json = File.ReadAllText(_saveFilePath);
                RunData data = JsonUtility.FromJson<RunData>(json);

                ServiceLocator.Instance.LevelManager.SetLevel(data.dungeonLevel);
                
                ServiceLocator.Instance.Player.ResetPlayer();
                ServiceLocator.Instance.Player.TEMP_SetClass(data.playerClass, false);
                ServiceLocator.Instance.Player.ShopXp = data.shopXp;
                
                ServiceLocator.Instance.Player.Inventory.Clear();
                foreach (var item in data.items)
                {
                    ServiceLocator.Instance.Player.Inventory.AddItem(item);
                }
                
                ServiceLocator.Instance.LevelManager.RetryCurrentLevel();
            }
            else
            {
                Debug.LogWarning("No save file found!");
            }
        }

        public void Wipe()
        {
            File.Delete(_saveFilePath);
        }

        public bool HasSave()
        {
            return File.Exists(_saveFilePath);
        }
    }
}