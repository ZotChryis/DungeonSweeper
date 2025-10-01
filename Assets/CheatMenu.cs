using Schemas;
using UnityEngine;

// Need an intermediate because our singleton pattern is no good
public class CheatMenu : MonoBehaviour
{
    public void Restart()
    {
        CheatManager.Instance.Restart();
    }

    public void Regenerate()
    {
        CheatManager.Instance.Regenerate();
    }

    public void Regenerate(SpawnSettings spawnSettings)
    {
        CheatManager.Instance.Regenerate(spawnSettings);
    }

    public void RevealAll()
    {
        CheatManager.Instance.RevealAllTiles();
    }

    public void ConquerAll()
    {
        CheatManager.Instance.ConquerAll();
    }

    public void Cash()
    {
        CheatManager.Instance.AddCash();
    }

    public void LevelUp()
    {
        CheatManager.Instance.LevelUp();
    }

    public void GodMode()
    {
        CheatManager.Instance.GodMode();
    }

    public void RollShop()
    {
        CheatManager.Instance.RollShop();
    }

    public void RollShopAll()
    {
        CheatManager.Instance.RollShopWithAll();
    }

    public void AddRandomLegendary()
    {
        Tile.AddRandomItemToPlayer(new Rarity[] { Rarity.Legendary });
    }
}
