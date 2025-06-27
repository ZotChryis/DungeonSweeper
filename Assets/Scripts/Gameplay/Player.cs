using System.Collections.Generic;
using System.Linq;
using Gameplay;
using Schemas;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

// TODO: Separate the UI from the business logic. One should be in UI space, and one should be in Gameplay space
// We should have Player, and PlayerView/PlayerUI
// This is currently both
public class Player : MonoBehaviour, IPointerClickHandler
{
    public List<string> RevealedMonsters = new List<string>();

    [Tooltip("Player ability, bonus spawn count. Key is spawned creation id.")]
    public Dictionary<string, int> BonusSpawn = new Dictionary<string, int>();

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
    public Dictionary<string, int> ModDamageTaken = new Dictionary<string, int>();
    public Dictionary<string, int> ModXp = new Dictionary<string, int>();
    
    public Dictionary<TileObjectSchema.Tag, int> ModDamageTakenByTag = new Dictionary<TileObjectSchema.Tag, int>();
    public Dictionary<TileObjectSchema.Tag, int> ModXpByTag = new Dictionary<TileObjectSchema.Tag, int>();
    
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

    public HashSet<string> TilesWhichShowNeighborPower = new HashSet<string>();
    public Dictionary<string, int> TileObjectsThatShouldUpgrade = new Dictionary<string, int>();
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
        ModDamageTaken.Add("global", 0);
        ModXp.Add("global", 0);
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
    
    public bool TEMP_PredictDeath(TileObjectSchema source, int amount)
    {
        amount = GetModifiedDamage(source, amount);
        return CurrentHealth - amount < 0;
    }

    public int GetModifiedDamage(TileObjectSchema source, int amount)
    {
        if (source != null)
        {
            if (source.Tags.Contains(TileObjectSchema.Tag.Enemy))
            {
                amount += ModDamageTaken["global"];
            }

            if (ModDamageTaken.TryGetValue(source.Id, out int value))
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
    public bool UpdateHealth(TileObjectSchema source, int amount)
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
        return false;
    }

    public void TEMP_UpdateXP(TileObjectSchema source, int amount)
    {
        amount = GetModifiedXp(source, amount);
        CurrentXP += amount;
        TEMP_UpdateVisuals();
    }

    public int GetModifiedXp(TileObjectSchema source, int amount)
    {
        if (source != null)
        {
            // global bonuses currently only for enemies
            if (source != null)
            {
                if (source.Tags.Contains(TileObjectSchema.Tag.Enemy))
                {
                    amount += ModXp["global"];
                }

                if (ModXp.TryGetValue(source.Id, out int value))
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
    public void AddMonsterToAutoRevealedList(string monsterId)
    {
        RevealedMonsters.Add(monsterId);
    }

    // TODO: DEPRECATE
    public void AddAndIncrementMonsterToBonusSpawn(string monsterId)
    {
        int bonusSpawn = 0;
        BonusSpawn.TryGetValue(monsterId, out bonusSpawn);
        bonusSpawn++;
        BonusSpawn[monsterId] = bonusSpawn;
    }

    public void AddSpawnCount(string id, int amount)
    {
        BonusSpawn.TryGetValue(id, out int bonusSpawn);
        bonusSpawn += amount;
        BonusSpawn[id] = bonusSpawn;
    }

    // TODO: DEPRECATE
    public void AddPlayerBonusStartingHp()
    {
        BonusStartingHp++;
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
    
    public void AddRevealNeighborPower(string ObjectId)
    {
        TilesWhichShowNeighborPower.Add(ObjectId.ToLower());
    }
    
    public void AddOrIncrementTileLevel(string ObjectId)
    {
        int level = 0;
        TileObjectsThatShouldUpgrade.TryGetValue(ObjectId, out level);
        TileObjectsThatShouldUpgrade[ObjectId] = level + 1;
    }
    
    public void AddModDamageTaken(string id, int effectAmount)
    {
        if (!ModDamageTaken.TryAdd(id, effectAmount))
        {
            ModDamageTaken[id] += effectAmount;
        }
    }

    public void AddModXp(string id, int effectAmount)
    {
        if (!ModXp.TryAdd(id, effectAmount))
        {
            ModXp[id] += effectAmount;
        }
    }
    
    public void AddModDamageTakenByTag(TileObjectSchema.Tag objectTag, int effectAmount)
    {
        if (!ModDamageTakenByTag.TryAdd(objectTag, effectAmount))
        {
            ModDamageTakenByTag[objectTag] += effectAmount;
        }
    }

    public void AddModXpByTag(TileObjectSchema.Tag objectTag, int effectAmount)
    {
        if (!ModXpByTag.TryAdd(objectTag, effectAmount))
        {
            ModXpByTag[objectTag] += effectAmount;
        }
    }
    #endregion

}