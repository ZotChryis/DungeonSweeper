using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BaseScreen : MonoBehaviour
{
    [SerializeField]
    private GameObject Container;
    
    [SerializeField]
    private GameObject MainContentRoot;
    
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
        MainContentRoot.transform.localScale = Vector3.zero;
        MainContentRoot.transform.DOScale(Vector3.one, 0.25f);
        
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
        yield return MainContentRoot.transform.DOScale(Vector3.zero, 0.25f).WaitForCompletion();
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
