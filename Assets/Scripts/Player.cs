using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] 
    private SpriteRenderer SpriteRenderer;

    // TODO: Create a settings/player scriptable object instead
    [SerializeField] 
    private int MaxHealth = 5;

    [SerializeField]
    private int MaxXP = 4;

    private int Level;
    private int CurrentHealth;
    private int CurrentXP;

    private void Start()
    {
        Level = 1;
        CurrentXP = 0;
        CurrentHealth = MaxHealth;

        TEMP_UpdateVisuals();
    }

    /// <summary>
    /// Updates all visuals for the player. We should probably make a real MVC
    /// </summary>
    private void TEMP_UpdateVisuals()
    {
        
    }
}