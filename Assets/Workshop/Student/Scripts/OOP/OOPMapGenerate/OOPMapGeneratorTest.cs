using System.Collections.Generic;
using UnityEngine;

namespace Solution
{
    public partial class OOPMapGenerator
    {
        [Header("Test")]
        public int testFromSeed = 1;
        public int testToSeed = 1000;
        public bool randomSeed = true;
        [ContextMenu("Test Paths (All Seeds)")]
        public void TestAllSeeds()
        {
            int oldW = mapWidth, oldH = mapHeight;
            Vector2Int oldStart = startCell, oldExit = exitCell;
            System.Random oldRng = seedrng;
            bool[,] oldMap = pathMap;
            List<Vector2Int> oldCells = pathCells;
            FloorConfig oldConfig = activeConfig;
            float oldGoalBias = goalBias, oldObstacleRate = obstacleRate;
            int oldPotion = itemPotionCount, oldCollect = colloctItemCount;

            mapWidth = X;
            mapHeight = Y;
            startCell = playerStartPos;
            exitCell = new Vector2Int(X - 1, Y - 1);

            bool[,] occupied = new bool[X, Y];
            MarkOccupied(occupied, startCell);
            MarkOccupied(occupied, exitCell);
            if (Npc != null) MarkOccupied(occupied, new Vector2Int(Npc.positionX, Npc.positionY));
            if (NpcSkill != null) MarkOccupied(occupied, new Vector2Int(NpcSkill.positionX, NpcSkill.positionY));

            int total = 0, failed = 0;
            for (int s = testFromSeed; s <= testToSeed; s++)
            {
                for (int floor = 1; floor <= floors.Length; floor++)
                {
                    total++;
                    string error = CheckOneSeed(s, floor, occupied);
                    if (error != null)
                    {
                        failed++;
                        if (failed <= 10)
                            Debug.LogError("FAIL seed=" + s + " floor=" + floor + " : " + error);
                    }
                }
            }

            if (failed == 0)
                Debug.Log("PASS: ทางถึงทางออก + Checkpoint (" + total + " รอบ, seed " + testFromSeed + "-" + testToSeed + ", ชั้น 1-" + floors.Length + ")");
            else
                Debug.LogError("FAIL " + failed + " จาก " + total + " รอบ");

            mapWidth = oldW; mapHeight = oldH;
            startCell = oldStart; exitCell = oldExit;
            seedrng = oldRng; pathMap = oldMap; pathCells = oldCells;
            activeConfig = oldConfig;
            goalBias = oldGoalBias; obstacleRate = oldObstacleRate;
            itemPotionCount = oldPotion; colloctItemCount = oldCollect;
        }

        private void MarkOccupied(bool[,] occupied, Vector2Int c)
        {
            if (c.x >= 0 && c.x < occupied.GetLength(0) && c.y >= 0 && c.y < occupied.GetLength(1))
                occupied[c.x, c.y] = true;
        }

        private string CheckOneSeed(int s, int floor, bool[,] occupied)
        {
            ApplyFloorConfig(floor);
            seedrng = new System.Random(s + floor);
            CarvePath(occupied);
            List<Vector2Int> first = new List<Vector2Int>(pathCells);

            if (first.Count == 0) return "pathCells ว่าง";
            if (first[0] != startCell) return "ไม่ได้เริ่มที่ startCell(จุดเริ่ม)";
            if (first[first.Count - 1] != exitCell) return "ไม่ถึง exitCell(ทางตัน)";

            HashSet<Vector2Int> seen = new HashSet<Vector2Int>();
            for (int i = 0; i < first.Count; i++)
            {
                Vector2Int c = first[i];
                if (c.x < 0 || c.x >= mapWidth || c.y < 0 || c.y >= mapHeight) return "มีช่องนอกแมป " + c;
                if (!seen.Add(c)) return "มีช่องซ้ำ " + c;
                if (!pathMap[c.x, c.y]) return "pathMap ไม่ตรงกับ pathCells ที่ " + c;
                if (i > 0 && i < first.Count - 1 && occupied[c.x, c.y]) return "ทางผ่านช่อง NPC/วัตถุ " + c;
                if (i > 0 && Mathf.Abs(first[i - 1].x - c.x) + Mathf.Abs(first[i - 1].y - c.y) != 1)
                    return "ทางไม่ต่อเนื่อง ระหว่าง " + first[i - 1] + " กับ " + c;
            }

            int trueCount = 0;
            for (int x = 0; x < mapWidth; x++)
                for (int y = 0; y < mapHeight; y++)
                    if (pathMap[x, y]) trueCount++;
            if (trueCount != first.Count) return "pathMap มีช่องที่ไม่ได้อยู่ในทาง";

            Vector2Int? cp1 = FindCheckpointCell(occupied);
            string cpError = CheckCheckpointCell(cp1, occupied);
            if (cpError != null) return cpError;

            seedrng = new System.Random(s + floor);
            CarvePath(occupied);
            if (pathCells.Count != first.Count) return "seed เดิมแต่ได้ทางความยาวต่างกัน";
            for (int i = 0; i < first.Count; i++)
                if (pathCells[i] != first[i]) return "seed เดิมแต่ได้ทางต่างกัน";

            Vector2Int? cp2 = FindCheckpointCell(occupied);
            if (!cp2.HasValue || cp2.Value != cp1.Value) return "seed เดิมแต่ Checkpoint อยู่คนละช่อง";

            return null;
        }

