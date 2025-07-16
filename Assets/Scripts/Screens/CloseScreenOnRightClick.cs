using UnityEngine;
using UnityEngine.EventSystems;

public class CloseScreenOnRightClick : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            //ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.TileContextMenu);
            ServiceLocator.Instance.OverlayScreenManager.HideActiveScreen();
        }
    }
}
