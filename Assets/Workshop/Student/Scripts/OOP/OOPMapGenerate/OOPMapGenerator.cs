using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solution
{

    public partial class OOPMapGenerator : MonoBehaviour
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

        [Header("Enemy on Map")]
        public List<OOPEnemy> EnemysOnMap = new List<OOPEnemy>();

        [HideInInspector]
        public string empty = "";
        [HideInInspector]
        public const string DemonWall = "demonWall";
        [HideInInspector]
        public const string Potion = "potion";
        [HideInInspector]
        public string bonuesPotion = "bonuesPotion";
        [HideInInspector]
        public string exit = "exit";
        [HideInInspector]
        public string playerOnMap = "player";
        [HideInInspector]
        public const string CollectItem = "collectItem";
        [HideInInspector]
        public const string EnemyTag = "enemy";
        [HideInInspector]
        public string npc = "Npc";

        public int seed;
        private bool[,] pathMap;
        public int mapWidth;
        public int mapHeight;
        private List<Vector2Int> pathCells;
        private Vector2Int startCell, exitCell;
        private System.Random seedrng;

        public event System.Action FloorLoaded;
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
            if (!HasWalkablePath())
                Debug.LogError("ทางตัน! seed=" + seed + " floor=" + floor + " เดินจากจุดเริ่มถึงทางออกไม่ได้");

        }

        private void Awake()
        {
            CreateMap();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Equals)) LoadCheckpoint();
        }
        void Start()
        {
            StartCoroutine(SetUPMap());
        }

        private void CreateMap()
        {
            ResetLayers();
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
                    }
                }
            }
        }

        public OOPEnemy[] GetEnemies()
        {
            return EnemysOnMap.ToArray();
        }
        public void MoveEnemies()
        {
            foreach (var enemy in EnemysOnMap.ToArray())
            {
                enemy.RandomMove();
            }
        }
        public void RandomDamageToListEnemies()
        {
            Debug.Log($"Damage to {EnemysOnMap.Count} EnemysOnMap");
            foreach (var enemy in EnemysOnMap.ToArray())
            {
                int damage = Random.Range(1, 15);
                enemy.TakeDamage(damage);
            }
        }
        
        private void CreateGrid(int floor)
        {
            mapWidth = X;
            mapHeight = Y;
            if (mapdata == null) ResetLayers();
            pathMap = new bool[mapWidth, mapHeight];
            pathCells = new List<Vector2Int>();
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
            if (FloorLoaded != null) FloorLoaded();
        }

        private void ClearMap()
        {
            foreach (GameObject go in spawnedObjects)
                if (go != null) Destroy(go);
            spawnedObjects.Clear();
            allEnemies.Clear();
            EnemysOnMap.Clear();
            generatedObjects.Clear();
            ResetLayers();
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
    }
}