using UnityEngine.SceneManagement;

public class CheatManager : SingletonMonoBehaviour<CheatManager>
{
    private void Start()
    {
        ServiceLocator.Instance.Register(this);
    }

    public void RevealAllTiles()
    {
        ServiceLocator.Instance.Grid.TEMP_RevealAllTiles();
    }

    public void LevelUp()
    {
        ServiceLocator.Instance.Player.LevelUp();
    }

    public void GodMode()
    {
        ServiceLocator.Instance.Player.GodMode();
    }

    public void Regenerate()
    {
        ServiceLocator.Instance.Grid.GenerateGrid();
    }
    
    public void Regenerate(SpawnSettings spawnSettings)
    {
        ServiceLocator.Instance.Grid.SpawnSettings = spawnSettings;
        ServiceLocator.Instance.Grid.GenerateGrid();
    }
    
    public void Restart()
    {
        SceneManager.LoadScene("Game");
    }
}
