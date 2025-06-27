using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.Inventory
{
    public class InventoryDetails : MonoBehaviour
    {
        [SerializeField] private TMP_Text Name;
        [SerializeField] private TMP_Text Description;
        [SerializeField] protected Button Button;
        [SerializeField] private TMP_Text Charges;

        protected ItemInstance ItemInstance;
        
        private void Start()
        {
            Button.onClick.AddListener(OnButtonClicked);
            Button.interactable = false;
        }

        private void OnButtonClicked()
        {
            HandleButtonClicked();
        }

        protected virtual void HandleButtonClicked()
        {
            ServiceLocator.Instance.Player.Inventory.UseItem(ItemInstance);
            Button.interactable = ItemInstance.CanBeUsed();
        }

        public virtual void SetItem(ItemInstance itemInstance)
        {
            ItemInstance = itemInstance;
            
            Name.SetText(itemInstance.Schema.Name);
            Description.SetText(itemInstance.Schema.Description);

            Button.interactable = itemInstance.CanBeUsed();
            
            Charges.enabled = itemInstance.Schema.IsConsumbale;
            Charges.color = itemInstance.CanBeUsed() ? Color.white : Color.red;
            Charges.SetText($"{itemInstance.CurrentQuantity}/{itemInstance.MaxQuantity}");
        }

        public void ClearFocusedItem()
        {
            Name.SetText("CHOOSE AN ITEM");
            Description.SetText("");
            Button.GetComponentInChildren<TMP_Text>().SetText("---");
            Button.interactable = false;
        }
    }
}