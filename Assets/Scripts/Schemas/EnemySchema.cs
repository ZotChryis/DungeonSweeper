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
    // Heads up! SerializedDictionary has to be public to function in the editor.
    [SerializedDictionary("Tile State", "Sprite Visual")] 
    public SerializedDictionary<Tile.TileState, Sprite> Visuals;
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
    /** End ITileObject **/
}
