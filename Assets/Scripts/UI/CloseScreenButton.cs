using UnityEngine;
using UnityEngine.UI;

public class CloseScreenButton : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>()?.onClick.AddListener(OnCloseButtonClicked);
    }

    private void OnCloseButtonClicked()
    {
        ServiceLocator.Instance.OverlayScreenManager.HideActiveScreen();
    }
}
