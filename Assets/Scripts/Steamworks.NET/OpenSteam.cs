#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

using UnityEngine;

public class OpenSteam : MonoBehaviour
{
    private const uint DungeonSweeperAppId = 4109840;

    public void OpenSteamPage()
    {
        bool shouldUseSteam = false;
#if !DISABLESTEAMWORKS
        shouldUseSteam = SteamManager.Initialized;
#endif
        if (shouldUseSteam)
        {
#if !DISABLESTEAMWORKS
            try
            {
                SteamFriends.ActivateGameOverlayToStore(new AppId_t(DungeonSweeperAppId), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
            }
            catch
            {
                Application.OpenURL("steam://store/4109840");
            }
#endif
        }
        else
        {
            Application.OpenURL("steam://store/4109840");
        }
    }
}