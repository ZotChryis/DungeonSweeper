using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Screens.Shop;

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
        Achievements,
        Settings,
        MainMenu,
    }

    [SerializedDictionary("Screen Type", "Screen")]
    public SerializedDictionary<ScreenType, BaseScreen> Screens;

    //private BaseScreen ActiveScreen;
    
    private Stack<BaseScreen> ScreenStack;

    protected override void Awake()
    {
        base.Awake();

        ScreenStack = new Stack<BaseScreen>(4);
        
        ServiceLocator.Instance.Register(this);
    }

    // TODO: Deprecate ??
    public void ShowShopScreen()
    {
        // TODO: Find a better home for this logic
        ShopScreen shop = Screens[ScreenType.Shop] as ShopScreen;
        shop.Roll(1);
        
        SwitchScreen(ScreenType.Shop);
    }
    
    public void SwitchScreen(ScreenType screenType)
    {
        HideActiveScreen();
        RequestShowScreen(screenType);
    }

    // ew
    public void RequestShowScreen(int screenTypeAsInt)
    {
        RequestShowScreen((ScreenType)screenTypeAsInt);
    }
    
    // TODO: Make a stack/queue system. Right now its only 1 allowed at a time. Im lazy rn
    public void RequestShowScreen(ScreenType screenType)
    {
        if (!Screens.TryGetValue(screenType, out BaseScreen Screen))
        {
            return;
        }
        
        // TODO: We currently will only support 1 instance of the screen at a time
        if (ScreenStack.Contains(Screen))
        {
            return;
        }
        
        // TODO: Check if current screen allows for things to appear on top

        ScreenStack.Push(Screen);
        Screen.Show();
        Screen.transform.SetAsLastSibling();
    }

    public void HideActiveScreen()
    {
        if (ScreenStack.TryPop(out BaseScreen Screen))
        {
            Screen.Hide();
        }
    }

    public void HideAllScreens()
    {
        while (ScreenStack.TryPop(out BaseScreen Screen))
        {
            Screen.Hide();
        }
    }
}
