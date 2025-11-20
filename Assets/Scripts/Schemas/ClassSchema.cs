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
        [Tooltip("For the steam demo, some classes are impossible to unlock. Display this text instead.")]
        public string SteamDemoUnlockText;
        public ItemSchema.Id StartingItem;
        public GameObject SmallHitEffect;
        public GameObject BigHitEffect;

        // Some classes will be paid exclusive (for now?)
        [FormerlySerializedAs("SteamExclusive")] public bool PaidExclusive;

        public string GetUnlockText()
        {
            if (ServiceLocator.IsSteamDemo && !string.IsNullOrEmpty(SteamDemoUnlockText))
            {
                return SteamDemoUnlockText;
            }
            return UnlockText;
        }
    }
}