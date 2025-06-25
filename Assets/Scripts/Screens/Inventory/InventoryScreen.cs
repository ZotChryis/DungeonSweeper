using System.Collections.Generic;
using Gameplay;
using Screens.Inventory;
using UnityEngine;

public class InventoryScreen : BaseScreen
{
    [SerializeField] 
    private InventoryItem ItemPrefab;

    [SerializeField] 
    private Transform ItemListRoot;

    [SerializeField] 
    private InventoryDetails Details;
    
    private List<InventoryItem> Items;

    private void Start()
    {
        Items = new List<InventoryItem>();
            
        // Bind to the inventory events
        var inventory = ServiceLocator.Instance.Player.Inventory;
        inventory.OnItemAdded += OnItemAdded;
        inventory.OnItemChargeChanged += OnItemChargeChanged;
        
        // Handle any that are already there
        foreach (var item in inventory.GetAllItems())
        {
            OnItemAdded(item);
        }
    }

    private void OnItemAdded(Item item)
    {
        InventoryItem newItem = Instantiate<InventoryItem>(ItemPrefab, ItemListRoot);
        newItem.Initialize(this, item);
        Items.Add(newItem);
    }

    private void OnItemChargeChanged(Item item)
    {
        FocusItem(item);
    }

    public void FocusItem(Item item)
    {
        Details.SetItem(item);
    }
}