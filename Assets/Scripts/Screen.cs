using UnityEngine;
using UnityEngine.UI;

public class Screen : MonoBehaviour
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
    }

    public void Hide()
    {
        Container.SetActive(false);
    }
}
