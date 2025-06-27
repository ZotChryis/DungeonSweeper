using UnityEngine;
using UnityEngine.UI;

public class BaseScreen : MonoBehaviour
{
    [SerializeField]
    private GameObject Container;

    [SerializeField]
    private Button CloseButton;

    protected virtual void Awake()
    {
        CloseButton?.onClick.AddListener(OnCloseButtonClicked);
    }

    protected virtual void OnCloseButtonClicked()
    {
        ServiceLocator.Instance.OverlayScreenManager.HideActiveScreen();
    }

    // TODO: Add animations and shit
    public void Show()
    {
        Container.SetActive(true);
        OnShow();
    }

    protected virtual void OnShow()
    {
    }

    public void Hide()
    {
        Container.SetActive(false);
        OnHide();
    }
    
    protected virtual void OnHide()
    {
    }
}
