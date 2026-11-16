using UnityEngine;

namespace Solution
{
    // Checkpoint ตั้งอยู่ข้างทางเดิน ผ่านไม่ได้ ไม่หายไป
    // ผู้เล่นเดินเข้าใส่ครั้งไหน หน้าต่างเลือกช่องเซฟก็เปิดครั้งนั้น
    public class Checkpoint : Identity
    {
        public override bool Hit()
        {
            // ถ่ายสถานะ ณ ตอนที่ชน
            CheckpointData data = CheckpointData.Capture(mapGenerator.player, mapGenerator);

            if (SaveSlotUI.Instance != null)
            {
                SaveSlotUI.Instance.Open(data);   // เปิดหน้าต่าง รอผู้เล่นเลือกช่องหรือกดข้าม
            }
            else
            {
                // ไม่มี SaveSlotUI ใน Scene: เซฟลงช่องที่ใช้ล่าสุดเลย
                SaveManager.Save(SaveManager.currentSlot, data);
                Debug.LogWarning("ไม่พบ SaveSlotUI ใน Scene จึงเซฟลงช่อง " + SaveManager.currentSlot);
            }

            // false = ผู้เล่นไม่ก้าวเข้าช่องนี้ (เดินผ่านไม่ได้) และ Checkpoint ยังอยู่ที่เดิม
            // ถ้า OOPPlayer.Move ตีความค่าที่คืนต่างจากนี้ ให้ปรับตรงนี้
            return false;
        }
    }
}