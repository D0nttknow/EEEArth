using UnityEngine;

namespace Solution
{
    public class Checkpoint : Identity
    {
        public override bool Hit()
        {
            CheckpointData data = CheckpointData.Capture(mapGenerator.player, mapGenerator);

            if (SaveSlotUI.Instance != null)
            {
                SaveSlotUI.Instance.Open(data);
            }
            else
            {
                // ไม่มี SaveSlotUI ใน Scene: เซฟลงช่องที่ใช้ล่าสุดเลย
                SaveManager.Save(SaveManager.currentSlot, data);
                Debug.LogWarning("ไม่พบ SaveSlotUI ใน Scene จึงเซฟลงช่อง " + SaveManager.currentSlot);
            }

            return false;
        }
    }
}