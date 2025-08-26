using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
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
        [SerializeField] private CanvasGroup CanvasGroup;
        
        private Action OnAnimationEnded;

        
        private TweenerCore<Vector3, Vector3, VectorOptions> moveAnim;
        private TweenerCore<float, float, FloatOptions> fadeInAnim;
        private TweenerCore<float, float, FloatOptions> fadeOutAnim;
        
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

            RectTransform rectTransform = (RectTransform)transform;
            moveAnim = rectTransform.DOLocalMove(new Vector3(0, -300, 0), 1f).From(Vector3.zero, true);
            fadeInAnim = CanvasGroup.DOFade(1f, 0.5f).From(0f, true);
            fadeOutAnim = CanvasGroup.DOFade(0f, 0.25f).From(1f, true).SetDelay(1.75f).OnComplete(OnAnimationEnd);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // If the toast is pressed, we dismiss early
            moveAnim.Complete();
            fadeInAnim.Complete();
            fadeOutAnim.Complete();
        }
    }
}
