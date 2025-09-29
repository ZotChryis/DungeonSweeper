using System;
using AYellowpaper.SerializedCollections;
using Schemas;
using UnityEngine;

public class TutorialManager : SingletonMonoBehaviour<TutorialManager>
{
    [Serializable]
    public enum TutorialId
    {
        Welcome,        // Shown when first playing a map
        Dragon,         // Shown after leveling up third time in a run
        VisionOrb,      // Shown after dismissing Welcome
        EnemyPower,     // Shown after using the Vision Orb. Player is instructed to click tutorial slime.
        NeighborPower,  // Shown after leveling up first time in a run
        XP,             // Shown after getting enough XP to level and 0 health
        Library,        // Shown after leveling up second time in a run
        Item,           // Shown after getting first item from chest
        EndGame,        // Shown on Level Index == 4
        Shop,           // Shown on first shop
        SecondLevel,    // Shown on second level
        SafeToCollect,  // Shown after killing tutorial slime
    }

    public GameObject FocusObject;
    [Tooltip("Force the player to look at an object but they can't click it.")]
    public GameObject FocusDontForceClick;

    public RectTransform LevelupFocusTarget;
    public RectTransform LibraryFocusTarget;
    public RectTransform InventoryFocusTarget;

    [SerializeField]
    [SerializedDictionary("Tutorial ID", "Tutorial")]
    private SerializedDictionary<TutorialId, Tutorial> Tutorials = new();

    private bool CanShowTutorials = false;
    private Tutorial LastTutorial;

    protected override void Awake()
    {
        base.Awake();
        ServiceLocator.Instance.Register(this);

        ServiceLocator.Instance.Grid.OnGridGenerated += OnGridGenerated;
        ServiceLocator.Instance.Grid.OnTileStateChanged += OnAnyTileStateChanged;
        ServiceLocator.Instance.Player.OnLevelChanged += OnPlayerLevelChanged;

        foreach (var (tutorialId, tutorial) in Tutorials)
        {
            tutorial.OnCompleted += OnTutorialCompleted;
        }
    }

    private void OnPlayerLevelChanged(int newLevel)
    {
        switch (newLevel)
        {
            case 1:
                TryShowTutorial(TutorialId.NeighborPower);
                break;
            case 2:
                TryShowTutorial(TutorialId.Library, LibraryFocusTarget, true);
                break;
                //case 3:
                //var dragon = ServiceLocator.Instance.Grid.GetTileTransform(TileSchema.Id.Dragon0);
                //TryShowTutorial(TutorialId.Dragon, (RectTransform)dragon);
                //break;
        }
    }

    private void OnAnyTileStateChanged(Tile tile)
    {
        if (!tile.TEMP_IsEmpty())
        {
            Debug.Log("Tutorial on tile state changed: " + tile.GetHousedObject().TileId + ", state: " + tile.State);
        }
        if (!tile.TEMP_IsEmpty() &&
            tile.GetHousedObject().TileId == TileSchema.Id.VisionOrb &&
            tile.State == Tile.TileState.Collected
        )
        {
            var tutorialSlime = ServiceLocator.Instance.Grid.GetTileTransform(TileSchema.Id.TutorialSlime);
            TryShowTutorial(TutorialId.EnemyPower, (RectTransform)tutorialSlime, true);
        }
        if (!tile.TEMP_IsEmpty() &&
            tile.GetHousedObject().TileId == TileSchema.Id.TutorialSlime &&
            tile.State == Tile.TileState.Conquered)
        {
            TryShowTutorial(TutorialId.SafeToCollect, (RectTransform)tile.transform, true);
        }
    }

    private void OnGridGenerated()
    {
        CanShowTutorials = true;
        var dragon0 = ServiceLocator.Instance.Grid.GetTileTransform(TileSchema.Id.Dragon0);
        TryShowTutorial(TutorialId.Welcome, (RectTransform)dragon0, false, true);

        if (ServiceLocator.Instance.LevelManager.CurrentLevel == 1)
        {
            var dragon = ServiceLocator.Instance.Grid.GetTileTransform(TileSchema.Id.Dragon1);
            TryShowTutorial(TutorialId.SecondLevel, (RectTransform)dragon, false, true);
        }
        else if (ServiceLocator.Instance.LevelManager.CurrentLevel == 4)
        {
            TryShowTutorial(TutorialId.EndGame);
        }
    }

    private void OnTutorialCompleted(TutorialId tutorialId)
    {
        if (tutorialId == TutorialId.Welcome)
        {
            var visionOrb = ServiceLocator.Instance.Grid.GetTileTransform(TileSchema.Id.VisionOrb);
            TryShowTutorial(TutorialId.VisionOrb, (RectTransform)visionOrb, true);
            return;
        }
    }

    public void TryShowInventoryTutorial()
    {
        TryShowTutorial(TutorialManager.TutorialId.Item, InventoryFocusTarget, true);
    }

    public void TryShowTutorial(TutorialId tutorialId, RectTransform focus = null, bool forcePlayerToClickFocus = false, bool waitOneFrame = false)
    {
        if (!CanShowTutorials)
        {
            return;
        }

        var tutorialKey = tutorialId.GetTutorialKey();

        // Already seen this tutorial?
        if (FBPP.GetBool(tutorialKey, false))
        {
            return;
        }

        // No tutorial defined?
        if (!Tutorials.TryGetValue(tutorialId, out Tutorial tutorial))
        {
            return;
        }

        TryHideLastTutorial();
        tutorial.gameObject.SetActive(true);
        LastTutorial = tutorial;
        if (focus != null)
        {
            tutorial.SetFocus(focus, forcePlayerToClickFocus, waitOneFrame);
        }
        FBPP.SetBool(tutorialKey, true);
    }

    /// <summary>
    /// Determines if we should use a special tutorial level over the default level.
    /// </summary>
    /// <returns>true if we should use the tutorial level that has a slime at the starting orb.</returns>
    public bool ShouldUseTutorialLevel()
    {
        var shouldUseTutorial = !FBPP.GetBool(TutorialId.Welcome.GetTutorialKey(), false);
        Debug.Log("Should use tutorial level?: " + shouldUseTutorial);
        return shouldUseTutorial;
    }

    public void TryHideLastTutorial()
    {
        FocusObject.SetActive(false);
        FocusDontForceClick.SetActive(false);

        if (LastTutorial)
        {
            Tutorial foundTutorial = LastTutorial;
            LastTutorial = null;
            foundTutorial.CompleteTutorial();
        }
    }

    public void ClearTutorials()
    {
        foreach (var (tutorialId, tutorial) in Tutorials)
        {
            FBPP.DeleteBool(tutorialId.GetTutorialKey());
        }
    }
}
