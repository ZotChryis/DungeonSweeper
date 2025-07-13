using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Tooltip("Levels in order")]
    public SpawnSettings[] Levels;

    /// <summary>
    /// Levels are 0, 1, 2, and 3.
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

    // TODO: Let's remove asset usages of functions like this. If its on a button, we should bind the callback in code
    public void NextLevel()
    {
        SetLevel(CurrentLevel + 1);
    }
    
    public void SetLevel(int level)
    {
        CurrentLevel = level;
        ServiceLocator.Instance.Grid.SpawnSettings = Levels[level];
        ServiceLocator.Instance.Grid.GenerateGrid();
    }

    /// <summary>
    /// This preps the player for a "retry". Mostly this needs to clean up any residual state that occurred
    /// during the current level
    /// </summary>
    public void RetryCurrentLevel()
    {
        ServiceLocator.Instance.Player.MarkPlayerSoftcore();
        ServiceLocator.Instance.Player.RevokeItemsForCurrentDungeon();
        ServiceLocator.Instance.Player.ResetPlayer();
        ServiceLocator.Instance.Grid.GenerateGrid();
    }
}
