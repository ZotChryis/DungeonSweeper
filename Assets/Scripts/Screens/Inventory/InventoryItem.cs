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
        [SerializeField] private Image Icon;
        [SerializeField] private Image ConsumableEmpty;
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
            ConsumableEmpty.gameObject.SetActive(false);
        }

        private void CurrentChangesChanged(int remainingCharges)
        {
            ConsumableEmpty.gameObject.SetActive(!ItemInstance.CanBeUsed());
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