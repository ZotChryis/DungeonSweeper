using System;
using AYellowpaper.SerializedCollections;

public class OverlayScreenManager : SingletonMonoBehaviour<OverlayScreenManager>
{
    [Serializable]
    public enum ScreenType
    {
        GameOver,
        Victory,
        TileContextMenu,
        Instructions,
        Library,
        Shop,
        ClassSelection,
        Inventory,
    }

    [SerializedDictionary("Screen Type", "Screen")]
    public SerializedDictionary<ScreenType, Screen> Screens;

    private Screen ActiveScreen;

    protected override void Awake()
    {
        base.Awake();
        
        ServiceLocator.Instance.Register(this);
    }

    public void ShowShopScreen()
    {
        SwitchScreen(ScreenType.Shop);
    }

    // ew
    public void RequestShowScreen(int screenTypeAsInt)
    {
        RequestShowScreen((ScreenType)screenTypeAsInt);
    }
    
    // TODO: Make a stack/queue system. Right now its only 1 allowed at a time. Im lazy rn
    public void RequestShowScreen(ScreenType screenType)
    {
        if (ActiveScreen)
        {
            return;
        }

        if(!Screens.TryGetValue(screenType, out ActiveScreen))
        {
            return;
        }

        ActiveScreen.Show();
    }

    public void SwitchScreen(ScreenType screenType)
    {
        HideActiveScreen();
        RequestShowScreen(screenType);
    }

    public void HideActiveScreen()
    {
        if (!ActiveScreen)
        {
            return;
        }

        ActiveScreen.Hide();
        ActiveScreen = null;
    }
}
