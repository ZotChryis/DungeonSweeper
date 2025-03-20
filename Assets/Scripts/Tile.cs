using System;
using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Serializable]
    public enum TileState
    {
        Hidden,
        Revealed,
        Conquered,
        Collected,
        Empty
    }

    [SerializeField] 
    private TMP_Text Label; 
    
    private TileState State = TileState.Hidden;
    private int Cost = 0;

    /// <summary>
    /// Gets the health cost of interacting with this Tile.
    /// </summary>
    public int GetCost()
    {
        return Cost;
    }

    /// <summary>
    /// Replace this function eventually...
    /// </summary>
    public void TEMP_Place(int newCost)
    {
        Cost = newCost;
        Label.SetText(Cost.ToString());
    }
}
