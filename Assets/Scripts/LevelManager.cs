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

    public void NextLevel()
    {
        currentLevel++;
        SetLevel(currentLevel);
    }

    private void SetLevel(int level)
    {
        ServiceLocator.Instance.Grid.SpawnSettings = Levels[level];
        ServiceLocator.Instance.Grid.GenerateGrid();
        ServiceLocator.Instance.Player.ResetPlayer();

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
