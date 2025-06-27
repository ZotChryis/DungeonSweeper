using Schemas;
using TMPro;
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

    public void SetData(TileSchema tileObject, int amount)
    {
        Icon.sprite = tileObject.Sprite;
        Power.SetText(tileObject.Power.ToString());
        Amount.SetText("x" + amount.ToString());
        Name.SetText(tileObject.UserFacingName);
        Description.SetText(tileObject.UserFacingDescription);
    }
}
