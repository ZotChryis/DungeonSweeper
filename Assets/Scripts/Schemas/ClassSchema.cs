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
        [Tooltip("For non paid versions (WebGL) display this text instead.")]
        public string FreeVersionUnlockText;
        public ItemSchema.Id StartingItem;
        public GameObject SmallHitEffect;
        public GameObject BigHitEffect;

        // Some classes will be paid exclusive (for now?)
        [FormerlySerializedAs("SteamExclusive")] public bool PaidExclusive;

        public string GetUnlockText()
        {
            // Paid version always returns the normal unlock text
            if (ServiceLocator.Instance.IsPaidVersion())
            {
                return UnlockText;
            }

            // Free versions return the free version text if it exists, otherwise the standard one when this class is paid exclusive
            if (PaidExclusive)
            {
                return !string.IsNullOrEmpty(FreeVersionUnlockText) ? FreeVersionUnlockText : UnlockText;
            }
            
            // Default to the standard text
            return UnlockText;
        }
    }
}