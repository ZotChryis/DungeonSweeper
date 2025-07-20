using System.Collections.Generic;
using System.Linq;
using Gameplay;
using Screens.Inventory;
using UnityEngine;

public class InventoryScreen : BaseScreen
{
    [SerializeField] 
    protected InventoryItem ItemPrefab;

    [SerializeField] protected GameObject ConsumableLabel;
    [SerializeField] protected GameObject PassiveLabel;
    
    [SerializeField] 
    protected Transform ConsumableListRoot;
    [SerializeField]
    protected Transform PassiveListRoot;

    [SerializeField] 
    protected InventoryDetails Details;
    
    protected Inventory Inventory;
    protected List<InventoryItem> Items;
    protected ItemInstance FocusedItem;
    
    protected override void Awake()
    {
        base.Awake();
        
        Items = new List<InventoryItem>();
        
        SetupInventory();
        
        // Bind to the inventory change events 
        Inventory.OnItemAdded += OnItemAdded;
        Inventory.OnItemChargeChanged += OnItemChargeChanged;
        Inventory.OnItemRemoved += OnItemRemoved;
        
        // Handle any that are already there
        RefreshItems();
    }

    // TODO: Too lazy right now to remove items one at a time. need to track them better 
    private void RefreshItems()
    {
        ConsumableLabel.SetActive(false);
        PassiveLabel.SetActive(false);
        
        foreach (var inventoryItem in Items)
        {
            Destroy(inventoryItem.gameObject);
        }
        Items.Clear();
        
        foreach (var item in Inventory.GetAllItems())
        {
            OnItemAdded(item);
        }
    }

    protected virtual void SetupInventory()
    {
        Inventory = ServiceLocator.Instance.Player.Inventory;
    }

    private void OnItemAdded(ItemInstance itemInstance)
    {
        InventoryItem newItem = Instantiate<InventoryItem>(ItemPrefab, itemInstance.Schema.IsConsumbale ? ConsumableListRoot : PassiveListRoot);
        newItem.Initialize(this, itemInstance);
        Items.Add(newItem);

        bool hasAtLeastOneConsumable = Inventory.GetAllItems().Any(i => i.Schema.IsConsumbale);
        bool hasAtLeastOnePassive = Inventory.GetAllItems().Any(i => !i.Schema.IsConsumbale);
        ConsumableLabel.SetActive(hasAtLeastOneConsumable);
        PassiveLabel.SetActive(hasAtLeastOnePassive);
    }

    protected void OnItemChargeChanged(ItemInstance itemInstance)
    {
        FocusItem(itemInstance);
    }
    
    private void OnItemRemoved(ItemInstance itemInstance)
    {
        if (FocusedItem == itemInstance)
        {
            ClearFocusedItem();
        }

        RefreshItems();
    }

    public void FocusItem(ItemInstance itemInstance)
    {
        Details.SetItem(itemInstance);
        FocusedItem = itemInstance;
    }

    public void ClearFocusedItem()
    {
        FocusedItem = null;
        Details.ClearFocusedItem();
    }
}