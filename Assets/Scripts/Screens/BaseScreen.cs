using UnityEngine;
using UnityEngine.UI;

public class BaseScreen : MonoBehaviour
{
    [SerializeField]
    private GameObject Container;

    [SerializeField]
    private Button CloseButton;

    [Tooltip("The button that is triggered if Escape key is pressed")]
    [SerializeField]
    private Button EscapeButton;

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

    /// <summary>
    /// Method called if a generic "Escape" is asked for.
    /// </summary>
    public virtual void EscapeOut()
    {
        if (EscapeButton != null)
        {
            EscapeButton.onClick.Invoke();
        }
    }
}
