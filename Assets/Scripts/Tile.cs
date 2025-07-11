using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay;
using Schemas;
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

    public Image FlagAnnotation;

    [SerializeField]
    private Color PowerColor;

    [SerializeField]
    private Color RewardColor;

    [SerializeField]
    private TileSchema HousedObject;

    public TileState State { get; private set; } = TileState.Hidden;

    public int XCoordinate = 0;
    public int YCoordinate = 0;

    private int ObscureCounter;

    [HideInInspector]
    public Tile GuardingTile;
    public Tile BodyGuardedByTile;
    public CompassDirections DirectionToLook = CompassDirections.West;
    private bool IsEnraged = false;
    private bool ShouldStandUp = false;

    private void Start()
    {
        // TODO: this probably needs a better home
        ServiceLocator.Instance.Grid.OnTileStateChanged += OnAnyTileStateChanged;
        ServiceLocator.Instance.Grid.OnGridGenerated += TEMP_UpdateVisuals;
        ServiceLocator.Instance.Player.Inventory.OnItemChargeChanged += OnItemChargeChanged;

        TileButton.onClick.AddListener(OnTileClicked);
        ResetLook();

        TEMP_UpdateVisuals();
    }

    private void OnDestroy()
    {
        ServiceLocator.Instance.Grid.OnTileStateChanged -= OnAnyTileStateChanged;
        ServiceLocator.Instance.Grid.OnGridGenerated -= TEMP_UpdateVisuals;
        ServiceLocator.Instance.Player.Inventory.OnItemChargeChanged -= OnItemChargeChanged;
    }
    
    private void OnItemChargeChanged(ItemInstance obj)
    {
        TEMP_UpdateVisuals();
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

        if(FlagAnnotation.enabled && !ServiceLocator.Instance.Grid.MinesDiffused && FBPP.GetBool(PlayerOptions.IsSafeMinesOn, true))
        {
            // If safety on don't let the player blow themselves up.
            return;
        }

        // TODO: Refactor the click handle logic
        if (State == TileState.Revealed)
        {
            // Prevent death when clicking on a brick, basically. We can make other things too...
            Player player = ServiceLocator.Instance.Player;
            if (HousedObject.PreventConsumeIfKillingBlow && player.TEMP_PredictDeath(GetAdjustedPower()))
            {
                return;
            }
        }

        TEMP_SetState(State + 1);
    }
    
    /// <summary>
    /// This is the power that neighboring tiles will see when they ask about this tile.
    /// This is mostly useful for hiding the power in specific situations (currently right now Bricks do not
    /// contribute to neighbor info).
    /// </summary>
    public int GetPublicPower()
    {
        if (!HousedObject)
        {
            return 0;
        }

        if (HousedObject.HidePowerToNeighbors)
        {
            return 0;
        }

        if (State >= TileState.Conquered)
        {
            return 0;
        }

        // TODO: Move the prediction logic to Tile ONLY?
        return GetAdjustedPower();
    }
    
    /// <summary>
    /// Returns the 'real world' overall power of this tile. This means that the player's items and powers are
    /// reflected on this tile.
    /// Use this function when doing health calculations. 
    /// </summary>
    public int GetAdjustedPower()
    {
        int basePower = GetBasePower();
        return ServiceLocator.Instance.Player.GetAdjustedDamage(HousedObject, basePower);
    }

    /// <summary>
    /// Returns the power of the item this tile houses.
    /// Use this function when determining the power the underlying object is WITHOUT player item intervention.
    /// This means the object itself (and tile state) can dictate how powerful it is.
    /// TileSchema + TileState => Base Power
    /// Base Power + Player Powers (Items) => Adjusted Power
    /// </summary>
    /// <returns></returns>
    public int GetBasePower()
    {
        if (!HousedObject)
        {
            return 0;
        }
        
        // Special case: Gorgon is 0 when there are no nearby tile objects with power (it is always surrounded by
        // blocks, which means they have been cleared)
        if (HousedObject.TileId == TileSchema.Id.Gorgon)
        {
            int neighborPower = ServiceLocator.Instance.Grid.TEMP_GetUnconqueredNeighborCount(XCoordinate, YCoordinate);
            if (neighborPower == 0)
            {
                return 0;
            }
        }

        return HousedObject.Power;
    }

    /// <summary>
    /// Places the housedObj and optionally upgrades it based on player power.
    /// Obscures adjacent tiles if necessary.
    /// </summary>
    public void PlaceTileObj(TileSchema housedObject)
    {
        if (housedObject != null &&
            housedObject.UpgradedVersion != null &&
            ServiceLocator.Instance.Player.TileObjectsThatShouldUpgrade.TryGetValue(housedObject.TileId, out int level))
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

        TryPlaySfx(State);
        TEMP_UpdateVisuals();

        // TODO: Total hack, fix later
        if (!HousedObject && State == TileState.Revealed)
        {
            TEMP_SetState(TileState.Empty);
            return;
        }
    }

    private void TryPlaySfx(TileState state)
    {
        if (HousedObject)
        {
            var stateOverrides = HousedObject.GetOverrides(state);
            if (stateOverrides.Sfx.UseOverride && !string.IsNullOrEmpty(stateOverrides.Sfx.Value))
            {
                ServiceLocator.Instance.AudioManager.PlaySfx(stateOverrides.Sfx.Value);
            }
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
        FlagAnnotation.enabled = false;

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
                int neighborPower = ServiceLocator.Instance.Grid.TEMP_GetTotalNeighborPower(XCoordinate, YCoordinate);
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
        TryPlaySfx(State);
            
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

        // Associated guard gets enraged when yourself is conquered
        if (TileState.Conquered == State && BodyGuardedByTile != null && BodyGuardedByTile.State < TileState.Conquered)
        {
            BodyGuardedByTile.LookTowardsHorizontally(XCoordinate, YCoordinate, true, false);
            BodyGuardedByTile.TEMP_UpdateVisuals();
        }

        int basePower = GetBasePower();
        int adjustedPower = GetAdjustedPower();
        if (TileState.Conquered == State)
        {
            if (player.TEMP_PredictDeath(adjustedPower))
            {
                // TODO: I don't like that we enter conquer state before deducting cost, but w/e we can fix later
                //  player has died, so unconquer yourself 
                State = TileState.Revealed;
                ServiceLocator.Instance.Grid.OnTileStateChanged?.Invoke(this);
            }
            
            // If we die because of this, we can stop here
            // TODO: Refactor where we do the damage adjustment/prediction. We use base power because Damage() still does
            //  the damage adjustments internally as well
            if (player.Damage(HousedObject, basePower))
            {
                ServiceLocator.Instance.AudioManager.PlaySfx("Death");
                return;
            }

            if (!HousedObject.GetOverrides(State).Sfx.UseOverride)
            {
                ServiceLocator.Instance.AudioManager.PlaySfx("Attack");
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
        }

        if (TileState.Collected == State)
        {
            // Flee -> Itself moves to another location if possible (Gnome)
            if (HousedObject.CanFlee && ServiceLocator.Instance.Grid.TEMP_HandleFlee(HousedObject, HousedObject.RevealFlee))
            {
                TEMP_SetState(TileState.Empty);
                return;
            }
            
            // Fleeing Child -> Spawns a new object type at another location if possible (Faerie)
            if (HousedObject.SpawnsFleeingChild && ServiceLocator.Instance.Grid.TEMP_HandleFlee(HousedObject.FleeingChild, HousedObject.RevealFlee ))
            {
                TEMP_SetState(TileState.Empty);
                return;
            }

            int revealOriginX = XCoordinate;
            int revealOriginY = YCoordinate;
            if (HousedObject.RevealRandLocationNextToMine)
            {
                (int revealX, int revealY) = ServiceLocator.Instance.Grid.GetPositionOfRandomType(TileSchema.Id.Mine);
                List<(int, int)> adjacentToReveal = ServiceLocator.Instance.Grid.GetAdjacentValidPositions(revealX, revealY);
                adjacentToReveal.Remove((revealX, revealY));

                if (adjacentToReveal.Count > 0)
                {
                    var randomAdjacentToReveal = adjacentToReveal[UnityEngine.Random.Range(0, adjacentToReveal.Count)];
                    revealOriginX = randomAdjacentToReveal.Item1;
                    revealOriginY = randomAdjacentToReveal.Item2;
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

            player.TEMP_UpdateXP(HousedObject, HousedObject.XPReward);
            ServiceLocator.Instance.Player.ShopXp += HousedObject.ShopXp;

            if (HousedObject.WinReward)
            {
                ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.Victory);
            }

            if (HousedObject.FullHealReward)
            {
                ServiceLocator.Instance.Player.Heal(HousedObject, 999);
            }
            ServiceLocator.Instance.Player.Heal(HousedObject, HousedObject.HealReward);

            if (HousedObject.DiffuseMinesReward)
            {
                ServiceLocator.Instance.Grid.TEMP_DiffuseMarkedOrRevealedMines();
                StartCoroutine(ServiceLocator.Instance.Grid.Shake());
            }

            if (HousedObject.RevealAllRewards != null && HousedObject.RevealAllRewards.Length > 0)
            {
                foreach (var revealReward in HousedObject.RevealAllRewards)
                {
                    // TODO Remove HACK: George added a bug here with sprites, going to do standup only for the
                    // rat scroll for now since im too lazy to fix 
                    bool standUp = HousedObject.TileId == TileSchema.Id.ScrollRat;
                    ServiceLocator.Instance.Grid.TEMP_RevealAllOfType(revealReward, standUp);
                }
            }

            if (HousedObject.ItemsToReplenish != null && HousedObject.ItemsToReplenish.Length > 0)
            {
                ServiceLocator.Instance.Player.Inventory.ReplenishItems(HousedObject.ItemsToReplenish);
            }

            // Specific item reward
            if (HousedObject.ItemReward != ItemSchema.Id.None)
            {
                ServiceLocator.Instance.Player.Inventory.AddItem(HousedObject.ItemReward);
            }
            
            // Random reward from Rarity
            if (HousedObject.ItemRewardRarities != null)
            {
                var matchingItems = ServiceLocator.Instance.Schemas.ItemSchemas.FindAll(item =>
                    HousedObject.ItemRewardRarities.Contains(item.Rarity));
                if (matchingItems.Count > 0)
                {
                    var rewardItem = matchingItems.GetRandomItem();
                    ServiceLocator.Instance.Player.Inventory.AddItem(rewardItem.ItemId);
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

    public void LookAwayFrom(int xCoordinate, int yCoordinate, bool enrage)
    {
        if (enrage)
        {
            IsEnraged = true;
        }
        // if our position is to the right of our target flip us around.
        if (xCoordinate < this.XCoordinate)
        {
            DirectionToLook = CompassDirections.East;
        }
        else if (xCoordinate > this.XCoordinate)
        {
            DirectionToLook = CompassDirections.West;
        }
        else
        {
            // Rats stand up
            DirectionToLook = CompassDirections.West;
            ShouldStandUp = true;
        }
    }

    public void StandUp()
    {
        ShouldStandUp = true;
    }

    /// <summary>
    /// Look towards one another. Purely horizontally. For minotaurs who don't look diagonally.
    /// </summary>
    /// <param name="xCoordinate"></param>
    /// <param name="yCoordinate"></param>
    /// <param name="enrage"></param>
    /// <param name="canStandUp">Whether to stand up if on same row</param>
    public void LookTowardsHorizontally(int xCoordinate, int yCoordinate, bool enrage, bool canStandUp)
    {
        if (enrage)
        {
            IsEnraged = true;
        }
        if (xCoordinate > this.XCoordinate)
        {
            DirectionToLook = CompassDirections.East;
        }
        else if (xCoordinate < this.XCoordinate)
        {
            DirectionToLook = CompassDirections.West;
        }
        else
        {
            // Rats stand up
            ShouldStandUp = canStandUp;
            DirectionToLook = CompassDirections.West;
        }
    }

    /// <summary>
    /// For gargoyles which look north, east, south, west.
    /// </summary>
    /// <param name="xCoordinate"></param>
    /// <param name="yCoordinate"></param>
    /// <param name="enrage"></param>
    /// <param name="canStandUp"></param>
    public void LookTowardsOrthogonally(int xCoordinate, int yCoordinate, bool enrage, bool canStandUp)
    {
        if (enrage)
        {
            IsEnraged = true;
        }
        if (xCoordinate > this.XCoordinate)
        {
            DirectionToLook = CompassDirections.East;
        }
        else if (xCoordinate < this.XCoordinate)
        {
            DirectionToLook = CompassDirections.West;
        }
        else if (yCoordinate < this.YCoordinate)
        {
            DirectionToLook = CompassDirections.South;
        }
        else if (yCoordinate > this.YCoordinate)
        {
            DirectionToLook = CompassDirections.North;
        }
        else
        {
            // Rats stand up
            ShouldStandUp = canStandUp;
            DirectionToLook = CompassDirections.West;
        }
    }

    private void ResetLook()
    {
        if (ExclamationMarker != null)
        {
            ExclamationMarker.transform.localScale = new Vector3(1, 1, 1);
        }
        HousedObjectSprite.transform.localScale = new Vector3(1, 1, 1);
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
                NeighborPower.enabled = GetHousedObject() != null && ServiceLocator.Instance.Player.TilesWhichShowNeighborPower.Contains(GetHousedObject().TileId);
                HousedObjectSprite.enabled = true;
                XSpriteRenderer.enabled = false;
                Annotation.enabled = false;
                FlagAnnotation.enabled = false;
                ExclamationMarker.enabled = IsEnraged;
                break;

            case TileState.Conquered:
                Power.enabled = true;
                NeighborPower.enabled = false;
                HousedObjectSprite.enabled = true;
                XSpriteRenderer.enabled = true;
                Annotation.enabled = false;
                FlagAnnotation.enabled = false;
                ExclamationMarker.enabled = IsEnraged;
                break;

            case TileState.Collected:
                Power.enabled = false;
                NeighborPower.enabled = true;
                HousedObjectSprite.enabled = false;
                XSpriteRenderer.enabled = false;
                Annotation.enabled = false;
                FlagAnnotation.enabled = false;
                ExclamationMarker.enabled = false;
                break;

            case TileState.Empty:
                Power.enabled = false;
                NeighborPower.enabled = true;
                HousedObjectSprite.enabled = false;
                XSpriteRenderer.enabled = false;
                Annotation.enabled = false;
                FlagAnnotation.enabled = false;
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
            int neighborPower = ServiceLocator.Instance.Grid.TEMP_GetTotalNeighborPower(XCoordinate, YCoordinate);
            NeighborPower.SetText(neighborPower == 0 ? string.Empty : neighborPower.ToString());
        }
    }

    private void UpdateObjectSelfVisuals()
    {
        var objectOverrides = HousedObject ? HousedObject.GetOverrides(State) : default;
        Power.enabled = objectOverrides.EnablePower.UseOverride ? objectOverrides.EnablePower.Value : Power.enabled;
        HousedObjectSprite.enabled = objectOverrides.EnableSprite.UseOverride ? objectOverrides.EnableSprite.Value : HousedObjectSprite.enabled;
        XSpriteRenderer.enabled = objectOverrides.EnableDeathSprite.UseOverride ? objectOverrides.EnableDeathSprite.Value : XSpriteRenderer.enabled;

        HousedObjectSprite.sprite = HousedObject ? HousedObject.TEMP_GetSprite(ShouldStandUp, DirectionToLook) : null;

        if (DirectionToLook == CompassDirections.East)
        {
            if (ExclamationMarker != null)
            {
                ExclamationMarker.transform.localScale = new Vector3(-1, 1, 1);
            }
            HousedObjectSprite.transform.localScale = new Vector3(-1, 1, 1);
        }
        else // default scale looks right
        {
            if (ExclamationMarker != null)
            {
                ExclamationMarker.transform.localScale = Vector3.one;
            }
            HousedObjectSprite.transform.localScale = Vector3.one;
        }
        if (objectOverrides.Sprite.UseOverride)
        {
            HousedObjectSprite.sprite = objectOverrides.Sprite.Value;
        }

        // TEMP: Change color depending on the state
        // TODO: Seperate these 2 labels and control independently
        Power.color = State < TileState.Conquered ? PowerColor : RewardColor;
        
        Power.SetText(HousedObject? State < TileState.Conquered 
                ? GetAdjustedPower().ToString() 
                : ServiceLocator.Instance.Player.GetModifiedXp(HousedObject, HousedObject.XPReward).ToString()
            : string.Empty
        );
    }

    public TileSchema GetHousedObject()
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

    /// <summary>
    /// Sets the player annotation. The power the player thinks the tile is.
    /// </summary>
    /// <param name="annotation"></param>
    public void SetAnnotation(int power)
    {
        // Mines use a special flag.
        if(power == 100)
        {
            Annotation.SetText(string.Empty);
            FlagAnnotation.enabled = !FlagAnnotation.enabled;
            return;
        }

        if (Annotation.text.Equals(power.ToString()))
        {
            Annotation.SetText(string.Empty);
            FlagAnnotation.enabled = false;
            return;
        }

        Annotation.SetText(power.ToString());
        FlagAnnotation.enabled = false;
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

    public void TEMP_UnObscure()
    {
        ObscureCounter--;
        TEMP_UpdateVisuals();
    }
}