using System.Collections.Generic;
using System.Linq;
using Gameplay;
using Screens.Inventory;
using UnityEngine;
using UnityEngine.UI;

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

    protected int PassiveItems = 0;
    protected int ConsumableItems = 0;
    
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

    protected override void OnShow()
    {
        base.OnShow();
        
        ForceRefreshLayout();
    }

    // TODO: Too lazy right now to remove items one at a time. need to track them better 
    private void RefreshItems()
    {
        PassiveItems = 0;
        ConsumableItems = 0;
        
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
        bool isConsumable = itemInstance.Schema.IsConsumbale;
        if (isConsumable)
        {
            ConsumableItems++;
        }
        else
        {
            PassiveItems++;
        }
        
        InventoryItem newItem = Instantiate<InventoryItem>(ItemPrefab, isConsumable ? ConsumableListRoot : PassiveListRoot);
        newItem.Initialize(this, itemInstance);
        Items.Add(newItem);
        
        bool hasAtLeastOneConsumable = Inventory.GetAllItems().Any(i => i.Schema.IsConsumbale);
        bool hasAtLeastOnePassive = Inventory.GetAllItems().Any(i => !i.Schema.IsConsumbale);
        ConsumableLabel.SetActive(hasAtLeastOneConsumable);
        PassiveLabel.SetActive(hasAtLeastOnePassive);

        ForceRefreshLayout();
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

        for (var i = 0; i < Items.Count; i++)
        {
            if (Items[i].GetItemInstance() == itemInstance)
            {
                bool isConsumable = itemInstance.Schema.IsConsumbale;
                if (isConsumable)
                {
                    ConsumableItems--;
                }
                else
                {
                    PassiveItems--;
                }
                
                Destroy(Items[i].gameObject);
                ForceRefreshLayout();
                return;
            }
        }
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

    protected void ForceRefreshLayout()
    {
        //LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)ConsumableListRoot);
        //LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)PassiveListRoot);

        RectTransform consumableListRect = (RectTransform)ConsumableListRoot;
        RectTransform passiveListRect = (RectTransform)PassiveListRoot;

        int consumableRows = (ConsumableItems + 3) / 4;
        Vector2 newConsumableSizeDelta = consumableListRect.sizeDelta;
        newConsumableSizeDelta.y = consumableRows * 175;
        consumableListRect.sizeDelta = newConsumableSizeDelta;
        
        int  passiveRows = (PassiveItems + 3) / 4;
        Vector2 newPassiveSizeDelta = passiveListRect.sizeDelta;
        newPassiveSizeDelta.y = passiveRows * 175;
        passiveListRect.sizeDelta = newPassiveSizeDelta;
    }
}