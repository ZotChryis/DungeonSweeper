using System;
using System.Collections.Generic;
using System.Linq;
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
        public TileObjectSchema Object;
        public int Amount;
    }
    
    protected override void Awake()
    {
        base.Awake();
        ServiceLocator.Instance.Grid.OnGridGenerated += OnGridGenerated;
    }

    private void OnDestroy()
    {
        ServiceLocator.Instance.Grid.OnGridGenerated -= OnGridGenerated;
    }

    private void OnGridGenerated()
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
