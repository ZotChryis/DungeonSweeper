﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    // DS -> DungeonSweeper
    // Extended button class to wrap our own logic. Needed this for the animated text stuff
    // We must wrap and not extend because Unity has its own special Inspector drawer which I don't want to mess with
    public class DSButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button Button;
        [SerializeField] private bool AnimateTextOnHover;
        
        // If these are supplied, we will create a confirmation dialog. If that confirmation is positive,
        // then we issue the action callback. 
        // DO NOT use the Button's actual callback system
        [SerializeField] public bool RequireConfirmation;
        [SerializeField] public bool RequireConfirmationOnce;
        [SerializeField] private string ConfirmationTitle;
        [SerializeField] private string ConfirmationMessage;

        public Action OnConfirmed;
        
        private TextAnimation[] TextAnimations;
        private bool Confirmed = false;

        private void Start()
        {
            TextAnimations = GetComponentsInChildren<TextAnimation>();
            Button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (!RequireConfirmation)
            {
                OnConfirmed?.Invoke();
                return;
            }

            if (Confirmed && RequireConfirmationOnce)
            {
                OnConfirmed?.Invoke();
                return;
            }
            
            ServiceLocator.Instance.OverlayScreenManager.RequestConfirmationScreen(() => {
                    Confirmed = true;
                    OnConfirmed?.Invoke();
                }, 
                ConfirmationTitle, 
                ConfirmationMessage
            );
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!AnimateTextOnHover)
            {
                return;
            }
            
            foreach (var anim in TextAnimations)
            {
                anim.Enabled = true;
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!AnimateTextOnHover)
            {
                return;
            }
            
            foreach (var anim in TextAnimations)
            {
                anim.Enabled = false;
                anim.Reset();
            }
        }
        
    }
}