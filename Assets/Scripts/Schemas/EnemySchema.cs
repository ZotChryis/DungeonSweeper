using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

/// <summary>
/// Representation of an Enemy.
/// Intended to be housed within a Tile.
/// </summary>
[CreateAssetMenu(menuName = "Data/Enemy")]
public class EnemySchema : Schema, ITileObject
{
    [SerializedDictionary("Tile State", "Sprite Visual")] 
    public SerializedDictionary<Tile.TileState, Sprite> Visuals;
    
    // TODO: Probably doesnt need to be a map
    // Need to script special logic on each enemy when tile state changes
    // Probably need a EnemyInstance class detatched from the data RIP
    public SerializedDictionary<Tile.TileState, int> Power;
    
    /** ITileObject **/
    public int GetPower(Tile.TileState state = Tile.TileState.Hidden)
    {
        return Power.GetValueOrDefault(state);
    }

    public Sprite GetSprite(Tile.TileState state)
    {
        return Visuals.GetValueOrDefault(state);
    }

    public int GetRevealRadius(Tile.TileState state)
    {
        return 0;
    }
    /** End ITileObject **/
}
