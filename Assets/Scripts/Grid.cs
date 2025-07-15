using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Schemas;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// Quick and dirty implementation of an NxM Grid of Tiles for the gameplay.
/// Eventually this should be malleable enough to do different shapes with holes and corridors.
/// Probably using some sort of markup level file.
/// </summary>
public class Grid : MonoBehaviour
{
    // TODO: Make better way to reference this shit. Probably with an object refactor kekw
    [SerializeField] 
    private TileSchema Mine;
    
    [SerializeField] 
    private TileSchema DiffusedMine;
    
    /// <summary>
    /// The spawn settings for this enemy spawner.
    /// By having these as a separate data object, we can easily hot-swap for levels without haveing to serialize
    /// a new gameobject for every configuration.
    /// </summary>
    [SerializeField, Header("Grid Settings")]
    public SpawnSettings SpawnSettings;
    
    [SerializeField, Header("Grid Settings")]
    private Tile TilePrefab;

    [SerializeField, Header("Grid Settings")]
    private GridLayoutGroup Layout;
    
    private Tile[,] Tiles;
    public RandomBoard UnoccupiedSpaces;

    // TODO: Probably should be a cleaner delegate and not owned by this class?
    public Action<Tile> OnTileStateChanged;
    public Action OnGridGenerated;
    public bool MinesDiffused = false;

    private void Awake()
    {
        ServiceLocator.Instance.Register(this);
    }

    private void Start()
    {
        // Start the game level 1
        GenerateGrid();
    }

    private void ResetGrid()
    {
        MinesDiffused = false;
        // Clear anything we already have
        if (Tiles != null)
        {
            foreach (var item in Tiles)
            {
                Destroy(item.gameObject);
            }
        }
    }
    
    /// <summary>
    /// Creates the Grid using the Serialized Properties marked with Grid Settings.
    /// The position of the GameObject that owns the script is used as the center of the Grid.
    /// [0,0] of the Grid is the bottom-left corner.
    /// </summary>
    public void GenerateGrid()
    {
        ResetGrid();
        
        // [0,0] is the bottom left
        Layout.startCorner = GridLayoutGroup.Corner.LowerLeft;

        // Temporary solution for proper positioning, just fix the column sizes and add the tiles in order
        // We can make this more flexible if we:
        //  1) define the size of the container rect
        //  2) define the size of the grid cells by a % of the container rect size
        //  3) make the tiles non-fixed size and have them fill 
        // But for now this will do for a simple solution
        Layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        Layout.constraintCount = SpawnSettings.Width;
        
        Tiles = new Tile[SpawnSettings.Width, SpawnSettings.Height];
        UnoccupiedSpaces = new RandomBoard(SpawnSettings.Width, SpawnSettings.Height);

        // Spawn actual tiles.
        for (int y = 0; y < SpawnSettings.Height; y++)
        {
            for (int x = 0; x < SpawnSettings.Width; x++)
            {
                Tile tile = Instantiate(TilePrefab, transform);

                // For testing, update eventually
                tile.TEMP_SetCoordinates(x, y);

                tile.PlaceTileObj(null);
                Tiles[x, y] = tile;
            }
        }

        PlaceSpawns();
        
        OnGridGenerated?.Invoke();
    }

    private void PlaceSpawns()
    {
        // The spawn entries are handled in order, so fill those out with that in mind
        SpawnEntriesInArray(SpawnSettings.GridSpawns);
        SpawnEntriesInArray(SpawnSettings.NormalSpawns);
    }

