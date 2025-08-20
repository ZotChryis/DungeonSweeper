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
            RequestToast(newItem.Schema.Sprite, "Item Granted!", newItem.Schema.Name);
        }

        private void OnItemChargeChanged((ItemInstance, int) data)
        {
            if (data.Item2 > 0)
            {
                RequestToast(data.Item1.Schema.Sprite, "Item Granted!", data.Item1.Schema.Name);
            }
        }
        
        private void OnAchievementCompleted(AchievementSchema newAchievement)
        {
            RequestToast(null, "Achievement Unlocked!",newAchievement.Title);

            if (newAchievement.RewardClass != Class.Id.None)
            {
                var classSchema = ServiceLocator.Instance.Schemas.ClassSchemas.Find(c => c.Id == newAchievement.RewardClass);
                RequestToast(classSchema.Sprite, "Class Unlocked!", classSchema.Name);
            }
            
            if (newAchievement.RewardItem != ItemSchema.Id.None)
            {
                var itemSchema = ServiceLocator.Instance.Schemas.ItemSchemas.Find(i => i.ItemId == newAchievement.RewardItem);
                RequestToast(itemSchema.Sprite, "Item Unlocked!", itemSchema.Name + " can now appear in the shop!");
            }
        }

        public void RequestToast(Sprite sprite, string title, string message)
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
                Icon = sprite
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
            CurrentToast.SetData(dt.Icon, dt.Title, dt.Description, OnToastCompleted);
        }

        private void OnToastCompleted()
        {
            Destroy(CurrentToast.gameObject);
            CurrentToast = null;
            TryContinue();
        }
    }
}
