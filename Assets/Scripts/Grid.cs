using GameAnalyticsSDK;
using Schemas;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public Action OnGridRequestedVisualUpdate;
    public Action OnGridGenerated;
    public bool MinesDiffused = false;

    private void Awake()
    {
        ServiceLocator.Instance.Register(this);
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

        // Send game analytics that the player has started a new level. progression01=LevelName. progression02=Class.ToString. Score=PlayerLevel.
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, ServiceLocator.Instance.LevelManager.CurrentLevelName, ServiceLocator.Instance.Player.Class.ToString());

        OnGridGenerated?.Invoke();
    }

    private void PlaceSpawns()
    {
        // The spawn entries are handled in order, so fill those out with that in mind
        SpawnEntriesInArray(SpawnSettings.GridSpawns);
        SpawnEntriesInArray(SpawnSettings.NormalSpawns);
        
        // Now do the swaps once things are spawned
        HandleEntriesSwaps();
    }

    private void SpawnEntriesInArray(SpawnSettings.GridSpawnEntry[] entries)
    {
        
        // TODO: Issue here !! If they don't exist in the Entries list then they don't get the bonus spawns.
        //  I've added a helper button to add 0 entries on missing items but its not ideal. 
        //  The reason I can't just inject it is because the spawn requirements are not IN the tile object but they are 
        //  assosciated instead. We might wanna consider hard linking them? idk
        // TODO: Should we support bonus spawns when it comes to ConsecutiveSpawn types? maybe?? 
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
                            // Do not allow to go down in bonus copies; this might have some oddities but was causing a crash 
                            // with negative bonus (repellents + bricks, bascically)
                            consecutiveCopies = Mathf.Max(consecutiveCopies, consecutiveCopies + bonusCopies);
                        }
                        Debug.Log($"Try spawn {spawnEntry.ConsecutiveCopies}+{bonusCopies} consecutives given {additionalSpawnLocations.Count} possibilities. name:{spawnEntry.Requirement.name}");

                        var childTiles = new Tile[consecutiveCopies];
                        for (int add = 0; add < consecutiveCopies && add < additionalSpawnLocations.Count; add++)
                        {
                            UnoccupiedSpaces.RemoveUnoccupiedSpace(additionalSpawnLocations[add].x, additionalSpawnLocations[add].y);
                            Tile childTile = Tiles[additionalSpawnLocations[add].x, additionalSpawnLocations[add].y];
                            childTile.PlaceTileObj(spawnEntry.ConsecutiveSpawn);
                            childTiles[add] = childTile;
                            if (spawnEntry.GuardRelationship)
                            {
                                // Minotaur.GuardingTile = chest
                                childTile.GuardingTile = Tiles[coordinates.Item1, coordinates.Item2];
                                if(spawnEntry.LookTowardsEachOther)
                                {
                                    childTile.LookTowardsHorizontally(coordinates.Item1, coordinates.Item2, false, false);
                                }
                                else
                                {
                                    childTile.LookAwayFrom(coordinates.Item1, coordinates.Item2, false);
                                }

                                // Chest.BodyGuardedByTile = Minotaur
                                // TODO: Support multiple bodyguards
                                Tiles[coordinates.Item1, coordinates.Item2].BodyGuardedByTile = childTile;
                            }
                            else if (spawnEntry.LookTowardsEachOther)
                            {
                                childTile.LookTowardsOrthogonally(coordinates.Item1, coordinates.Item2, false, false);
                                Tiles[coordinates.Item1, coordinates.Item2].LookTowardsOrthogonally(additionalSpawnLocations[add].x, additionalSpawnLocations[add].y, false, false);
                            }
                        }
                        Tiles[coordinates.Item1, coordinates.Item2].ChildrenTiles = childTiles;
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

    private void HandleEntriesSwaps()
    {
        foreach (var ((fromTag, toTileId), amount) in ServiceLocator.Instance.Player.TileSwapsByTag)
        {
            List<Tile> validGridTiles = new List<Tile>();
            TileSchema schema = ServiceLocator.Instance.Schemas.TileObjectSchemas.Find(s => s.TileId == toTileId);
            if (schema == null)
            {
                continue;
            }
            
            for (int y = 0; y < SpawnSettings.Height; y++)
            {
                for (int x = 0; x < SpawnSettings.Width; x++)
                {
                    if (!Tiles[x, y].TEMP_IsEmpty() && Tiles[x, y].GetHousedObject().Tags.Contains(fromTag))
                    {
                        validGridTiles.Add(Tiles[x, y]);
                    }
                }
            }
            
            validGridTiles.Shuffle();
            for (var i = 0; i < validGridTiles.Count && i < amount; i++)
            {
                validGridTiles[i].UndoPlacedTileObj();
                validGridTiles[i].PlaceTileObj(schema);
            }
        }
        
        foreach (var ((fromId, toTileId), amount) in ServiceLocator.Instance.Player.TileSwaps)
        {
            List<Tile> validGridTiles = new List<Tile>();
            TileSchema schema = ServiceLocator.Instance.Schemas.TileObjectSchemas.Find(s => s.TileId == toTileId);
            if (schema == null)
            {
                continue;
            }
            
            for (int y = 0; y < SpawnSettings.Height; y++)
            {
                for (int x = 0; x < SpawnSettings.Width; x++)
                {
                    if (!Tiles[x, y].TEMP_IsEmpty() && Tiles[x, y].GetHousedObject().TileId == fromId)
                    {
                        validGridTiles.Add(Tiles[x, y]);
                    }
                }
            }
            
            validGridTiles.Shuffle();
            for (var i = 0; i < validGridTiles.Count && i < amount; i++)
            {
                validGridTiles[i].UndoPlacedTileObj();
                validGridTiles[i].PlaceTileObj(schema);
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

    public Transform GetTileTransform(TileSchema.Id tileId)
    {
        for (int y = 0; y < SpawnSettings.Height; y++)
        {
            for (int x = 0; x < SpawnSettings.Width; x++)
            {
                if (!Tiles[x, y].TEMP_IsEmpty() && Tiles[x, y].GetHousedObject().TileId == tileId)
                {
                    return Tiles[x,  y].transform;
                }
            }
        }

        return null;
    }

    public bool IsTileRevealed(int x, int y)
    {
        return InGridBounds(x, y) && Tiles[x, y].State >= Tile.TileState.Revealed;
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
                    // This is the grid reveal so use the fast reveal without logic
                    Tiles[x + i, y + j].FastRevealWithoutLogic_VisionOrb(vfx);
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
        // perf reasons....dont do it on mobile :\
        /*
        if (Application.isMobilePlatform)
        {
            return;
        }
        */
        
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

    public void RevealRandomUnoccupiedTile(GameObject vfx = null)
    {
        if (!UnoccupiedSpaces.HasEmptySpace())
        {
            return;
        }

        var space = UnoccupiedSpaces.PeekUnoccupiedRandomSpace();
        Tiles[space.Item1, space.Item2].TEMP_RevealWithoutLogic(vfx);
        UnoccupiedSpaces.RemoveUnoccupiedSpace(space.Item1, space.Item2);
    }

    public void UpdateRandomTileById(TileSchema.Id from, TileSchema.Id to)
    {
        var toSchema = ServiceLocator.Instance.Schemas.TileObjectSchemas.Find(s => s.TileId == to);
        if (toSchema == null)
        {
            return;
        }
        
        (int, int) coord = GetPositionOfRandomType(from);
        if (!InGridBounds(coord.Item1, coord.Item2))
        {
            return;
        }
        
        Tiles[coord.Item1, coord.Item2].UndoPlacedTileObj();
        Tiles[coord.Item1, coord.Item2].PlaceTileObj(toSchema);
        OnGridRequestedVisualUpdate?.Invoke();
    }

    public void UpdateTile(Tile from, TileSchema.Id to)
    {
        var toSchema = ServiceLocator.Instance.Schemas.TileObjectSchemas.Find(s => s.TileId == to);
        if (toSchema == null)
        {
            return;
        }
        
        from.UndoPlacedTileObj();
        from.PlaceTileObj(toSchema);
        OnGridRequestedVisualUpdate?.Invoke();
    }
    
    // TODO: Refactor Tag vs Id
    public void RevealRandomOfTag(TileSchema.Tag tileTag, GameObject vfx = null, List<Tile.TileState> validStates = null)
    {
        (int, int) coord = GetPositionOfRandomTag(tileTag, validStates);
        if (!InGridBounds(coord.Item1, coord.Item2))
        {
            return;
        }
        
        Tiles[coord.Item1, coord.Item2].TEMP_RevealWithoutLogic(vfx);
    }
    
    /// <summary>
    /// Reveals a random tile that has the matching monster id.
    /// </summary>
    public void RevealRandomOfType(TileSchema.Id tileId, GameObject vfx = null, List<Tile.TileState> validStates = null)
    {
        (int, int) coord = GetPositionOfRandomType(tileId, validStates);
        if (!InGridBounds(coord.Item1, coord.Item2))
        {
            return;
        }
        
        Tiles[coord.Item1, coord.Item2].TEMP_RevealWithoutLogic(vfx);
    }

    /// <summary>
    /// Get position of a random monster of type with specified state
    /// </summary>
    /// <param name="tileId">monster id</param>
    /// <returns>x and y int coordinates</returns>
    public (int, int) GetPositionOfRandomType(TileSchema.Id tileId, List<Tile.TileState> validStates = null)
    {
        int yStarting = Random.Range(0, SpawnSettings.Height);
        int xStarting = Random.Range(0, SpawnSettings.Width);
        for (int y = yStarting; y < SpawnSettings.Height; y++)
        {
            for (int x = xStarting; x < SpawnSettings.Width; x++)
            {
                if (Tiles[x, y].GetHousedObject() &&
                    Tiles[x, y].GetHousedObject().TileId == tileId &&
                    (validStates == null || validStates.Contains(Tiles[x, y].State)))
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
                    (validStates == null || validStates.Contains(Tiles[x, y].State)))
                {
                    return (x, y);
                }
            }
        }
        return (-1, -1);
    }
    
    /// <summary>
    /// Get position of a random monster of tag with specified state
    /// We EXCLUDE dragons because we dont wanna do anything with that...
    /// </summary>
    /// <param name="tileId">monster id</param>
    /// <returns>x and y int coordinates</returns>
    public (int, int) GetPositionOfRandomTag(TileSchema.Tag tagId, List<Tile.TileState> validStates = null)
    {
        int yStarting = Random.Range(0, SpawnSettings.Height);
        int xStarting = Random.Range(0, SpawnSettings.Width);
        for (int y = yStarting; y < SpawnSettings.Height; y++)
        {
            for (int x = xStarting; x < SpawnSettings.Width; x++)
            {
                if (Tiles[x, y].GetHousedObject() &&
                    Tiles[x, y].GetHousedObject().Tags.Contains(tagId) &&
                    !Tiles[x, y].GetHousedObject().Tags.Contains(TileSchema.Tag.Dragon) &&
                    (validStates == null || validStates.Contains(Tiles[x, y].State)))
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
                    Tiles[x, y].GetHousedObject().Tags.Contains(tagId) &&
                    !Tiles[x, y].GetHousedObject().Tags.Contains(TileSchema.Tag.Dragon) &&
                    (validStates == null  || validStates.Contains(Tiles[x, y].State)))
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
    /// This includes NON enemies like blocks by default!
    /// </summary>
    public int TEMP_GetUnconqueredNeighborCount(int x, int y, bool onlyEnemies = false)
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

                if (onlyEnemies &&
                    !Tiles[x + i, y + j].TEMP_IsEmpty() &&
                    !Tiles[x + i, y + j].GetHousedObject().Tags.Contains(TileSchema.Tag.Enemy)
                )
                {
                    continue;
                }

                amount += Tiles[x + i, y + j].State < Tile.TileState.Conquered ? 1 : 0;
            }
        }

        return amount;
    }

    public List<TileSchema> GetUnconqueredNeighborSchemas(int x, int y, int radius = 1, bool onlyEnemies = false)
    {
        List<TileSchema> neighbors = new List<TileSchema>();
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                if (!InGridBounds(x + i, y + j))
                {
                    continue;
                }

                Tile tile = Tiles[x + i, y + j];
                if (tile.TEMP_IsEmpty())
                {
                    continue;
                }

                if (onlyEnemies && !tile.GetHousedObject().Tags.Contains(TileSchema.Tag.Enemy))
                {
                    continue;
                }
                
                if (tile.State >= Tile.TileState.Conquered)
                {
                    continue;
                }
                
                neighbors.Add(tile.GetHousedObject());
            }
        }

        return neighbors;
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
    public void TEMP_DiffuseMines()
    {
        StartCoroutine(DiffuseMinesOverTime());
    }

    private IEnumerator DiffuseMinesOverTime()
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

                tile.PlaceTileObj(DiffusedMine);
                // explosion VFX and SFX is handled by setting the DiffusedMine state to conquered
                tile.TEMP_SetState(Tile.TileState.Conquered);
                yield return new WaitForSeconds(0.2f);
            }
        }

        MinesDiffused = true;
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

    public List<TileSchema> GetAllTileObjects(Tile.TileState[] allowedStates = null)
    {
        List<TileSchema> tileObjects = new List<TileSchema>();
        if (Tiles == null || Tiles.Length == 0)
        {
            return tileObjects;
        }
        
       for (int y = 0; y < SpawnSettings.Height; y++)
        {
            for (int x = 0; x < SpawnSettings.Width; x++)
            {
                if (!Tiles[x, y].TEMP_IsEmpty())
                {
                    if (allowedStates != null && !allowedStates.Contains(Tiles[x, y].State))
                    {
                        continue;
                    }
                    tileObjects.Add(Tiles[x, y].GetHousedObject());
                }
            }
        }

        return tileObjects;
    }

    public void MassTeleport(List<TileSchema.Tag> tags, GameObject vfx = null)
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

                // Do not mass Teleport the dragon
                if (housedObject.Tags.Contains(TileSchema.Tag.Dragon))
                {
                    continue;
                }
                
                if (!housedObject.Tags.Intersect(tags).Any())
                {
                    continue;
                }
                
                tileObjects.Add(housedObject);
                spots.Add((x, y));
            }
        }
        
        spots.Shuffle();
        tileObjects.Shuffle();

        for (int i = 0; i < spots.Count; i++)
        {
            Tiles[spots[i].Item1, spots[i].Item2].UndoPlacedTileObj();
            Tiles[spots[i].Item1, spots[i].Item2].PlaceTileObj(tileObjects[i]);

            if (vfx)
            {
                Instantiate(vfx, Tiles[spots[i].Item1, spots[i].Item2].transform);
            }
        }

        OnGridRequestedVisualUpdate?.Invoke();
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

    public void RevealRandomCol(GameObject vfx = null)
    {
        int col = Random.Range(0, SpawnSettings.Width);
        for (int i = 0; i < SpawnSettings.Height; i++)
        {
            Tiles[col, i].TEMP_RevealWithoutLogic(vfx);
        }
    }
    
    public void RevealRandomRow(GameObject vfx = null)
    {
        int row = Random.Range(0, SpawnSettings.Height);
        for (int i = 0; i < SpawnSettings.Width; i++)
        {
            Tiles[i, row].TEMP_RevealWithoutLogic(vfx);
        }
    }

    public RectTransform GetBottomLeft()
    {
        return Tiles[0, 0].transform as RectTransform;
    }

    public RectTransform GetTopRight()
    {
        return Tiles[SpawnSettings.Width - 1, SpawnSettings.Height - 1].transform as RectTransform;
    }
    
    public void MassPolymorph(TileSchema.Id transformTo, GameObject vfx = null)
    {
        var transformSchema = ServiceLocator.Instance.Schemas.TileObjectSchemas.Find(i => i.TileId == transformTo);
        if (transformSchema == null)
        {
            Debug.Log("Transform id was not found -- cannot polymorph!");
            return;
        }
        
        for (int y = 0; y < SpawnSettings.Height; y++)
        {
            for (int x = 0; x < SpawnSettings.Width; x++)
            {
                var tile = Tiles[x, y];
                if (tile.TEMP_IsEmpty())
                {
                    continue;
                }

                var tags = tile.GetHousedObject().Tags;
                if (tags.Contains(TileSchema.Tag.Dragon))
                {
                    continue;
                }
                
                if (!tags.Contains(TileSchema.Tag.Enemy))
                {
                    continue;
                }

                if (tile.State != Tile.TileState.Revealed && tile.State != Tile.TileState.RevealThroughCombat)
                {
                    continue;
                }

                Tiles[x, y].UndoPlacedTileObj();
                Tiles[x, y].PlaceTileObj(transformSchema);
                
                if (vfx != null)
                {
                    Instantiate(vfx, Tiles[x, y].transform);
                }
            }
        }
    }

    public void ConquerRandomOfType(TileSchema.Id tileId)
    {
        (int, int) coord = GetPositionOfRandomType(tileId, new List<Tile.TileState>() {Tile.TileState.Hidden, Tile.TileState.Revealed, Tile.TileState.RevealThroughCombat});
        if (!InGridBounds(coord.Item1, coord.Item2))
        {
            return;
        }

        Tiles[coord.Item1, coord.Item2].AllowDamage = false;
        Tiles[coord.Item1, coord.Item2].TEMP_SetState(Tile.TileState.Conquered);
        Tiles[coord.Item1, coord.Item2].AllowDamage = true;
    }

    public void ConquerRandomOfTag(TileSchema.Tag tileTag)
    {
        (int, int) coord = GetPositionOfRandomTag(tileTag, new List<Tile.TileState>() {Tile.TileState.Hidden, Tile.TileState.Revealed, Tile.TileState.RevealThroughCombat});
        if (!InGridBounds(coord.Item1, coord.Item2))
        {
            return;
        }
        
        Tiles[coord.Item1, coord.Item2].AllowDamage = false;
        Tiles[coord.Item1, coord.Item2].TEMP_SetState(Tile.TileState.Conquered);
        Tiles[coord.Item1, coord.Item2].AllowDamage = true;
    }
}
