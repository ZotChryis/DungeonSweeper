using System;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private TutorialManager.TutorialId Id;
    [SerializeField] private Button Button;
    [SerializeField] private RectTransform Content;
    
    public Action<TutorialManager.TutorialId> OnCompleted;

    private void Awake()
    {
        Button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        OnCompleted?.Invoke(Id);
        gameObject.SetActive(false);
    }

    public void SetFocus(RectTransform focus)
    {
        var canvas = GetComponentInParent<Canvas>();
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, focus.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPoint
        );

        Content.localPosition = localPoint + new Vector2(0, Content.rect.height / 2 + focus.rect.height / 2 + 5);
    }
}
