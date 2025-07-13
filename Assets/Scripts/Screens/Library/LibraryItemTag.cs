using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Schemas;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.Library
{
    public class LibraryItemTag : MonoBehaviour
    {
        [SerializeField] 
        private Image Icon;
        
        [SerializeField]
        [SerializedDictionary("Tile Tag", "Sprite")]
        private SerializedDictionary<TileSchema.Tag, Sprite> TagSprites = new SerializedDictionary<TileSchema.Tag, Sprite>();
        
        public void SetData(TileSchema.Tag tileObjectTag)
        {
            Icon.sprite = GetSpriteForTag(tileObjectTag);
            
            // TODO: Fix hack later
            //  When no sprite is found, lets just hide this whole thing
            if (Icon.sprite == null)
            {
                gameObject.SetActive(false);
            }
        }

        private Sprite GetSpriteForTag(TileSchema.Tag tileObjectTag)
        {
            return TagSprites.GetValueOrDefault(tileObjectTag);
        }
    }
}
