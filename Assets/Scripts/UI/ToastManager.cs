using System;
using System.Collections.Generic;
using Gameplay;
using Schemas;
using UnityEngine;

namespace UI
{
    public class ToastManager : SingletonMonoBehaviour<ToastManager>
    {
        private struct ToastData
        {
            public string Title;
            public string Description;
            public Sprite Icon;
            public float StickTime;
        }
        
        [SerializeField] private ToastItem ToastPrefab;
        [SerializeField] private Transform ToastRoot;
        
        private Queue<ToastData> Requests = new();
        private ToastItem CurrentToast;
        public bool SuppressAllToasts = false;

        private void Start()
        {
            ServiceLocator.Instance.Register(this);

            ServiceLocator.Instance.Player.Inventory.OnItemAdded += OnItemAdded;
            ServiceLocator.Instance.Player.Inventory.OnItemStackChanged += OnItemChargeChanged;
            ServiceLocator.Instance.AchievementSystem.OnAchievementCompleted += OnAchievementCompleted;
        }

        private void OnDestroy()
        {
            ServiceLocator.Instance.Player.Inventory.OnItemAdded -= OnItemAdded;
            ServiceLocator.Instance.AchievementSystem.OnAchievementCompleted -= OnAchievementCompleted;
        }

        private void OnItemAdded(ItemInstance newItem)
        {
            if (IsItemBannedFromToasts(newItem))
            {
                return;
            }
            
            RequestToast(newItem.Schema.Sprite, "Item Granted!", newItem.Schema.Name);
        }
        
        private void OnItemChargeChanged((ItemInstance, int) data)
        {
            if (IsItemBannedFromToasts(data.Item1))
            {
                return;
            }
            
            if (data.Item2 > 0)
            {
                RequestToast(data.Item1.Schema.Sprite, "Item Granted!", data.Item1.Schema.Name);
            }
        }
        
        private bool IsItemBannedFromToasts(ItemInstance newItem)
        {
            // Coins do not get a toast, they now appear in-scene next to the player
            if (newItem.Schema.ItemId == ItemSchema.Id.CoinCopper ||
                newItem.Schema.ItemId == ItemSchema.Id.CoinSilver ||
                newItem.Schema.ItemId == ItemSchema.Id.CoinGold
               )
            {
                return true;
            }

            return false;
        }

        
        private void OnAchievementCompleted(AchievementSchema newAchievement)
        {
            RequestToast(null, "Achievement Unlocked!",newAchievement.Title);

            if (newAchievement.RewardClass != Class.Id.None)
            {
                bool skipRewardClass = false;
                if (newAchievement.RewardClass == Class.Id.Ranger ||
                    newAchievement.RewardClass == Class.Id.Warrior ||
                    newAchievement.RewardClass == Class.Id.Wizard ||
                    newAchievement.RewardClass == Class.Id.Ascetic)
                {
                    var achievements = ServiceLocator.Instance.Schemas.AchievementSchemas
                        .FindAll(a => a.RewardClass != Class.Id.None);
                    foreach (var schema in achievements)
                    {
                        // Not completed
                        if (schema.AchievementId.IsAchieved() && schema.AchievementId != newAchievement.AchievementId)
                        {
                            skipRewardClass = true;
                            break;
                        }
                    }
                }

                if (!skipRewardClass)
                {
                    var classSchema = ServiceLocator.Instance.Schemas.ClassSchemas.Find(c => c.Id == newAchievement.RewardClass);
                    RequestToast(classSchema.Sprite, "Class Unlocked!", classSchema.Name);
                }
            }
            
            if (newAchievement.RewardItem != ItemSchema.Id.None)
            {
                var itemSchema = ServiceLocator.Instance.Schemas.ItemSchemas.Find(i => i.ItemId == newAchievement.RewardItem);
                RequestToast(itemSchema.Sprite, "Item Unlocked!", itemSchema.Name + " can now appear in the shop!");
            }
        }

        public void RequestToast(Sprite sprite, string title, string message, float stickTime = 1.75f)
        {
            if (SuppressAllToasts)
            {
                Debug.Log($"Toast was suppressed: {title}");
                return;
            }
            
            ToastData td = new ToastData()
            {
                Title = title,
                Description = message,
                Icon = sprite,
                StickTime = stickTime
            };
            
            Requests.Enqueue(td);
            TryContinue();
        }

        private void TryContinue()
        {
            if (CurrentToast || Requests.Count == 0)
            {
                return;
            }
            
            CurrentToast = Instantiate(ToastPrefab, ToastRoot);
            
            var dt = Requests.Dequeue();
            CurrentToast.SetData(dt.Icon, dt.Title, dt.Description, dt.StickTime, OnToastCompleted);
        }

        private void OnToastCompleted()
        {
            Destroy(CurrentToast.gameObject);
            CurrentToast = null;
            TryContinue();
        }
    }
}
