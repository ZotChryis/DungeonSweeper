using UnityEngine;
using UnityEngine.EventSystems;

public class CloseScreenOnMiddleClick : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            //ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.TileContextMenu);
            ServiceLocator.Instance.OverlayScreenManager.HideActiveScreen();
        }
    }
}
