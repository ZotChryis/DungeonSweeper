using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// TODO: Create a mvc/settings/player scriptable object instead
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
    private int CurrentXP;

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
        
        LevelUp();

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
    #endregion

    public bool TEMP_PredictDeath(int amount)
    {
        return CurrentHealth - amount < 0;
    }
    
    public void TEMP_UpdateHealth(int amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, -1, MaxHealth);
        TEMP_UpdateVisuals();

        if (CurrentHealth == -1)
        {
            ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.GameOver);
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
            Hearts[i].gameObject.SetActive(i < MaxHealth);
            Hearts[i].SetFull(CurrentHealth > i);
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
        CurrentHealth = MaxHealth;

        TEMP_UpdateVisuals();
    }

    public void GodMode()
    {
        MaxHealth = 999999;
        CurrentHealth = MaxHealth;
    }
}