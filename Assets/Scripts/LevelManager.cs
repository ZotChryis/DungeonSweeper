using System;
using System.Linq;
using Schemas;
using Sirenix.OdinInspector;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Tooltip("Levels in order")]
    public SpawnSettings[] Levels;

    [HideInInspector]
    public int StartingLevel = 0;
    
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

    public Action<int> OnLevelChanged;

    [ReadOnly]
    [SerializeField]
    private int currentLevel = 0;

    private void Awake()
    {
        ServiceLocator.Instance.Register(this);
    }
    
    public void NextLevel()
    {
        SetLevel(CurrentLevel + 1);
        ServiceLocator.Instance.Grid.GenerateGrid();
    }
    
    public void SetLevel(int level)
    {
        CurrentLevel = level;
        ServiceLocator.Instance.Grid.SpawnSettings = Levels[level];
    }

    public void SetToStartingLevel()
    {
        SetLevel(StartingLevel);
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
