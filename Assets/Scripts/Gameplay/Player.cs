using System;
using System.Collections.Generic;
using Gameplay;
using Schemas;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// TODO: Separate the UI from the business logic. One should be in UI space, and one should be in Gameplay space
// We should have Player, and PlayerView/PlayerUI
// This is currently both
public class Player : MonoBehaviour, IPointerClickHandler
{
    public List<TileSchema.Id> RevealedMonsters = new();

    [Tooltip("Player ability, bonus spawn count. Key is spawned creation id.")]
    public Dictionary<TileSchema.Id, int> BonusSpawn = new();

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
    public int BonusStartingHp = 0;
    public int BonusStartXp = 0;
    public int SecondWindRegeneration = 0;
    
    //TODO: Merge these concepts (id and Tag)
    // By id, "global" is an overall id
    public Dictionary<TileSchema.Id, int> ModDamageTaken = new();
    public Dictionary<TileSchema.Id, int> ModXp = new();
    
    public Dictionary<TileSchema.Tag, int> ModDamageTakenByTag = new();
    public Dictionary<TileSchema.Tag, int> ModXpByTag = new();
    
    public Class.Id Class = Gameplay.Class.Id.Adventurer;
    public Inventory Inventory;

    /// <summary>
    /// Bonus maxHP added to the level up table.
    /// Mostly, for god mode.
    /// </summary>
    private int BonusMaxHp = 0;

    // deprecated
    [Tooltip("If true, we prevent half damage from the very first 7 power demon.")]
    public bool HasDemonBanePowers = false;
    private bool HasUsedDemonBanePowers = false;
    private bool HasRegeneratedThisRound = false;
    private int CurrentXP;

    public HashSet<TileSchema.Id> TilesWhichShowNeighborPower = new();
    public Dictionary<TileSchema.Id, int> TileObjectsThatShouldUpgrade = new();
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

    public Action<int> OnLevelChanged;
    public Action<TileSchema> OnConquer;

    private int m_shopXp;
    public OnPlayerPropertyChanged OnShopXpChanged;
    public delegate void OnPlayerPropertyChanged();

    // TODO: Spawn these in dynamically, im just lazy atm
    private PlayerUIItem[] Hearts;
    private PlayerUIItem[] XPGems;

    private void Awake()
    {
        ServiceLocator.Instance.Register(this);
        
        Hearts = HeartContainer.GetComponentsInChildren<PlayerUIItem>();
        XPGems = XPContainer.GetComponentsInChildren<PlayerUIItem>();
        
        Inventory = new Inventory(true);
        
        // TODO: Make this system better
        ModDamageTaken.Add(TileSchema.Id.Global, 0);
        ModXp.Add(TileSchema.Id.Global, 0);
    }

    private void Start()
    {
        ServiceLocator.Instance.Grid.OnGridGenerated += OnGridGeneratedPlayerAbilities;
        ResetPlayer();
    }

    private void OnDestroy()
    {
        ServiceLocator.Instance.Grid.OnGridGenerated -= OnGridGeneratedPlayerAbilities;
    }

    private void OnGridGeneratedPlayerAbilities()
    {
        foreach (var monsterId in RevealedMonsters)
        {
            ServiceLocator.Instance.Grid.RevealRandomOfType(monsterId);
        }
    }
    
    public void TEMP_SetClass(Class.Id classId)
    {
        ClassSchema schema = ServiceLocator.Instance.Schemas.ClassSchemas.Find(c => c.Id == classId);
        if (schema == null)
        {
            return;
        }
     
        Class = classId;
        PlayerIcon.sprite = schema.Sprite;

        Inventory.AddItem(schema.StartingItem);
        
        ResetPlayer();
    }
    
    public bool TEMP_PredictDeath(TileSchema source, int amount)
    {
        amount = GetModifiedDamage(source, amount);
        return CurrentHealth - amount < 0;
    }

    public int GetModifiedDamage(TileSchema source, int amount)
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

