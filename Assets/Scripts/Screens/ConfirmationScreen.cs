using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Screens
{
    public class ConfirmationScreen : BaseScreen
    {
        [SerializeField] private TMP_Text Title;
        [SerializeField] private TMP_Text Message;
        [SerializeField] private Button Yes;
        [SerializeField] private Button No;

        private Action Callback;
        
        protected override void Awake()
        {
            Yes.onClick.AddListener(OnYesClicked);
            No.onClick.AddListener(OnNoClicked);
        }

        public void SetData(Action callback, string title, string message)
        {
            Title.SetText(title);
            Message.SetText(message);
            Callback = callback;
        }
        
        private void OnYesClicked()
        {
            ServiceLocator.Instance.OverlayScreenManager.HideActiveScreen();
            Callback?.Invoke();
        }
        
        private void OnNoClicked()
        {
            ServiceLocator.Instance.OverlayScreenManager.HideActiveScreen();
            Callback = null;
            Yes.onClick.RemoveAllListeners();
        }
    }
}
