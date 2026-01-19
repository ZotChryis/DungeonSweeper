using System;
using Gameplay;
using UnityEngine;

namespace Schemas
{
    [CreateAssetMenu(menuName = "Data/Challenge")]
    public class ChallengeSchema : Schema
    {
        [Serializable]
        public enum Id
        {
            Test1,
            Test2,
            Test3
        }

        public Id ChallengeId;
        public string Title;
        public Class.Id StartingClass = Class.Id.None;
        public ItemSchema[] StartingItems;
        public string Context;

        public bool OverrideLevels;
        public SpawnSettings[] Levels;
    }
}