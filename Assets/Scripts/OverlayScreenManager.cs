using AYellowpaper.SerializedCollections;

public class OverlayScreenManager : SingletonMonoBehaviour<OverlayScreenManager>
{
    public enum ScreenType
    {
        GameOver,
        Victory,
        TileContextMenu
    }

    [SerializedDictionary("Screen Type", "Screen")]
    public SerializedDictionary<ScreenType, Screen> Screens;

    private Screen ActiveScreen;

    protected override void Awake()
    {
        base.Awake();
        
        ServiceLocator.Instance.Register(this);
    }

    // TODO: Make a stack/queue system. Right now its only 1 allowed at a time. Im lazy rn
    public void RequestShowScreen(ScreenType screenType)
    {
        if (ActiveScreen)
        {
            return;
        }

        if (!Screens.ContainsKey(screenType))
        {
            return;
        }

        ActiveScreen = Screens[screenType];
        ActiveScreen.Show();
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
