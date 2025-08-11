using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay;
using Schemas;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

// TODO: Separate the UI from the business logic. One should be in UI space, and one should be in Gameplay space
// We should have Player, and PlayerView/PlayerUI
// This is currently both
public class Player : MonoBehaviour, IPointerClickHandler
{
    // TODO: Data-fy this a bit better? ughhh this is what we get for vibe coding lol
    [SerializeField] 
    private GameObject DefaultVisionVfx;
    
    [HideInInspector]
    public List<TileSchema.Id> AutoRevealedMonsters = new();
    
    [HideInInspector]
    public List<TileSchema.Tag> AutoRevealedMonstersByTag = new();

    [Tooltip("Player ability, bonus spawn count. Key is spawned creation id.")]
    public Dictionary<TileSchema.Id, int> BonusSpawn = new();
    
    public Dictionary<(TileSchema.Id, TileSchema.Id), int> TileSwaps = new();
    public Dictionary<(TileSchema.Tag, TileSchema.Id), int> TileSwapsByTag = new();

    [SerializeField] 
    private Image PlayerIcon;
    
    [SerializeField] 
    private GameObject LevelUpRoot;

    [SerializeField]
    private Transform HeartContainer;

    [SerializeField]
    private Transform XPContainer;

    private int Level;
    private int CurrentHealth;
    private int MaxHealth;
    private int Shield;

    private int BonusMaxHp = 0;
    private int BonusStartXp = 0;
    private int SecondWindRegeneration = 0;
    
    //TODO: Merge these concepts (id and Tag)
    // By id, "global" is an overall id
    public Dictionary<TileSchema.Id, int> ModDamageTaken = new();
    public Dictionary<TileSchema.Id, int> ModXp = new();
    
    public Dictionary<TileSchema.Tag, int> ModDamageTakenByTag = new();
    public Dictionary<TileSchema.Tag, int> ModXpByTag = new();
    
    // TODO: Support DecayingEffects on ALL types. Currently only support ModDamage
    private List<Effect> DecayingEffects = new();
    
    public Class.Id Class = Gameplay.Class.Id.Adventurer;
    public ClassSchema ClassSchema;
    public Inventory Inventory;
    
    private bool IsGod = false;
    private bool HasRegeneratedThisRound = false;

    [SerializeField]
    [ReadOnly]
    private int CurrentXP;

    public int ModXpCurve = 0;
    
    public Dictionary<TileSchema.Id, int> TileObjectsThatShouldUpgrade = new();
    
    private Dictionary<TileSchema.Id, int> Kills = new Dictionary<TileSchema.Id, int>();
    
    public int ShopXp
    {
        get { return m_shopXp; }
        set
        {
            if (m_shopXp != value)
            {
                m_shopXp = value;
                if (OnShopXpChanged != null)
                {
                    OnShopXpChanged();
                }
            }
        }
    }

    public bool IsHardcore = true;
    
    public Action<int> OnLevelChanged;
    public Action<TileSchema> OnConquer;
    public Action<TileSchema> OnHeal;       // does not fire when healed from level ups

    private int m_shopXp;
    public OnPlayerPropertyChanged OnShopXpChanged;
    public delegate void OnPlayerPropertyChanged();
    
    // TODO: Spawn these in dynamically, im just lazy atm
    private PlayerUIItem[] Hearts;
    private PlayerUIItem[] XPGems;
    
    private List<ItemInstance> ItemsAddedThisDungeon = new();

    private void Awake()
    {
        ServiceLocator.Instance.Register(this);
        
        Hearts = HeartContainer.GetComponentsInChildren<PlayerUIItem>();
        XPGems = XPContainer.GetComponentsInChildren<PlayerUIItem>();
        
        Inventory = new Inventory(true);
        
        // TODO: Make this system better
        ModDamageTaken.Add(TileSchema.Id.Global, 0);
        ModXp.Add(TileSchema.Id.Global, 0);

        IsHardcore = true;
    }

    private void Start()
    {
        ServiceLocator.Instance.Grid.OnGridGenerated += OnGridGenerated;
        ServiceLocator.Instance.LevelManager.OnLevelChanged += OnDungeonLevelChanged;
        ResetPlayer();
    }

