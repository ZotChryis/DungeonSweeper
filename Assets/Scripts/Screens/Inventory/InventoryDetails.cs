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
        [SerializeField] private Button Button;

        private Item Item;
        
        private void Start()
        {
            Button.onClick.AddListener(OnButtonClicked);
            Button.interactable = false;
        }

        private void OnButtonClicked()
        {
            ServiceLocator.Instance.Player.Inventory.UseItem(Item);
            Button.interactable = Item.CanBeUsed();
        }

        public void SetItem(Item item)
        {
            Item = item;
            
            Name.SetText(item.Schema.Name);
            Description.SetText(item.Schema.Description);

            Button.interactable = item.CanBeUsed();
        }
    }
}