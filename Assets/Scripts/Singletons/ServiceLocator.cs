using System.IO;
using Gameplay;
using Schemas;
using Screens;
using Singletons;
using UI;
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
    public OverlayScreenManager OverlayScreenManager;
    
    [HideInInspector]
    public TileContextMenuScreen TileContextMenu;
    
    // TODO: We need a better home for all the level/shop transition logic
    [HideInInspector]
    public LevelManager LevelManager;
    
    [HideInInspector]
    public AudioManager AudioManager;
    
    [HideInInspector]
    public ToastManager ToastManager;
    
    [HideInInspector]
    public TutorialManager TutorialManager;
    
    [HideInInspector]
    public GridDragger GridDragger;

    // Non-MonoBehavior backed systems
    // These are managed within this class
    [HideInInspector]
    public SchemaContainer Schemas;
    
    [HideInInspector]
    public AchievementSystem AchievementSystem;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            DontDestroyOnLoad(gameObject);

            Schemas = new SchemaContainer();
            Schemas.Initialize();

            AchievementSystem = new AchievementSystem();
            
            string path = Application.persistentDataPath;
            #if PLATFORM_WEBGL
            path = "idbfs/DungeonSweeper";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            #endif
            
            FBPP.Start(new FBPPConfig()
            {
                SaveFileName = "DungeonSweeperSaveData.txt",
                AutoSaveData = true,
                ScrambleSaveData = true,
                EncryptionSecret = "DungeonSweeperSecret",
                SaveFilePath = path,
            });
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
    
    public void Register(AudioManager audioManager)
    {
        AudioManager = audioManager;
    }
    
    public void Register(ToastManager toastManager)
    {
        ToastManager = toastManager;
    }
    
    public void Register(TutorialManager tutorialManager)
    {
        TutorialManager = tutorialManager;
    }
    
    public void Register(GridDragger gridDragger)
    {
        GridDragger = gridDragger;
    }

    public void DeleteSaveFile()
    {
        FBPP.DeleteAll();
        CheatManager.Restart();
    }

    public void ResetTutorial()
    {
        TutorialManager.ClearTutorials();
    }
}
