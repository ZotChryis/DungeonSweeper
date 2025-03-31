using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// TODO: Create a mvc/settings/player scriptable object instead
public class Player : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] 
    private Image PlayerIcon;
    
    [SerializeField] 
    private Image LevelUpIcon;
    
    [SerializeField] 
    private int MaxHealth = 5;

    [SerializeField]
    private int MaxXP = 4;

    [SerializeField]
    private Transform HeartContainer;

    [SerializeField]
    private Transform XPContainer;

    private int Level;
    private int LevelLap;
    private int CurrentHealth;
    private int CurrentXP;

    // TODO: Spawn these in dynamically, im just lazy atm
    private PlayerUIItem[] Hearts;
    private PlayerUIItem[] XPGems;

    private void Awake()
    {
        ServiceLocator.Instance.Register(this);

        Hearts = HeartContainer.GetComponentsInChildren<PlayerUIItem>();
        XPGems = XPContainer.GetComponentsInChildren<PlayerUIItem>();

        Level = 1;
        CurrentXP = 0;
        CurrentHealth = MaxHealth;

        TEMP_UpdateVisuals();
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
            Hearts[i].gameObject.SetActive(MaxHealth - 1 >= i);
            Hearts[i].SetFull(CurrentHealth > i);
        }

        int xpRequiredToLevel = ServiceLocator.Instance.Schemas.LevelProgression.GetXPRequiredForLevel(Level);
        for (int i = 0; i < XPGems.Length; i++)
        {
            XPGems[i].gameObject.SetActive(i < xpRequiredToLevel);
            XPGems[i].SetFull(CurrentXP > i);
        }

        LevelUpIcon.enabled = CurrentXP >= xpRequiredToLevel;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int xpRequiredToLevel = ServiceLocator.Instance.Schemas.LevelProgression.GetXPRequiredForLevel(Level);
        if (CurrentXP >= xpRequiredToLevel)
        {
            CurrentXP -= xpRequiredToLevel;
            XPLap();
        }
    }

    public void XPLap()
    {
        LevelLap++;

        int requiredLaps = ServiceLocator.Instance.Schemas.LevelProgression.GetLapsForLevel(Level);
        if (LevelLap >= requiredLaps)
        {
            LevelLap = 0;
            LevelUp();
            return;
        }

        MaxHealth = ServiceLocator.Instance.Schemas.LevelProgression.GetMaxHealthForLevel(Level);
        CurrentHealth = MaxHealth;
        TEMP_UpdateVisuals();
    }

    public void LevelUp()
    {
        Level++;
        
        MaxHealth = ServiceLocator.Instance.Schemas.LevelProgression.GetMaxHealthForLevel(Level);
        CurrentHealth = MaxHealth;

        TEMP_UpdateVisuals();
    }
}