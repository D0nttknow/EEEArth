using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solution
{
    public class ZombieParade : OOPEnemy
    {
        private LinkedList<GameObject> Parade = new LinkedList<GameObject>();
        public int SizeParade = 3;
        int timer = 0;
        public GameObject[] bodyPrefab;
        public float moveInterval = 0.5f;

        private Vector3 moveDirection;

        public override void SetUP()
        {
            base.SetUP();
            moveDirection = Vector3.up;
            positionX = (int)transform.position.x;
            positionY = (int)transform.position.y;
            StartCoroutine(MoveParade());
        }
        private Vector3 RandomizeDirection()
        {
            List<Vector3> possibleDirections = new List<Vector3>
            {
                Vector3.up,
                Vector3.down,
                Vector3.left,
                Vector3.right
            };

            return possibleDirections[Random.Range(0, possibleDirections.Count)];
        }
        IEnumerator MoveParade()
        {
            Parade.AddFirst(this.gameObject);
            while (isAlive)
            {
                GameObject firstPart = Parade.First.Value;
                LinkedListNode<GameObject> lastNode = Parade.Last;
                GameObject lastPart = lastNode.Value;
                Identity headId = firstPart.GetComponent<Identity>();
                Identity tailId = lastPart.GetComponent<Identity>();

                // หาช่องข้างหัวที่ไม่ชน ลองสูงสุด 10 ครั้ง
                bool found = false;
                int toX = 0, toY = 0;
                for (int i = 0; i < 10 && !found; i++)
                {
                    moveDirection = RandomizeDirection();
                    toX = headId.positionX + (int)moveDirection.x;
                    toY = headId.positionY + (int)moveDirection.y;
                    found = !IsCollision(toX, toY);
                }

                if (found)
                {
                    Parade.RemoveLast();
                    mapGenerator.MoveIdentityTo(tailId, toX, toY);
                    lastPart.GetComponent<SpriteRenderer>().flipX = moveDirection == Vector3.right;
                    Parade.AddFirst(lastNode);
                }

                if (Parade.Count < SizeParade)
                {
                    timer++;
                    if (timer > 3) { Grow(); timer = 0; }
                }
                yield return new WaitForSeconds(moveInterval);
            }
        }
        private bool IsCollision(int x, int y)
        {
            if (HasPlacement(x, y))
            {
                return true;
            }
            return false;
        }
        
        private void Grow()
        {
            GameObject newPart = Instantiate(bodyPrefab[0]);
            GameObject lastPart = Parade.Last.Value;
            Identity lastId = lastPart.GetComponent<Identity>();
            newPart.transform.position = lastPart.transform.position;
            mapGenerator.SetUpItem(lastId.positionX, lastId.positionY, newPart, mapGenerator.enemyParent, OOPMapGenerator.EnemyTag);
            Parade.AddLast(newPart);
        }

    }
}
