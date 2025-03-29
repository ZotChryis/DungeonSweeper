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
    public CheatManager CheatManager;
    public Grid Grid;
    public Player Player;
    public EnemySpawner EnemySpawner;

    // Non-MonoBehavior backed systems
    // These are managed within this class
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

    public void Register(CheatManager cheatManager)
    {
        CheatManager = cheatManager;
    }
}
