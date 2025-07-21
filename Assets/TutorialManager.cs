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
        Dragon,         // Shown after dismissing Welcome
        VisionOrb,      // Shown after dismissing Dragon
        EnemyPower,     // Shown after revealing first enemy
        NeighborPower,  // Shown after revealing first empty tile
        XP,             // Shown after getting enough XP to level
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
        
        foreach (var (tutorialId, tutorial) in Tutorials)
        {
            tutorial.OnCompleted += OnTutorialCompleted;
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
            var dragon = ServiceLocator.Instance.Grid.GetTileTransform(TileSchema.Id.Dragon0);
            TryShowTutorial(TutorialId.Dragon, (RectTransform)dragon);
            return;
        }

        if (tutorialId == TutorialId.Dragon)
        {
            var visionOrb = ServiceLocator.Instance.Grid.GetTileTransform(TileSchema.Id.VisionOrb);
            TryShowTutorial(TutorialId.VisionOrb, (RectTransform)visionOrb);
            return;
        }
        
        if (tutorialId == TutorialId.EnemyPower)
        {
            TryShowTutorial(TutorialId.NeighborPower);
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
