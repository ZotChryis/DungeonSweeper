using System;
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
    // TODO: Create a settings/grid scriptable object instead
    [SerializeField, Header("Grid Settings")]
    private int Width;
    
    [SerializeField, Header("Grid Settings")]
    private int Height;

    [SerializeField, Header("Grid Settings")]
    private Tile TilePrefab;

    [SerializeField, Header("Grid Settings")]
    private GridLayoutGroup Layout;

    [SerializeField, Header("Grid Settings")]
    private GridSpawnSettings Settings;
    
    private Tile[,] Tiles;

    // TODO: Probably should be a cleaner delegate and not owned by this class?
    public Action<Tile> OnTileStateChanged;
    public Action OnGridGenerated;
    
    private void Start()
    {
        ServiceLocator.Instance.Register(this);
        
        // TODO: Super temporary way to start the game
        GenerateGrid();
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
        Layout.constraintCount = Width;

        Tiles = new Tile[Width, Height];

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Tile tile = Instantiate(TilePrefab, transform);

                // For testing, update eventually
                tile.TEMP_SetCoordinates(x, y);
                tile.TEMP_Place(null);
                Tiles[x, y] = tile;
            }
        }
        
        // For testing, remove eventually
        // The center of the grid is the Dragon (13)
        Tiles[Width/2, Height/2].TEMP_Place(ServiceLocator.Instance.EnemySpawner.GetRandomBoss());
        // Make this spot the vision orb
        Tiles[3 * Width/4, 3 * Height/4].TEMP_Place(ServiceLocator.Instance.EnemySpawner.GetRandomStartingBoon());
        Tiles[Width/4, Height/4].TEMP_Place(ServiceLocator.Instance.EnemySpawner.GetRandomStartingBoon());
        
        // TODO: Make this better lmao maybe move to enemy spawner?
        for (var i = 0; i < Settings.GridSpawns.Length; i++)
        {
            var entry = Settings.GridSpawns[i];
            for (int j = 0; j < entry.Amount; j++)
            {
                bool placed = false;
                while (!placed)
                {
                    // Get a random point, lol this can be smarter
                    int xCoordinate = Random.Range(0, Width);
                    int yCoordinate = Random.Range(0, Height);

                    if (GetObject(xCoordinate, yCoordinate) != null)
                    {
                        continue;
                    }
                    
                    // TODO: This is not smart enough for overlapped requirements
                    bool allRequirementsMet = true;
                    foreach (var requirement in entry.Requirements)
                    {
                        if (!requirement.IsValid(xCoordinate, yCoordinate))
                        {
                            allRequirementsMet = false;
                            break;
                        }
                    }

                    if (!allRequirementsMet)
                    {
                        continue;
                    }
                    
                    Tiles[xCoordinate, yCoordinate].TEMP_Place(entry.Object);
                    foreach (var requirement in entry.Requirements)
                    {
                        foreach (var objectPair in requirement.ObjectRequirements)
                        {
                            Tiles[xCoordinate + objectPair.XCoordinateOffset, yCoordinate + objectPair.YCoordinateOffset].TEMP_Place(objectPair.ObjectSchema);
                        }
                    }

                    placed = true;
                }
            }
        }
        
        //  Reveal after everything is placed
        Tiles[Width/2, Height/2].TEMP_RevealWithoutLogic();
        Tiles[3 * Width / 4, 3 * Height / 4].TEMP_RevealWithoutLogic();
        Tiles[Width/4, Height/4].TEMP_RevealWithoutLogic();
    }

    public bool InGridBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
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
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Tiles[x, y].TEMP_RevealWithoutLogic();
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
                if (i == x && j == y)
                {
                    continue;
                }
                
                if (InGridBounds(x + i, y + j))
                {
                    cost += Tiles[x + i, y + j].TEMP_GetPublicCost();
                }
            }
        }

        return cost;
    }

    public TileObjectSchema GetObject(int xCoordinate, int yCoordinate)
    {
        return Tiles[xCoordinate, yCoordinate].GetHousedObject();
    }
}
