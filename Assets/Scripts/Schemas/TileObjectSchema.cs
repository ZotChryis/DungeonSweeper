using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

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
    public Sprite Sprite;
    public int Power;
    public bool HidePowerToNeighbors;
    public bool PreventConsumeIfKillingBlow;
    public int XPReward;
    
    // When fully collected, instead of becoming Empty, this object will be the new housed object on the tile.
    public TileObjectSchema DropReward;

    // TODO: Refactor all this shit honestly
    public int RevealRadius;
    public int ObscureRadius;
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
}
