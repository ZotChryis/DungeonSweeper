using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    [SerializeField] public TutorialManager.TutorialId Id;
    [SerializeField] private Button Button;
    [SerializeField] private RectTransform Content;
    private Button buttonInContent;

    public Action<TutorialManager.TutorialId> OnCompleted;

    private void Awake()
    {
        Button.onClick.AddListener(OnButtonClicked);
        buttonInContent = Content.GetComponentInChildren<Button>();
        if (buttonInContent)
        {
            buttonInContent.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        CompleteTutorial();
    }

    public void CompleteTutorial()
    {
        TutorialManager.Instance.FocusObject.SetActive(false);
        TutorialManager.Instance.FocusDontForceClick.SetActive(false);
        OnCompleted?.Invoke(Id);
        gameObject.SetActive(false);
    }

    public void SetFocus(RectTransform focus, bool forcePlayerToClickFocus)
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
        Debug.Log("Translating recttransform global position: " + focus.position + " into " + localPoint + " screen point: " + screenPoint);

        if (forcePlayerToClickFocus)
        {
            TutorialManager.Instance.FocusObject.GetComponent<FocusBox>().SetPositionAndShow(localPoint, focus, Screen.width / 2f > screenPoint.x);

            if (buttonInContent)
            {
                buttonInContent.enabled = false;
            }
        }
        else
        {
            TutorialManager.Instance.FocusDontForceClick.GetComponent<FocusBox>().SetPositionAndShow(localPoint, focus, false);
        }

        // Disable the default close tutorial bg buttons.
        // Focus will control the bg and the player must click what is focused on.
        Button.gameObject.SetActive(false);
    }
}
