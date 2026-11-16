using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solution
{

    public class OOPMapGenerator : MonoBehaviour
    {
        [Header("Set Generate")]
        public GameObject checkpointPrefab;
        public GameObject keyPrefab;
        [Range(0f, 1f)] public float goalBias = 0.6f;
        [Range(0f, 1f)] public float obstacleRate = 0.2f;
        [Range(0f, 1f)] public float iceWallRate = 0.2f;
        public int enemySafeDistance = 4;
        [Header("Set MapGenerator")]
        public int X;
        public int Y;

        [Header("Floors")]
        public FloorConfig[] floors = FloorConfig.CreateDefault();
        public int currentFloor = 1;
        [HideInInspector] public int currentSeed;
        [HideInInspector] public HistoryManager history;

        private FloorConfig activeConfig;
        private readonly List<GameObject> spawnedObjects = new List<GameObject>();
        private readonly List<OOPEnemy> allEnemies = new List<OOPEnemy>();
        private readonly Dictionary<Vector2Int, Identity> generatedObjects = new Dictionary<Vector2Int, Identity>();

        public List<OOPEnemy> AllEnemies { get { return allEnemies; } }
        public bool IsLastFloor { get { return currentFloor >= floors.Length; } }

        [Header("Set Player")]
        public OOPPlayer player;
        public Vector2Int playerStartPos;
        [Header("Set NPC")]
        public NPC Npc;
        public NPCSkill NpcSkill;
        [Header("Set Exit")]
        public OOPExit Exit;
        [Header("Set Wall")]
        public Identity Wall;

        [Header("Set Prefab")]
        public GameObject[] floorsPrefab;
        public GameObject[] wallsPrefab;
        public GameObject[] demonWallsPrefab;
        public GameObject[] itemsPrefab;
        public GameObject[] collectItemsPrefab;
        public GameObject[] EnemyPrefab;
        public GameObject[] SkillPrefab;

        [Header("Set Transform")]
        public Transform floorParent;
        public Transform wallParent;
        public Transform itemParent;
        public Transform enemyParent;

        [Header("Set object Count")]
        public int obsatcleCount;
        public int itemPotionCount;
        public int colloctItemCount;
        public int EnemyCount;
        public int SkillCount;

        public Identity[,] mapdata;
        [Header("Enemy on Map")]
        public List<OOPEnemy> EnemysOnMap = new List<OOPEnemy>();

        [HideInInspector]
        public string empty = "";
        [HideInInspector]
        public string demonWall = "demonWall";
        [HideInInspector]
        public string potion = "potion";
        [HideInInspector]
        public string bonuesPotion = "bonuesPotion";
        [HideInInspector]
        public string exit = "exit";
        [HideInInspector]
        public string playerOnMap = "player";
        [HideInInspector]
        public string collectItem = "collectItem";
        [HideInInspector]
        public string enemy = "enemy";
        [HideInInspector]
        public string npc = "Npc";

        public int seed;
        private bool[,] pathMap;
        public int mapWidth;
        public int mapHeight;
        private List<Vector2Int> pathCells;
        private Vector2Int startCell, exitCell;
        private System.Random seedrng;

        public void GenerateMap(int seed, int floor)
        {
            this.seed = seed;
            currentSeed = seed;
            currentFloor = floor;
            ApplyFloorConfig(floor);
            seedrng = new System.Random(seed + floor);
            CreateGrid(floor);
            PlacePlayerAndExit();
            CarvePath();
            PlaceCheckpoint();
            PlaceObstacles();
            PlaceItems();
            PlaceEnemies();
        }

        private void Awake()
        {
            history = new HistoryManager(this);
            CreateMap();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Z)) history.Undo();
            if (Input.GetKeyDown(KeyCode.Equals)) LoadCheckpoint();
        }
        void Start()
        {
            StartCoroutine(SetUPMap());
        }
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
            mapdata[Npc.positionX, Npc.positionY] = Npc;
            mapdata[NpcSkill.positionX, NpcSkill.positionY] = NpcSkill;
            Npc.transform.position = new Vector3(Npc.positionX, Npc.positionY, 0);
            NpcSkill.transform.position = new Vector3(NpcSkill.positionX, NpcSkill.positionY, 0);
            Npc.mapGenerator = this;
            NpcSkill.mapGenerator = this;
        }
        private void SetUpExit()
        {
            mapdata[X - 1, Y - 1] = Exit;
            Exit.transform.position = new Vector3(X - 1, Y - 1, 0);
        }

        private void SetUpPlayer()
        {
            player.mapGenerator = this;
            player.positionX = playerStartPos.x;
            player.positionY = playerStartPos.y;
            player.transform.position = new Vector3(playerStartPos.x, playerStartPos.y, -0.1f);
            mapdata[playerStartPos.x, playerStartPos.y] = player;
        }

        private void CreateMap()
        {
            mapdata = new Identity[X, Y];
            for (int x = -1; x < X + 1; x++)
            {
                for (int y = -1; y < Y + 1; y++)
                {
                    if (x == -1 || x == X || y == -1 || y == Y)
                    {
                        int r = Random.Range(0, wallsPrefab.Length);
                        GameObject obj = Instantiate(wallsPrefab[r], new Vector3(x, y, 0), Quaternion.identity);
                        obj.transform.parent = wallParent;
                        obj.name = "Wall_" + x + ", " + y;
                    }
                    else
                    {
                        int r = Random.Range(0, floorsPrefab.Length);
                        GameObject obj = Instantiate(floorsPrefab[r], new Vector3(x, y, 1), Quaternion.identity);
                        obj.transform.parent = floorParent;
                        obj.name = "floor_" + x + ", " + y;
                        mapdata[x, y] = null;
                    }
                }
            }
        }

        public Identity GetMapData(float x, float y)
        {
            if (x >= X || x < 0 || y >= Y || y < 0) {
                return Wall;
            }

            return mapdata[(int)x, (int)y];
        }

        public void SetUpItem(int x, int y,GameObject[] _itemsPrefab,Transform parrent,string _name)
        {
            int r = Random.Range(0, _itemsPrefab.Length);
            GameObject obj = Instantiate(_itemsPrefab[r], new Vector3(x, y, 0), Quaternion.identity);
            obj.transform.parent = parrent;
            mapdata[x, y] = obj.GetComponent<Identity>();
            mapdata[x, y].positionX = x;
            mapdata[x, y].positionY = y;
            mapdata[x, y].mapGenerator = this;
            if (_name != collectItem) {
                mapdata[x, y].Name = _name;
            }
            if (_name == enemy) {
                EnemysOnMap.Add(obj.GetComponent<OOPEnemy>());
            }
            obj.name = $"Object_{mapdata[x, y].Name} {x}, {y}";
        }
        public void SetUpItem(int x, int y, GameObject _itemsPrefab, Transform parrent, string _name)
        {
            _itemsPrefab.transform.parent = parrent;
            mapdata[x, y] = _itemsPrefab.GetComponent<Identity>();
            mapdata[x, y].positionX = x;
            mapdata[x, y].positionY = y;
            mapdata[x, y].mapGenerator = this;
            if (_name != collectItem)
            {
                mapdata[x, y].Name = _name;
            }
            if (_name == enemy)
            {
                EnemysOnMap.Add(_itemsPrefab.GetComponent<OOPEnemy>());
            }
            _itemsPrefab.name = $"Object_{mapdata[x, y].Name} {x}, {y}";
        }

        public OOPEnemy[] GetEnemies()
        {
            return EnemysOnMap.ToArray();
        }
        public void MoveEnemies()
        {
            foreach (var enemy in EnemysOnMap)
            {
                enemy.RandomMove();
            }
        }
        public void RandomDamageToListEnemies() {
            Debug.Log($"Damage to {EnemysOnMap.Count} EnemysOnMap");
            foreach (var enemy in EnemysOnMap)
            {
                int damage = Random.Range(1, 15);
                enemy.TakeDamage(damage);
            }
        }

        private void CreateGrid(int floor)
        {
            mapWidth = X;
            mapHeight = Y;
            if (mapdata == null) mapdata = new Identity[mapWidth, mapHeight];
            pathMap = new bool[mapWidth, mapHeight];
            pathCells = new List<Vector2Int>();
        }

        private void PlacePlayerAndExit()
        {
            startCell = playerStartPos;
            exitCell = new Vector2Int(mapWidth - 1, mapHeight - 1);
            SetUpPlayer();
            SetUpExit();
            SetUpNpc();
        }

        private void CarvePath()
        {
            CarvePath(BuildOccupied());
        }

        private void CarvePath(bool[,] blocked)
        {
            if (TryCarve(blocked)) return;
            TryCarve(new bool[mapWidth, mapHeight]);
        }

        private bool TryCarve(bool[,] blocked)
        {
            pathMap = new bool[mapWidth, mapHeight];
            pathCells = new List<Vector2Int>();
            bool[,] visited = new bool[mapWidth, mapHeight];

            Stack<Vector2Int> stack = new Stack<Vector2Int>();
            visited[startCell.x, startCell.y] = true;
            stack.Push(startCell);
            bool reached = false;

            while (stack.Count > 0)
            {
                Vector2Int currentCell = stack.Peek();
                if (currentCell == exitCell) { reached = true; break; }

                List<Vector2Int> neighbors = GetUnvisitedNeighbors(currentCell, visited, blocked);

                if (neighbors.Count > 0)
                {
                    Vector2Int next = neighbors[seedrng.Next(neighbors.Count)];

                    if (seedrng.NextDouble() < goalBias)
                    {
                        int bestWay = int.MaxValue;
                        foreach (Vector2Int n in neighbors)
                        {
                            int distanceN = Mathf.Abs(n.x - exitCell.x) + Mathf.Abs(n.y - exitCell.y);
                            if (distanceN < bestWay) { bestWay = distanceN; next = n; }
                        }
                    }

                    visited[next.x, next.y] = true;
                    stack.Push(next);
                }
                else
                {
                    stack.Pop();
                }
            }

            if (!reached) return false;
            pathCells.AddRange(stack);
            pathCells.Reverse();
            foreach (Vector2Int cell in pathCells)
                pathMap[cell.x, cell.y] = true;
            return true;
        }

        private List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell, bool[,] visited, bool[,] blocked)
        {
            List<Vector2Int> result = new List<Vector2Int>();
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (Vector2Int direction in directions)
            {
                Vector2Int n = cell + direction;
                bool inside = n.x >= 0 && n.x < mapWidth && n.y >= 0 && n.y < mapHeight;
                if (!inside) continue;
                if (visited[n.x, n.y]) continue;
                if (blocked[n.x, n.y] && n != exitCell) continue;

                result.Add(n);
            }
            return result;
        }

        private void SpawnFrom(GameObject prefab, int x, int y, Transform parent, string typeName)
        {
            GameObject obj = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
            spawnedObjects.Add(obj);
            SetUpItem(x, y, obj, parent, typeName);

            Identity id = mapdata[x, y];
            if (typeName == enemy)
            {
                OOPEnemy en = id as OOPEnemy;
                if (en != null) allEnemies.Add(en);
            }
            else
            {
                generatedObjects[new Vector2Int(x, y)] = id;
            }
        }

        private List<Vector2Int> GetFreeCells(bool includePath)
        {
            List<Vector2Int> cells = new List<Vector2Int>();
            for (int x = 0; x < mapWidth; x++)
                for (int y = 0; y < mapHeight; y++)
                {
                    if (mapdata[x, y] != null) continue;
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

        private void PlaceObstacles()
        {
            if (demonWallsPrefab == null || demonWallsPrefab.Length == 0) return;

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    if (pathMap[x, y]) continue;
                    if (mapdata[x, y] != null) continue;

                    if (seedrng.NextDouble() < obstacleRate)
                        SpawnWall(x, y);
                }
            }
        }

        private void SpawnWall(int x, int y)
        {
            GameObject prefab = demonWallsPrefab[seedrng.Next(demonWallsPrefab.Length)];
            SpawnFrom(prefab, x, y, wallParent, demonWall);

            OOPWall wall = mapdata[x, y] as OOPWall;
            if (wall != null)
                wall.Setup(wall.Damage, seedrng.NextDouble() < iceWallRate);
        }

        private bool[,] BuildOccupied()
        {
            bool[,] occ = new bool[mapWidth, mapHeight];
            for (int x = 0; x < mapWidth; x++)
                for (int y = 0; y < mapHeight; y++)
                    occ[x, y] = mapdata[x, y] != null;
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
                if (mapdata[c.x, c.y] == null) candidates.Add(c);
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
                PlaceOnPath(keyPrefab, Exit.ItemAmountToOpen, itemParent, collectItem);
            else
                Debug.LogWarning("ยังไม่ได้ใส่ keyPrefab ใน Inspector");

            PlaceRandomFree(itemPotionCount, itemsPrefab, itemParent, potion);
            PlaceRandomFree(colloctItemCount, collectItemsPrefab, itemParent, collectItem);
            PlaceRandomFree(SkillCount, SkillPrefab, itemParent, collectItem);
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
                SpawnFrom(prefab, c.x, c.y, enemyParent, enemy);
                placed++;
            }
        }

        private void ApplyFloorConfig(int floor)
        {
            if (floors == null || floors.Length == 0) floors = FloorConfig.CreateDefault();
            activeConfig = floors[Mathf.Clamp(floor - 1, 0, floors.Length - 1)];
            obstacleRate = activeConfig.obstacleRate;
            goalBias = activeConfig.goalBias;
            itemPotionCount = activeConfig.potionCount;
            colloctItemCount = activeConfig.collectItemCount;
        }

        public void LoadFloor(int seed, int floor)
        {
            ClearMap();
            GenerateMap(seed, floor);
            history.Clear();
        }

        private void ClearMap()
        {
            foreach (GameObject go in spawnedObjects)
                if (go != null) Destroy(go);
            spawnedObjects.Clear();
            allEnemies.Clear();
            EnemysOnMap.Clear();
            generatedObjects.Clear();
            mapdata = new Identity[X, Y];
        }
        public void NextFloor()
        {
            StartCoroutine(NextFloorRoutine());
        }

        private IEnumerator NextFloorRoutine()
        {
            yield return null;
            if (IsLastFloor) yield break;

            LoadFloor(currentSeed, currentFloor + 1);
            Debug.Log("Floor " + currentFloor);
            yield return new WaitForSeconds(0.5f);
            RandomDamageToListEnemies();
        }

        public void LoadCheckpoint()
        {
            CheckpointData d = SaveManager.Load(SaveManager.currentSlot);
            if (d == null) { Debug.Log("ไม่มีไฟล์เซฟช่อง " + SaveManager.currentSlot); return; }
            d.Restore(player, this);
        }

        public List<Vector2Int> GetRemovedCells()
        {
            List<Vector2Int> list = new List<Vector2Int>();
            foreach (KeyValuePair<Vector2Int, Identity> kv in generatedObjects)
                if (kv.Value == null || !kv.Value.gameObject.activeInHierarchy)
                    list.Add(kv.Key);
            return list;
        }

        public void ApplyRemovedCells(List<CellPos> cells)
        {
            foreach (CellPos c in cells)
            {
                Identity id;
                if (generatedObjects.TryGetValue(new Vector2Int(c.x, c.y), out id) && id != null)
                    id.Despawn();
            }
        }

        public void ApplyEnemyStates(List<EnemyState> states)
        {
            for (int i = 0; i < allEnemies.Count; i++)
            {
                OOPEnemy e = allEnemies[i];
                if (e != null && mapdata[e.positionX, e.positionY] == e)
                    mapdata[e.positionX, e.positionY] = null;
            }

            HashSet<int> alive = new HashSet<int>();
            foreach (EnemyState s in states) alive.Add(s.index);
            for (int i = 0; i < allEnemies.Count; i++)
            {
                if (alive.Contains(i) || allEnemies[i] == null) continue;
                EnemysOnMap.Remove(allEnemies[i]);
                allEnemies[i].gameObject.SetActive(false);
            }

            foreach (EnemyState s in states)
            {
                if (s.index < 0 || s.index >= allEnemies.Count) continue;
                OOPEnemy e = allEnemies[s.index];
                if (e == null) continue;
                MoveIdentityTo(e, s.x, s.y);
                e.energy = s.energy;
                if (e.txtHp != null) e.txtHp.text = e.energy.ToString();
            }
        }

        public void MoveIdentityTo(Identity obj, int x, int y)
        {
            if (obj == null) return;
            if (mapdata[obj.positionX, obj.positionY] == obj)
                mapdata[obj.positionX, obj.positionY] = null;
            obj.positionX = x;
            obj.positionY = y;
            obj.transform.position = new Vector3(x, y, obj.transform.position.z);
            mapdata[x, y] = obj;
        }

        [Header("Test")]
        public int testFromSeed = 1;
        public int testToSeed = 1000;
        public bool randomSeed = true;
        [ContextMenu("Test Paths (All Seeds)")]
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
                for (int floor = 1; floor <= 4; floor++)
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
                Debug.Log("PASS: ทางถึงทางออก + Checkpoint (" + total + " รอบ, seed " + testFromSeed + "-" + testToSeed + ", ชั้น 1-4)");
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
    }
}