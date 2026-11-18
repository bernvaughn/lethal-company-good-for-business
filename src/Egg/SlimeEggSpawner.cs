using GoodForBusiness.Core;
using System.Collections.Generic;
using UnityEngine;

namespace GoodForBusiness.Egg
{
    public class SlimeEggSpawner : MonoBehaviour
    {
        public List<GameObject> EggPrefabs = new List<GameObject>();
        public float GlobalSpawnChance = 1.0f;

        public void SpawnEggsInLevel()
        {
            // Hook into dungeon generation to place slime eggs
        }

        public GameObject SpawnEgg(Vector3 position, SlimeType type)
        {
            var egg = new GameObject("SlimeEgg");
            egg.transform.position = position;
            var eggItem = egg.AddComponent<SlimeEggItem>();
            eggItem.EggType = type;
            return egg;
        }
    }
}