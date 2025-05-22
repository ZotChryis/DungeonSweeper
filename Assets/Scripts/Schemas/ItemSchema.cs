using Gameplay;
using UnityEngine;

namespace Schemas
{
    [CreateAssetMenu(menuName = "Data/Item")]
    public class ItemSchema : Schema
    {
        public Item.Id Id;
        public string Name;
        public string Description;
        public Sprite Sprite;
    }
}