using System;
using System.Linq;
using Schemas;
using Sirenix.OdinInspector;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Tooltip("Base levels in order")] 
    [SerializeField]
    private SpawnSettings[] Levels;

    [Tooltip("Special version of level 0.")]
    public SpawnSettings TutorialLevel;

    [HideInInspector]
    public int StartingLevel = 0;

    public bool IsTutorialLevel = false;

    private SpawnSettings[] _levels;
    
    /// <summary>
    /// Levels are 0, 1, 2, 3, and 4.
    /// </summary>
    public int CurrentLevel
    {
        set
        {
            currentLevel = value;
            OnLevelChanged?.Invoke(currentLevel);
        }
        get
        {
            return currentLevel;
        }
    }

    public string CurrentLevelName
    {
        get
        {
            if (IsTutorialLevel)
            {
                return "TutorialLevel0";
            }
            else
            {
                return "Level" + CurrentLevel.ToString();
            }
        }
    }

    public Action<int> OnLevelChanged;

    [ReadOnly]
    [SerializeField]
    private int currentLevel = 0;

    private void Awake()
    {
        ServiceLocator.Instance.Register(this);
        UseDefaultLevels();
    }

    // TODO: Update for XP Curve too
    /// <summary>
    /// Challenges can change the level structure 
    /// </summary>
    public void OverrideLevels(SpawnSettings[] levels)
    {
        _levels = levels;
    }
    public void UseDefaultLevels()
    {
        _levels = Levels;
    }

    public bool IsCurrentLevelFinalLevel()
    {
        return CurrentLevel == _levels.Length - 1;
    }
    
    public void NextLevel()
    {
        SetLevel(CurrentLevel + 1);
        ServiceLocator.Instance.Grid.GenerateGrid();
    }
    
    public void SetLevel(int level)
    {
        CurrentLevel = level;
        StartingLevel = level;
        ServiceLocator.Instance.Grid.SpawnSettings = _levels[level];
    }

    public void SetToStartingLevel()
    {
        Debug.Log("Starting level: " + StartingLevel);
        if (StartingLevel == 0 && TutorialManager.Instance.ShouldUseTutorialLevel())
        {
            CurrentLevel = 0;
            ServiceLocator.Instance.Grid.SpawnSettings = TutorialLevel;
            IsTutorialLevel = true;
        }
        else
        {
            SetLevel(StartingLevel);
            IsTutorialLevel = false;
        }
    }

    /// <summary>
    /// This preps the player for a "retry". Mostly this needs to clean up any residual state that occurred
    /// during the current level
    /// </summary>
    public void RetryCurrentLevel()
    {
        ServiceLocator.Instance.Player.IsHardcore = false;
        ServiceLocator.Instance.Player.RevokeItemsForCurrentDungeon();
        ServiceLocator.Instance.Player.ResetPlayer();
        ServiceLocator.Instance.Grid.GenerateGrid();
    }

    public bool IsFinalLevel()
    {
        return CurrentLevel == Levels.Length - 1;
    }
    
    public void TryGrantAnnihilatorBonus()
    {
        if (IsFinalLevel())
        {
            return;
        }

        var tileObjects = ServiceLocator.Instance.Grid.GetAllTileObjects();
        tileObjects.RemoveAll(i => i.TileId == TileSchema.Id.Balrog || i.TileId == TileSchema.Id.Crown);

        if (tileObjects.Any())
        {
            return;
        }
        
        foreach (var id in Levels[CurrentLevel].AnnihilatorRewards)
        {
            ServiceLocator.Instance.Player.Inventory.AddItem(id);
        }
    }
}
