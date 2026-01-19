using AYellowpaper.SerializedCollections;
using Gameplay;
using Schemas;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.Inventory
{
    public class InventoryItem : MonoBehaviour
    {
        [SerializeField] private CanvasGroup CanvasGroup;
        [SerializeField] private Image Icon;
        [SerializeField] private Image RarityFrame;
        [SerializeField] private Button Button;
        [SerializeField] private GameObject Sale;
        [SerializeField] private TMP_Text StackCount;
        [SerializeField] [SerializedDictionary] private SerializedDictionary<Rarity, Color> RarityColors = new();
        
        private InventoryScreen Screen;
        private ItemInstance ItemInstance;

        private void Start()
        {
            Button.onClick.AddListener(OnButtonClicked);
            SetStackCount(1);
        }

        private void SetStackCount(int _)
        {
            StackCount.SetText(ItemInstance != null ? ItemInstance.StackCount.ToString() : "1");
        }

        private void OnButtonClicked()
        {
            if (Screen)
            {
                Screen.FocusItem(ItemInstance);
                ServiceLocator.Instance.AudioManager.PlaySfx("ClickGood");
            }
        }

        public void Initialize(InventoryScreen screen, ItemInstance itemInstance)
        {
            Screen = screen;
            ItemInstance = itemInstance;

            Icon.sprite = itemInstance.Schema.Sprite;
            SetSaleStatus(itemInstance.IsOnSale);

            itemInstance.IsOnSaleChanged += SetSaleStatus;
            itemInstance.StackCountChanged += SetStackCount;
            itemInstance.CurrentChargesChanged += CurrentChangesChanged;

            RarityFrame.color = GetColorFromRarity(ItemInstance.Schema.Rarity);
            
            StackCount.transform.parent.gameObject.SetActive(itemInstance.Schema.CanStack);
            CanvasGroup.alpha = 1.0f;
        }

        // Use this version for 'static' UIs. Things that dont need to track an actual instance, but rather just
        // display the item 
        public void Initialize(ItemSchema schema)
        {
            Icon.sprite = schema.Sprite;
            SetSaleStatus(false);
            RarityFrame.color = GetColorFromRarity(schema.Rarity);
            StackCount.transform.parent.gameObject.SetActive(false);
            CanvasGroup.alpha = 1.0f;
        }

        private void CurrentChangesChanged(int remainingCharges)
        {
            CanvasGroup.alpha = ItemInstance.CanBeUsed() ? 1.0f : 0.5f;
        }

        private Color GetColorFromRarity(Rarity rarity)
        {
            if (RarityColors.TryGetValue(rarity, out Color color))
            {
                return color;
            }

            return Color.white;
        }

        public ItemInstance GetItemInstance()
        {
            return ItemInstance;
        }
        
        private void OnDestroy()
        {
            if (ItemInstance != null)
            {
                ItemInstance.IsOnSaleChanged -= SetSaleStatus;
                ItemInstance.StackCountChanged -= SetStackCount;
            }
        }

        public void SetSaleStatus(bool isOnSale)
        {
            Sale.SetActive(isOnSale);
        }
    }
}