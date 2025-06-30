using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Schemas
{
    /// <summary>
    /// Representation of an Enemy.
    /// Intended to be housed within a Tile.
    /// </summary>
    // TODO: Separate into data and logic, aka TileObjectSchema class? See what Im doing with Class/Item
    // TODO: Tile vs TileObject should have their own states
    //      Tile should not be concerned with what the TileObject does beyond "I am dead now" or w/e
    //      This way we can divorce the tile's logic from the object's logic, and let the objects do whatever they want
    //      We should then have SingleHitObject : TileObject (most enemies), MultiHitObject : TileObject (blocks) ? idk
    [CreateAssetMenu(menuName = "Data/TileObject")]
    public class TileSchema : Schema
    {
        private void OnValidate()
        {
            if (TileId == Id.None || TileId == Id.Global)
            {
                Debug.LogError($"{nameof(TileSchema)}.{UserFacingName} requires a valid tile ID");
            }
        }

        // !!WARNING!! DO NOT REORDER
        [Serializable]
        public enum Id
        {
            // TODO: Ideally we get rid of this and use TileObjectSchema.Tags
            // Special cases for item logic purposes (match any id)
            None,
            Global,
        
            Egg,
            Pizza,
            TarotCard,
            VisionOrb,
        
            Brick,
            Chest,
            ChestHeal,
        
            ScrollHeal,
            ScrollVision,
            ScrollRat,
            ScrollSlime,
            ScrollMine,
            
            TreasureMap,
        
            Rat,
            Bat,
            Skeleton,
            GargoyleL,
            GargoyleR,
            PiedPiper,
            Beholder,
            Minotaur,
        
            DemonKnight,
        
            SlimeGreen,
            SlimePurple,
            SlimeWizard,
            
            LoverMale,
            LoverFemale,
            Gnome,
            
            Lich,
            Mimic,
        
            Dragon,
            Mine,
            
            Faerie,
            Gorgon,
            Balrog,
            
            Crown,
            MineDiffused,
            
            ScrollHealChest,
            ScrollVisionLevel2,
            VisionOrbLevel2,
            
            BrickFinal,
            
        }
        
        // !!WARNING!! DO NOT REORDER
        [Serializable]
        public enum Tag
        {
            // Used mostly for items/cards/bricks,
            Neutral,
        
            // Used for things that deal damage to you
            Enemy,
        
            // Role
            Melee,
            Ranged,
            Flying,
        
            // Families
            Beast,
        
        }
    
        [Serializable]
        public struct SpriteFacing
        {
            // Object to look for. We find the nearest one. There should be only one.
            public TileSchema ObjectToLookAtOverride;

            // The direction to look. Only used if not null.
            public Sprite Above;
            public Sprite Below;

            /// <summary>
            /// Facing right is special. We default flip this sprite horizontally in case you don't set it.
            /// </summary>
            public Sprite Right;
            // Above/Below and Right/Left are exclusive. You cant use both
            public Sprite Left;

            // If no nearest object type is found.
            public Sprite Missing;
        }

        public string UserFacingName;
        public string UserFacingDescription;

        public List<Tag> Tags = new List<Tag>();
    
        // TODO: Make this an enum and add to all data objects created
        public Id TileId;
        public Sprite Sprite;
        public SpriteFacing SpriteFacingData;

        public int Power;
        public bool HidePowerToNeighbors;
        public bool PreventConsumeIfKillingBlow;
        public int XPReward;

        // When fully collected, instead of becoming Empty, this object will be the new housed object on the tile.
        public TileSchema DropReward;

        // TODO: Refactor all this shit honestly
        public bool RevealRandLocationNextToMine;
        public Vector2Int[] RevealOffsets;
        public int RevealRadius;
        public Vector2Int[] ObscureOffsets;
        public int ObscureRadius;

        // Hack: basically Gnome behavior
        public bool CanFlee;
        public bool CanEnrage;
        public bool SpawnsFleeingChild;
        public TileSchema FleeingChild;

        public TileSchema[] RevealAllRewards;
        public bool WinReward;
        public bool FullHealReward;
        public int HealReward = 0;
        public bool DiffuseMinesReward;
        public int ShopXp = 0;
        public bool ScreenshakeOnConquer = false;

        public TileSchema UpgradedVersion;

        [Serializable]
        public struct ValueOverride<T>
        {
            public bool UseOverride;
            public T Value;
        }

        [Serializable]
        public struct TileStateData
        {
            // Wether or not to override the standard behavior of tile state flow
            public ValueOverride<bool> AutoContinue;

            // Allow to change the sprite for this specific state
            public ValueOverride<Sprite> Sprite;

            // Whether or not to display  certain visual elements
            public ValueOverride<bool> EnablePower;
            public ValueOverride<bool> EnableSprite;
            public ValueOverride<bool> EnableDeathSprite;
        }

        [SerializedDictionary("Tile State", "State Overrides")]
        public SerializedDictionary<Tile.TileState, TileStateData> Data;

        public TileStateData GetOverrides(Tile.TileState state)
        {
            return Data.GetValueOrDefault(state);
        }

        // TODO HACK fix later
        public Sprite TEMP_GetSprite(bool shouldStandUp, CompassDirections directionToLook)
        {
            if (shouldStandUp)
            {
                return SpriteFacingData.Missing;
            }

            switch (directionToLook)
            {
                case CompassDirections.North:
                    if (SpriteFacingData.Above != null)
                    {
                        return SpriteFacingData.Above;
                    }
                    return Sprite;
                case CompassDirections.South:
                    if (SpriteFacingData.Below != null)
                    {
                        return SpriteFacingData.Below;
                    }
                    return Sprite;
                case CompassDirections.East:
                    if (SpriteFacingData.Right != null)
                    {
                        return SpriteFacingData.Right;
                    }
                    return Sprite;
                case CompassDirections.West:
                    if (SpriteFacingData.Left != null)
                    {
                        return SpriteFacingData.Left;
                    }
                    return Sprite;
                default:
                    return Sprite;
            }
        }
    }
}
