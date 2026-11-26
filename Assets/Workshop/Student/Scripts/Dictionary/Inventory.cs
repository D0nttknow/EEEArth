using System.Collections.Generic;
using UnityEngine;

namespace Solution {
    public class Inventory : MonoBehaviour
    {
        public Dictionary<string, int> inventory = new Dictionary<string, int>();

        // เพิ่มไอเท็ม
        public void AddItem(string item, int amount)
        {
            // 1. ตรวจสอบว่ามีไอเท็มนี้ในคลังแล้วหรือยัง
            if (inventory.ContainsKey(item))
            {
                inventory[item] += amount;
            }
            else
            {
                // ถ้ายังไม่มี ให้เพิ่มไอเท็มใหม่เข้าไปใน Dictionary
                inventory.Add(item, amount);
            }

            Debug.Log("Added " + amount + " " + item + ". Total: " + inventory[item]);
        }

        // ลบไอเท็ม
        public void UseItem(string item, int amount)
        {
            //4. ตรวจสอบว่ามีไอเท็มนี้ในคลังหรือไม่
            if (inventory.ContainsKey(item))
            {
                // ลบจำนวนไอเท็มออก
                inventory[item] -= amount;

                // ถ้าจำนวนไอเท็มเหลือ 0 หรือน้อยกว่า ให้ลบไอเท็มออกจาก Dictionary
                if (inventory[item] <= 0)
                {
                    inventory.Remove(item);
                    Debug.Log("Removed all " + item + " from inventory.");
                }
                else
                {
                    Debug.Log("Removed " + amount + " " + item + ". Remaining: " + inventory[item]);
                }
            }
            else
            {
                Debug.Log("Cannot remove " + item + ". Not found in inventory.");
            }
        }
        public bool HasItem(string item, int amount)
        {
            //2. ตรวจสอบว่ามีไอเท็มนี้ในคลังหรือไม่ และมีจำนวนเพียงพอหรือไม่
            return inventory.ContainsKey(item) && inventory[item] >= amount;
        }
        // ตรวจสอบจำนวนไอเท็ม
        public int GetItemCount(string item)
        {
            //3. ตรวจสอบว่ามีไอเท็มนี้ในคลังหรือไม่ ถ้ามีให้คืนค่าจำนวนไอเท็มนั้น
            if (inventory.ContainsKey(item))
            {
                return inventory[item];
            }
            return 0;
        }

        // แสดงรายการทั้งหมดในคลัง
        public void PrintInventory()
        {
            Debug.Log("--- Inventory Content ---");
            if (inventory.Count == 0)
            {
                Debug.Log("Inventory is empty.");
                return;
            }

            foreach (var itemEntry in inventory)
            {
                Debug.Log(itemEntry.Key + ": " + itemEntry.Value);
            }
        }
    }
}

