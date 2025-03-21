using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] 
    private Image PlayerIcon;

    // TODO: Create a mvc/settings/player scriptable object instead
    [SerializeField] 
    private int MaxHealth = 5;

    [SerializeField]
    private int MaxXP = 4;

    [SerializeField]
    private Transform HeartContainer;

    [SerializeField]
    private Transform XPContainer;

    private int Level;
    private int CurrentHealth;
    private int CurrentXP;

    // TODO: Spawn these in dynamically, im just lazy atm
    private PlayerUIItem[] Hearts;
    private PlayerUIItem[] XPGems;

    private void Start()
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
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
        TEMP_UpdateVisuals();
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

        // TODO: XP scale system. Dont go over 20 xp or you crash kekw
        for (int i = 0; i < XPGems.Length; i++)
        {
            XPGems[i].SetFull(CurrentXP > i);
        }
    }
}