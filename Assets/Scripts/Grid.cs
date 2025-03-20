using UnityEngine;

/// <summary>
/// Quick and dirty implementation of an NxM Grid of Tiles for the gameplay.
/// Eventually this should be malleable enough to do different shapes with holes and corridors.
/// Probably using some sort of markup level file.
/// </summary>
public class Grid : SingletonMonoBehaviour<Grid>
{
    
    [SerializeField, Header("Grid Settings")]
    private int Width;
    
    [SerializeField, Header("Grid Settings")]
    private int Height;

    [SerializeField, Header("Grid Settings")]
    private float TileWidth;
    
    [SerializeField, Header("Grid Settings")]
    private float TileHeight;
    
    [SerializeField, Header("Grid Settings")]
    private Tile TilePrefab;

    private Tile[,] Tiles;

    private void Start()
    {
        ServiceLocator.Instance.Register(this);
    }
    
    /// <summary>
    /// Creates the Grid using the Serialized Properties marked with Grid Settings.
    /// The position of the GameObject that owns the script is used as the center of the Grid.
    /// [0,0] of the Grid is the bottom-left corner.
    /// </summary>
    public void GenerateGrid()
    {
        Tiles = new Tile[Width, Height];
        
        Vector3 origin = transform.position;
        float gridOffsetX = (Width - 1) * TileWidth * 0.5f;
        float gridOffsetY = (Height - 1) * TileHeight * 0.5f;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Vector3 position = new Vector3(x * TileWidth - gridOffsetX, y * TileHeight - gridOffsetY, 0f);
                Tile tile = Instantiate(TilePrefab, position + origin, Quaternion.identity, transform);

                // For testing, update eventually
                tile.TEMP_Place(ServiceLocator.Instance.Schemas.TEMP_GetNonDragon());
                
                Tiles[x, y] = tile;
            }
        }
        
        // For testing, remove eventually
        // The center of the grid is the Dragon (13)
        Tiles[Width/2, Height/2].TEMP_Place(ServiceLocator.Instance.Schemas.TEMP_GetDragon());
        Tiles[Width/2, Height/2].TEMP_SetState(Tile.TileState.Revealed);
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
                Tiles[x, y].TEMP_SetState(Tile.TileState.Revealed);
            }
        }
    }
}
