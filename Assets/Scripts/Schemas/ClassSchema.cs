using Gameplay;
using UnityEngine;

namespace Schemas
{
    [CreateAssetMenu(menuName = "Data/Class")]
    public class ClassSchema : Schema
    {
        public Class.Id Id;
        public Sprite Sprite;
        public string Name;
        public string Description;
        public ItemSchema.Id StartingItem;
        public GameObject SmallHitEffect;
        public GameObject BigHitEffect;
    }
}