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
            StackCount.SetText(ItemInstance.StackCount.ToString());
        }

        private void OnButtonClicked()
        {
            Screen.FocusItem(ItemInstance);
            ServiceLocator.Instance.AudioManager.PlaySfx("ClickGood");
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

            RarityFrame.color = GetColorFromRarity();
            
            StackCount.transform.parent.gameObject.SetActive(itemInstance.Schema.CanStack);
            CanvasGroup.alpha = 1.0f;
        }

        private void CurrentChangesChanged(int remainingCharges)
        {
            CanvasGroup.alpha = ItemInstance.CanBeUsed() ? 1.0f : 0.5f;
        }

        private Color GetColorFromRarity()
        {
            if (RarityColors.TryGetValue(ItemInstance.Schema.Rarity, out Color color))
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
            ItemInstance.IsOnSaleChanged -= SetSaleStatus;
            ItemInstance.StackCountChanged -= SetStackCount;
        }

        public void SetSaleStatus(bool isOnSale)
        {
            Sale.SetActive(isOnSale);
        }
    }
}