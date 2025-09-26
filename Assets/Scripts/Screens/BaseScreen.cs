using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BaseScreen : MonoBehaviour
{
    [SerializeField]
    private GameObject Container;
    
    [SerializeField]
    protected GameObject MainContentRoot;
    
    [SerializeField]
    private Button CloseButton;

    [Tooltip("The button that is triggered if Escape key is pressed")]
    [SerializeField]
    private Button EscapeButton;
    
    private CanvasGroup CanvasGroup;
    
    protected virtual void Awake()
    {
        CloseButton?.onClick.AddListener(OnCloseButtonClicked);
        CanvasGroup = GetComponent<CanvasGroup>();
    }

    protected virtual void OnCloseButtonClicked()
    {
        ServiceLocator.Instance.OverlayScreenManager.HideActiveScreen();
    }

    // TODO: Add animations and shit
    public void Show()
    {
        Container.SetActive(true);
        MainContentRoot.transform.localScale = Vector3.zero;
        MainContentRoot.transform.DOScale(Vector3.one, 0.25f);
        CanvasGroup.DOFade(1, 0.25f).From(0.0f);
        
        OnShow();
    }

    protected virtual void OnShow()
    {
    }

    public void Hide()
    {
        StartCoroutine(HideHelper());
        OnHide();
    }

    private IEnumerator HideHelper()
    {
        OverlayScreenManager.Instance.inputBlocker.SetActive(true);
        yield return CanvasGroup.DOFade(0.0f, 0.25f).From(1.0f).WaitForCompletion();
        OverlayScreenManager.Instance.inputBlocker.SetActive(false);
        Container.SetActive(false);
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
