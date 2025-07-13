using System;
using System.Collections.Generic;
using Gameplay;
using Schemas;
using UnityEngine;

namespace UI
{
    public class ToastManager : SingletonMonoBehaviour<ToastManager>
    {
        [SerializeField] private ToastItem ToastPrefab;
        [SerializeField] private Transform ToastRoot;

        private Queue<Schema> Requests = new();
        private ToastItem CurrentToast;
        
        private void Start()
        {
            ServiceLocator.Instance.Register(this);

            ServiceLocator.Instance.Player.Inventory.OnItemAdded += OnItemAdded;
            ServiceLocator.Instance.AchievementSystem.OnAchievementCompleted += OnAchievementCompleted;
        }

        private void OnDestroy()
        {
            ServiceLocator.Instance.Player.Inventory.OnItemAdded -= OnItemAdded;
            ServiceLocator.Instance.AchievementSystem.OnAchievementCompleted -= OnAchievementCompleted;
        }

        private void OnItemAdded(ItemInstance newItem)
        {
            RequestToast(newItem.Schema);
        }
        
        private void OnAchievementCompleted(AchievementSchema newAchievement)
        {
            RequestToast(newAchievement);
        }

        /// <summary>
        /// We only support Item and Achievement for now.
        /// TODO: Prob should just be completely explicit and allow all icon/text combos. Meh
        /// </summary>
        /// <param name="schema"></param>
        public void RequestToast(Schema schema)
        {
            if (schema is not AchievementSchema && schema is not ItemSchema)
            {
                return;
            }
            
            Requests.Enqueue(schema);
            TryContinue();
        }

        private void TryContinue()
        {
            if (CurrentToast || Requests.Count == 0)
            {
                return;
            }
            
            CurrentToast = Instantiate(ToastPrefab, ToastRoot);
            
            Schema schema = Requests.Dequeue();
            if (schema is AchievementSchema achievementSchema)
            {
                CurrentToast.SetData(null, "Achievement Unlocked!", achievementSchema.Title, OnToastCompleted);
            }

            if (schema is ItemSchema itemSchema)
            {
                CurrentToast.SetData(itemSchema.Sprite, "Item Granted!", itemSchema.Name, OnToastCompleted);
            }
        }

        private void OnToastCompleted()
        {
            Destroy(CurrentToast.gameObject);
            CurrentToast = null;
            TryContinue();
        }
    }
}
