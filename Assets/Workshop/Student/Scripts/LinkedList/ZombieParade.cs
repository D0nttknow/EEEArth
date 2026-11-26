using System.Collections.Generic;
using UnityEngine;

namespace Solution
{
    public class ZombieParade : OOPEnemy
    {
        private readonly LinkedList<GameObject> Parade = new LinkedList<GameObject>();
        public int SizeParade = 3;
        public int growEveryTurns = 4;            // เดินครบกี่ก้าวถึงงอกชิ้นใหม่
        public GameObject[] bodyPrefab;

        private int stepsSinceGrow = 0;

        public override void SetUP()
        {
            base.SetUP();
            positionX = (int)transform.position.x;
            positionY = (int)transform.position.y;
            Parade.AddFirst(gameObject);
        }

        // MoveEnemies() เรียกตรงนี้ 1 ครั้งต่อการเดินของผู้เล่น 1 ครั้ง
        public override void RandomMove()
        {
            List<GameObject> parts = ActiveParts();
            if (parts.Count == 0) return;

            Identity head = parts[0].GetComponent<Identity>();

            bool found = false;
            int toX = 0, toY = 0;
            Vector3 dir = Vector3.up;
            for (int i = 0; i < 10 && !found; i++)
            {
                dir = RandomizeDirection();
                toX = head.positionX + (int)dir.x;
                toY = head.positionY + (int)dir.y;
                found = !HasPlacement(toX, toY);
            }
            if (!found) return;                   // ติดหมดทุกด้าน ยืนอยู่กับที่

            // จดตำแหน่งเดิมของทุกชิ้นก่อนขยับ
            int n = parts.Count;
            Vector2Int[] old = new Vector2Int[n];
            for (int i = 0; i < n; i++)
            {
                Identity id = parts[i].GetComponent<Identity>();
                old[i] = new Vector2Int(id.positionX, id.positionY);
            }

            // หัวก้าวไปข้างหน้า ชิ้นที่เหลือเดินไปยืนแทนตำแหน่งเดิมของชิ้นข้างหน้า
            mapGenerator.MoveIdentityTo(head, toX, toY);
            SpriteRenderer sr = parts[0].GetComponent<SpriteRenderer>();
            if (sr != null) sr.flipX = dir == Vector3.right;

            for (int i = 1; i < n; i++)
                mapGenerator.MoveIdentityTo(parts[i].GetComponent<Identity>(), old[i - 1].x, old[i - 1].y);

            // ต่อชิ้นใหม่ที่ช่องท้ายสุดที่เพิ่งว่าง
            if (n < SizeParade)
            {
                stepsSinceGrow++;
                if (stepsSinceGrow >= growEveryTurns)
                {
                    Grow(old[n - 1].x, old[n - 1].y);
                    stepsSinceGrow = 0;
                }
            }
        }

        // เฉพาะชิ้นที่ยังอยู่ (ชิ้นที่ตายยังอยู่ในลิสต์ เผื่อ Undo ฟื้นกลับเข้าที่เดิม)
        private List<GameObject> ActiveParts()
        {
            List<GameObject> list = new List<GameObject>();
            LinkedListNode<GameObject> node = Parade.First;
            while (node != null)
            {
                LinkedListNode<GameObject> next = node.Next;
                if (node.Value == null) Parade.Remove(node);              // ถูก Destroy ไปแล้ว
                else if (node.Value.activeInHierarchy) list.Add(node.Value);
                node = next;
            }
            return list;
        }

        private Vector3 RandomizeDirection()
        {
            Vector3[] dirs = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
            return dirs[Random.Range(0, dirs.Length)];
        }

        private void Grow(int x, int y)
        {
            if (bodyPrefab == null || bodyPrefab.Length == 0) return;

            GameObject newPart = Instantiate(bodyPrefab[0], new Vector3(x, y, 0), Quaternion.identity);
            mapGenerator.TrackSpawned(newPart);       // ให้ ClearMap ลบตอนเปลี่ยนชั้น
            Identity id = mapGenerator.SetUpItem(x, y, newPart, mapGenerator.enemyParent, OOPMapGenerator.EnemyTag);
            if (id == null) return;

            OOPEnemy en = id as OOPEnemy;
            if (en != null) en.isFollower = true;     // ชิ้นส่วนตาม ไม่เดินเอง
            Parade.AddLast(newPart);
        }
    }
}