using Gameplay;
using Schemas;
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
        [SerializeField] private TMP_Text ItemRarity;

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

            Button.GetComponentInChildren<TMP_Text>().SetText(itemInstance.CanBeUsed() ? "Use" : "---");

            Charges.enabled = itemInstance.Schema.IsConsumbale;
            Charges.color = itemInstance.CanBeUsed() ? Color.white : Color.red;

            Charges.SetText($"{itemInstance.CurrentCharges}/{itemInstance.MaxCharges}");

            ItemRarity.color = itemInstance.Schema.Rarity.GetRarityColor();
            ItemRarity.SetText(itemInstance.Schema.Rarity.ToString());
        }

        public virtual void ClearFocusedItem()
        {
            Name.SetText("---");
            Description.SetText("---");
            Button.GetComponentInChildren<TMP_Text>().SetText("---");
            Button.interactable = false;
            ItemRarity.SetText("---");
            ItemRarity.color = Color.white;
        }
    }
}