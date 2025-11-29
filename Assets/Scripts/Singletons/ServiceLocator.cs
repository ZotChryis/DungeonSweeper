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
    
    [HideInInspector]
    public SaveSystem SaveSystem;

    [HideInInspector]
    public SteamStatsAndAchievements SteamStatsAndAchievements;

    /// <summary>
    /// Set to true if this is steam demo.
    /// </summary>
    public static bool IsSteamDemo
    {
        get
        {
            return false;
        }
    }

    /// <summary>
    /// Better to use SteamWorks.IsInitialized, but this returns true if you are compatible with Steam like UNITY_STANDALONE
    /// </summary>
    public bool IsSteamCompatibleVersion = false;
    /// <summary>
    /// Determines if this application is iOS (iPhone).
    /// </summary>
    public bool IsIOS = false;
    /// <summary>
    /// Determines if this application is android.
    /// </summary>
    public bool IsAndroid = false;
    /// <summary>
    /// Determines if this application is webGL (browser game)
    /// </summary>
    public bool IsWebGL = false;
    /// <summary>
    /// Determines if this application is linux.
    /// </summary>
    public bool IsLinux = false;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = 60;
            
            Schemas = new SchemaContainer();
            Schemas.Initialize();

            AchievementSystem = new AchievementSystem();
            SaveSystem = new SaveSystem();

            string path = SaveSystem.GetSaveFilePath();
            Debug.Log("Saving to path : " + path + " filename: DungeonSweeperSaveData.txt");

            FBPP.Start(new FBPPConfig()
            {
                SaveFileName = "DungeonSweeperSaveData.txt",
                AutoSaveData = true,
                ScrambleSaveData = true,
                EncryptionSecret = "DungeonSweeperSecret",
                SaveFilePath = path,
            });
        }

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
        IsSteamCompatibleVersion = true;
#endif
#if UNITY_IOS
        IsIOS = true;
#endif
#if UNITY_ANDROID
        IsAndroid = true;
#endif
#if UNITY_WEBGL
        IsWebGL = true;
#endif
#if UNITY_STANDALONE_LINUX
        IsLinux = true;
#endif
    }

    private void Start()
    {
        if (!IsSteamDemo && IsPaidVersion())
        {
            AchievementSystem.AwardSteamDemoAchievements();
        }
    }

    /// <summary>
    /// We assume that if you're playing on Steam or Android, it's a paid application.
    /// For Android, we should probably verify you downloaded it from Google Play Store....
    /// </summary>
    /// <returns></returns>
    public bool IsPaidVersion()
    {
        if (IsSteamDemo)
        {
            return false;
        }
        else
        {
            return SteamManager.Initialized || IsAndroid || Application.isEditor;
        }
    }
    
    public void Register(SteamStatsAndAchievements steamStatsAndAchievements)
    {
        SteamStatsAndAchievements = steamStatsAndAchievements;
    }
    
    public void Register(Grid grid)
    {
        Grid = grid;
    }

    public void Register(Player player)
    {
        Player = player;
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
        SaveSystem.Wipe();
        CheatManager.Instance.Restart();
    }

    public void ResetTutorial()
    {
        TutorialManager.ClearTutorials();
    }
}
