using Screens.Shop;
using UnityEngine.SceneManagement;

public class CheatManager : SingletonMonoBehaviour<CheatManager>
{
    protected override void Awake()
    {
        base.Awake();

        if (Instance == this)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public void RevealAllTiles()
    {
        ServiceLocator.Instance.AchievementSystem.AllowAchievementsToBeCompleted = false;
        ServiceLocator.Instance.Grid.TEMP_RevealAllTiles();
    }

    public void LevelUp()
    {
        ServiceLocator.Instance.AchievementSystem.AllowAchievementsToBeCompleted = false;
        ServiceLocator.Instance.Player.LevelUp();
    }

    /// <summary>
    /// Toggles godmode.
    /// </summary>
    public void GodMode()
    {
        ServiceLocator.Instance.AchievementSystem.AllowAchievementsToBeCompleted = false;
        ServiceLocator.Instance.Player.ToggleGodMode();
    }

    public void Regenerate()
    {
        ServiceLocator.Instance.AchievementSystem.AllowAchievementsToBeCompleted = false;
        ServiceLocator.Instance.Grid.GenerateGrid();
    }
    
    public void Regenerate(SpawnSettings spawnSettings)
    {
        ServiceLocator.Instance.AchievementSystem.AllowAchievementsToBeCompleted = false;
        ServiceLocator.Instance.Grid.SpawnSettings = spawnSettings;
        ServiceLocator.Instance.Grid.GenerateGrid();
    }
    
    public void Restart()
    {
        SceneManager.LoadScene("Game");
    }

    public void RollShop()
    {
        ServiceLocator.Instance.AchievementSystem.AllowAchievementsToBeCompleted = false;
        ShopScreen shop = ServiceLocator.Instance.OverlayScreenManager.Screens[OverlayScreenManager.ScreenType.Shop] as ShopScreen;
        shop.Roll(ServiceLocator.Instance.LevelManager.CurrentLevel, false);
    }

    public void RollShopWithAll()
    {
        ServiceLocator.Instance.AchievementSystem.AllowAchievementsToBeCompleted = false;
        ShopScreen shop = ServiceLocator.Instance.OverlayScreenManager.Screens[OverlayScreenManager.ScreenType.Shop] as ShopScreen;
        shop.CheatRollAll();
    }

    public void ConquerAll()
    {
        ServiceLocator.Instance.AchievementSystem.AllowAchievementsToBeCompleted = false;
        ServiceLocator.Instance.Grid.TEMP_ConquerAllTiles();
    }

    public void AddCash()
    {
        ServiceLocator.Instance.AchievementSystem.AllowAchievementsToBeCompleted = false;
        ServiceLocator.Instance.Player.ShopXp += 10;
    }
}
