using Sirenix.OdinInspector;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Tooltip("Levels in order")]
    public SpawnSettings[] Levels;

    /// <summary>
    /// Levels are 1, 2, and 3.
    /// </summary>
    public int CurrentLevel
    {
        get
        {
            return currentLevel;
        }
    }

    [ReadOnly]
    [SerializeField]
    private int currentLevel = 0;

    // TODO: Let's remove asset usages of functions like this. If its on a button, we should bind the callback in code
    public void NextLevel()
    {
        currentLevel++;
        SetLevel(currentLevel);
    }

    // TODO: Go through GameManager just for clarity
    private void SetLevel(int level)
    {
        ServiceLocator.Instance.Grid.SpawnSettings = Levels[level];
        ServiceLocator.Instance.Grid.GenerateGrid();
        ServiceLocator.Instance.Player.ResetPlayer();

        // TODO: Find a better way to do this, in data? This is gross
        if (level == 1)
        {
            foreach(var obj in ServiceLocator.Instance.TileContextMenu.level2Objects)
            {
                obj.SetActive(true);
            }
        }
        if (level == 2)
        {
            foreach (var obj in ServiceLocator.Instance.TileContextMenu.level3Objects)
            {
                obj.SetActive(true);
            }
        }
    }
}