    private void SpawnEntriesInArray(SpawnSettings.GridSpawnEntry[] entries)
    {
        // TODO: Issue here !! If they don't exist in the Entries list then they don't get the bonus spawns.
        //  I've added a helper button to add 0 entries on missing items but its not ideal. 
        //  The reason I can't just inject it is because the spawn requirements are not IN the tile object but they are 
        //  assosciated instead. We might wanna consider hard linking them? idk
        // TODO: Should we suppor bonus spawns when it comes to ConsecutiveSpawn types? maybe?? 
        foreach (var (key, bonus) in ServiceLocator.Instance.Player.BonusSpawn)
        {
            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry.Object.TileId == key)
                {
                    entry.Amount += bonus;
                }
            }
        }

        foreach (var spawnEntry in entries)
        {
            ServiceLocator.Instance.Player.BonusSpawn.TryGetValue(spawnEntry.Object.TileId, out int bonusAmount);
            var amount = spawnEntry.Amount + bonusAmount;
            
            int desiredLookX = 0, desiredLookY = 0;
            if (spawnEntry.Object.SpriteFacingData.ObjectToLookAtOverride != null)
            {
                (desiredLookX, desiredLookY) = ServiceLocator.Instance.Grid.FindNearest(spawnEntry.Object.SpriteFacingData.ObjectToLookAtOverride, 0, 0);
            }

            for (int i = 0; i < amount; i++)
            {
                (int, int) coordinates;
                if (spawnEntry.Requirement != null)
                {
                    coordinates = spawnEntry.Requirement.GetRandomCoordinate(UnoccupiedSpaces);
                    UnoccupiedSpaces.RemoveUnoccupiedSpace(coordinates.Item1, coordinates.Item2);
                    Tiles[coordinates.Item1, coordinates.Item2].PlaceTileObj(spawnEntry.Object);
                    
                    if (spawnEntry.Requirement.RevealAfterSpawn)
                    {
                        Tiles[coordinates.Item1, coordinates.Item2].TEMP_RevealWithoutLogic();
                    }
                    if (spawnEntry.ConsecutiveSpawn != null)
                    {
                        var additionalSpawnLocations = spawnEntry.Requirement.GetRandomConsecutiveNeighborLocations(UnoccupiedSpaces, coordinates.Item1, coordinates.Item2);
                        additionalSpawnLocations.Shuffle();
                        int consecutiveCopies = spawnEntry.ConsecutiveCopies;
                        if (ServiceLocator.Instance.Player.BonusSpawn.TryGetValue(spawnEntry.ConsecutiveSpawn.TileId, out int bonusCopies))
                        {
                            consecutiveCopies += bonusCopies;
                        }
                        Debug.Log($"Try spawn {spawnEntry.ConsecutiveCopies}+{bonusCopies} consecutives given {additionalSpawnLocations.Count} possibilities. name:{spawnEntry.Requirement.name}");

                        for (int add = 0; add < consecutiveCopies && add < additionalSpawnLocations.Count; add++)
                        {
                            UnoccupiedSpaces.RemoveUnoccupiedSpace(additionalSpawnLocations[add].x, additionalSpawnLocations[add].y);
                            Tiles[additionalSpawnLocations[add].x, additionalSpawnLocations[add].y].PlaceTileObj(spawnEntry.ConsecutiveSpawn);
                            if (spawnEntry.GuardRelationship)
                            {
                                // Minotaur.GuardingTile = chest
                                Tiles[additionalSpawnLocations[add].x, additionalSpawnLocations[add].y].GuardingTile = Tiles[coordinates.Item1, coordinates.Item2];
                                if(spawnEntry.LookTowardsEachOther)
                                {
                                    Tiles[additionalSpawnLocations[add].x, additionalSpawnLocations[add].y].LookTowardsHorizontally(coordinates.Item1, coordinates.Item2, false, false);
                                }
                                else
                                {
                                    Tiles[additionalSpawnLocations[add].x, additionalSpawnLocations[add].y].LookAwayFrom(coordinates.Item1, coordinates.Item2, false);
                                }

                                // Chest.BodyGuardedByTile = Minotaur
                                Tiles[coordinates.Item1, coordinates.Item2].BodyGuardedByTile = Tiles[additionalSpawnLocations[add].x, additionalSpawnLocations[add].y];
                            }
                            else if (spawnEntry.LookTowardsEachOther)
                            {
                                Tiles[additionalSpawnLocations[add].x, additionalSpawnLocations[add].y].LookTowardsOrthogonally(coordinates.Item1, coordinates.Item2, false, false);
                                Tiles[coordinates.Item1, coordinates.Item2].LookTowardsOrthogonally(additionalSpawnLocations[add].x, additionalSpawnLocations[add].y, false, false);
                            }
                        }
                    }
                }
                else
                {
                    // Default to random unoccupied space.
                    coordinates = UnoccupiedSpaces.PeekUnoccupiedRandomSpace();
                    UnoccupiedSpaces.RemoveUnoccupiedSpace(coordinates.Item1, coordinates.Item2);
                    Tiles[coordinates.Item1, coordinates.Item2].PlaceTileObj(spawnEntry.Object);
                }

                if (spawnEntry.Object.SpriteFacingData.ObjectToLookAtOverride != null)
                {
                    Tiles[coordinates.Item1, coordinates.Item2].LookTowardsHorizontally(desiredLookX, desiredLookY, true, true);
                }
            }
        }
    }

    public bool InGridBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < SpawnSettings.Width && y < SpawnSettings.Height;
    }

    public Transform GetTileTransform(int x, int y)
    {
        return !InGridBounds(x, y) ? null : Tiles[x, y].transform;
    }
    
    /// <summary>
    /// Reveals tiles in a radius from the origin x,y.
    /// </summary>
    public void TEMP_RevealTilesInRadius(int x, int y, int radius, GameObject vfx = null)
    {
        if (radius <= 0)
        {
            return;
        }

        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                if (InGridBounds(x + i, y + j))
                {
                    Tiles[x + i, y + j].TEMP_RevealWithoutLogic(vfx);
                }
            }
        }
    }
    
    /// <summary>
    /// Reveals tiles from specific offsets from the origin x,y.
    /// </summary>
    public void TEMP_RevealTiles(int x, int y, Vector2Int[] offets, GameObject vfx = null)
    {
        foreach (var offet in offets)
        {
            int newX = offet.x + x;
            int newY = offet.y + y;
            if (!InGridBounds(newX, newY))
            {
                continue;
            }
            
            Tiles[newX, newY].TEMP_RevealWithoutLogic(vfx);
        }
    }
    
    /// <summary>
    /// Used for cheats.
    /// </summary>
    public void TEMP_RevealAllTiles()
    {
        for (int y = 0; y < SpawnSettings.Height; y++)
        {
            for (int x = 0; x < SpawnSettings.Width; x++)
            {
                Tiles[x, y].FastRevealWithoutLogic();
            }
        }
    }
    
    /// <summary>
    /// Reveals all tiles that have the matching object schema.
    /// </summary>
    public void TEMP_RevealAllOfType(TileSchema objectSchema, bool standUp, GameObject vfx = null)
    {
        for (int y = 0; y < SpawnSettings.Height; y++)
        {
            for (int x = 0; x < SpawnSettings.Width; x++)
            {
                if (Tiles[x, y].GetHousedObject() == objectSchema)
                {
                    if (standUp)
                    {
                        Tiles[x, y].StandUp();
                    }
                    
                    Tiles[x, y].TEMP_RevealWithoutLogic(vfx);
                }
            }
        }
    }

    public void RevealRandomUnoccupiedTile()
    {
        if (!UnoccupiedSpaces.HasEmptySpace())
        {
            return;
        }

        var space = UnoccupiedSpaces.PeekUnoccupiedRandomSpace();
        Tiles[space.Item1, space.Item2].TEMP_RevealWithoutLogic();
        UnoccupiedSpaces.RemoveUnoccupiedSpace(space.Item1, space.Item2);
    }

    // TODO: Refactor Tag vs Id
    public void RevealRandomOfTag(TileSchema.Tag tileTag)
    {
        int yStarting = Random.Range(0, SpawnSettings.Height);
        int xStarting = Random.Range(0, SpawnSettings.Width);
        for (int y = yStarting; y < SpawnSettings.Height; y++)
        {
            for (int x = xStarting; x < SpawnSettings.Width; x++)
            {
                if (Tiles[x, y].GetHousedObject() &&
                    Tiles[x, y].GetHousedObject().Tags.Contains(tileTag) &&
                    Tiles[x, y].State == Tile.TileState.Hidden)
                {
                    Tiles[x, y].TEMP_RevealWithoutLogic();
                    return;
                }
            }
        }

        for (int y = 0; y < SpawnSettings.Height; y++)
        {
            for (int x = 0; x < SpawnSettings.Width; x++)
            {
                if (Tiles[x, y].GetHousedObject() &&
                    Tiles[x, y].GetHousedObject().Tags.Contains(tileTag) &&
                    Tiles[x, y].State == Tile.TileState.Hidden)
                {
                    Tiles[x, y].TEMP_RevealWithoutLogic();
                    return;
                }
            }
        }
    }
    
    /// <summary>
    /// Reveals a random tile that has the matching monster id.
    /// </summary>
    public void RevealRandomOfType(TileSchema.Id tileId, GameObject vfx = null)
    {
        int yStarting = Random.Range(0, SpawnSettings.Height);
        int xStarting = Random.Range(0, SpawnSettings.Width);
        for (int y = yStarting; y < SpawnSettings.Height; y++)
        {
            for (int x = xStarting; x < SpawnSettings.Width; x++)
            {
                if (Tiles[x, y].GetHousedObject() &&
                    Tiles[x, y].GetHousedObject().TileId == tileId &&
                    Tiles[x, y].State == Tile.TileState.Hidden)
                {
                    Tiles[x, y].TEMP_RevealWithoutLogic(vfx);
                    return;
                }
            }
        }

        for (int y = 0; y < SpawnSettings.Height; y++)
        {
            for (int x = 0; x < SpawnSettings.Width; x++)
            {
                if (Tiles[x, y].GetHousedObject() &&
                    Tiles[x, y].GetHousedObject().TileId == tileId &&
                    Tiles[x, y].State == Tile.TileState.Hidden)
                {
                    Tiles[x, y].TEMP_RevealWithoutLogic(vfx);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Get position of a random unrevealed monster type
    /// </summary>
    /// <param name="tileId">monster id</param>
    /// <returns>x and y int coordinates</returns>
    public (int, int) GetPositionOfRandomType(TileSchema.Id tileId)
    {
        int yStarting = Random.Range(0, SpawnSettings.Height);
        int xStarting = Random.Range(0, SpawnSettings.Width);
        for (int y = yStarting; y < SpawnSettings.Height; y++)
        {
            for (int x = xStarting; x < SpawnSettings.Width; x++)
            {
                if (Tiles[x, y].GetHousedObject() &&
                    Tiles[x, y].GetHousedObject().TileId == tileId &&
                    Tiles[x, y].State == Tile.TileState.Hidden)
                {
                    return (x, y);
                }
            }
        }

        for (int y = 0; y < SpawnSettings.Height; y++)
        {
            for (int x = 0; x < SpawnSettings.Width; x++)
            {
                if (Tiles[x, y].GetHousedObject() &&
                    Tiles[x, y].GetHousedObject().TileId == tileId &&
                    Tiles[x, y].State == Tile.TileState.Hidden)
                {
                    return (x, y);
                }
            }
        }
        return (-1, -1);
    }

    public List<(int, int)> GetAdjacentValidPositions(int x, int y, int distance = 1)
    {
        int totalNumberOfAdjacentThings = (distance + 1) * (distance + 1) - 1;
        List<(int, int)> returnValue = new List<(int, int)>(totalNumberOfAdjacentThings);
        for (int i = x - distance; i < x + distance && i >= 0 && i < SpawnSettings.Width; i++)
        {
            for(int j = y - distance; j < y + distance && j >= 0 && j < SpawnSettings.Height; j++)
            {
                returnValue.Add((i, j));
            }
        }
        return returnValue;
    }

    /// <summary>
    /// Gets the total cost of all neighbors for a given coordinate.
    /// </summary>
    public int TEMP_GetTotalNeighborPower(int x, int y)
    {
        int cost = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                if (!InGridBounds(x + i, y + j))
                {
                    continue;
                }
                
                cost += Tiles[x + i, y + j].GetPublicPower();
            }
        }

        return cost;
    }

    /// <summary>
    /// Gets the total amount of neighbors that are not conquered+.
    /// </summary>
    public int TEMP_GetUnconqueredNeighborCount(int x, int y)
    {
        int amount = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                if (!InGridBounds(x + i, y + j))
                {
                    continue;
                }

                amount += Tiles[x + i, y + j].State < Tile.TileState.Conquered ? 1 : 0;
            }
        }

        return amount;
    }
    
    /// <summary>
    /// Returns the object in the given coordinates. Can be null.
    /// </summary>
    public TileSchema GetObject(int xCoordinate, int yCoordinate)
    {
        return Tiles[xCoordinate, yCoordinate].GetHousedObject();
    }

    /// <summary>
    /// Returns the distance between the two coordinates.
    /// </summary>
    /// <returns></returns>
    public int GetDistance(int xCoordinate, int yCoordinate, int xCoordinate2, int yCoordinate2)
    {
        // Calculate the Manhattan distance
        int deltaX = Math.Abs(xCoordinate2 - xCoordinate);
        int deltaY = Math.Abs(yCoordinate2 - yCoordinate);
        return deltaX + deltaY;
    }

    /// <summary>
    /// Diffuses all mines by replacing them with Diffused Mines.
    /// </summary>
    public void TEMP_DiffuseMarkedOrRevealedMines()
    {
        for (int y = 0; y < SpawnSettings.Height; y++)
        {
            for (int x = 0; x < SpawnSettings.Width; x++)
            {
                var tile = Tiles[x, y];
                if (tile.GetHousedObject() != Mine)
                {
                    continue;
                }

                // Revealed mines become diffused
                if (tile.TEMP_IsRevealed())
                {
                    tile.PlaceTileObj(DiffusedMine);
                    tile.TEMP_SetState(Tile.TileState.Revealed);
                }
                else
                {
                    tile.PlaceTileObj(DiffusedMine);
                }
            }
        }
        MinesDiffused = true;

        StartCoroutine(Shake());
    }

    public IEnumerator Shake(float duration = 1.25f, float magnitude = 30)
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float percentElapsedInvertSquared = 1f - elapsed / duration;
            percentElapsedInvertSquared = percentElapsedInvertSquared * percentElapsedInvertSquared;
            float x = Random.Range(-1, 1f) * magnitude * percentElapsedInvertSquared + originalPosition.x;
            float y = Random.Range(-1, 1f) * magnitude * percentElapsedInvertSquared + originalPosition.y;
            transform.localPosition = new Vector3(x, y, originalPosition.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    public void Obscure(int xCoordinate, int yCoordinate, int radius)
    {
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (!InGridBounds(i + xCoordinate, j + yCoordinate))
                {
                    continue;
                }
                Tiles[i + xCoordinate, j + yCoordinate].TEMP_Obscure();
            }
        }
    }

    public void Unobscure(int xCoordinate, int yCoordinate, int radius)
    {
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                if (!InGridBounds(i + xCoordinate, j + yCoordinate))
                {
                    continue;
                }
                Tiles[i + xCoordinate, j + yCoordinate].TEMP_UnObscure();
            }
        }
    }
    
    public void Obscure(int xCoordinate, int yCoordinate, Vector2Int[] offsets)
    {
        foreach (var offset in offsets)
        {
            int newX = xCoordinate + offset.x;
            int newY = yCoordinate + offset.y;
            
            if (!InGridBounds(newX, newY))
            {
                continue;
            }
            
            Tiles[newX, newY].TEMP_Obscure();
        }
    }
    
    public void Unobscure(int xCoordinate, int yCoordinate, Vector2Int[] offsets)
    {
        foreach (var offset in offsets)
        {
            int newX = xCoordinate + offset.x;
            int newY = yCoordinate + offset.y;
            
            if (!InGridBounds(newX, newY))
            {
                continue;
            }
            
            Tiles[newX, newY].TEMP_UnObscure();
        }
    }

    /// <summary>
    /// Returns true if the housed object can flee.
    /// If there is space, place the housed object on that tile.
    /// </summary>
    /// <param name="housedObject"></param>
    /// <param name="reveal"></param>
    /// <returns></returns>
    public bool TEMP_HandleFlee(TileSchema housedObject, bool reveal)
    {
        if (!UnoccupiedSpaces.HasEmptySpace())
        {
            return false;
        }
        
        (int, int) newCoord = UnoccupiedSpaces.PeekUnoccupiedRandomSpace();
        
        Tiles[newCoord.Item1, newCoord.Item2].PlaceTileObj(housedObject);
        Tiles[newCoord.Item1, newCoord.Item2].TEMP_SetState(Tile.TileState.Hidden);

        if (reveal)
        {
            Tiles[newCoord.Item1, newCoord.Item2].TEMP_RevealWithoutLogic();
        }
        
        UnoccupiedSpaces.RemoveUnoccupiedSpace(newCoord.Item1, newCoord.Item2);

        return true;
    }
    
    // I feel like this is some leetcode shit
    // todo: use Vector2Int
    public (int, int) FindNearest(TileSchema toFind, int xOrigin, int yOrigin)
    {
        (int, int)[] directions = { (0, 1), (0, -1), (1, 0), (-1, 0) };
        Queue<(int x, int y, int dist)> queue = new();
        HashSet<(int, int)> visited = new();

        queue.Enqueue((xOrigin, yOrigin, 0));
        visited.Add((xOrigin, yOrigin));

        while (queue.Count > 0)
        {
            var (x, y, dist) = queue.Dequeue();

            if (InGridBounds(x, y) && !Tiles[x, y].TEMP_IsEmpty() && Tiles[x, y].GetHousedObject() == toFind)
            {
                return (x, y);
            }
            
            foreach ((int,int) dir in directions)
            {
                int newX = x + dir.Item1;
                int newY = y + dir.Item2;

                if (newX >= 0 && newX < SpawnSettings.Width && newY >= 0 && newY < SpawnSettings.Height && !visited.Contains((newX, newY)))
                {
                    queue.Enqueue((newX, newY, dist + 1));
                    visited.Add((newX, newY));
                }
            }
        }
        
        return (-1, -1);
    }

    public List<TileSchema> GetAllTileObjects()
    {
        List<TileSchema> tileObjects = new List<TileSchema>();
        for (int y = 0; y < SpawnSettings.Height; y++)
        {
            for (int x = 0; x < SpawnSettings.Width; x++)
            {
                if (!Tiles[x, y].TEMP_IsEmpty())
                {
                    tileObjects.Add(Tiles[x, y].GetHousedObject());
                }
            }
        }

        return tileObjects;
    }

    public void MassTeleport(List<TileSchema.Tag> tags)
    {
        List<(int, int)> spots = new();
        List<TileSchema> tileObjects = new List<TileSchema>();
        for (int y = 0; y < SpawnSettings.Height; y++)
        {
            for (int x = 0; x < SpawnSettings.Width; x++)
            {
                if (Tiles[x, y].TEMP_IsEmpty())
                {
                    continue;
                }

                var housedObject = Tiles[x, y].GetHousedObject();
                if (!housedObject.Tags.Intersect(tags).Any())
                {
                    continue;
                }

                Tiles[x, y].TEMP_UnObscure();
                tileObjects.Add(housedObject);
                spots.Add((x, y));
            }
        }
        
        spots.Shuffle();
        tileObjects.Shuffle();

        for (int i = 0; i < spots.Count; i++)
        {
            Tiles[spots[i].Item1, spots[i].Item2].PlaceTileObj(tileObjects[i]);
        }
    }

    
    /// <summary>
    /// CHEAT METHOD
    /// </summary>
    public void TEMP_ConquerAllTiles()
    {
        for (int y = 0; y < SpawnSettings.Height; y++)
        {
            for (int x = 0; x < SpawnSettings.Width; x++)
            {
                if (Tiles[x, y].TEMP_IsEmpty())
                {
                    continue;
                }
                
                Tiles[x, y].TEMP_SetState(Tile.TileState.Conquered);
            }
        }
    }
}
