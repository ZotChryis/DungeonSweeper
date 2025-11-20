
using Steamworks;
using UnityEngine;

public class OpenSteam : MonoBehaviour
{
    private const uint DungeonSweeperAppId = 4109840;

    public void OpenSteamPage()
    {
        if (SteamManager.Initialized)
        {
            try
            {
                SteamFriends.ActivateGameOverlayToStore(new AppId_t(DungeonSweeperAppId), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
            }
            catch
            {
                Application.OpenURL("steam://store/4109840");
            }
        }
        else
        {
            Application.OpenURL("steam://store/4109840");
        }
    }
}