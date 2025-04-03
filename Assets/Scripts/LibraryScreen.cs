using UnityEngine;

public class LibraryScreen : Screen
{
    [SerializeField] 
    private LibraryItem LibraryItemPrefab;

    [SerializeField] 
    private Transform LibraryListRoot;

    private LibraryItem[] Items;
    
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
        if (Items != null && Items.Length > 0)
        {
            foreach (var libraryItem in Items)
            {
                Destroy(libraryItem.gameObject);
            }
        }

        var gridSpawns = ServiceLocator.Instance.Grid.SpawnSettings.GridSpawns;
        Items = new  LibraryItem[gridSpawns.Length];
        for (int i = 0; i < gridSpawns.Length; i++)
        {
            LibraryItem libraryItem = Instantiate(LibraryItemPrefab, LibraryListRoot);
            libraryItem.SetGridSpawn(gridSpawns[i]);
            Items[i] = libraryItem;
        }
    }
}
