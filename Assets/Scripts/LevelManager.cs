using Sirenix.OdinInspector;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Tooltip("Levels in order")]
    public SpawnSettings[] Levels;

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
    }
}
