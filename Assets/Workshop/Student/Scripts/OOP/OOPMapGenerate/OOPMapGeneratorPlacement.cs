using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solution
{
    public partial class OOPMapGenerator
    {
        IEnumerator SetUPMap()
        {
            if (randomSeed) seed = UnityEngine.Random.Range(1, 100000);
            Debug.Log("Seed = " + seed);
            LoadFloor(seed, 1);
            yield return new WaitForSeconds(0.5f);
            RandomDamageToListEnemies();
        }

        private void SetUpNpc()
        {
            if (Npc != null)
            {
                Register(Npc, Npc.positionX, Npc.positionY);
                Npc.transform.position = new Vector3(Npc.positionX, Npc.positionY, 0);
            }
            if (NpcSkill != null)
            {
                Register(NpcSkill, NpcSkill.positionX, NpcSkill.positionY);
                NpcSkill.transform.position = new Vector3(NpcSkill.positionX, NpcSkill.positionY, 0);
            }
        }

        private void SetUpExit()
        {
            Register(Exit, X - 1, Y - 1);
            Exit.transform.position = new Vector3(X - 1, Y - 1, 0);
        }

        private void SetUpPlayer()
        {
            Register(player, playerStartPos.x, playerStartPos.y);
            player.transform.position = new Vector3(playerStartPos.x, playerStartPos.y, -0.1f);
        }
        public Identity SetUpItem(int x, int y, GameObject[] _itemsPrefab, Transform parrent, string _name)
        {
            int r = Random.Range(0, _itemsPrefab.Length);
            GameObject obj = Instantiate(_itemsPrefab[r], new Vector3(x, y, 0), Quaternion.identity);
            return SetUpItem(x, y, obj, parrent, _name);
        }

        public Identity SetUpItem(int x, int y, GameObject _itemsPrefab, Transform parrent, string _name)
        {
            Identity id = _itemsPrefab.GetComponent<Identity>();
            if (id == null)
            {
                Debug.LogError(_itemsPrefab.name + " ไม่มี component Identity");
                return null;
            }

            _itemsPrefab.transform.parent = parrent;
            Register(id, x, y);
            if (_name != CollectItem)
                id.Name = _name;
            if (_name == EnemyTag)
            {
                OOPEnemy en = id as OOPEnemy;
                if (en != null) EnemysOnMap.Add(en);
            }
            _itemsPrefab.name = $"Object_{id.Name} {x}, {y}";
            return id;
        }
        private Identity SpawnFrom(GameObject prefab, int x, int y, Transform parent, string typeName)
        {
            GameObject obj = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
            spawnedObjects.Add(obj);
            Identity id = SetUpItem(x, y, obj, parent, typeName);
            if (id == null) return null;

            if (typeName == EnemyTag)
            {
                OOPEnemy en = id as OOPEnemy;
                if (en != null) allEnemies.Add(en);
            }
            else
            {
                generatedObjects[new Vector2Int(x, y)] = id;
            }
            return id;
        }
        private void SpawnWall(int x, int y)
        {
            GameObject prefab = demonWallsPrefab[seedrng.Next(demonWallsPrefab.Length)];
            Identity id = SpawnFrom(prefab, x, y, wallParent, DemonWall);

            OOPWall wall = id as OOPWall;
            if (wall != null)
                wall.Setup(wall.Damage, seedrng.NextDouble() < iceWallRate);
        }
        private List<Vector2Int> GetFreeCells(bool includePath)
        {
            List<Vector2Int> cells = new List<Vector2Int>();
            for (int x = 0; x < mapWidth; x++)
                for (int y = 0; y < mapHeight; y++)
                {
                    if (!IsCellEmpty(x, y)) continue;
                    if (!includePath && pathMap[x, y]) continue;
                    cells.Add(new Vector2Int(x, y));
                }
            return cells;
        }

        private void Shuffle(List<Vector2Int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = seedrng.Next(i + 1);
                Vector2Int tmp = list[i]; list[i] = list[j]; list[j] = tmp;
            }
        }
        private void PlacePlayerAndExit()
        {
            startCell = playerStartPos;
            exitCell = new Vector2Int(mapWidth - 1, mapHeight - 1);
            SetUpPlayer();
            SetUpExit();
            SetUpNpc();
        }

        private void PlaceObstacles()
        {
            if (demonWallsPrefab == null || demonWallsPrefab.Length == 0) return;

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    if (pathMap[x, y]) continue;
                    if (!IsCellEmpty(x, y)) continue;

                    if (seedrng.NextDouble() < obstacleRate)
                        SpawnWall(x, y);
                }
            }
        }
        private void PlaceCheckpoint()
        {
            if (checkpointPrefab == null)
            {
                Debug.LogWarning("checkpointPrefab = null");
                return;
            }

            Vector2Int? cell = FindCheckpointCell(BuildOccupied());
            if (!cell.HasValue)
            {
                Debug.LogWarning("Can't find a cell for the checkpoint.");
                return;
            }

            SpawnFrom(checkpointPrefab, cell.Value.x, cell.Value.y, itemParent, "checkpoint");
        }

        private void PlaceOnPath(GameObject prefab, int count, Transform parent, string typeName)
        {
            List<Vector2Int> candidates = new List<Vector2Int>();
            for (int i = 1; i < pathCells.Count - 1; i++)
            {
                Vector2Int c = pathCells[i];
                if (IsCellEmpty(c.x, c.y)) candidates.Add(c);
            }
            Shuffle(candidates);

            if (candidates.Count < count)
                Debug.LogWarning("ช่องบนทางไม่พอ วางได้ " + candidates.Count + " จาก " + count);

            for (int i = 0; i < count && i < candidates.Count; i++)
                SpawnFrom(prefab, candidates[i].x, candidates[i].y, parent, typeName);
        }

        private void PlaceRandomFree(int count, GameObject[] prefabs, Transform parent, string typeName)
        {
            if (count <= 0 || prefabs == null || prefabs.Length == 0) return;

            List<Vector2Int> free = GetFreeCells(false);
            Shuffle(free);
            for (int i = 0; i < count && i < free.Count; i++)
            {
                GameObject prefab = prefabs[seedrng.Next(prefabs.Length)];
                SpawnFrom(prefab, free[i].x, free[i].y, parent, typeName);
            }
        }

        private void PlaceItems()
        {
            if (keyPrefab != null)
                PlaceOnPath(keyPrefab, Exit.ItemAmountToOpen, itemParent, CollectItem);
            else
                Debug.LogWarning("ยังไม่ได้ใส่ keyPrefab ใน Inspector");

            PlaceRandomFree(itemPotionCount, itemsPrefab, itemParent, Potion);
            PlaceRandomFree(colloctItemCount, collectItemsPrefab, itemParent, CollectItem);
            PlaceRandomFree(SkillCount, SkillPrefab, itemParent, CollectItem);
        }

        private void PlaceEnemies()
        {
            if (EnemyPrefab == null || EnemyPrefab.Length == 0) return;

            int count = seedrng.Next(activeConfig.enemyMin, activeConfig.enemyMax + 1);
            int types = Mathf.Clamp(activeConfig.unlockedEnemyTypes, 1, EnemyPrefab.Length);

            List<Vector2Int> free = GetFreeCells(true);
            Shuffle(free);

            int placed = 0;
            foreach (Vector2Int c in free)
            {
                if (placed >= count) break;

                int dist = Mathf.Abs(c.x - startCell.x) + Mathf.Abs(c.y - startCell.y);
                if (dist < enemySafeDistance) continue;

                GameObject prefab = EnemyPrefab[seedrng.Next(types)];
                SpawnFrom(prefab, c.x, c.y, enemyParent, EnemyTag);
                placed++;
            }
        }
        private bool[,] BuildOccupied()
        {
            bool[,] occ = new bool[mapWidth, mapHeight];
            for (int x = 0; x < mapWidth; x++)
                for (int y = 0; y < mapHeight; y++)
                    occ[x, y] = !IsCellEmpty(x, y);
            return occ;
        }

        private Vector2Int? FindCheckpointCell(bool[,] occupied)
        {
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            int mid = pathCells.Count / 2;

            for (int offset = 0; offset < pathCells.Count; offset++)
            {
                int[] indexes = { mid + offset, mid - offset };
                foreach (int i in indexes)
                {
                    if (i <= 0 || i >= pathCells.Count - 1) continue;

                    foreach (Vector2Int d in dirs)
                    {
                        Vector2Int c = pathCells[i] + d;

                        bool inside = c.x >= 0 && c.x < mapWidth && c.y >= 0 && c.y < mapHeight;
                        if (!inside) continue;
                        if (pathMap[c.x, c.y]) continue;
                        if (occupied[c.x, c.y]) continue;

                        return c;
                    }
                }
            }
            return null;
        }
    }   
}
