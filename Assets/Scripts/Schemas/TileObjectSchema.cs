using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using static TileObjectSchema;

/// <summary>
/// Representation of an Enemy.
/// Intended to be housed within a Tile.
/// </summary>
[CreateAssetMenu(menuName = "Data/Enemy")]
public class TileObjectSchema : Schema
{
    public Sprite Sprite;
    public int Power;
    public bool HidePowerToNeighbors;
    public bool PreventConsumeIfKillingBlow;
    public int XPReward;
    public int RevealRadius;

    // When fully collected, instead of becoming Empty, this object will be the new housed object on the tile.
    public TileObjectSchema DropReward;

    // TODO: Total hack. When an item with this is collected, the game is victorious
    public bool WinReward;

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
