using System;
using Schemas;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ToastItem : MonoBehaviour
    {
        [SerializeField] private Image Icon;
        [SerializeField] private TMP_Text Title;
        [SerializeField] private TMP_Text Message;
        [SerializeField] private Animator Animator;

        private Action OnAnimationEnded;
        
        /// <summary>
        /// Called by Unity through event system.
        /// </summary>
        private void OnAnimationEnd()
        {
            OnAnimationEnded?.Invoke();
        }

        public void SetData(Sprite sprite, string title, string message, Action onAnimationEnded)
        {
            Icon.sprite = sprite;
            Title.text = title;
            Message.text = message;
            Icon.gameObject.SetActive(sprite != null);

            
            OnAnimationEnded += onAnimationEnded;
        }
    }
}
