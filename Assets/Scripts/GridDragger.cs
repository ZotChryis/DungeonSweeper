using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridDragger : SingletonMonoBehaviour<GridDragger>, IDragHandler
{
    [SerializeField] private Canvas Canvas;
    [SerializeField] private GraphicRaycaster Raycaster;


    [SerializeField] private RectTransform Left;
    [SerializeField] private RectTransform Right;
    
    private Vector3 StartDragPosition;
    private bool IsValidDrag = false;

    public Action OnValidDrag;
    private RectTransform GridRect;

    private Vector2 LeftWall;
    private Vector2 RightWall;

    protected override void Awake()
    {
        base.Awake();
        
        ServiceLocator.Instance.Register(this);
        ServiceLocator.Instance.OverlayScreenManager.OnScrenShown += OnScreenShown;
        
        GridRect = ServiceLocator.Instance.Grid.transform as RectTransform;
        
        Vector3[] lCorners = new Vector3[4];
        Left.GetWorldCorners(lCorners);
        LeftWall = lCorners[0];
        
        Vector3[] rCorners = new Vector3[4];
        Right.GetWorldCorners(rCorners);
        RightWall = rCorners[3];
    }

    private void OnDestroy()
    {
        ServiceLocator.Instance.OverlayScreenManager.OnScrenShown -= OnScreenShown;
    }

    private void OnScreenShown(OverlayScreenManager.ScreenType screen)
    {
        IsValidDrag = false;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 amount = eventData.delta / Canvas.scaleFactor;
        amount.y = 0;

        RectTransform bottomLeft = ServiceLocator.Instance.Grid.GetBottomLeft();
        RectTransform topRight = ServiceLocator.Instance.Grid.GetTopRight();

        // Apply movement first
        GridRect.anchoredPosition += amount;

        Vector3[] corners = new Vector3[4];

        // Bottom Left corner screen position
        bottomLeft.GetWorldCorners(corners);
        Vector2 bottomLeftScreen = RectTransformUtility.WorldToScreenPoint(
            Canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Canvas.worldCamera,
            corners[0]);

        // Top Right corner screen position
        topRight.GetWorldCorners(corners);
        Vector2 topRightScreen = RectTransformUtility.WorldToScreenPoint(
            Canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Canvas.worldCamera,
            corners[3]);

        // LeftWall and RightWall screen positions
        Vector2 leftWallScreen = RectTransformUtility.WorldToScreenPoint(
            Canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Canvas.worldCamera,
            LeftWall);

        Vector2 rightWallScreen = RectTransformUtility.WorldToScreenPoint(
            Canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Canvas.worldCamera,
            RightWall);

        // If bottom left corner went past left wall, revert move
        if (bottomLeftScreen.x > leftWallScreen.x)
        {
            GridRect.anchoredPosition -= amount;
            return;
        }

        // If top right corner went past right wall, revert move
        if (topRightScreen.x < rightWallScreen.x)
        {
            GridRect.anchoredPosition -= amount;
            return;
        }
    }

    // TODO: I was trying to allow dragging over the Tile buttons but it needed for us to pass
    // a lot of events through the raycaster, and also to do logical disabling of the context menu...
    // I deemed it too much hassle but left the shitty code i wrote for it
    /*
    private void ForwardEvent<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> handler)
        where T : IEventSystemHandler
    {
        PointerEventData newEventData = new PointerEventData(EventSystem.current);
        newEventData.position = eventData.position;

        List<RaycastResult> results = new List<RaycastResult>();
        Raycaster.Raycast(newEventData, results);

        bool skipSelf = false;

        foreach (var result in results)
        {
            if (!skipSelf)
            {
                if (result.gameObject == gameObject)
                {
                    skipSelf = true;
                    continue;
                }
            }
            else
            {
                // We are simply trying to forward the event to the button...try to find it
                GameObject button = FindButtonObject(result.gameObject); 
                if (button != null && ExecuteEvents.CanHandleEvent<T>(button))
                {
                    ExecuteEvents.Execute(button, newEventData, handler);
                    break;
                }
            }
        }
    }
    
    GameObject FindButtonObject(GameObject go)
    {
        GameObject current = go;
        while (current != null)
        {
            if (current.GetComponent<Button>() != null)
            {
                return current;
            }

            if (current.transform.parent == null)
            {
                return null;
            }
            
            current = current.transform.parent.gameObject;
        }
        
        return null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ForwardEvent<IPointerDownHandler>(eventData, ExecuteEvents.pointerDownHandler);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ForwardEvent<IPointerUpHandler>(eventData, ExecuteEvents.pointerUpHandler);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ForwardEvent<IPointerClickHandler>(eventData, ExecuteEvents.pointerClickHandler);
    }
    */
}
