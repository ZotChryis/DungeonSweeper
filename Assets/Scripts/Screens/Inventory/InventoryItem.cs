using Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.Inventory
{
    public class InventoryItem : MonoBehaviour
    {
        [SerializeField] private Image Icon;
        [SerializeField] private Button Button;
        [SerializeField] private GameObject Sale;
        
        private InventoryScreen Screen;
        private ItemInstance ItemInstance;

        private void Start()
        {
            Button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            Screen.FocusItem(ItemInstance);
        }

        public void Initialize(InventoryScreen screen, ItemInstance itemInstance)
        {
            Screen = screen;
            ItemInstance = itemInstance;

            Icon.sprite = itemInstance.Schema.Sprite;
            SetSaleStatus(itemInstance.IsOnSale);

            itemInstance.IsOnSaleChanged += SetSaleStatus;
        }

        public ItemInstance GetItemInstance()
        {
            return ItemInstance;
        }
        
        private void OnDestroy()
        {
            ItemInstance.IsOnSaleChanged -= SetSaleStatus;
        }

        public void SetSaleStatus(bool isOnSale)
        {
            Sale.SetActive(isOnSale);
        }
    }
}