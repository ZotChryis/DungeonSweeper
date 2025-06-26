using UnityEngine;

/// <summary>
/// This class acts as a ServiceLocator root and can be statically accessed via ServiceLocator.Instance.
/// This is what will eventually bootstrap the game scene.
/// </summary>
public class ServiceLocator : SingletonMonoBehaviour<ServiceLocator>
{
    // MonoBehavior backed systems
    // They must self-register
    [HideInInspector]
    public CheatManager CheatManager;

    [HideInInspector]
    public Grid Grid;

    [HideInInspector]
    public Player Player;

    [HideInInspector]
    public EnemySpawner EnemySpawner;

    [HideInInspector]
    public OverlayScreenManager OverlayScreenManager;
    
    [HideInInspector]
    public TileContextMenuScreen TileContextMenu;
    
    // TODO: We need a better home for all the level/shop transition logic
    [HideInInspector]
    public LevelManager LevelManager;

    // Non-MonoBehavior backed systems
    // These are managed within this class
    [HideInInspector]
    public SchemaContainer Schemas;
    
    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            DontDestroyOnLoad(gameObject);

            Schemas = new SchemaContainer();
            Schemas.Initialize();
        }
    }
    
    public void Register(Grid grid)
    {
        Grid = grid;
    }

    public void Register(Player player)
    {
        Player = player;
    }

    public void Register(EnemySpawner enemySpawner)
    {
        EnemySpawner = enemySpawner;
    }

    public void Register(CheatManager cheatManager)
    {
        CheatManager = cheatManager;
    }

    public void Register(OverlayScreenManager screenManager)
    {
        OverlayScreenManager = screenManager;
    }
    
    public void Register(TileContextMenuScreen tileContextMenu)
    {
        TileContextMenu = tileContextMenu;
    }
    
    public void Register(LevelManager levelManager)
    {
        LevelManager = levelManager;
    }
}
