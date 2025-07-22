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
        EnemyPower,     // Shown after using the Vision Orb
        NeighborPower,  // Shown after leveling up first time in a run
        XP,             // Shown after getting enough XP to level
        Library,        // Shown after leveling up second time in a run
        Item,           // Shown after getting first item from chest
    }
    
    
    [SerializeField] [SerializedDictionary("Tutorial ID", "Tutorial")]
    private SerializedDictionary<TutorialId, Tutorial> Tutorials = new();

    private bool CanShowTutorials = false;
    
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
                TryShowTutorial(TutorialId.Library);
                break;
            case 3:
                var dragon = ServiceLocator.Instance.Grid.GetTileTransform(TileSchema.Id.Dragon0);
                TryShowTutorial(TutorialId.Dragon, (RectTransform)dragon);
                break;
        }
    }

    private void OnAnyTileStateChanged(Tile tile)
    {
        if (!tile.TEMP_IsEmpty() && 
            tile.GetHousedObject().TileId == TileSchema.Id.VisionOrb &&
            tile.State == Tile.TileState.Collected
        ) {
            TryShowTutorial(TutorialId.EnemyPower);
        }
    }

    private void OnGridGenerated()
    {
        CanShowTutorials = true;
        TryShowTutorial(TutorialId.Welcome);
    }

    private void OnTutorialCompleted(TutorialId tutorialId)
    {
        if (tutorialId == TutorialId.Welcome)
        {
            var visionOrb = ServiceLocator.Instance.Grid.GetTileTransform(TileSchema.Id.VisionOrb);
            TryShowTutorial(TutorialId.VisionOrb, (RectTransform)visionOrb);
            return;
        }
    }

    public void TryShowTutorial(TutorialId tutorialId, RectTransform focus = null)
    {
        if (!CanShowTutorials)
        {
            return;
        }
        
        var tutorialKey = "Tutorial" + tutorialId;
        
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
        
        tutorial.gameObject.SetActive(true);
        if (focus != null)
        {
            tutorial.SetFocus(focus);
        }
        FBPP.SetBool(tutorialKey, true);
    }

    public void ClearTutorials()
    {
        foreach (var (tutorialId, tutorial) in Tutorials)
        {
            FBPP.DeleteBool("Tutorial" + tutorialId);
        }
    }
}
