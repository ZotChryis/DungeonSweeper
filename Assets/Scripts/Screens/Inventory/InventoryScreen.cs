using System.Collections.Generic;
using Gameplay;
using Screens.Inventory;
using UnityEngine;

public class InventoryScreen : BaseScreen
{
    [SerializeField] 
    protected InventoryItem ItemPrefab;
    
    [SerializeField] 
    protected Transform ConsumableLabel;
    [SerializeField] 
    protected Transform PassiveLabel;
    
    [SerializeField] 
    protected Transform AllInventoryContent;
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

    protected virtual void OnDestroy()
    {
        Inventory.OnItemAdded -= OnItemAdded;
        Inventory.OnItemChargeChanged -= OnItemChargeChanged;
        Inventory.OnItemRemoved -= OnItemRemoved;
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
        
        foreach (var inventoryItem in Items)
        {
            Destroy(inventoryItem.gameObject);
        }
        Items.Clear();
        
        foreach (var item in Inventory.GetAllItems())
        {
            OnItemAdded(item, false);
        }
        
        ForceRefreshLayout();
    }

    protected virtual void SetupInventory()
    {
        Inventory = ServiceLocator.Instance.Player.Inventory;
    }

    private void OnItemAdded(ItemInstance item)
    {
        OnItemAdded(item, true);
    }
    
    private void OnItemAdded(ItemInstance itemInstance, bool forceLayoutRefresh)
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

        if (forceLayoutRefresh)
        {
            ForceRefreshLayout();
        }
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

    /// <summary>
    /// We needed to homebrew this positioning because originally it was created as:
    ///     VerticalLayoutGroup (ContentSizeFitter)
    ///         Header Label
    ///         GridLayoutGroup (ContentSizeFitter)
    ///         Header Label
    ///         GridLayoutGroup (ContentSizeFitter)
    ///  However, the VerticalLayoutGroup + GridLayoutGroups do NOT play well with each other in mobile.
    ///  So, we now mimic the VerticalLayoutGroup's job here.
    ///  Additionally, the (ContentSizeFitter) were also eliminated to help with performance on this screen.
    ///  Really a shamne....
    /// </summary>
    protected void ForceRefreshLayout()
    {
        bool hasAtLeastOneConsumable = ConsumableItems > 0;
        bool hasAtLeastOnePassive = PassiveItems > 0;
        
        ConsumableLabel.gameObject.SetActive(hasAtLeastOneConsumable);
        PassiveLabel.gameObject.SetActive(hasAtLeastOnePassive);
        
        RectTransform consumableLabelRect = (RectTransform)ConsumableLabel;
        RectTransform passiveLabelRect = (RectTransform)PassiveLabel;
        RectTransform allInventoryRect = (RectTransform)AllInventoryContent;
        RectTransform consumableListRect = (RectTransform)ConsumableListRoot;
        RectTransform passiveListRect = (RectTransform)PassiveListRoot;

        int currentDeltaY = 0;
        consumableLabelRect.anchoredPosition = new Vector2(0, currentDeltaY);
        if (hasAtLeastOneConsumable)
        {
            currentDeltaY -= 75;
        }
        
        int consumableRows = (ConsumableItems + 3) / 4;
        Vector2 newConsumableSizeDelta = consumableListRect.sizeDelta;
        newConsumableSizeDelta.y = consumableRows * 175;
        consumableListRect.sizeDelta = newConsumableSizeDelta;
        consumableListRect.anchoredPosition = new Vector2(0, currentDeltaY);
        currentDeltaY -= consumableRows * 175;
        
        passiveLabelRect.anchoredPosition = new Vector2(0, currentDeltaY);
        if (hasAtLeastOnePassive)
        {
            currentDeltaY -= 75;
        }
        
        int  passiveRows = (PassiveItems + 3) / 4;
        Vector2 newPassiveSizeDelta = passiveListRect.sizeDelta;
        newPassiveSizeDelta.y = passiveRows * 175;
        passiveListRect.sizeDelta = newPassiveSizeDelta;
        passiveListRect.anchoredPosition = new Vector2(0, currentDeltaY);
        currentDeltaY -= consumableRows * 175;

        Vector2 newOverallSizeDelta = allInventoryRect.sizeDelta;
        newOverallSizeDelta.y = newConsumableSizeDelta.y + newPassiveSizeDelta.y;
        newOverallSizeDelta.y += hasAtLeastOneConsumable ? 75 : 0;
        newOverallSizeDelta.y += hasAtLeastOnePassive ? 75 : 0;
        allInventoryRect.sizeDelta = newOverallSizeDelta;
    }
}