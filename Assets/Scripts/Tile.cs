using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour, IPointerDownHandler
{
    /// <summary>
    /// The states of a tile.
    /// By default, going from Hidden -> Revealed will automatically go to Conquered with no user input.
    /// Going from Conquered -> Collected, it will automatically go to Hidden with no extra user input.
    /// </summary>
    /// TODO: Support a better state machine logical control. For example, right now we cannot implement the block with this strict logic
    ///       We may need to create controller classes for tileobject, instead of driving everything through a scriptable object with tons of logical overrides. 
    ///       idk. im too lazy to think of better
    /// TODO: Better support HousedObject == null case
    [Serializable]
    public enum TileState
    {
        // No number, unknown to player
        Hidden = 0,
        // No number, if monster/item is on this tile it is revealed
        Revealed = 1,
        // Monster is defeated and ready for collection
        Conquered = 2,
        // Monster/item has been collected
        Collected = 3,
        // No monster/item at this tile
        Empty = 4
    }

    [SerializeField]
    private Button TileButton;
    
    [SerializeField] 
    private Image SpriteRenderer;

    [SerializeField]
    private Image XSpriteRenderer;

    [SerializeField] 
    private TMP_Text NeighborPower;
    
    [SerializeField] 
    private TMP_Text Power;
    
    [SerializeField] 
    private TMP_Text Annotation;

    [SerializeField]
    private Color PowerColor;

    [SerializeField]
    private Color RewardColor;

    [SerializeField]
    private TileObjectSchema HousedObject;
    private TileState State = TileState.Hidden;
    
    public int XCoordinate = 0;
    public int YCoordinate = 0;
    
    private int ObscureCounter;
    
    private void Start()
    {
        // TODO: this probably needs a better home
        ServiceLocator.Instance.Grid.OnTileStateChanged += OnAnyTileStateChanged;
        ServiceLocator.Instance.Grid.OnGridGenerated += TEMP_UpdateVisuals;

        TileButton.onClick.AddListener(OnTileClicked);

        TEMP_UpdateVisuals();
    }

    private void OnDestroy()
    {
        ServiceLocator.Instance.Grid.OnTileStateChanged -= OnAnyTileStateChanged;
        ServiceLocator.Instance.Grid.OnGridGenerated -= TEMP_UpdateVisuals;
    }

    public void TEMP_SetCoordinates(int xCoordinate, int yCoordinate)
    {
        XCoordinate = xCoordinate;
        YCoordinate = yCoordinate;
    }
    
    private void OnAnyTileStateChanged(Tile tile)
    {
        TEMP_UpdateVisuals();
    }

    /// <summary>
    /// Tile was clicked
    /// </summary>
    public void OnTileClicked()
    {
        if (State == TileState.Empty)
        {
            return;
        }

        if (State == TileState.Revealed)
        {
            Player player = ServiceLocator.Instance.Player;
            if (HousedObject.PreventConsumeIfKillingBlow && player.TEMP_PredictDeath(TEMP_GetCost()))
            {
                return;
            }
        }
        
        TEMP_SetState(State + 1);
    }

    public int TEMP_GetPublicCost()
    {
        if (!HousedObject)
        {
            return 0;
        }
        
        if (HousedObject.HidePowerToNeighbors)
        {
            return 0;
        }

        return TEMP_GetCost();
    }
    
    public int TEMP_GetCost()
    {
        if (!HousedObject)
        {
            return 0;
        }
        
        return State < TileState.Conquered ? HousedObject.Power : 0;
    }
    
    /// <summary>
    /// Replace this function eventually...
    /// </summary>
    public void TEMP_Place(TileObjectSchema housedObject)
    {
        HousedObject = housedObject;
        
        if (HousedObject&& HousedObject.ObscureRadius > 0)
        {
            ServiceLocator.Instance.Grid.Obscure(XCoordinate, YCoordinate, HousedObject.ObscureRadius);
        }

        if (HousedObject && HousedObject.ObscureOffsets != null && HousedObject.ObscureOffsets.Length > 0)
        {
            ServiceLocator.Instance.Grid.Obscure(XCoordinate, YCoordinate, HousedObject.ObscureOffsets);
        }
    }

    /// <summary>
    /// Does the "Vision Orb" start of game thing. The only bad thing here is 
    /// we won't run any logic on the state change (revealed) logic, since
    /// we don't want to do a lot of the automatic stuff
    /// </summary>
    public void TEMP_RevealWithoutLogic()
    {
        if (State > TileState.Revealed)
        {
            return;
        }

        State = TileState.Revealed;
        ServiceLocator.Instance.Grid.OnTileStateChanged?.Invoke(this);
        TEMP_UpdateVisuals();
        
        // TODO: Total hack, fix later
        if (!HousedObject && State == TileState.Revealed)
        {
            TEMP_SetState(TileState.Empty);
            return;
        }
    }
    
    /// <summary>
    /// Replace this function eventually...
    /// </summary>
    public void TEMP_SetState(TileState state)
    {
        if (state > TileState.Empty)
        {
            return;
        }

        State = state;
        ServiceLocator.Instance.Grid.OnTileStateChanged?.Invoke(this);
        HandleStateChanged();
    }

    // TODO: We should look into using Observables/real state machine
    private void HandleStateChanged()
    {
        if (State == TileState.Empty)
        {
            // TODO: HACK - need to remove any fully empty tile when it is revealed to make Flee work
            ServiceLocator.Instance.Grid.UnoccupiedSpaces.RemoveUnoccupiedSpace(XCoordinate, YCoordinate); 
            
            if (HousedObject && HousedObject.DropReward)
            {
                TEMP_Place(HousedObject.DropReward);
                TEMP_SetState(TileState.Revealed);
                return;
            }
        }
        
        TEMP_UpdateVisuals();

        // TODO: Total hack, fix later
        if (!HousedObject && State == TileState.Revealed)
        {
            TEMP_SetState(TileState.Empty);
            return;
        }
        
        Player player = ServiceLocator.Instance.Player;
        
        if (TileState.Conquered == State && HousedObject.Power > 0)
        {
            player.TEMP_UpdateHealth(-HousedObject.Power);
            
            if (HousedObject && HousedObject.ObscureRadius > 0)
            {
                ServiceLocator.Instance.Grid.Unobscure(XCoordinate, YCoordinate, HousedObject.ObscureRadius);
            }
            if (HousedObject && HousedObject.ObscureOffsets != null && HousedObject.ObscureOffsets.Length > 0)
            {
                ServiceLocator.Instance.Grid.Unobscure(XCoordinate, YCoordinate, HousedObject.ObscureOffsets);
            }
        }

        if (TileState.Collected == State)
        {
            if (HousedObject.CanFlee && ServiceLocator.Instance.Grid.TEMP_HandleFlee(HousedObject))
            {
                TEMP_SetState(TileState.Empty);
                return;
            }

            int revealOriginX = XCoordinate;
            int revealOriginY = YCoordinate;
            if (HousedObject.GenerateRandomLocationForReveal)
            {
                var randomBoard = ServiceLocator.Instance.Grid.UnoccupiedSpaces;
                if (randomBoard.HasEmptySpace())
                {
                    (revealOriginX, revealOriginY) = randomBoard.PeekUnoccupiedSpace();
                    randomBoard.RemoveUnoccupiedSpace(revealOriginX, revealOriginY);
                }
            }
            
            if (HousedObject.RevealRadius > 0)
            {
                ServiceLocator.Instance.Grid.TEMP_RevealTilesInRadius(revealOriginX, revealOriginY, HousedObject.RevealRadius);
            }

            if (HousedObject.RevealOffsets != null && HousedObject.RevealOffsets.Length > 0)
            {
                ServiceLocator.Instance.Grid.TEMP_RevealTiles(revealOriginX, revealOriginY, HousedObject.RevealOffsets);
            }
            
            player.TEMP_UpdateXP(HousedObject.XPReward);
            
            if (HousedObject.WinReward)
            {
                ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.Victory);
            }

            if (HousedObject.FullHealReward)
            {
                ServiceLocator.Instance.Player.TEMP_UpdateHealth(99);
            }

            if (HousedObject.DiffuseMinesReward)
            {
                ServiceLocator.Instance.Grid.TEMP_DiffuseMarkedOrRevealedMines();
            }
            
            if (HousedObject.RevealAllRewards != null && HousedObject.RevealAllRewards.Length > 0)
            {
                foreach (var revealReward in HousedObject.RevealAllRewards)
                {
                    ServiceLocator.Instance.Grid.TEMP_RevealAllOfType(revealReward);
                }
            }
        }

        var objectOverrides = HousedObject ? HousedObject.GetOverrides(State) : default;
        if (objectOverrides.AutoContinue.UseOverride)
        {
            if (objectOverrides.AutoContinue.Value)
            {
                TEMP_SetState(State + 1);
                return;
            }
        }
        else if (IsAutomaticState(State))
        {
            TEMP_SetState(State + 1);
        }
    }

    private bool IsAutomaticState(TileState state)
    {
        return state == TileState.Revealed || state == TileState.Collected;
    }

    private void TEMP_UpdateVisuals()
    {
        // Set the default configuration for tile items
        TileButton.interactable = true;
        switch (State)
        {
            case TileState.Hidden:
                Power.enabled = false;
                NeighborPower.enabled = false;
                SpriteRenderer.enabled = false;
                XSpriteRenderer.enabled = false;
                Annotation.enabled = true;
                break;
            
            case TileState.Revealed:
                Power.enabled = true;
                NeighborPower.enabled = false;
                SpriteRenderer.enabled = true;
                XSpriteRenderer.enabled = false;
                Annotation.enabled = false;
                break;

            case TileState.Conquered:
                Power.enabled = true;
                NeighborPower.enabled = false;
                SpriteRenderer.enabled = true;
                XSpriteRenderer.enabled = true;
                Annotation.enabled = false;
                break;
            
            case TileState.Collected:
                Power.enabled = false;
                NeighborPower.enabled = true;
                SpriteRenderer.enabled = false;
                XSpriteRenderer.enabled = false;
                Annotation.enabled = false;
                break;
            
            case TileState.Empty:
                Power.enabled = false;
                NeighborPower.enabled = true;
                SpriteRenderer.enabled = false;
                XSpriteRenderer.enabled = false;
                Annotation.enabled = false;

                TileButton.interactable = false;
                break;
        }
        
        // Allow the object itself to override these settings
        var objectOverrides = HousedObject ? HousedObject.GetOverrides(State) : default;
        Power.enabled = objectOverrides.EnablePower.UseOverride ? objectOverrides.EnablePower.Value : Power.enabled;
        SpriteRenderer.enabled = objectOverrides.EnableSprite.UseOverride ? objectOverrides.EnableSprite.Value : SpriteRenderer.enabled;
        XSpriteRenderer.enabled = objectOverrides.EnableDeathSprite.UseOverride ? objectOverrides.EnableDeathSprite.Value : XSpriteRenderer.enabled;
            
        SpriteRenderer.sprite = HousedObject ? HousedObject.TEMP_GetSprite(XCoordinate, YCoordinate) : null;
        if (objectOverrides.Sprite.UseOverride)
        {
            SpriteRenderer.sprite = objectOverrides.Sprite.Value;
        }

        // TEMP: Change color depending on the state
        // TODO: Seperate these 2 labels and control independently
        Power.color = State < TileState.Conquered ? PowerColor : RewardColor;
        Power.SetText(HousedObject ? State < TileState.Conquered ? HousedObject.Power.ToString() : HousedObject.XPReward.ToString() : string.Empty);

        int neighborPower = ServiceLocator.Instance.Grid.TEMP_GetTotalNeighborCost(XCoordinate, YCoordinate);
        NeighborPower.SetText(neighborPower == 0 ? string.Empty : neighborPower.ToString());

        if (ObscureCounter > 0)
        {
            NeighborPower.SetText("?");
        }
    }

    public TileObjectSchema GetHousedObject()
    {
        return HousedObject;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Allow the button component handle left clicks
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            return;
        }

        // Right click should open the menu to set player tag
        if (eventData.button == PointerEventData.InputButton.Right && State == TileState.Hidden)
        {
            ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.TileContextMenu);
            ServiceLocator.Instance.TileContextMenu.SetActiveTile(this);
        }
    }

    // TODO: Make this better haha
    public void TEMP_SetAnnotation(string annotation)
    {
        if (Annotation.text.Equals(annotation))
        {
            Annotation.SetText(string.Empty);
            return;
        }
        
        Annotation.SetText(annotation);
    }

    public string GetAnnotationText()
    {
        return Annotation.text;
    }

    public bool TEMP_IsRevealed()
    {
        return State >= TileState.Revealed;
    }

    public bool TEMP_IsEmpty()
    {
        return !HousedObject || State == TileState.Empty;
    }

    public void TEMP_Obscure()
    {
        ObscureCounter++;
        TEMP_UpdateVisuals();
    }

    public void TEMP_Unobscure()
    {
        ObscureCounter--;
        TEMP_UpdateVisuals();
    }
}