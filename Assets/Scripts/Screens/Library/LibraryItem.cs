using Schemas;
using Screens.Library;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LibraryItem : MonoBehaviour
{
    [SerializeField] 
    private Image Icon;

    [SerializeField] 
    private TMP_Text Name;
    
    [SerializeField] 
    private TMP_Text Description;
    
    [SerializeField] 
    private TMP_Text Power;
    
    [SerializeField] 
    private TMP_Text Amount;
    
    [SerializeField] 
    private LibraryItemTag TagPrefab;

    [SerializeField] 
    private Transform TagRoot;

    private List<LibraryItemTag> Tags = new();

    public void SetData(TileSchema tileObject, int amount)
    {
        Icon.sprite = tileObject.Sprite;

        string hexColor;
        if (tileObject.OverridePowerColor != UnityEngine.Color.white)
        {
            hexColor = tileObject.OverridePowerColor.ToHexString();
        }
        else
        {
            hexColor = Tile.GetHexColorBasedOnPower(tileObject.Power, tileObject.ObscureRadius > 0);
        }
        Power.SetText($"<color=#{hexColor}>{tileObject.Power.ToString()}</color>");

        Amount.SetText("x" + amount.ToString());
        Name.SetText(tileObject.UserFacingName);
        Description.SetText(tileObject.UserFacingDescription);
        
        foreach (var libraryItemTag in Tags)
        {
            Destroy(libraryItemTag.gameObject);
        }
        Tags.Clear();
        
        foreach (var tileObjectTag in tileObject.Tags)
        {
            LibraryItemTag newTag = Instantiate(TagPrefab, TagRoot);
            newTag.SetData(tileObjectTag);
            Tags.Add(newTag);
        }
    }
}
