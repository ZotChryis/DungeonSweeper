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

        protected Item Item;
        
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
            ServiceLocator.Instance.Player.Inventory.UseItem(Item);
            Button.interactable = Item.CanBeUsed();
        }

        public virtual void SetItem(Item item)
        {
            Item = item;
            
            Name.SetText(item.Schema.Name);
            Description.SetText(item.Schema.Description);

            Button.interactable = item.CanBeUsed();
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