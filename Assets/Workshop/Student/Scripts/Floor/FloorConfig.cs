using System;
using UnityEngine;

namespace Solution
{
    [Serializable]
    public class FloorConfig
    {
        public int floor = 1;

        [Header("ศัตรู")]
        public int enemyMin = 1;
        public int enemyMax = 2;
        [Tooltip("จำนวนประเภทศัตรูที่ปลดล็อก นับจากช่องแรกของ Enemy Prefab (ปลดล็อกเพิ่มขึ้นตามชั้น)")]
        public int unlockedEnemyTypes = 1;

        [Header("แมป")]
        [Range(0f, 1f)] public float obstacleRate = 0.10f;
        [Range(0f, 1f)] public float goalBias = 0.8f;

        [Header("ไอเทม")]
        public int potionCount = 2;
        public int collectItemCount = 1;

        public static FloorConfig[] CreateDefault()
        {
            return new FloorConfig[]
            {
                new FloorConfig { floor = 1, enemyMin = 1, enemyMax = 2, unlockedEnemyTypes = 1, obstacleRate = 0.10f, goalBias = 0.8f, potionCount = 3, collectItemCount = 1 },
                new FloorConfig { floor = 2, enemyMin = 2, enemyMax = 3, unlockedEnemyTypes = 2, obstacleRate = 0.15f, goalBias = 0.7f, potionCount = 3, collectItemCount = 1 },
                new FloorConfig { floor = 3, enemyMin = 3, enemyMax = 4, unlockedEnemyTypes = 3, obstacleRate = 0.20f, goalBias = 0.6f, potionCount = 2, collectItemCount = 1 },
                new FloorConfig { floor = 4, enemyMin = 4, enemyMax = 5, unlockedEnemyTypes = 4, obstacleRate = 0.25f, goalBias = 0.5f, potionCount = 2, collectItemCount = 1 },
            };
        }
    }
}