    private void OnDestroy()
    {
        ServiceLocator.Instance.Grid.OnGridGenerated -= OnGridGenerated;
        ServiceLocator.Instance.LevelManager.OnLevelChanged -= OnDungeonLevelChanged;
    }

    // TODO: Fix hack....retry has a lot of holes :(
    public void TrackItemForDungeon(ItemInstance item)
    {
        ItemsAddedThisDungeon.Add(item);
    }
    
    private void OnGridGenerated()
    {
        // Do the reveal abilities
        foreach (var monsterId in AutoRevealedMonsters)
        {
            ServiceLocator.Instance.Grid.RevealRandomOfType(monsterId, DefaultVisionVfx);
        }
        foreach (var monsterTag in AutoRevealedMonstersByTag)
        {
            ServiceLocator.Instance.Grid.RevealRandomOfTag(monsterTag, DefaultVisionVfx);
        }
        
        // Clear any state for each dungeon run
        Kills.Clear();
        

        // TODO: Should decaying effects be cleared here??
    }
    
    private void OnDungeonLevelChanged(int newLevel)
    {
        AdvanceEffects(null, DecayTrigger.DungeonLevel);
    }

    public int GetKillCount(TileSchema.Id tileId)
    {
        return Kills.GetValueOrDefault(tileId, 0);
    }
    
    public void TEMP_SetClass(Class.Id classId)
    {
        ClassSchema schema = ServiceLocator.Instance.Schemas.ClassSchemas.Find(c => c.Id == classId);
        if (schema == null)
        {
            return;
        }
     
        Class = classId;
        ClassSchema = schema;
        PlayerIcon.sprite = schema.Sprite;

        Inventory.AddItem(schema.StartingItem);
        
        ResetPlayer();
    }
    
    public bool TEMP_PredictDeath(int amount)
    {
        return CurrentHealth + Shield - amount < 0;
    }

    public int GetAdjustedDamage(TileSchema source, int amount)
    {
        if (source != null)
        {
            if (source.Tags.Contains(TileSchema.Tag.Enemy))
            {
                amount += ModDamageTaken[TileSchema.Id.Global];
            }

            if (ModDamageTaken.TryGetValue(source.TileId, out int value))
            {
                amount += value;
            }
            
            foreach (var sourceTag in source.Tags)
            {
                if (ModDamageTakenByTag.TryGetValue(sourceTag, out int tagValue))
                {
                    amount += tagValue;
                }
            }
        }

        // Do not let it go below 0
        return Math.Max(0, amount);
    }

    /// <summary>
    /// A bespoke function that deals damage to the player. You must provide a source to do any item checks.
    /// Returns true if the player dies.
    /// The amount should always be positive.
    /// </summary>
    public bool Damage(TileSchema source, int amount)
    {
        // Do not allow negative damage. 0 should be okay because of potential items
        if (amount < 0)
        {
            amount = 0;
        }
        
        // Now deal with item bonuses
        amount = GetAdjustedDamage(source, amount);
        
        // Handle shield first
        if (amount >= Shield)
        {
            amount -= Shield;
            Shield = 0;
        }
        else
        {
            Shield -= amount;
            amount = 0;
        }
        
        CurrentHealth = Math.Max(-1, CurrentHealth - amount);
        
        // Special Case: Regeneration power (currently unused)
        if (CurrentHealth < 0 && !HasRegeneratedThisRound && SecondWindRegeneration > 0)
        {
            HasRegeneratedThisRound = true;
            CurrentHealth = SecondWindRegeneration;
        }
        
        TEMP_UpdateVisuals();
        
        // Handle death
        if (CurrentHealth <= -1)
        {
            ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.GameOver);
            StartCoroutine(ServiceLocator.Instance.Grid.Shake());
            ServiceLocator.Instance.Grid.TEMP_RevealAllTiles();
            ServiceLocator.Instance.AudioManager.PlaySfx("Death");
            return true;
        }
        
        // TODO: Find a better home for this call... should not be occurring in health delta calcs
        if (source != null)
        {
            Kills.TryAdd(source.TileId, 0);
            Kills[source.TileId] += 1;

            AdvanceEffects(source, DecayTrigger.Conquer);
            OnConquer?.Invoke(source);
        }
        
