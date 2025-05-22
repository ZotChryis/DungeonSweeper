using System.Collections.Generic;
using Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// TODO: Separate the UI from the business logic
// We should have Player, and PlayerView/PlayerUI
// This is currently both
public class Player : MonoBehaviour, IPointerClickHandler
{
    public enum Class
    {
        Adventurer, // Standard 
        Warrior,
        Ranger,
        Wizard,
        Bard,
        FortuneTeller,
        Miner,
        Priest,
        Apothecary
    }
    
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
    public int HpRegeneration = 0;
    public Class PlayerClass = Class.Adventurer;
    public Inventory Inventory;

    /// <summary>
    /// Bonus maxHP added to the level up table.
    /// Mostly, for god mode.
    /// </summary>
    private int BonusMaxHp = 0;
    
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
    }

    private void Start()
    {
        Hearts = HeartContainer.GetComponentsInChildren<PlayerUIItem>();
        XPGems = XPContainer.GetComponentsInChildren<PlayerUIItem>();

        ResetPlayer();
        Inventory = new Inventory();

        ServiceLocator.Instance.Grid.OnGridGenerated += OnGridGeneratedPlayerAbilities;
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

    #region PlayerPowers
    public void AddMonsterToAutoRevealedList(string monsterId)
    {
        RevealedMonsters.Add(monsterId);
    }

    public void AddAndIncrementMonsterToBonusSpawn(string monsterId)
    {
        int bonusSpawn = 0;
        BonusSpawn.TryGetValue(monsterId, out bonusSpawn);
        bonusSpawn++;
        BonusSpawn[monsterId] = bonusSpawn;
    }

    public void AddPlayerBonusStartingHp()
    {
        BonusStartingHp++;
    }

    public void AddPlayerRegeneration()
    {
        HpRegeneration++;
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
    #endregion

    public bool TEMP_PredictDeath(int amount)
    {
        return CurrentHealth - amount < 0;
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
    public void TEMP_UpdateHealth(int amount)
    {
        if (amount == -7 && HasDemonBanePowers && !HasUsedDemonBanePowers)
        {
            amount = -3;
        }

        CurrentHealth = Mathf.Max(CurrentHealth + amount, -1);
        if (CurrentHealth == 0 && !HasRegeneratedThisRound)
        {
            HasRegeneratedThisRound = true;
            CurrentHealth += HpRegeneration;
        }

        TEMP_UpdateVisuals();

        if (CurrentHealth <= -1)
        {
            ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.GameOver);
            ServiceLocator.Instance.Grid.TEMP_RevealAllTiles();
            return;
        }
    }

    public void TEMP_UpdateXP(int amount)
    {
        CurrentXP += amount;
        TEMP_UpdateVisuals();
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
        TEMP_UpdateHealth(BonusStartingHp);
    }
}