        return amount;
    }

    public void HealPlayerNoOverheal(int amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, -1, (MaxHealth + BonusMaxHp));
        TEMP_UpdateVisuals();
    }

    /// <summary>
    /// Update health. Allow overhealing. Mostly for damaging player.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>true if the player is dead</returns>
    public bool UpdateHealth(TileSchema source, int amount)
    {
        // pre-process any weird enemy logic, pre items
        if (amount == -7 && HasDemonBanePowers && !HasUsedDemonBanePowers)
        {
            amount = -3;
        }
        
        // Now deal with item bonuses
        // TODO: we need a better way to deal with heals and damage
        if (amount < 0)
        {
            // Flip the damage to positive for calculations
            amount *= -1;
            
            // Get the modified damage result
            amount = GetModifiedDamage(source, amount);
            
            // Flip it back to negative for damage
            amount *= -1;
        }

        CurrentHealth = Mathf.Max(CurrentHealth + amount, -1);
        if (CurrentHealth == 0 && !HasRegeneratedThisRound)
        {
            HasRegeneratedThisRound = true;
            CurrentHealth += SecondWindRegeneration;
        }

        TEMP_UpdateVisuals();

        if (CurrentHealth <= -1)
        {
            ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.GameOver);
            StartCoroutine(ServiceLocator.Instance.Grid.Shake());
            ServiceLocator.Instance.Grid.TEMP_RevealAllTiles();
            return true;
        }
        
        OnConquer?.Invoke(source);
        return false;
    }

    public void TEMP_UpdateXP(TileSchema source, int amount)
    {
        amount = GetModifiedXp(source, amount);
        CurrentXP += amount;
        TEMP_UpdateVisuals();
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
            Hearts[i].gameObject.SetActive(i < Mathf.Max(MaxHealth, CurrentHealth));
            Hearts[i].SetFull(CurrentHealth > i);
            Hearts[i].SetGhostFull(CurrentHealth > i && i >= MaxHealth);
            Hearts[i].SetLabelText((i + 1).ToString());
        }

        int xpRequiredToLevel = ServiceLocator.Instance.Schemas.LevelProgression.GetXPRequiredForLevel(Level);
        for (int i = 0; i < XPGems.Length; i++)
        {
            XPGems[i].gameObject.SetActive(i < xpRequiredToLevel);
            XPGems[i].SetFull(CurrentXP > i);
            XPGems[i].SetLabelText((i + 1).ToString());
        }

        LevelUpRoot.SetActive(CurrentXP >= xpRequiredToLevel);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int xpRequiredToLevel = ServiceLocator.Instance.Schemas.LevelProgression.GetXPRequiredForLevel(Level);
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

        MaxHealth = ServiceLocator.Instance.Schemas.LevelProgression.GetMaxHealthForLevel(Level);
        CurrentHealth = MaxHealth + BonusMaxHp;

        TEMP_UpdateVisuals();
    }

    public void GodMode()
    {
        if (BonusMaxHp == 0)
        {
            BonusMaxHp = 999999;
        }
        else
        {
            BonusMaxHp = 0;
        }
        CurrentHealth = MaxHealth + BonusMaxHp;
        TEMP_UpdateVisuals();
    }

    public void ResetPlayer()
    {
        Level = 0;
        CurrentXP = 0;
        HasUsedDemonBanePowers = false;
        LevelUp();
        
        // Apply any bonuses from items
        UpdateHealth(null, BonusStartingHp);
        TEMP_UpdateXP(null, BonusStartXp);
    }
    
    #region PlayerPowers
    public void AddMonsterToAutoRevealedList(TileSchema.Id monsterId)
    {
        RevealedMonsters.Add(monsterId);
    }
    
    public void AddSpawnCount(TileSchema.Id id, int amount)
    {
        BonusSpawn.TryGetValue(id, out int bonusSpawn);
        bonusSpawn += amount;
        BonusSpawn[id] = bonusSpawn;
    }

    public void AddBonusStartHp(int amount)
    {
        BonusStartingHp += amount;
    }
    
    public void AddBonusStartXp(int amount)
    {
        BonusStartXp += amount;
    }

    public void AddPlayerRegeneration()
    {
        SecondWindRegeneration++;
    }

    public void AddDemonBanePower()
    {
        HasDemonBanePowers = true;
    }
    
    public void AddRevealNeighborPower(TileSchema.Id ObjectId)
    {
        TilesWhichShowNeighborPower.Add(ObjectId);
    }
    
    public void AddOrIncrementTileLevel(TileSchema.Id ObjectId)
    {
        int level = 0;
        TileObjectsThatShouldUpgrade.TryGetValue(ObjectId, out level);
        TileObjectsThatShouldUpgrade[ObjectId] = level + 1;
    }
    
    public void AddModDamageTaken(TileSchema.Id id, int effectAmount)
    {
        if (!ModDamageTaken.TryAdd(id, effectAmount))
        {
            ModDamageTaken[id] += effectAmount;
        }
    }

    public void AddModXp(TileSchema.Id id, int effectAmount)
    {
        if (!ModXp.TryAdd(id, effectAmount))
        {
            ModXp[id] += effectAmount;
        }
    }
    
    public void AddModDamageTakenByTag(TileSchema.Tag objectTag, int effectAmount)
    {
        if (!ModDamageTakenByTag.TryAdd(objectTag, effectAmount))
        {
            ModDamageTakenByTag[objectTag] += effectAmount;
        }
    }

    public void AddModXpByTag(TileSchema.Tag objectTag, int effectAmount)
    {
        if (!ModXpByTag.TryAdd(objectTag, effectAmount))
        {
            ModXpByTag[objectTag] += effectAmount;
        }
    }
    #endregion

}