        return false;
    }

    /// <summary>
    /// Heals the player for the specified amount.
    /// Amount should never be 0 or less.
    /// </summary>
    public void Heal(TileSchema source, int amount)
    {
        // Do not damage with negative heal
        if (amount <= 0)
        {
            return;
        }
        
        CurrentHealth = Math.Min(CurrentHealth + amount, MaxHealth);
        
        ServiceLocator.Instance.AudioManager.PlaySfx("Heal");
        
        TEMP_UpdateVisuals();

        OnHeal?.Invoke(source);
    }

    public void TEMP_UpdateXP(TileSchema source, int amount)
    {
        // Just in case, we shouldn't process any 0 or less (for now)
        amount = GetModifiedXp(source, amount);
        if (amount <= 0)
        {
            return;
        }
        
        CurrentXP += amount;
        TEMP_UpdateVisuals();
        
        ServiceLocator.Instance.AudioManager.PlaySfx("XP");
    }

    public int GetModifiedXp(TileSchema source, int amount)
    {
        if (source != null)
        {
            // TODO: FIX IN DATA FIRST
            // global bonuses currently only for enemies
            if (source.Tags.Contains(TileSchema.Tag.Enemy))
            {
                amount += ModXp[TileSchema.Id.Global];
            }

            if (ModXp.TryGetValue(source.TileId, out int value))
            {
                amount += value;
            }
        
            foreach (var sourceTag in source.Tags)
            {
                if (ModXpByTag.TryGetValue(sourceTag, out int tagValue))
                {
                    amount += tagValue;
                }
            }
        }

        return amount;
    }

    /// <summary>
    /// Updates all visuals for the player. We should probably make a real MVC
    /// </summary>
    private void TEMP_UpdateVisuals()
    {
        for (int i = 0; i < Hearts.Length; i++)
        {
            Hearts[i].SetHalf(false);
            Hearts[i].gameObject.SetActive(i < MaxHealth + Shield);
            Hearts[i].SetFull(i < CurrentHealth);
            Hearts[i].SetShield(i >= MaxHealth);
            Hearts[i].SetLabelText((i + 1).ToString());
        }
        
        var currentLevelHealth = ServiceLocator.Instance.Schemas.LevelProgression.GetMaxHealthForLevel(Level);
        var nextLevelHealth = ServiceLocator.Instance.Schemas.LevelProgression.GetMaxHealthForLevel(Level + 1);
        bool showHalfHeart = nextLevelHealth > currentLevelHealth;
        if (!IsGod && showHalfHeart)
        {
            Hearts[MaxHealth + Shield].gameObject.SetActive(true);
            Hearts[MaxHealth + Shield].SetHalf(true);
        }

        int xpRequiredToLevel = ServiceLocator.Instance.Schemas.LevelProgression.GetXPRequiredForLevel(Level) + ModXpCurve;
        for (int i = 0; i < XPGems.Length; i++)
        {
            XPGems[i].gameObject.SetActive(i < xpRequiredToLevel);
            XPGems[i].SetFull(CurrentXP > i);
            XPGems[i].SetLabelText((i + 1).ToString());
        }

        bool canLevel = CurrentXP >= xpRequiredToLevel;
        LevelUpRoot.SetActive(canLevel);

        if (canLevel)
        {
            ServiceLocator.Instance.TutorialManager.TryShowTutorial(TutorialManager.TutorialId.XP);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int xpRequiredToLevel = ServiceLocator.Instance.Schemas.LevelProgression.GetXPRequiredForLevel(Level) + ModXpCurve;
        if (CurrentXP >= xpRequiredToLevel)
        {
            CurrentXP -= xpRequiredToLevel;
            
            LevelUp();
            
            // TODO: refactor LevelUp vs ResetLevel so this can live in LevelUp()
            OnLevelChanged?.Invoke(Level);
        }
    }

    public void LevelUp()
    {
        Level++;

        MaxHealth = IsGod
            ? 99999
            : ServiceLocator.Instance.Schemas.LevelProgression.GetMaxHealthForLevel(Level) + BonusMaxHp;
        CurrentHealth = MaxHealth;

        TEMP_UpdateVisuals();
        
        ServiceLocator.Instance.AudioManager.PlaySfx("LevelUp");
        
        AdvanceEffects(null, DecayTrigger.PlayerLevel);
    }

    public void ToggleGodMode()
    {
        IsGod = !IsGod;

        MaxHealth = IsGod
            ? 99999
            : ServiceLocator.Instance.Schemas.LevelProgression.GetMaxHealthForLevel(Level) + BonusMaxHp;
        
        CurrentHealth = MaxHealth;
        TEMP_UpdateVisuals();
    }

    private void RevertAllDecayingEffects()
    {
        foreach (var decayingEffect in DecayingEffects)
        {
            switch (decayingEffect.Type)
            {
                case EffectType.ModDamageTaken:
                    UndoModDamageTaken(decayingEffect);
                    break;
                case EffectType.ModXp:
                    UndoModXp(decayingEffect);
                    break;
            }
        }
        
        DecayingEffects.Clear();
    }

    public void MarkPlayerSoftcore()
    {
        IsHardcore = false;
    }
    
    public void RevokeItemsForCurrentDungeon()
    {
        foreach (var itemInstance in ItemsAddedThisDungeon)
        {
            Inventory.RemoveItem(itemInstance);
        }
        ItemsAddedThisDungeon.Clear();
        
        // Remaining items themselves might want to do something special on retry
        Inventory.RevertForRetry();
    }
    
    public void ResetPlayer()
    {
        // Remove any decaying effects that may be lingering
        RevertAllDecayingEffects();
        ItemsAddedThisDungeon.Clear();
        
        Level = 0;
        CurrentXP = 0;
        Shield = 0;
        HasRegeneratedThisRound = false;
        
        MaxHealth = IsGod
            ? 99999
            : ServiceLocator.Instance.Schemas.LevelProgression.GetMaxHealthForLevel(Level) + BonusMaxHp;
        
        CurrentHealth = MaxHealth;
        
        // Apply any bonuses from items
        TEMP_UpdateXP(null, BonusStartXp);
        
        TEMP_UpdateVisuals();
        
        // All consumables are re-filled
        Inventory.ReplenishItems();
    }
    
    #region PlayerPowers
    public void AddMonsterToAutoRevealedList(TileSchema.Id monsterId)
    {
        AutoRevealedMonsters.Add(monsterId);
    }
    
    public void RemoveMonsterFromAutoRevealedList(TileSchema.Id monsterId)
    {
        AutoRevealedMonsters.Remove(monsterId);
    }
    
    public void AddMonsterToAutoRevealedByTagList(TileSchema.Tag tag)
    {
        AutoRevealedMonstersByTag.Add(tag);
    }
    
    public void RemoveMonsterToAutoRevealedByTagList(TileSchema.Tag tag)
    {
        AutoRevealedMonstersByTag.Remove(tag);
    }
    
    public void AddSpawnCount(TileSchema.Id id, int amount)
    {
        BonusSpawn.TryGetValue(id, out int bonusSpawn);
        bonusSpawn += amount;
        BonusSpawn[id] = bonusSpawn;
    }

    public void AddTileSwapByTagEntry(TileSchema.Tag fromTag, TileSchema.Id toTile, int amount)
    {
        TileSwapsByTag.TryGetValue((fromTag, toTile), out int swapCount);
        swapCount += amount;
        TileSwapsByTag[(fromTag, toTile)] = swapCount;
    }
    
    public void AddTileSwapEntry(TileSchema.Id fromTile, TileSchema.Id toTile, int amount)
    {
        TileSwaps.TryGetValue((fromTile, toTile), out int swapCount);
        swapCount += amount;
        TileSwaps[(fromTile, toTile)] = swapCount;
    }
    
    public void AddBonusStartHp(int amount)
    {
        BonusMaxHp += amount;
    }
    
    public void AddBonusStartXp(int amount)
    {
        BonusStartXp += amount;
    }

    public void AddPlayerRegeneration()
    {
        SecondWindRegeneration++;
    }

    public void AddShield(int amount)
    {
        Shield += amount;
        TEMP_UpdateVisuals();
    }
    
    public void AddOrIncrementTileLevel(TileSchema.Id ObjectId)
    {
        int level = 0;
        TileObjectsThatShouldUpgrade.TryGetValue(ObjectId, out level);
        TileObjectsThatShouldUpgrade[ObjectId] = level + 1;
    }

    public void DecrementTileLevel(TileSchema.Id ObjectId)
    {
        int level = 0;
        if (TileObjectsThatShouldUpgrade.TryGetValue(ObjectId, out level))
        {
            TileObjectsThatShouldUpgrade[ObjectId] = level - 1;
        }
    }
    
    public void AddModDamageTaken(Effect effect)
    {
        var effectAmount = effect.Amount;
        
        var tileId = effect.Id;
        if (tileId != TileSchema.Id.None && !ModDamageTaken.TryAdd(tileId, effectAmount))
        {
            ModDamageTaken[tileId] += effectAmount;
        }

        if (effect.Tags != null)
        {
            foreach (var effectTag in effect.Tags)
            {
                if (!ModDamageTakenByTag.TryAdd(effectTag, effectAmount))
                {
                    ModDamageTakenByTag[effectTag] += effectAmount;
                }
            }
        }

        if (effect.Decay > 0)
        {
            DecayingEffects.Add(effect);
        }
    }
    
    public void UndoModDamageTaken(Effect effect)
    {
        var effectAmount = effect.Amount;
        
        var tileId = effect.Id;
        if (tileId != TileSchema.Id.None && ModDamageTaken.ContainsKey(tileId))
        {
            ModDamageTaken[tileId] -= effectAmount;
        }

        if (effect.Tags != null)
        {
            foreach (var effectTag in effect.Tags)
            {
                if (ModDamageTakenByTag.ContainsKey(effectTag))
                {
                    ModDamageTakenByTag[effectTag] -= effectAmount;
                }
            }
        }
    }
    
    public void AddModXp(Effect effect)
    {
        var effectAmount = effect.Amount;
        
        var tileId = effect.Id;
        if (tileId != TileSchema.Id.None && !ModXp.TryAdd(tileId, effectAmount))
        {
            ModXp[tileId] += effectAmount;
        }

        if (effect.Tags != null)
        {
            foreach (var effectTag in effect.Tags)
            {
                if (!ModXpByTag.TryAdd(effectTag, effectAmount))
                {
                    ModXpByTag[effectTag] += effectAmount;
                }
            }
        }

        if (effect.Decay > 0)
        {
            DecayingEffects.Add(effect);
        }
    }

    public void UndoModXp(Effect effect)
    {
        var effectAmount = effect.Amount;
        
        var tileId = effect.Id;
        if (tileId != TileSchema.Id.None && ModXp.ContainsKey(tileId))
        {
            ModXp[tileId] = Mathf.Max(0, ModXp[tileId] - effectAmount);
        }

        if (effect.Tags != null)
        {
            foreach (var effectTag in effect.Tags)
            {
                if (ModXpByTag.ContainsKey(effectTag))
                {
                    ModXpByTag[effectTag] = Mathf.Max(0, ModXpByTag[effectTag]  - effectAmount);
                }
            }
        }
    }

    public void AdvanceEffects(TileSchema source, DecayTrigger decayTrigger)
    {
        for (var i = DecayingEffects.Count - 1; i >= 0; i--)
        {
            var  effect = DecayingEffects[i];

            if (effect.DecayTrigger != decayTrigger)
            {
                continue;
            }

            if (effect.DecayTags != null && effect.DecayTags.Count > 0)
            {
                if (!source.Tags.Intersect(effect.DecayTags).Any())
                {
                    continue;
                }
            }
            
            effect.Decay--;
            DecayingEffects[i] = effect;

            if (effect.Decay > 0)
            {
                continue;
            }

            switch (effect.Type)
            {
                case EffectType.ModXp:
                    UndoModXp(effect);
                    break;
                
                case EffectType.ModDamageTaken:
                    UndoModDamageTaken(effect);
                    break;
            }
            
            DecayingEffects.RemoveAt(i);
        }
    }
    
    #endregion
    
}