        private string CheckCheckpointCell(Vector2Int? cp, bool[,] occupied)
        {
            if (!cp.HasValue) return "หาช่อง Checkpoint ไม่ได้";

            Vector2Int c = cp.Value;
            if (c.x < 0 || c.x >= mapWidth || c.y < 0 || c.y >= mapHeight) return "Checkpoint อยู่นอกแมป " + c;
            if (pathMap[c.x, c.y]) return "Checkpoint อยู่บนทาง (ปิดทาง) " + c;
            if (occupied[c.x, c.y]) return "Checkpoint ทับจุดเริ่ม/ทางออก/NPC " + c;

            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            bool nextToPath = false;
            foreach (Vector2Int d in dirs)
            {
                Vector2Int n = c + d;
                bool inside = n.x >= 0 && n.x < mapWidth && n.y >= 0 && n.y < mapHeight;
                if (inside && pathMap[n.x, n.y]) { nextToPath = true; break; }
            }
            if (!nextToPath) return "Checkpoint ไม่ติดทาง เดินชนไม่ได้ " + c;

            return null;
        }
        private int MapHash()
        {
            unchecked
            {
                int h = 17;
                foreach (Vector2Int c in pathCells)
                    h = h * 31 + c.x * 100 + c.y;

                foreach (Identity id in AllIdentities())
                {
                    if (!id.gameObject.activeInHierarchy) continue;
                    h = h * 31 + id.positionX * 100 + id.positionY;
                    foreach (char ch in id.Name ?? "") h = h * 31 + ch;
                }
                return h;
            }
        }
        [ContextMenu("Log State (for Save test)")]
        public void LogState()
        {
            string enemies = "";
            for (int i = 0; i < allEnemies.Count; i++)
            {
                OOPEnemy e = allEnemies[i];
                if (e == null || !e.gameObject.activeInHierarchy) continue;   // ตายแล้ว ไม่นับ
                enemies += "[" + i + ":(" + e.positionX + "," + e.positionY + ") hp=" + e.energy + "] ";
            }

            string inv = "";
            foreach (KeyValuePair<string, int> kv in player.inventory.inventory)
                inv += kv.Key + "=" + kv.Value + " ";

            Debug.Log("STATE floor=" + currentFloor + " seed=" + currentSeed +
                      " player=(" + player.positionX + "," + player.positionY + ") hp=" + player.energy +
                      " inv=" + inv +
                      " enemies=" + enemies +
                      " removed=" + GetRemovedCells().Count +
                      " hash=" + MapHash());
        }
        [ContextMenu("Test Save Roundtrip")]
        public void TestSaveRoundtrip()
        {
            CheckpointData a = CheckpointData.Capture(player, this);
            string json = JsonUtility.ToJson(a, true);
            CheckpointData b = JsonUtility.FromJson<CheckpointData>(json);

            string err = null;

            if (a.floor != b.floor || a.seed != b.seed) err = "floor/seed ไม่ตรง";
            else if (a.history.Count != b.history.Count) err = "จำนวนตาในประวัติ Undo ไม่ตรง";
            else if (a.playerX != b.playerX || a.playerY != b.playerY || a.energy != b.energy) err = "ข้อมูลผู้เล่นไม่ตรง";
            else if (a.inventory.Count != b.inventory.Count) err = "จำนวนชนิดไอเทมไม่ตรง";
            else if (a.enemies.Count != b.enemies.Count) err = "จำนวนศัตรูไม่ตรง";
            else if (a.removedCells.Count != b.removedCells.Count) err = "จำนวนช่องที่หายไปไม่ตรง";

            if (err == null)
            {
                for (int i = 0; i < a.inventory.Count; i++)
                    if (a.inventory[i].itemName != b.inventory[i].itemName ||
                        a.inventory[i].itemAmount != b.inventory[i].itemAmount)
                    { err = "ไอเทมลำดับที่ " + i + " ไม่ตรง"; break; }
            }
            if (err == null)
            {
                for (int i = 0; i < a.enemies.Count; i++)
                    if (a.enemies[i].index != b.enemies[i].index || a.enemies[i].x != b.enemies[i].x ||
                        a.enemies[i].y != b.enemies[i].y || a.enemies[i].energy != b.enemies[i].energy)
                    { err = "ศัตรูลำดับที่ " + i + " ไม่ตรง"; break; }
            }
            if (err == null)
            {
                for (int i = 0; i < a.removedCells.Count; i++)
                    if (a.removedCells[i].x != b.removedCells[i].x || a.removedCells[i].y != b.removedCells[i].y)
                    { err = "ช่องที่หายไปลำดับที่ " + i + " ไม่ตรง"; break; }
            }

            if (err == null) Debug.Log("PASS: Save roundtrip ตรงทุกค่า\n" + json);
            else Debug.LogError("FAIL: " + err + "\n" + json);
        }
    }
}
