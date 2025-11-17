using Gameplay;
using UnityEngine;
using UnityEngine.Serialization;

namespace Schemas
{
    [CreateAssetMenu(menuName = "Data/Class")]
    public class ClassSchema : Schema
    {
        public Class.Id Id;
        public Sprite Sprite;
        public string Name;
        public string Description;
        public string UnlockText;
        public ItemSchema.Id StartingItem;
        public GameObject SmallHitEffect;
        public GameObject BigHitEffect;

        // Some classes will be paid exclusive (for now?)
        [FormerlySerializedAs("SteamExclusive")] public bool PaidExclusive;
    }
}