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
            PizzaSlice,
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
            SlimeWizardPurple,

            LoverMale,
            LoverFemale,
            Gnome,

            Lich,
            Mimic,

            Dragon0,
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

            MagicFountain,
            ChestItemCommon,
            ChestItemUncommon,
            ChestItemRare,
            ChestItemEpic,
            ChestItemLegendary,

            Ouroboros,
            SlimeBlue,
            SlimeWizardGreen,
            SlimeWizardBlue,

            Firelord,
            Firefly,
            FireflyCalm,

            CyclopsL,
            CyclopsR,

            Dragon1,
            Dragon2,
            Dragon3,
            Dragon4,

            Sheep,

            SnakeCharmer,
            Snake,
            SnakeCalm,

            Pizza,
            ForestSpirit,
            WerewolfBeast,
            WerewolfHuman,
            BloodMoon,

            Jinn,
            JinnLamp,

            LesserGolem,
            GreaterGolem,
            Vampire,
            Jello,
            Lich1,
            Lich2,
            Lich3,
            Lich4,
            Lich5,
            Chest_Generic, // Unified library description for all chests.
            TutorialSlime, // Slime you defeat during the tutorial.
            GoldBug,
            
            DemonKnightBrotherBlue,
            DemonKnightBrotherRed,
            
            MetalBlock,
            Bee,
            Beehive,
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

            // Families/Groupings
            Beast,
            Demon,
            Slime,
            Elemental,
            Humanoid,
            Lover,
            Chest,
            Scroll,
            Dragon,
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

            /// <summary>
            /// If your BodyGuard or tile you are Guarding are dead
            /// </summary>
            public Sprite BodyGuardedByOrGuardingDead;
        }

        public string UserFacingName;
        public string UserFacingDescription;

        public List<Tag> Tags = new List<Tag>();

        public Id TileId;

        // If this is not None, when this tile object is on the map, it will appear as this object in the library
        // This is necessary for some oddities like Bricks and GargoyleL/R, etc.
        public Id LibraryOverrideTileId;

        public Sprite Sprite;
        public SpriteFacing SpriteFacingData;

        public Color OverridePowerColor = Color.white;
        public int Power;
        public bool HidePowerToNeighbors;
        public bool PreventConsumeIfKillingBlow;
        public int XPReward;

        public ItemSchema.Id[] ItemsToReplenish;

        // When fully collected, instead of becoming Empty, this object will be the new housed object on the tile.
        public TileSchema DropReward;

        // When fully collected AND HAS X ENEMY NEIGHBORS, instead of becoming Empty, this object will be the new housed object on the tile.
        public SerializedDictionary<int, TileSchema> NumNeighborDropReward = new();
        public SerializedDictionary<int, ItemSchema.Id> NumNeighborItemReward = new();

        public ItemSchema.Id ItemReward;
        public Rarity[] ItemRewardRarities;

        /// <summary>
        /// When this tile is collected, all tile keys become tile value.
        /// </summary>
        [Serializable]
        public struct TileSwapRewardEntry
        {
            public TileSchema.Id From;
            public TileSchema.Id To;
            public int Amount;
        }

        public List<TileSwapRewardEntry> TileUpdateReward = new();
        public TileSwapRewardEntry ChildUpdateReward;   // If this tile spawned children (from SpawnSettings) then update them here

        // TODO: Refactor all this shit honestly
        public Vector2Int[] RevealOffsets;
        public int RevealOffsetCount = -1; // If -1, all will be used, otherwise randomly for count
        public int RevealRadius;
        public Vector2Int[] ObscureOffsets;
        public int ObscureRadius;

        // Hack: basically Gnome behavior
        public bool CanFlee;
        [Tooltip("Fleeing VFX played when this successfully flees.")]
        public GameObject FleeVfx;
        public string FleeSfx;
        public bool RevealFlee;
        public bool CanEnrage;

        /// <summary>
        /// Optional grunt to play, on conquer, if the tile you are guarding is conquered while you are revealed.
        /// </summary>
        public string BodyguardGrunt;
        /// <summary>
        /// Optional grunt to play, on conquer, if the you are guarding is revealed while you are conquered.
        /// </summary>
        public string GuardingGrunt;
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

            /// <summary>
            /// If not empty, will be attempted to be played when the corresponding state is entered.
            /// DEFAULT: Always play "Attack" when conquering
            /// </summary>
            public ValueOverride<string> Sfx;

            /// <summary>
            /// This vfx is spawned when this state occurs on this tile.
            /// Be careful because our "state" code is pretty bad. We tend to re-state stuff :\
            /// </summary>
            public ValueOverride<GameObject> Vfx;
        }

        /// <summary>
        /// This is what will be spawned on every tile that is revealed by this object's reveal radius/offsets.
        /// </summary>
        [SerializeField]
        public GameObject RevealVfx;

        [SerializedDictionary("Tile State", "State Overrides")]
        public SerializedDictionary<Tile.TileState, TileStateData> Data;

        public TileStateData GetOverrides(Tile.TileState state)
        {
            return Data.GetValueOrDefault(state);
        }

        // TODO HACK fix later
        public Sprite TEMP_GetSprite(bool shouldStandUp, CompassDirections directionToLook, bool isBodyGuardByOrGuardingDead)
        {
            if (shouldStandUp)
            {
                return SpriteFacingData.Missing;
            }

            if (SpriteFacingData.BodyGuardedByOrGuardingDead != null && isBodyGuardByOrGuardingDead)
            {
                return SpriteFacingData.BodyGuardedByOrGuardingDead;
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
