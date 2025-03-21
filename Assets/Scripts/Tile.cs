using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

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
    private TMP_Text NeighborPower;
    
    [SerializeField] 
    private TMP_Text Power;
    
    private ITileObject HousedObject;
    private TileState State = TileState.Hidden;

    // TODO: Deprecate this shit in favor of just looking it up from the contained item
    private int Cost = 0;
    
    // TODO: Probably should formalize this better
    private int XCoordinate = 0;
    private int YCoordinate = 0;

    private void Awake()
    {
        // TODO: this probably needs a better home
        ServiceLocator.Instance.Grid.OnTileStateChanged += OnAnyTileStateChanged;
        ServiceLocator.Instance.Grid.OnGridGenerated += TEMP_UpdateInformation;
    }
    
    private void Start()
    {
        TEMP_UpdateVisibility();
    }

    public void TEMP_SetCoordinates(int xCoordinate, int yCoordinate)
    {
        XCoordinate = xCoordinate;
        YCoordinate = yCoordinate;
    }
    
    private void OnAnyTileStateChanged(Tile tile)
    {
        TEMP_UpdateInformation();
    }

    private void OnMouseDown()
    {
        // TODO: For now we just follow along the states in order
        switch (State)
        {
            // TODO: For now, we just reveal
            case TileState.Hidden:
                TEMP_SetState(TileState.Revealed, true);
                return;
            
            case TileState.Revealed:
                TEMP_SetState(TileState.Conquered, true);
                return;    
            
            // Except this one, skip Collected for now
            case TileState.Conquered:
                TEMP_SetState(TileState.Collected, true);
                return;

            case TileState.Collected:
                TEMP_SetState(TileState.Empty, true);
                return;
        }
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
    }

    public void TEMP_Reveal()
    {
        if (State < TileState.Revealed)
        {
            TEMP_SetState(TileState.Revealed);
        }
    }
    
    /// <summary>
    /// Replace this function eventually...
    /// </summary>
    public void TEMP_SetState(TileState state, bool causedByInput = false)
    {
        State = state;
        HandleStateChanged(causedByInput);
        
        ServiceLocator.Instance.Grid.OnTileStateChanged?.Invoke(this);
    }

    // TODO: We should look into using Observables
    private void HandleStateChanged(bool causedByInput = false)
    {
        int revealRadius = HousedObject.GetRevealRadius(State);
        ServiceLocator.Instance.Grid.TEMP_RevealTilesInRadius(XCoordinate, YCoordinate, revealRadius);
        
        // TODO: Clean up these special cases in a systemic way
        // Enemies are auto conquered when revealed by user input
        if (State == TileState.Revealed && causedByInput)
        {
            TEMP_SetState(TileState.Conquered);
        }
        else if (State == TileState.Conquered)
        {
            if (causedByInput && HousedObject is EnemySchema enemy)
            {
                ServiceLocator.Instance.Player.TEMP_UpdateHealth(-GetCost());
            }

            Cost = 0;

            if (HousedObject is ItemSchema item && item.AutoCollect)
            {
                TEMP_SetState(TileState.Collected);
            }
        }
        else if (State == TileState.Collected)
        {
            ServiceLocator.Instance.Player.TEMP_UpdateXP(HousedObject.GetPower(State));
            TEMP_SetState(TileState.Empty);
        }

        // Update the information to display
        TEMP_UpdateInformation();
        TEMP_UpdateVisibility();
    }

    private void TEMP_UpdateVisibility()
    {
        // TODO: Cleanup plz. Maybe make an animator? idk
        // Update the state of the rendering
        switch (State)
        {
            case TileState.Hidden:
                Power.enabled = false;
                NeighborPower.enabled = false;
                SpriteRenderer.enabled = false;
                return;
            
            case TileState.Revealed:
                Power.enabled = true;
                NeighborPower.enabled = false;
                SpriteRenderer.enabled = true;
                return;
            case TileState.Conquered:
                Power.enabled = true;
                NeighborPower.enabled = false;
                SpriteRenderer.enabled = true;
                return;
            
            case TileState.Collected:
                Power.enabled = false;
                NeighborPower.enabled = true;
                SpriteRenderer.enabled = false;
                return;
            
            case TileState.Empty:
                Power.enabled = false;
                NeighborPower.enabled = true;
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

        int neighborPower = ServiceLocator.Instance.Grid.TEMP_GetTotalNeighborCost(XCoordinate, YCoordinate);
        NeighborPower.SetText(neighborPower == 0 ? string.Empty : neighborPower.ToString());
        
        Power.SetText(Cost == 0 ? string.Empty : Cost.ToString());
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

    /// <summary>
    /// Given the owned Tile state, this object must report how many neighboring tiles to reveal.
    /// </summary>
    int GetRevealRadius(Tile.TileState state);
}
