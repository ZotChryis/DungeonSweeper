using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TileContextMenu : Screen
{
    [SerializeField] private RectTransform Content;

    private Tile ActiveTile;
    
    protected override void Awake()
    {
        base.Awake();
        
        ServiceLocator.Instance.Register(this);

        var buttons = GetComponentsInChildren<Button>(true).Where(b => b.gameObject.name.Contains("Option")).ToArray();
        for (int i = 0; i < buttons.Length; i++)
        {
            int capturedIndex = i;
            buttons[i].onClick.AddListener(() =>
            {
                OnOptionSelected(capturedIndex);
            });
        }
    }

    protected override void OnCloseButtonClicked()
    {
        ServiceLocator.Instance.OverlayScreenManager.HideActiveScreen();
        ActiveTile = null;
    }

    public void OnOptionSelected(int option)
    {
        int optionAdjusted = option + 1;
        string annotationText = optionAdjusted == 12 ? "*" : optionAdjusted.ToString();
        ActiveTile.TEMP_SetAnnotation(annotationText);
        ServiceLocator.Instance.OverlayScreenManager.HideActiveScreen();
        ActiveTile = null;
    }

    public void SetActiveTile(Tile tile)
    {
        ActiveTile = tile;
        Content.position = ((RectTransform)(tile.transform)).position;
    }
}
