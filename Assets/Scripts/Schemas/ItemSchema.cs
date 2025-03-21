using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Item")]
public class ItemSchema : Schema, ITileObject
{
    /// <summary>
    ///  The visuals for each state of this item.
    /// </summary>
    [SerializedDictionary("Tile State", "Sprite Visual")] 
    public SerializedDictionary<Tile.TileState, Sprite> Visuals;

    /// <summary>
    /// The amount of tiles from center to reveal when this is conquered.
    /// </summary>
    [SerializedDictionary("Tile State", "Sprite Visual")] 
    public SerializedDictionary<Tile.TileState, int> RevealRadius;

    /// <summary>
    /// If true, when Conquered, this item is automatically collected.
    /// </summary>
    public bool AutoCollect;

    /** ITileObject **/
    public int GetPower(Tile.TileState state = Tile.TileState.Hidden)
    {
        return 0;
    }

    public Sprite GetSprite(Tile.TileState state)
    {
        return Visuals.GetValueOrDefault(state);
    }

    public int GetRevealRadius(Tile.TileState state)
    {
        return RevealRadius.GetValueOrDefault(state);
    }

    /** End ITileObject **/
}
