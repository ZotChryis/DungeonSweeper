using System;
using System.Collections.Generic;
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
    private TileObjectSchema Mine;
    
    [SerializeField] 
    private TileObjectSchema DiffusedMine;
    
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

    private int PlacementTryLimit = 1000;
    
    private void Start()
    {
        ServiceLocator.Instance.Register(this);

        // TODO: Super temporary way to start the game
        GenerateGrid();
    }

    public int GetHeight()
    {
        return SpawnSettings.Height;
    }
    
    public int GetWidth()
    {
        return SpawnSettings.Width;
    }
    
    /// <summary>
    /// Creates the Grid using the Serialized Properties marked with Grid Settings.
    /// The position of the GameObject that owns the script is used as the center of the Grid.
    /// [0,0] of the Grid is the bottom-left corner.
    /// </summary>
    public void GenerateGrid()
    {
        // Clear anything we already have
        if (Tiles != null)
        {
            foreach (var item in Tiles)
            {
                Destroy(item.gameObject);
            }
        }
        
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

                tile.TEMP_Place(null);
                Tiles[x, y] = tile;
            }
        }

        // TODO: These spawns should be moved to data ?
        // For testing, remove eventually
        // The center of the grid is the Dragon (13)
        PlaceDragon(SpawnSettings.Width / 2, SpawnSettings.Height / 2);

        // Make this spot the vision orb
        PlaceStartingBoon(3 * SpawnSettings.Width / 4, 3 * SpawnSettings.Height / 4);
        PlaceStartingBoon(SpawnSettings.Width / 4, SpawnSettings.Height / 4);

        PlaceSpawns();
        
        OnGridGenerated?.Invoke();
    }

    private void PlaceSpawns()
    {
        // The spawn entries are handled in order, so fill those out with that in mind
        foreach (var spawnEntry in SpawnSettings.GridSpawns)
        {
            // We spawn all of the instances of each enemy before moving on. We can change this if needed
            for (int i = 0; i < spawnEntry.Amount; i++)
            {
                // TODO: This is a dangerous infinite loop possibility. We should guard against it :shrug:
                bool placed = false;
                int limitCheck = 0;
                while (!placed)
                {
                    // TODO: Super hacky - but if we reach a limit, start over
                    if (limitCheck >= PlacementTryLimit)
                    {
                        Debug.Log("Could not find a valid spawn location for " + spawnEntry.Object.ToString());
                        GenerateGrid();
                        return;
                    }
                    
                    (int, int) coordinates = UnoccupiedSpaces.PeekUnoccupiedSpace();

                    bool validCoordinate = true;
                    foreach (var requirement in spawnEntry.Requirements)
                    {
                        if (!requirement.IsValid(coordinates.Item1, coordinates.Item2))
                        {
                            validCoordinate = false;
                            break;
                        }
                    }

                    if (!validCoordinate)
                    {
                        limitCheck++;
                        continue;
                    }

                    placed = true;
                    UnoccupiedSpaces.RemoveUnoccupiedSpace(coordinates.Item1, coordinates.Item2);
                    Tiles[coordinates.Item1, coordinates.Item2].TEMP_Place(spawnEntry.Object);
                }
            }
        }
    }

    private void PlaceStartingBoon(int x, int y)
    {
        UnoccupiedSpaces.RemoveUnoccupiedSpace(x, y);
        Tiles[x, y].TEMP_Place(ServiceLocator.Instance.EnemySpawner.GetRandomStartingBoon());
        Tiles[x, y].TEMP_RevealWithoutLogic();
    }

    private void PlaceDragon(int x, int y)
    {
        UnoccupiedSpaces.RemoveUnoccupiedSpace(x, y);
        Tiles[x, y].TEMP_Place(ServiceLocator.Instance.EnemySpawner.GetRandomBoss());
        Tiles[x, y].TEMP_RevealWithoutLogic();
    }

    public bool InGridBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < SpawnSettings.Width && y < SpawnSettings.Height;
    }
    
    public void TEMP_RevealTilesInRadius(int x, int y, int radius)
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
                    Tiles[x + i, y + j].TEMP_RevealWithoutLogic();
                }
            }
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
                Tiles[x, y].TEMP_RevealWithoutLogic();
            }
        }
    }
    
    /// <summary>
    /// Reveals all tiles that have the matching object schema.
    /// </summary>
    public void TEMP_RevealAllOfType(TileObjectSchema objectSchema)
    {
        for (int y = 0; y < SpawnSettings.Height; y++)
        {
            for (int x = 0; x < SpawnSettings.Width; x++)
            {
                if (Tiles[x, y].GetHousedObject() == objectSchema)
                {
                    Tiles[x, y].TEMP_RevealWithoutLogic();
                }
            }
        }
    }
    
    /// <summary>
    /// Gets the total cost of all neighbors for a given coordinate.
    /// </summary>
    public int TEMP_GetTotalNeighborCost(int x, int y)
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
                
                cost += Tiles[x + i, y + j].TEMP_GetPublicCost();
            }
        }

        return cost;
    }

    /// <summary>
    /// Returns the object in the given coordinates. Can be null.
    /// </summary>
    public TileObjectSchema GetObject(int xCoordinate, int yCoordinate)
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

                // Marked or revealed mines become diffused
                if (tile.TEMP_IsRevealed() || tile.GetAnnotationText() == "*")
                {
                    tile.TEMP_Place(DiffusedMine);
                    tile.TEMP_SetState(Tile.TileState.Revealed);
                }
                else
                {
                    // The rest are just removed
                    tile.TEMP_Place(null);
                    tile.TEMP_SetState(Tile.TileState.Hidden);
                }
            }
        }
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
                Tiles[i + xCoordinate, j + yCoordinate].TEMP_Unobscure();
            }
        }
    }

    public bool TEMP_HandleFlee(TileObjectSchema housedObject)
    {
        if (!UnoccupiedSpaces.HasEmptySpace())
        {
            return false;
        }
        
        (int, int) newCoord = UnoccupiedSpaces.PeekUnoccupiedSpace();
        
        Tiles[newCoord.Item1, newCoord.Item2].TEMP_Place(housedObject);
        Tiles[newCoord.Item1, newCoord.Item2].TEMP_SetState(Tile.TileState.Hidden);
        
        UnoccupiedSpaces.RemoveUnoccupiedSpace(newCoord.Item1, newCoord.Item2);

        return true;
    }
    
    // I feel like this is some leetcode shit
    public (int, int) FindNearest(TileObjectSchema toFind, int xOrigin, int yOrigin)
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
}
