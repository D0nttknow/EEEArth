using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solution
{
    [Serializable]
    public class ItemEntry
    {
        public string itemName;
        public int itemAmount;
    }

    [Serializable]
    public class EnemyState
    {
        public int index;   // ลำดับตอนวางศัตรูตอนเจนแมป (ใช้จับคู่ตอนโหลด)
        public int x;
        public int y;
        public int energy;
    }

    [Serializable]
    public class CellPos
    {
        public int x;
        public int y;
    }

    [Serializable]
    public class CheckpointData
    {
        public string saveName;
        public string saveTime;
        public int floor;
        public int seed;
        public int playerX;
        public int playerY;
        public int energy;
        public List<ItemEntry> inventory = new List<ItemEntry>();
        public List<EnemyState> enemies = new List<EnemyState>();
        public List<CellPos> removedCells = new List<CellPos>();   // ของ/กำแพงที่ถูกเก็บหรือทำลายไปแล้ว

        // ---------- ตอนเซฟ: ถ่ายสถานะปัจจุบันลง data ----------
        public static CheckpointData Capture(OOPPlayer p, OOPMapGenerator map)
        {
            CheckpointData d = new CheckpointData();
            d.saveName = "Floor " + map.currentFloor;
            d.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            d.floor = map.currentFloor;
            d.seed = map.currentSeed;
            d.playerX = p.positionX;
            d.playerY = p.positionY;
            d.energy = p.energy;

            foreach (KeyValuePair<string, int> kv in p.inventory.inventory)
                d.inventory.Add(new ItemEntry { itemName = kv.Key, itemAmount = kv.Value });

            for (int i = 0; i < map.AllEnemies.Count; i++)
            {
                OOPEnemy e = map.AllEnemies[i];
                if (e == null || !e.gameObject.activeInHierarchy) continue;   // ตายไปแล้ว ไม่เซฟ
                d.enemies.Add(new EnemyState { index = i, x = e.positionX, y = e.positionY, energy = e.energy });
            }

            foreach (Vector2Int c in map.GetRemovedCells())
                d.removedCells.Add(new CellPos { x = c.x, y = c.y });

            return d;
        }

        // ---------- ตอนโหลด: สร้างแมปเดิมจาก seed แล้วใส่สถานะกลับ ----------
        public void Restore(OOPPlayer p, OOPMapGenerator map)
        {
            map.StartCoroutine(RestoreRoutine(p, map));
        }

        private IEnumerator RestoreRoutine(OOPPlayer p, OOPMapGenerator map)
        {
            map.LoadFloor(seed, floor);                 // 1. สร้างแมปเดิม
            yield return new WaitForSeconds(0.2f);      // รอให้ Start()/SetUP() ของวัตถุใหม่ทำงานก่อน ไม่งั้นค่าที่ใส่จะถูกทับ

            map.ApplyRemovedCells(removedCells);        // 2. เอาของที่เก็บ/ทำลายไปแล้วออก
            map.ApplyEnemyStates(enemies);              // 3. ศัตรู: ตำแหน่ง พลังงาน ตัวที่ตายแล้วปิด

            map.MoveIdentityTo(p, playerX, playerY);    // 4. ผู้เล่น
            p.energy = energy;

            Dictionary<string, int> inv = p.inventory.inventory;
            inv.Clear();
            foreach (ItemEntry it in inventory)
                inv[it.itemName] = it.itemAmount;

            map.history.Clear();                        // 5. ล้างกอง Undo
        }
    }
}