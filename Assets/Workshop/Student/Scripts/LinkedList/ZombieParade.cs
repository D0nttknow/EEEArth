using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solution
{
    public class ZombieParade : OOPEnemy
    {
        // ใช้ LinkedList ในการจัดการส่วนของงูเพื่อประสิทธิภาพในการเพิ่ม/ลบ
        // ใช้ LinkedList ในการจัดการส่วนของงูเพื่อประสิทธิภาพในการเพิ่ม/ลบ
        private LinkedList<GameObject> Parade = new LinkedList<GameObject>();
        public int SizeParade = 3;
        int timer = 0;
        public GameObject[] bodyPrefab; // Prefab ของส่วนลำตัวงู
        public float moveInterval = 0.5f; // ช่วงเวลาในการเคลื่อนที่ (0.5 วินาที)

        private Vector3 moveDirection;

        public override void SetUP()
        {
            base.SetUP();
            moveDirection = Vector3.up;
            // เริ่ม Coroutine สำหรับการเคลื่อนที่
            positionX = (int)transform.position.x;
            positionX = (int)transform.position.y;
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
        // Coroutine สำหรับการเคลื่อนที่ทีละช่อง
        IEnumerator MoveParade()
        {      
            //0. สร้างหัวงู
            Parade.AddFirst(this.gameObject);
            while (isAlive)
            {
                // 1. ดึงส่วนแรกของงูออกมา
                LinkedListNode<GameObject> firstNode = Parade.First;
                GameObject firstPart = firstNode.Value;

                // 2. ดึงส่วนสุดท้ายของงูออกมา
                LinkedListNode<GameObject> lastNode = Parade.Last;
                GameObject lastPart = lastNode.Value;
             
                // 3. ลบส่วนสุดท้ายออกจาก LinkedList
                Parade.RemoveLast();

                // 5. กำหนดตำแหน่งและทิศทางของส่วนที่ถูกย้ายมาใหม่
                // ให้ไปอยู่ที่ตำแหน่งของส่วนหัวงู (ซึ่งเพิ่งเคลื่อนที่ไปเมื่อครู่)
                int toX = 0;
                int toY = 0;

                bool isCollide = true;
                int countTryToFind = 0;
                while (isCollide == true || countTryToFind>10)
                {
                    moveDirection = RandomizeDirection();
                    toX = (int)(firstPart.transform.position.x + moveDirection.x);
                    toY = (int)(firstPart.transform.position.y + moveDirection.y);
                    countTryToFind++;
                    if (countTryToFind > 10) { 
                        toX = positionX;
                        toY = positionY;
                    }
                    isCollide = IsCollision(toX, toY);
                }

                //6. เคลื่อนที่
                mapGenerator.mapdata[positionX, positionY] = null;
                positionX = toX;
                positionY = toY;
                lastPart.transform.position = new Vector3(positionX, positionY, 0);
                lastPart.GetComponent<SpriteRenderer>().flipX = moveDirection == Vector3.right;
                mapGenerator.mapdata[positionX, positionY] = lastPart.GetComponent<Identity>();


                // 7. เพิ่มส่วนนั้นกลับเข้าไปเป็นส่วนที่สองของ LinkedList
                // (ซึ่งก็คือส่วนแรกของลำตัว)
                Parade.AddFirst(lastNode);
                // รอตามเวลาที่กำหนดก่อนการเคลื่อนที่ครั้งต่อไป
                if (Parade.Count < SizeParade) {
                    timer++;
                    if (timer > 3)
                    {
                        Grow();
                        timer = 0;
                    }
                }
                yield return new WaitForSeconds(moveInterval);
            }
        }
        private bool IsCollision(int x, int y)
        {
            // 4. ตรวจสอบสิ่งกีดขวาง
            if (HasPlacement(x, y))
            {
                return true;
            }
            return false;
        }
        
        // ฟังก์ชันสำหรับเพิ่มส่วนของงู (Grow)
        private void Grow()
        {
            GameObject newPart = Instantiate(bodyPrefab[0]);
            // กำหนดตำแหน่งเริ่มต้นของส่วนใหม่ให้อยู่ที่เดียวกับส่วนสุดท้ายของงู
            GameObject lastPart = Parade.Last.Value;
            newPart.transform.position = lastPart.transform.position;
            mapGenerator.SetUpItem(positionX,positionY, newPart, mapGenerator.enemyParent, mapGenerator.enemy);
            //newPart.transform.rotation = lastPart.transform.rotation;
            // เพิ่มส่วนใหม่เข้าไปใน Linked List
            Parade.AddLast(newPart);
        }

    }
}
