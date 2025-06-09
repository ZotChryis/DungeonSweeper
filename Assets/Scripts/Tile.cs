using System;
using System.Collections.Generic;
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
    private Image HousedObjectSprite;

    [Tooltip("When you open the Minotaur guarded chest, this will appear as he turns around. Should also be mirrored if enemy is mirrored.")]
    [SerializeField]
    private Image ExclamationMarker;

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
    public TileState State { get; private set; } = TileState.Hidden;

    public int XCoordinate = 0;
    public int YCoordinate = 0;

    private int ObscureCounter;

    [HideInInspector]
    public Tile GuardingTile;
    private bool IsEnraged = false;
    private bool ShouldStandUp = false;

    private void Start()
    {
        // TODO: this probably needs a better home
        ServiceLocator.Instance.Grid.OnTileStateChanged += OnAnyTileStateChanged;
        ServiceLocator.Instance.Grid.OnGridGenerated += TEMP_UpdateVisuals;

        TileButton.onClick.AddListener(OnTileClicked);
        ResetLook();

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
    /// Places the housedObj and optionally upgrades it based on player power.
    /// Obscures adjacent tiles if necessary.
    /// </summary>
    public void PlaceTileObj(TileObjectSchema housedObject)
    {
        if (housedObject != null &&
            housedObject.UpgradedVersion != null &&
            !string.IsNullOrWhiteSpace(housedObject.Id) &&
            ServiceLocator.Instance.Player.TileObjectsThatShouldUpgrade.TryGetValue(housedObject.Id, out int level))
        {
            for (int i = 0; i < level && housedObject && housedObject.UpgradedVersion; i++)
            {
                housedObject = housedObject.UpgradedVersion;
            }
        }
        HousedObject = housedObject;

        if (HousedObject && HousedObject.ObscureRadius > 0)
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
        if (State >= TileState.Revealed)
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
    /// For use specifically when the player dies and we need to reveal
    /// the entire board at once. Which can be very CPU intensive. Copy of above TEMP_RevealWithoutLogic.
    /// </summary>
    public void FastRevealWithoutLogic()
    {
        if (State >= TileState.Revealed)
        {
            return;
        }

        Power.enabled = true;
        Annotation.enabled = false;

        UpdateObjectSelfVisuals();

        if (GetHousedObject() == null)
        {
            if (ObscureCounter > 0)
            {
                NeighborPower.SetText("?");
                NeighborPower.enabled = true;
            }
            else
            {
                int neighborPower = ServiceLocator.Instance.Grid.TEMP_GetTotalNeighborCost(XCoordinate, YCoordinate);
                if (neighborPower != 0)
                {
                    NeighborPower.SetText(neighborPower.ToString());
                    NeighborPower.enabled = true;
                }
            }
            TileButton.interactable = false;
        }
        else
        {
            HousedObjectSprite.enabled = true;
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
                PlaceTileObj(HousedObject.DropReward);
                TEMP_SetState(TileState.Revealed);
                return;
            }
        }

        TEMP_UpdateVisuals();

        // Early return if housed object does not exist.
        if (!HousedObject && State == TileState.Revealed)
        {
            TEMP_SetState(TileState.Empty);
            return;
        }

        Player player = ServiceLocator.Instance.Player;

        if (TileState.Conquered == State && HousedObject.Power > 0)
        {
            if (player.TEMP_PredictDeath(HousedObject.Power))
            {
                // player has died unconquer yourself
                State = TileState.Revealed;
                ServiceLocator.Instance.Grid.OnTileStateChanged?.Invoke(this);
            }
            if (player.UpdateHealth(-HousedObject.Power))
            {
                return;
            }

            if (HousedObject && HousedObject.ObscureRadius > 0)
            {
                ServiceLocator.Instance.Grid.Unobscure(XCoordinate, YCoordinate, HousedObject.ObscureRadius);
            }
            if (HousedObject && HousedObject.ObscureOffsets != null && HousedObject.ObscureOffsets.Length > 0)
            {
                ServiceLocator.Instance.Grid.Unobscure(XCoordinate, YCoordinate, HousedObject.ObscureOffsets);
            }
            if (HousedObject && HousedObject.ScreenshakeOnConquer)
            {
                StartCoroutine(ServiceLocator.Instance.Grid.Shake());
            }
            if (GuardingTile != null)
            {
                GuardingTile.LookTowards(XCoordinate, YCoordinate, true);
            }
        }

        if (TileState.Collected == State)
        {
            if (HousedObject.CanFlee && ServiceLocator.Instance.Grid.TEMP_HandleFlee(HousedObject))
            {
                TEMP_SetState(TileState.Empty);
                return;
            }
            if (HousedObject.SpawnsFleeingChild && ServiceLocator.Instance.Grid.TEMP_HandleFlee(HousedObject.FleeingChild))
            {
                TEMP_SetState(TileState.Empty);
                return;
            }

            int revealOriginX = XCoordinate;
            int revealOriginY = YCoordinate;
            if (HousedObject.RevealRandLocationNextToMine)
            {
                (int revealX, int revealY) = ServiceLocator.Instance.Grid.GetPositionOfRandomType("mine");
                List<(int, int)> adjacentToReveal = ServiceLocator.Instance.Grid.GetAdjacentValidPositions(revealX, revealY);
                adjacentToReveal.Remove((revealX, revealY));
                var randomAdjacentToReveal = adjacentToReveal[UnityEngine.Random.Range(0, adjacentToReveal.Count)];
                revealOriginX = randomAdjacentToReveal.Item1;
                revealOriginY = randomAdjacentToReveal.Item2;
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
            ServiceLocator.Instance.Player.ShopXp += HousedObject.ShopXp;

            if (HousedObject.WinReward)
            {
                ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.Victory);
            }

            if (HousedObject.FullHealReward)
            {
                ServiceLocator.Instance.Player.HealPlayerNoOverheal(999);
            }
            ServiceLocator.Instance.Player.HealPlayerNoOverheal(HousedObject.HealReward);

            if (HousedObject.DiffuseMinesReward)
            {
                ServiceLocator.Instance.Grid.TEMP_DiffuseMarkedOrRevealedMines();
                StartCoroutine(ServiceLocator.Instance.Grid.Shake());
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

    private void LookTowards(int xCoordinate, int yCoordinate, bool enrage)
    {
        if (!HousedObject.CanEnrage)
        {
            return;
        }
        if (enrage)
        {
            IsEnraged = true;
        }
        // if our position is to the left of our enrage target flip us around.
        if (xCoordinate > this.XCoordinate)
        {
            if (ExclamationMarker != null)
            {
                ExclamationMarker.transform.localScale = new Vector3(-1, 1, 1);
            }
            if (HousedObjectSprite != null)
            {
                HousedObjectSprite.transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        else if (xCoordinate < this.XCoordinate)
        {
            ResetLook();
        }
        else
        {
            // Rats stand up
            ResetLook();
            ShouldStandUp = true;
        }
        TEMP_UpdateVisuals();
    }

    private void ResetLook()
    {
        if (ExclamationMarker != null)
        {
            ExclamationMarker.transform.localScale = new Vector3(1, 1, 1);
        }
        if (HousedObject != null)
        {
            HousedObjectSprite.transform.localScale = new Vector3(1, 1, 1);
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
                HousedObjectSprite.enabled = false;
                XSpriteRenderer.enabled = false;
                Annotation.enabled = true;
                ExclamationMarker.enabled = false;
                break;

            case TileState.Revealed:
                Power.enabled = true;
                NeighborPower.enabled = GetHousedObject() != null && ServiceLocator.Instance.Player.TilesWhichShowNeighborPower.Contains(GetHousedObject().Id.ToLower());
                HousedObjectSprite.enabled = true;
                XSpriteRenderer.enabled = false;
                Annotation.enabled = false;
                ExclamationMarker.enabled = IsEnraged;
                break;

            case TileState.Conquered:
                Power.enabled = true;
                NeighborPower.enabled = false;
                HousedObjectSprite.enabled = true;
                XSpriteRenderer.enabled = true;
                Annotation.enabled = false;
                ExclamationMarker.enabled = IsEnraged;
                break;

            case TileState.Collected:
                Power.enabled = false;
                NeighborPower.enabled = true;
                HousedObjectSprite.enabled = false;
                XSpriteRenderer.enabled = false;
                Annotation.enabled = false;
                ExclamationMarker.enabled = false;
                break;

            case TileState.Empty:
                Power.enabled = false;
                NeighborPower.enabled = true;
                HousedObjectSprite.enabled = false;
                XSpriteRenderer.enabled = false;
                Annotation.enabled = false;
                ExclamationMarker.enabled = false;
                TileButton.interactable = false;
                break;
        }

        // Allow the object itself to override these settings
        UpdateObjectSelfVisuals();

        if (ObscureCounter > 0)
        {
            NeighborPower.SetText("?");
        }
        else
        {
            int neighborPower = ServiceLocator.Instance.Grid.TEMP_GetTotalNeighborCost(XCoordinate, YCoordinate);
            NeighborPower.SetText(neighborPower == 0 ? string.Empty : neighborPower.ToString());
        }
    }

    private void UpdateObjectSelfVisuals()
    {
        var objectOverrides = HousedObject ? HousedObject.GetOverrides(State) : default;
        Power.enabled = objectOverrides.EnablePower.UseOverride ? objectOverrides.EnablePower.Value : Power.enabled;
        HousedObjectSprite.enabled = objectOverrides.EnableSprite.UseOverride ? objectOverrides.EnableSprite.Value : HousedObjectSprite.enabled;
        XSpriteRenderer.enabled = objectOverrides.EnableDeathSprite.UseOverride ? objectOverrides.EnableDeathSprite.Value : XSpriteRenderer.enabled;

        // TODO Here the get sprite and guard logic should set....
        HousedObjectSprite.sprite = HousedObject ? HousedObject.TEMP_GetSprite(ShouldStandUp, CompassDirections.None) : null;
        if (objectOverrides.Sprite.UseOverride)
        {
            HousedObjectSprite.sprite = objectOverrides.Sprite.Value;
        }

        // TEMP: Change color depending on the state
        // TODO: Seperate these 2 labels and control independently
        Power.color = State < TileState.Conquered ? PowerColor : RewardColor;
        Power.SetText(HousedObject ? State < TileState.Conquered ? HousedObject.Power.ToString() : HousedObject.XPReward.ToString() : string.Empty);
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