using System;
using Schemas;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ToastItem : MonoBehaviour, IPointerDownHandler
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
            Icon.transform.parent.gameObject.SetActive(sprite != null);

            
            OnAnimationEnded += onAnimationEnded;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // If the toast is pressed, we dismiss early
            OnAnimationEnded?.Invoke();
        }
    }
}
