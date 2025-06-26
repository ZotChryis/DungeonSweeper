using Screens.Shop;
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

    /// <summary>
    /// Toggles godmode.
    /// </summary>
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

    public void RollShop()
    {
        ShopScreen shop = ServiceLocator.Instance.OverlayScreenManager.Screens[OverlayScreenManager.ScreenType.Shop] as ShopScreen;
        shop.Roll(ServiceLocator.Instance.LevelManager.CurrentLevel);
    }

    public void RollShopWithAll()
    {
        ShopScreen shop = ServiceLocator.Instance.OverlayScreenManager.Screens[OverlayScreenManager.ScreenType.Shop] as ShopScreen;
        shop.CheatRollAll();
    }
}
