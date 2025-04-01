using UnityEngine;

/// <summary>
/// This class acts as a ServiceLocator root and can be statically accessed via ServiceLocator.Instance.
/// This is what will eventually bootstrap the game scene.
/// </summary>
public class ServiceLocator : SingletonMonoBehaviour<ServiceLocator>
{
    // Configure what level of production status to run the game on.
    public Schema.ProductionStatus MininmumStatus = Schema.ProductionStatus.Debug;

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
    public TileContextMenu TileContextMenu;

    // Non-MonoBehavior backed systems
    // These are managed within this class
    [HideInInspector]
    public SchemaContainer Schemas;
    
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        
        Schemas = new SchemaContainer();
        Schemas.Initialize(MininmumStatus);
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
    
    public void Register(TileContextMenu tileContextMenu)
    {
        TileContextMenu = tileContextMenu;
    }
}
