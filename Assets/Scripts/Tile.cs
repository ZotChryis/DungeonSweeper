using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Gameplay;
using Schemas;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
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
        Hidden,
        // No number, if monster/item is on this tile it is revealed
        Revealed,
        // Same as revealed. Except you get to this state from going from hidden=>click on this tile directly
        RevealThroughCombat,
        // Monster is defeated and ready for collection
        Conquered,
        // Monster/item has been collected
        Collected,
        // No monster/item at this tile
        Empty
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

    [SerializedDictionary]
    public SerializedDictionary<int, GameObject> SpecialAnnotations = new();

    [SerializeField]
    private Color PowerColor;

    [SerializeField]
    private Color RewardColor;

    [SerializeField]
    private TileSchema HousedObject;

    public TileState State { get; private set; } = TileState.Hidden;
    
    // TODO: HUGE HACK HERE to allow for Rain of Fire (aka, conquering stuff for 0 dmg taken)
    public bool AllowDamage = true;

    public int XCoordinate = 0;
    public int YCoordinate = 0;

    private int ObscureCounter;

    /// <summary>
    /// A reference to the tile you yourself are guarding.
    /// </summary>
    [HideInInspector]
    public Tile GuardingTile;

    /// <summary>
    /// References to all tiles that were spawned from this tile's spawn requirements.
    /// </summary>
    [HideInInspector] 
    public Tile[] ChildrenTiles;
    
    /// <summary>
    /// A reference to the tile that is guarding you.
    /// </summary>
    public Tile BodyGuardedByTile;
    public bool IsBodyGuardByOrGuardingDead = false;
    public CompassDirections DirectionToLook = CompassDirections.West;
    private bool IsEnraged = false;
    private bool ShouldStandUp = false;
    private bool IsAlreadyShakingAnnotation = false;

    private Coroutine MobileContextMenuHandle;

    private void Start()
    {
        // TODO: this probably needs a better home
        ServiceLocator.Instance.Grid.OnTileStateChanged += OnAnyTileStateChanged;
        ServiceLocator.Instance.Grid.OnGridGenerated += TEMP_UpdateVisuals;
        ServiceLocator.Instance.Grid.OnGridRequestedVisualUpdate += TEMP_UpdateVisuals;
        ServiceLocator.Instance.Player.Inventory.OnItemChargeChanged += OnItemChargeChanged;
        ServiceLocator.Instance.Player.OnConquer += OnPlayerConquered;
        ServiceLocator.Instance.GridDragger.OnValidDrag += CancelMobileContextMenu;

        TileButton.onClick.AddListener(OnTileClicked);
        ResetLook();

        TEMP_UpdateVisuals();
    }

    private void OnPlayerConquered(TileSchema obj)
    {
        TEMP_UpdateVisuals();
    }

    private void OnDestroy()
    {
        ServiceLocator.Instance.Grid.OnTileStateChanged -= OnAnyTileStateChanged;
        ServiceLocator.Instance.Grid.OnGridGenerated -= TEMP_UpdateVisuals;
        ServiceLocator.Instance.Grid.OnGridRequestedVisualUpdate -= TEMP_UpdateVisuals;
        ServiceLocator.Instance.Player.Inventory.OnItemChargeChanged -= OnItemChargeChanged;
        ServiceLocator.Instance.Player.OnConquer -= OnPlayerConquered;
        ServiceLocator.Instance.GridDragger.OnValidDrag -= CancelMobileContextMenu;
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

        foreach (var specialAnnotation in SpecialAnnotations)
        {
            if (specialAnnotation.Value.activeInHierarchy && !ServiceLocator.Instance.Grid.MinesDiffused && FBPP.GetBool(PlayerOptions.IsSafeMinesOn, true))
            {
                // If safety on don't let the player blow themselves up.
                StartCoroutine(ShakeAnnotation(specialAnnotation.Value));
                return;
            }
        }
        

        // TODO: Refactor the click handle logic
        if (State == TileState.Revealed || State == TileState.RevealThroughCombat)
        {
            // Prevent death when clicking on a brick, basically. We can make other things too...
            Player player = ServiceLocator.Instance.Player;
            if (HousedObject.PreventConsumeIfKillingBlow && player.TEMP_PredictDeath(GetAdjustedPower()))
            {
                return;
            }
        }

        // Hidden skips to RevealedThroughCombat
        // Revealed skips to Conquered
        if (State == TileState.Hidden || State == TileState.Revealed)
        {
            TEMP_SetState(State + 2);
        }
        else
        {
            TEMP_SetState(State + 1);
        }
    }
    
    /// <summary>
    /// Shakes the FlagAnnotation. Copied from Grid.cs.
    /// </summary>
    /// <returns></returns>
    public IEnumerator ShakeAnnotation(GameObject toShake, float duration = 0.50f, float magnitude = 10)
    {
        if (!IsAlreadyShakingAnnotation)
        {
            IsAlreadyShakingAnnotation = true;
            Vector3 originalPosition = toShake.transform.localPosition;
            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                float percentElapsedInvertSquared = 1f - elapsed / duration;
                percentElapsedInvertSquared = percentElapsedInvertSquared * percentElapsedInvertSquared;
                float x = UnityEngine.Random.Range(-1, 1f) * magnitude * percentElapsedInvertSquared + originalPosition.x;
                toShake.transform.localPosition = new Vector3(x, originalPosition.y, originalPosition.z);
                elapsed += Time.deltaTime;
                yield return null;
            }

            toShake.transform.localPosition = originalPosition;
            IsAlreadyShakingAnnotation = false;
        }
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

    public void UndoPlacedTileObj()
    {
        if (TEMP_IsEmpty())
        {
            return;
        }

        DirectionToLook = CompassDirections.West;
        IsEnraged = false;
        ShouldStandUp = false;
        
        if (HousedObject && HousedObject.ObscureRadius > 0)
        {
            ServiceLocator.Instance.Grid.Unobscure(XCoordinate, YCoordinate, HousedObject.ObscureRadius);
        }

        if (HousedObject && HousedObject.ObscureOffsets != null && HousedObject.ObscureOffsets.Length > 0)
        {
            ServiceLocator.Instance.Grid.Unobscure(XCoordinate, YCoordinate, HousedObject.ObscureOffsets);
        }
    }

    /// <summary>
    /// Does the "Vision Orb" start of game thing. The only bad thing here is 
    /// we won't run any logic on the state change (revealed) logic, since
    /// we don't want to do a lot of the automatic stuff
    /// </summary>
    public void TEMP_RevealWithoutLogic(GameObject vfx = null)
    {
        if (State >= TileState.Revealed)
        {
            return;
        }

        State = TileState.Revealed;
        ServiceLocator.Instance.Grid.OnTileStateChanged?.Invoke(this);

        TryPlaySfx(State);
        TryPlayVfx(State);

        if (vfx != null)
        {
            Instantiate(vfx, transform);
        }

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

    private void TryPlayVfx(TileState state)
    {
        if (HousedObject)
        {
            var stateOverrides = HousedObject.GetOverrides(state);
            if (stateOverrides.Vfx.UseOverride && stateOverrides.Vfx.Value != null)
            {
                Instantiate(stateOverrides.Vfx.Value, transform);
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
        
        foreach (var keyValuePair in SpecialAnnotations)
        {
            keyValuePair.Value.SetActive(false);
        }
        
        UpdateObjectSelfVisuals();

        if (GetHousedObject() == null)
        {
            bool hasMysticalMagnifyingGlass = ServiceLocator.Instance.Player.Inventory.HasItem(ItemSchema.Id.MysticMagnifyingGlass);
            if (ObscureCounter > 0 && !hasMysticalMagnifyingGlass)
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
        TryPlayVfx(State);

        int neighborEnemies = ServiceLocator.Instance.Grid.TEMP_GetUnconqueredNeighborCount(XCoordinate, YCoordinate);
        
        if (State == TileState.Empty)
        {
            // TODO: HACK - need to remove any fully empty tile when it is revealed to make Flee work
            ServiceLocator.Instance.Grid.UnoccupiedSpaces.RemoveUnoccupiedSpace(XCoordinate, YCoordinate);
            
            if (HousedObject && HousedObject.NumNeighborDropReward.TryGetValue(neighborEnemies, out TileSchema reward)) {
                UndoPlacedTileObj();
                PlaceTileObj(reward);
                TEMP_SetState(TileState.Revealed);
                return;
            }
            
            if (HousedObject && HousedObject.DropReward)
            {
                UndoPlacedTileObj();
                PlaceTileObj(HousedObject.DropReward);
                TEMP_SetState(TileState.Revealed);
                return;
            }
        }

        TEMP_UpdateVisuals();

        // Early return if housed object does not exist.
        if (!HousedObject && (State == TileState.Revealed || State == TileState.RevealThroughCombat))
        {
            TEMP_SetState(TileState.Empty);
            return;
        }

        Player player = ServiceLocator.Instance.Player;
        int basePower = GetBasePower();
        int adjustedPower = GetAdjustedPower();
        if (TileState.Conquered == State)
        {
            if (AllowDamage)
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
                    // HACK: We dont currently support this weird edge case, so do it here
                    if (HousedObject.TileId == TileSchema.Id.Mimic)
                    {
                        HousedObjectSprite.sprite = HousedObject.GetOverrides(TileState.Conquered).Sprite.Value;
                    }
                    return;
                }
            }

            if (HousedObject.TileId == TileSchema.Id.Balrog)
            {
                ServiceLocator.Instance.AchievementSystem.CheckAchievements(AchievementSchema.TriggerType.DemonLord);
            }

            if (!HousedObject.GetOverrides(TileState.Conquered).Sfx.UseOverride)
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

            // TODO: Find a better spot for this
            if (HousedObject &&
                HousedObject.Tags.Contains(TileSchema.Tag.Enemy) &&
                ServiceLocator.Instance.Player.ClassSchema.HitEffect != null
            )
            {
                Instantiate(ServiceLocator.Instance.Player.ClassSchema.HitEffect, transform);
            }

            // Associated guard gets enraged when yourself is conquered
            if (BodyGuardedByTile != null && BodyGuardedByTile.State < TileState.Conquered)
            {
                BodyGuardedByTile.IsBodyGuardByOrGuardingDead = true;
                BodyGuardedByTile.LookTowardsHorizontally(XCoordinate, YCoordinate, true, false);
                BodyGuardedByTile.TEMP_UpdateVisuals();
            }

            if (GuardingTile != null && GuardingTile.State < TileState.Conquered)
            {
                GuardingTile.IsBodyGuardByOrGuardingDead = true;
            }

            // Flee as soon as clicked
            // Flee -> Itself moves to another location if possible (Gnome)
            if (HousedObject.CanFlee && ServiceLocator.Instance.Grid.TEMP_HandleFlee(HousedObject, HousedObject.RevealFlee))
            {
                TEMP_SetState(TileState.Empty);
                if (HousedObject.FleeVfx != null)
                {
                    Instantiate(HousedObject.FleeVfx, transform);
                }

                if (!string.IsNullOrEmpty(HousedObject.FleeSfx))
                {
                    ServiceLocator.Instance.AudioManager.PlaySfx(HousedObject.FleeSfx);
                }
                return;
            }
        }

        if (TileState.Collected == State)
        {
            // Fleeing Child -> Spawns a new object type at another location if possible (Faerie)
            
            if (HousedObject.SpawnsFleeingChild)
            {
                if (HousedObject.FleeVfx != null)
                {
                    Instantiate(HousedObject.FleeVfx, transform);
                }
                if (!string.IsNullOrEmpty(HousedObject.FleeSfx))
                {
                    ServiceLocator.Instance.AudioManager.PlaySfx(HousedObject.FleeSfx);
                }
                
                bool newLocation =
                    ServiceLocator.Instance.Grid.TEMP_HandleFlee(HousedObject.FleeingChild, HousedObject.RevealFlee);

                // Special case: If it cant find a spot to spawn the fleeing child, spawns the child at current location
                // to ensure it spawns somewhere
                if (!newLocation)
                {
                    PlaceTileObj(HousedObject.FleeingChild);
                    TEMP_SetState(TileState.Hidden);
                    TEMP_RevealWithoutLogic();
                    return;
                }
                
                TEMP_SetState(TileState.Empty);
                return;
            }

            int revealOriginX = XCoordinate;
            int revealOriginY = YCoordinate;

            if (HousedObject.RevealRadius > 0)
            {
                ServiceLocator.Instance.Grid.TEMP_RevealTilesInRadius(
                    revealOriginX,
                    revealOriginY,
                    HousedObject.RevealRadius,
                    HousedObject.RevealVfx
                );
            }

            if (HousedObject.RevealOffsets != null && HousedObject.RevealOffsets.Length > 0)
            {
                if (HousedObject.RevealOffsetCount == -1)
                {
                    ServiceLocator.Instance.Grid.TEMP_RevealTiles(
                        revealOriginX,
                        revealOriginY,
                        HousedObject.RevealOffsets,
                        HousedObject.RevealVfx
                    );
                }
                else
                {
                    List<Vector2Int> offsets = new List<Vector2Int>();
                    List<Vector2Int> offsetsRemaining = new List<Vector2Int>();
                    offsetsRemaining.AddRange(HousedObject.RevealOffsets);
                    for (int i = 0; offsetsRemaining.Count > 0 && i < HousedObject.RevealOffsetCount; i++)
                    {
                        var offset = offsetsRemaining.GetRandomItem();
                        offsetsRemaining.Remove(offset);
                        offsets.Add(offset);
                    }
                    
                    ServiceLocator.Instance.Grid.TEMP_RevealTiles(
                        revealOriginX,
                        revealOriginY,
                        offsets.ToArray(),
                        HousedObject.RevealVfx
                    );
                }
                
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
                ServiceLocator.Instance.Grid.TEMP_DiffuseMines();
            }

            // Try to swap tiles
            // It is important to do this before any reveal rewards
            // TODO: Incorporate the Any state for better control. For now this works...
            foreach (var entry in HousedObject.TileUpdateReward)
            {
                if (entry.Amount <= 0)
                {
                    continue;
                }

                for (int i = 0; i < entry.Amount; i++)
                {
                    ServiceLocator.Instance.Grid.UpdateRandomTileById(entry.From, entry.To);
                }
            }
            
            if (HousedObject.RevealAllRewards != null && HousedObject.RevealAllRewards.Length > 0)
            {
                foreach (var revealReward in HousedObject.RevealAllRewards)
                {
                    // TODO Remove HACK: George added a bug here with sprites, going to do standup only for the
                    // rat scroll for now since im too lazy to fix 
                    bool standUp = HousedObject.TileId == TileSchema.Id.ScrollRat;
                    ServiceLocator.Instance.Grid.TEMP_RevealAllOfType(revealReward, standUp, HousedObject.RevealVfx);
                }
            }

            if (HousedObject.ItemsToReplenish != null && HousedObject.ItemsToReplenish.Length > 0)
            {
                ServiceLocator.Instance.Player.Inventory.ReplenishItems(HousedObject.ItemsToReplenish);
            }

            // Specific item reward
            if (HousedObject.ItemReward != ItemSchema.Id.None)
            {
                ItemInstance itemInstance = ServiceLocator.Instance.Player.Inventory.AddItem(HousedObject.ItemReward);
                if (itemInstance != null)
                {
                    ServiceLocator.Instance.Player.TrackItemForDungeon(itemInstance);
                    ServiceLocator.Instance.TutorialManager.TryShowTutorial(TutorialManager.TutorialId.Item);
                }
            }

            if (HousedObject.NumNeighborItemReward.TryGetValue(neighborEnemies, out ItemSchema.Id itemId))
            {
                ItemInstance itemInstance = ServiceLocator.Instance.Player.Inventory.AddItem(itemId);
                if (itemInstance != null)
                {
                    ServiceLocator.Instance.Player.TrackItemForDungeon(itemInstance);
                    ServiceLocator.Instance.TutorialManager.TryShowTutorial(TutorialManager.TutorialId.Item);
                }
            }

            // Random reward from Rarity
            if (HousedObject.ItemRewardRarities != null)
            {
                var matchingItems = ServiceLocator.Instance.Schemas.ItemSchemas.FindAll(item =>
                    HousedObject.ItemRewardRarities.Contains(item.Rarity));
                
                // Remove any item that is locked
                var lockedItemIds = ServiceLocator.Instance.AchievementSystem.GetLockedItems();
                foreach (var lockedItemId in lockedItemIds)
                {
                    matchingItems.RemoveAll(schema => schema.ItemId == lockedItemId);
                }
                
                // Try to adhere to maximums
                matchingItems.RemoveAll(schema =>
                    schema.Max != -1 && ServiceLocator.Instance.Player.Inventory.GetItemCount(schema.ItemId) >= schema.Max
                );
                
                if (matchingItems.Count > 0)
                {
                    var rewardItem = matchingItems.GetRandomItem();
                    ItemInstance itemInstance = ServiceLocator.Instance.Player.Inventory.AddItem(rewardItem.ItemId);
                    if (itemInstance != null)
                    {
                        ServiceLocator.Instance.Player.TrackItemForDungeon(itemInstance);
                    }
                    
                    ServiceLocator.Instance.TutorialManager.TryShowTutorial(TutorialManager.TutorialId.Item);
                }
            }

            if (HousedObject.ChildUpdateReward.Amount > 0)
            {
                ChildrenTiles.Shuffle();
                int transformed = 0;
                for (int i = 0; transformed < HousedObject.ChildUpdateReward.Amount && i < ChildrenTiles.Length; i++)
                {
                    var childTile = ChildrenTiles[i];
                    if (childTile.GetHousedObject().TileId != HousedObject.ChildUpdateReward.From)
                    {
                        continue;
                    }

                    transformed++;
                    ServiceLocator.Instance.Grid.UpdateTile(ChildrenTiles[i], HousedObject.ChildUpdateReward.To);
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
            if (State == TileState.Revealed)
            {
                TEMP_SetState(TileState.Conquered);
            }
            else
            {
                TEMP_SetState(State + 1);
            }
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
        if (enrage && HousedObject.CanEnrage)
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
        return state == TileState.Revealed || state == TileState.Collected || state == TileState.RevealThroughCombat;
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
            case TileState.RevealThroughCombat:
                Power.enabled = true;
                NeighborPower.enabled = false;
                HousedObjectSprite.enabled = true;
                XSpriteRenderer.enabled = false;
                Annotation.enabled = false;
                foreach (var keyValuePair in SpecialAnnotations)
                {
                    keyValuePair.Value.SetActive(false);
                }
                ExclamationMarker.enabled = IsEnraged;
                break;

            case TileState.Conquered:
                Power.enabled = true;
                NeighborPower.enabled = false;
                HousedObjectSprite.enabled = true;
                XSpriteRenderer.enabled = true;
                Annotation.enabled = false;
                foreach (var keyValuePair in SpecialAnnotations)
                {
                    keyValuePair.Value.SetActive(false);
                }
                ExclamationMarker.enabled = IsEnraged;
                break;

            case TileState.Collected:
                Power.enabled = false;
                NeighborPower.enabled = true;
                HousedObjectSprite.enabled = false;
                XSpriteRenderer.enabled = false;
                Annotation.enabled = false;
                foreach (var keyValuePair in SpecialAnnotations)
                {
                    keyValuePair.Value.SetActive(false);
                }
                ExclamationMarker.enabled = false;
                break;

            case TileState.Empty:
                Power.enabled = false;
                NeighborPower.enabled = true;
                HousedObjectSprite.enabled = false;
                XSpriteRenderer.enabled = false;
                Annotation.enabled = false;
                foreach (var keyValuePair in SpecialAnnotations)
                {
                    keyValuePair.Value.SetActive(false);
                }
                ExclamationMarker.enabled = false;
                TileButton.interactable = false;
                break;
        }

        // Allow the object itself to override these settings
        UpdateObjectSelfVisuals();

        bool hasMysticalMagnifyingGlass = ServiceLocator.Instance.Player.Inventory.HasItem(ItemSchema.Id.MysticMagnifyingGlass);
        if (ObscureCounter > 0 && !hasMysticalMagnifyingGlass)
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

        HousedObjectSprite.sprite = HousedObject ? HousedObject.TEMP_GetSprite(ShouldStandUp, DirectionToLook, IsBodyGuardByOrGuardingDead) : null;

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

        Power.SetText(HousedObject ? State < TileState.Conquered
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
        if (State != TileState.Hidden)
        {
            return;
        }

        // Setting allows left-hold to open context menu
        if (eventData.button == PointerEventData.InputButton.Left && FBPP.GetBool(PlayerOptions.AllowLeftHoldContextMenu, true))
        {
            MobileContextMenuHandle = StartCoroutine(nameof(HandleMobileContextMenu));
            return;
        }

        // Right click should open the menu to set player tag
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            //ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.TileContextMenu);
            ServiceLocator.Instance.OverlayScreenManager.RequestToggleScreen(OverlayScreenManager.ScreenType.TileContextMenu);
            ServiceLocator.Instance.TileContextMenu.SetActiveTile(this);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelMobileContextMenu();
    }

    private void CancelMobileContextMenu()
    {
        if (MobileContextMenuHandle != null)
        {
            StopCoroutine(MobileContextMenuHandle);
            MobileContextMenuHandle = null;
        }
    }

    private IEnumerator HandleMobileContextMenu()
    {
        yield return new WaitForSeconds(0.5f);
        
        ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.TileContextMenu);
        ServiceLocator.Instance.TileContextMenu.SetActiveTile(this);
    }

    /// <summary>
    /// Sets the player annotation. The power the player thinks the tile is.
    /// </summary>
    /// <param name="annotation"></param>
    public void SetAnnotation(int power)
    {
        // Handle special annotations first
        if (SpecialAnnotations.ContainsKey(power))
        {
            Annotation.SetText(string.Empty);
            SpecialAnnotations[power].SetActive(!SpecialAnnotations[power].activeInHierarchy);
            
            // Then we must do the rest of the images and hide them
            foreach (var keyValuePair in SpecialAnnotations)
            {
                if (keyValuePair.Key == power)
                {
                    continue;
                }
                keyValuePair.Value.SetActive(false);
            }
            
            return;
        }
        
        foreach (var keyValuePair in SpecialAnnotations)
        {
            keyValuePair.Value.SetActive(false);
        }
        
        if (Annotation.text.Equals(power.ToString()))
        {
            Annotation.SetText(string.Empty);
            return;
        }

        Annotation.SetText(power.ToString());
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