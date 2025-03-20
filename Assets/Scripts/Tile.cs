using System;
using AYellowpaper.SerializedCollections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

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
    private SpriteRenderer SpriteRenderer; 
    
    [SerializeField] 
    private TMP_Text Label;

    private ITileObject HousedObject;
    private TileState State = TileState.Hidden;
    private int Cost = 0;

    private void Start()
    {
        HandleStateChanged();
    }

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
    public void TEMP_Place(ITileObject housedObject)
    {
        HousedObject = housedObject;
        TEMP_UpdateInformation();
    }

    /// <summary>
    /// Replace this function eventually...
    /// </summary>
    public void TEMP_SetState(TileState state)
    {
        State = state;
        HandleStateChanged();
    }

    // TODO: We should look into using Observables
    private void HandleStateChanged()
    {
        // Update the information to display
        TEMP_UpdateInformation();
        
        // Update the state of the rendering
        switch (State)
        {
            case TileState.Hidden:
                Label.enabled = false;
                SpriteRenderer.enabled = false;
                return;
            
            case TileState.Revealed:
            case TileState.Conquered:
                Label.enabled = false;
                SpriteRenderer.enabled = true;
                return;
            
            case TileState.Collected:
                Label.enabled = false;
                SpriteRenderer.enabled = false;
                return;
            
            case TileState.Empty:
                Label.enabled = true;
                SpriteRenderer.enabled = false;
                return;
        }
    }

    /// <summary>
    /// When neighbors change, or the tile itself changed, we need to update visuals.
    /// </summary>
    private void TEMP_UpdateInformation()
    {
        Cost = HousedObject.GetPower(State);
        SpriteRenderer.sprite = HousedObject.GetSprite(State);
        Label.SetText(Cost.ToString());
    }
}

/// <summary>
/// Interface for all things that a Tile can house.
/// </summary>
public interface ITileObject
{
    /// <summary>
    /// Returns the power of the object. This is used by the owning Tile to determine it's state with player input.
    /// </summary>
    int GetPower(Tile.TileState state);
    
    /// <summary>
    /// Given the owned Tile state, this object must report how it should look.
    /// This can be null.
    /// </summary>
    Sprite GetSprite(Tile.TileState state);
}
