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

    public void Regenerate()
    {
        ServiceLocator.Instance.Grid.GenerateGrid();
    }
    
    public void Restart()
    {
        SceneManager.LoadScene("Game");
    }
}
