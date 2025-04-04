using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Representation of an Enemy.
/// Intended to be housed within a Tile.
/// </summary>
// TODO: Tile vs TileObject should have their own states
//      Tile should not be concerned with what the TileObject does beyond "I am dead now" or w/e
//      This way we can divorce the tile's logic from the object's logic, and let the objects do whatever they want
//      We should then have SingleHitObject : TileObject (most enemies), MultiHitObject : TileObject (blocks) ? idk
[CreateAssetMenu(menuName = "Data/TileObject")]
public class TileObjectSchema : Schema
{
    [Serializable]
    public struct SpriteFacing
    {
        // Object to look for. We find the nearest one. If none provided, logic will not run
        public TileObjectSchema NearestObject;
        
        // Relative to self. As in, if THIS object is above Nearest object, use this. Only used if not null.
        public Sprite Above;
        public Sprite Below;
        
        // Above/Below and Behind/Front are exclusive. You cant use both
        public Sprite Behind;
        public Sprite Front;
        
        // If no nearest object type is found.
        public Sprite Missing;
    }
    
    public Sprite Sprite;
    public SpriteFacing SpriteFacingData;
    
    public int Power;
    public bool HidePowerToNeighbors;
    public bool PreventConsumeIfKillingBlow;
    public int XPReward;
    
    // When fully collected, instead of becoming Empty, this object will be the new housed object on the tile.
    public TileObjectSchema DropReward;

    // TODO: Refactor all this shit honestly
    public bool GenerateRandomLocationForReveal;
    public Vector2Int[] RevealOffsets;
    public int RevealRadius;
    public Vector2Int[] ObscureOffsets;
    public int ObscureRadius;
    
    // Hack: basically Gnome behavior
    public bool CanFlee;
    
    public TileObjectSchema[] RevealAllRewards;
    public bool WinReward;
    public bool FullHealReward;
    public bool DiffuseMinesReward;

    [Serializable]
    public struct ValueOverride<T>
    {
        public bool UseOverride;
        public T Value;
    }

    [Serializable]
    public struct TileStateData {
        // Wether or not to override the standard behavior of tile state flow
        public ValueOverride<bool> AutoContinue;

        // Allow to change the sprite for this specific state
        public ValueOverride<Sprite> Sprite;

        // Whether or not to display  certain visual elements
        public ValueOverride<bool> EnablePower;
        public ValueOverride<bool> EnableSprite;
        public ValueOverride<bool> EnableDeathSprite;
    }

    [SerializedDictionary("Tile State", "State Overrides")] 
    public SerializedDictionary<Tile.TileState, TileStateData> Data;

    public TileStateData GetOverrides(Tile.TileState state)
    {
        return Data.GetValueOrDefault(state);
    }

    // TODO HACK fix later
    public Sprite TEMP_GetSprite(int xCoordinate, int yCoordinate)
    {
        if (!SpriteFacingData.NearestObject)
        {
            return Sprite;
        }

        (int, int) coordinates = ServiceLocator.Instance.Grid.FindNearest(SpriteFacingData.NearestObject, xCoordinate, yCoordinate);
        if (coordinates.Item1 == -1 || coordinates.Item2 == -1)
        {
            return SpriteFacingData.Missing;
        }

        if (SpriteFacingData.Above && SpriteFacingData.Below)
        {
            if (coordinates.Item2 > yCoordinate)
            {
                return SpriteFacingData.Below;
            }
            else
            {
                return SpriteFacingData.Above;
            }
        }
        else if (SpriteFacingData.Behind && SpriteFacingData.Front)
        {
            if (coordinates.Item1 > xCoordinate)
            {
                return SpriteFacingData.Behind;
            }
            else
            {
                return SpriteFacingData.Front;
            }
        }
        
        return SpriteFacingData.Missing;
    }
}
