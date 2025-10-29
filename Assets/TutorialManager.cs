using AYellowpaper.SerializedCollections;
using Schemas;
using System;
using System.Collections;
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
        WarnLevelUpNonEmpty, // Shown on level up but the player had some remaining health
        WarnBrickBehavior, // Shown on attacking a brick.
        WarnGnomeBehavior, // Shown on clicking a gnome.
        TutorialRightClick, // Shown after leveling up third time
        WarnConqueredEnemies, // Shown after killing something. Delayed by 3s and only shown again if tile state still conquered.
        WarnObscure, // Shown after revealing a tile that is obscured and you have already leveled up once.
        WarnHealWhenNoHealth, // Shown after leveling up four times.
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
        if (ServiceLocator.Instance.Player.WasLastLevelUpInefficient && TryShowTutorial(TutorialId.WarnLevelUpNonEmpty))
        {
            return;
        }
        if (TryShowTutorial(TutorialId.NeighborPower))
        {
            return;
        }
        if (TryShowTutorial(TutorialId.Library, LibraryFocusTarget, true))
        {
            return;
        }
        if (TryShowTutorial(TutorialId.TutorialRightClick))
        {
            return;
        }
        if (TryShowTutorial(TutorialId.WarnHealWhenNoHealth))
        {
            return;
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
            if (tutorialSlime != null)
            {
                if (TryShowTutorial(TutorialId.EnemyPower, (RectTransform)tutorialSlime, true))
                {
                    return;
                }
            }
        }
        if (!tile.TEMP_IsEmpty() &&
            tile.GetHousedObject().TileId == TileSchema.Id.TutorialSlime &&
            tile.State == Tile.TileState.Conquered)
        {
            if (TryShowTutorial(TutorialId.SafeToCollect, (RectTransform)tile.transform, true))
            {
                return;
            }
        }
        if (!tile.TEMP_IsEmpty() &&
            tile.GetHousedObject().TileId == TileSchema.Id.Gnome &&
            tile.State == Tile.TileState.Conquered)
        {
            if (TryShowTutorial(TutorialId.WarnGnomeBehavior, LibraryFocusTarget, false, 0.8f))
            {
                return;
            }
        }
        if (!tile.TEMP_IsEmpty() &&
            tile.GetHousedObject().TileId == TileSchema.Id.Brick &&
            tile.State == Tile.TileState.Conquered)
        {
            if (TryShowTutorial(TutorialId.WarnBrickBehavior, LibraryFocusTarget, false, 0.8f))
            {
                return;
            }
        }
        if (!tile.TEMP_IsEmpty() &&
            tile.GetAdjustedPower() <= 6 &&
            tile.GetAdjustedPower() >= 1 &&
            tile.State == Tile.TileState.Conquered)
        {
            if (TryShowTutorialAfterDelay(TutorialId.WarnConqueredEnemies, tile, (RectTransform)tile.transform, false, 3f))
            {
                return;
            }
        }

        if (tile.TEMP_IsEmpty() &&
            tile.IsObscured() &&
            FBPP.GetBool(TutorialId.NeighborPower.GetTutorialKey()))
        {
            if (TryShowTutorial(TutorialId.WarnObscure, LibraryFocusTarget, false, 0.8f))
            {
                return;
            }
        }
    }

    private void OnGridGenerated()
    {
        CanShowTutorials = true;
        var dragon0 = ServiceLocator.Instance.Grid.GetTileTransform(TileSchema.Id.Dragon0);
        TryShowTutorial(TutorialId.Welcome, (RectTransform)dragon0, false, 0f);

        if (ServiceLocator.Instance.LevelManager.CurrentLevel == 1)
        {
            var dragon = ServiceLocator.Instance.Grid.GetTileTransform(TileSchema.Id.Dragon1);
            TryShowTutorial(TutorialId.SecondLevel, (RectTransform)dragon, false, 0f);
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tutorialId">The tutorialId</param>
    /// <param name="focus">The RectTransform to focus on.</param>
    /// <param name="forcePlayerToClickFocus">If true, force the player to click the <paramref name="focus"/></param>
    /// <param name="waitOneFrame">Sometimes a transition, animation, or grid generation needs to happen first. Set to true to account for that.</param>
    /// <returns>True if successfully showed a tutorial</returns>
    public bool TryShowTutorial(TutorialId tutorialId, RectTransform focus = null, bool forcePlayerToClickFocus = false, float waitSeconds = -1f)
    {
        if (!CanShowTutorials)
        {
            return false;
        }

        var tutorialKey = tutorialId.GetTutorialKey();

        // Already seen this tutorial?
        if (FBPP.GetBool(tutorialKey, false))
        {
            return false;
        }
        Debug.Log("Trying to show tutorial: " + tutorialKey);

        // No tutorial defined?
        if (!Tutorials.TryGetValue(tutorialId, out Tutorial tutorial))
        {
            return false;
        }

        TryHideLastTutorial();
        LastTutorial = tutorial;

        if (waitSeconds >= 0)
        {
            if (waitSeconds == 0f)
            {
                StartCoroutine(TryShowTutorialAfterFrame(tutorial, focus, forcePlayerToClickFocus));
            }
            else
            {
                StartCoroutine(TryShowTutorialAfterSeconds(tutorial, focus, forcePlayerToClickFocus, waitSeconds));
            }
        }
        else
        {
             TryShowTutorialAfter(tutorial, focus, forcePlayerToClickFocus);
        }
        FBPP.SetBool(tutorial.Id.GetTutorialKey(), true);
        return true;
    }

    /// <summary>
    /// Copy of TryShowTutorial, but only trigger showing the tutorial after a large delay. And only if the target is still conquered.
    /// </summary>
    /// <param name="tutorialId"></param>
    /// <param name="tile"></param>
    /// <returns></returns>
    public bool TryShowTutorialAfterDelay(TutorialId tutorialId, Tile tile, RectTransform focus, bool forcePlayerToClickFocus, float waitSeconds)
    {
        if (!CanShowTutorials)
        {
            return false;
        }

        var tutorialKey = tutorialId.GetTutorialKey();

        // Already seen this tutorial?
        if (FBPP.GetBool(tutorialKey, false))
        {
            return false;
        }
        Debug.Log("Trying to show tutorial: after delay" + tutorialKey);

        // No tutorial defined?
        if (!Tutorials.TryGetValue(tutorialId, out Tutorial tutorial))
        {
            Debug.LogWarning("No tutorial defined for tutorialId: " + tutorialId);
            return false;
        }

        StartCoroutine(WaitThenTryShowTutorial(tutorial, tile, focus, forcePlayerToClickFocus, waitSeconds));
        return true;
    }

    private void TryShowTutorialAfter(Tutorial tutorial, RectTransform focus = null, bool forcePlayerToClickFocus = false)
    {
        tutorial.gameObject.SetActive(true);
        if (focus != null)
        {
            tutorial.SetFocus(focus, forcePlayerToClickFocus);
        }
    }

    private IEnumerator WaitThenTryShowTutorial(Tutorial tutorial, Tile tile, RectTransform focus, bool forcePlayerToClickFocus, float waitSeconds)
    {
        yield return new WaitForSeconds(waitSeconds);
        if (tile.State == Tile.TileState.Conquered && FBPP.GetBool(tutorial.Id.GetTutorialKey(), false))
        {
            TryHideLastTutorial();
            LastTutorial = tutorial;
            TryShowTutorialAfter(tutorial, focus, forcePlayerToClickFocus);
            FBPP.SetBool(tutorial.Id.GetTutorialKey(), true);
        }
    }

    private IEnumerator TryShowTutorialAfterFrame(Tutorial tutorial, RectTransform focus, bool forcePlayerToClickFocus)
    {
        // Some tutorials are triggered after grid generation. Wait 1 frame so things are moved.
        yield return 0;
        TryShowTutorialAfter(tutorial, focus, forcePlayerToClickFocus);
    }

    private IEnumerator TryShowTutorialAfterSeconds(Tutorial tutorial, RectTransform focus, bool forcePlayerToClickFocus, float seconds)
    {
        // Some tutorials are triggered after grid generation. Wait 1 frame so things are moved.
        yield return new WaitForSeconds(seconds);
        TryShowTutorialAfter(tutorial, focus, forcePlayerToClickFocus);
    }

    /// <summary>
    /// Determines if we should use a special tutorial level over the default level.
    /// </summary>
    /// <returns>true if we should use the tutorial level that has a slime at the starting orb.</returns>
    public bool ShouldUseTutorialLevel()
    {
        var shouldUseTutorial = !FBPP.GetBool(TutorialId.EnemyPower.GetTutorialKey(), false);
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
