using Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.Inventory
{
    public class InventoryItem : MonoBehaviour
    {
        [SerializeField] private Image Icon;
        [SerializeField] private Button Button;

        private InventoryScreen Screen;
        private Item Item;

        private void Start()
        {
            Button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            Screen.FocusItem(Item);
        }

        public void Initialize(InventoryScreen screen, Item item)
        {
            Screen = screen;
            Item = item;

            Icon.sprite = item.Schema.Sprite;
        }
    }
}