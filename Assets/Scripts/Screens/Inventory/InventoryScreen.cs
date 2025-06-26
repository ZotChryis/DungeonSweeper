using System.Collections.Generic;
using Gameplay;
using Screens.Inventory;
using UnityEngine;

public class InventoryScreen : BaseScreen
{
    [SerializeField] 
    protected InventoryItem ItemPrefab;

    [SerializeField] 
    protected Transform ItemListRoot;

    [SerializeField] 
    protected InventoryDetails Details;
    
    protected Inventory Inventory;
    protected List<InventoryItem> Items;

    private void Start()
    {
        Items = new List<InventoryItem>();
        
        SetupInventory();
        
        // Bind to the inventory change events 
        Inventory.OnItemAdded += OnItemAdded;
        Inventory.OnItemChargeChanged += OnItemChargeChanged;
        
        // Handle any that are already there
        foreach (var item in Inventory.GetAllItems())
        {
            OnItemAdded(item);
        }
    }

    protected virtual void SetupInventory()
    {
        Inventory = ServiceLocator.Instance.Player.Inventory;
    }

    private void OnItemAdded(Item item)
    {
        InventoryItem newItem = Instantiate<InventoryItem>(ItemPrefab, ItemListRoot);
        newItem.Initialize(this, item);
        Items.Add(newItem);
    }

    protected void OnItemChargeChanged(Item item)
    {
        FocusItem(item);
    }

    public void FocusItem(Item item)
    {
        Details.SetItem(item);
    }

    public void ClearFocusedItem()
    {
        Details.ClearFocusedItem();
    }
}