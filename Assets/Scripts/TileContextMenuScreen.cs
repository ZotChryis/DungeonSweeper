using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TileContextMenuScreen : Screen
{
    [SerializeField] private RectTransform Content;

    public GameObject[] level2Objects;
    public GameObject[] level3Objects;

    private Tile ActiveTile;

    protected override void Awake()
    {
        base.Awake();

        ServiceLocator.Instance.Register(this);

        var buttons = GetComponentsInChildren<Button>(true).Where(b => b.gameObject.name.Contains("Option")).ToArray();
        for (int i = 0; i < buttons.Length; i++)
        {
            string buttonNumber = buttons[i].gameObject.name.Substring(7);
            int.TryParse(buttonNumber, out int number);
            buttons[i].onClick.AddListener(() =>
            {
                OnOptionSelected(number);
            });
        }
        foreach (var obj in level2Objects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in level3Objects)
        {
            obj.SetActive(false);
        }
    }

    protected override void OnCloseButtonClicked()
    {
        ServiceLocator.Instance.OverlayScreenManager.HideActiveScreen();
        ActiveTile = null;
    }

    public void OnOptionSelected(int option)
    {
        string annotationText = option == 100 ? "*" : option.ToString();
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
