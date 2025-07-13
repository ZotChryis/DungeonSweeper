using System;
using System.Collections.Generic;
using System.Linq;
using Schemas;
using UnityEngine;

public class LibraryScreen : BaseScreen
{
    [SerializeField] 
    private LibraryItem LibraryItemPrefab;

    [SerializeField] 
    private Transform LibraryListRoot;

    private List<LibraryItem> Items;

    private struct LibraryItemDTO
    {
        public TileSchema Object;
        public int Amount;
    }
    
    protected override void Awake()
    {
        base.Awake();
        ServiceLocator.Instance.Grid.OnGridGenerated += Refresh;
        ServiceLocator.Instance.Grid.OnTileStateChanged += OnTileStateChanged;
    }

    private void OnDestroy()
    {
        ServiceLocator.Instance.Grid.OnGridGenerated -= Refresh;
        ServiceLocator.Instance.Grid.OnTileStateChanged -= OnTileStateChanged;
    }

    private void OnTileStateChanged(Tile tile)
    {
        Refresh();
    }
    
    private void Refresh()
    {
        // TODO: Hack -- we should keep a running list instead of destroying and recreating so much
        if (Items != null && Items.Count > 0)
        {
            foreach (var libraryItem in Items)
            {
                Destroy(libraryItem.gameObject);
            }
            Items.Clear();
        }

        Dictionary<TileSchema.Id, int> filteredItems = new Dictionary<TileSchema.Id, int>();
        var tileObjects = ServiceLocator.Instance.Grid.GetAllTileObjects();
        foreach (var tileObject in tileObjects)
        {
            TileSchema.Id tileIdToUse = tileObject.LibraryOverrideTileId != TileSchema.Id.None ? tileObject.LibraryOverrideTileId : tileObject.TileId;
            filteredItems.TryAdd(tileIdToUse, 0);
            filteredItems[tileIdToUse] += 1;
        }
        
        List<LibraryItemDTO> libraryItemDTOs = new List<LibraryItemDTO>(filteredItems.Keys.Count);
        foreach (var filteredItemsKey in filteredItems.Keys)
        {
            libraryItemDTOs.Add(new LibraryItemDTO()
            {
                Amount = filteredItems[filteredItemsKey],
                Object = ServiceLocator.Instance.Schemas.TileObjectSchemas.Find(x => x.TileId == filteredItemsKey)
            });
        }
        
        // Sort the items
        libraryItemDTOs.Sort((x, y) =>
        {
            int compare = x.Object.Power.CompareTo(y.Object.Power);
            if (compare == 0)
            {
                compare = String.Compare(x.Object.UserFacingName, y.Object.UserFacingName, StringComparison.Ordinal);
            }
            return compare;
        });
        
        // Make items
        Items = new  List<LibraryItem>(libraryItemDTOs.Count);
        foreach (var itemDTO in libraryItemDTOs)
        {
            LibraryItem libraryItem = Instantiate(LibraryItemPrefab, LibraryListRoot);
            libraryItem.SetData(itemDTO.Object, itemDTO.Amount);
            Items.Add(libraryItem);
        }
    }

    private void OnGridGenerated_OLD()
    {
        if (Items != null && Items.Count > 0)
        {
            foreach (var libraryItem in Items)
            {
                Destroy(libraryItem.gameObject);
            }
        }

        // TODO: Should we just grab the ACTUAL spawns left in the map? There would be logic about hiding/showing stuff
        
        List<SpawnSettings.GridSpawnEntry> gridSpawns = ServiceLocator.Instance.Grid.SpawnSettings.GridSpawns.ToList();
        gridSpawns.AddRange(ServiceLocator.Instance.Grid.SpawnSettings.NormalSpawns.ToList());
        Items = new  List<LibraryItem>(gridSpawns.Count);

        // Extract the items and collapse in a sortable fashion
        List<LibraryItemDTO> libraryItemDTOs = new List<LibraryItemDTO>(gridSpawns.Count);
        for (int i = 0; i < gridSpawns.Count; i++)
        {
            SpawnSettings.GridSpawnEntry spawn = gridSpawns[i];
            
            libraryItemDTOs.Add(new LibraryItemDTO()
            {
                Object = spawn.Object,
                Amount = spawn.Amount + spawn.Amount * (spawn.ConsecutiveStackedInLibrary ? spawn.ConsecutiveCopies : 0)
            });

            // TODO: Should we show drops?
            /*
            if (spawn.Object.DropReward)
            {
                libraryItemDTOs.Add(new LibraryItemDTO()
                {
                    Object = spawn.Object.DropReward,
                    Amount = spawn.Amount + (spawn.ConsecutiveStackedInLibrary ? spawn.ConsecutiveCopies : 0)
                });
            }
            */

            if (spawn.ConsecutiveSpawn && !spawn.ConsecutiveStackedInLibrary)
            {
                libraryItemDTOs.Add(new LibraryItemDTO()
                {
                    Object = spawn.ConsecutiveSpawn,
                    Amount = spawn.ConsecutiveCopies
                });
                
                // TODO: Should we show drops?
                /*
                if (spawn.ConsecutiveSpawn.DropReward)
                {
                    libraryItemDTOs.Add(new LibraryItemDTO()
                    {
                        Object = spawn.ConsecutiveSpawn.DropReward,
                        Amount = spawn.ConsecutiveCopies
                    });
                }
                */
            }
        }
        
        // Sort the items
        libraryItemDTOs.Sort((x, y) =>
        {
            int compare = x.Object.Power.CompareTo(y.Object.Power);
            if (compare == 0)
            {
                compare = String.Compare(x.Object.UserFacingName, y.Object.UserFacingName, StringComparison.Ordinal);
            }
            return compare;
        });
        
        // Make items
        foreach (var itemDTO in libraryItemDTOs)
        {
            LibraryItem libraryItem = Instantiate(LibraryItemPrefab, LibraryListRoot);
            libraryItem.SetData(itemDTO.Object, itemDTO.Amount);
            Items.Add(libraryItem);
        }
    }
}
