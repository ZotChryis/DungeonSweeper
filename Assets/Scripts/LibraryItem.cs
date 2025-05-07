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
    
    public void SetGridSpawn(SpawnSettings.GridSpawnEntry gridSpawn)
    {
        Icon.sprite = gridSpawn.Object.Sprite;
        Power.SetText(gridSpawn.Object.Power.ToString());
        Amount.SetText("x" + gridSpawn.Amount.ToString());
        Name.SetText(gridSpawn.Object.UserFacingName);
        Description.SetText(gridSpawn.Object.UserFacingDescription);
    }
}
