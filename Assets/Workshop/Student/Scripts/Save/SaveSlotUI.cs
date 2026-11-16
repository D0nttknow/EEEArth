using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Solution
{
    // หน้าต่างเลือกช่องเซฟ เปิดโดย Checkpoint.Hit()
    // สำคัญ: ติดสคริปต์นี้ไว้กับวัตถุที่ "เปิดอยู่ตลอด" (เช่น ลูกของ Canvas)
    // อย่าติดกับตัว Panel เอง เพราะ Panel ถูกปิดตอนเริ่มเกม และสคริปต์จะไม่ทำงาน
    public class SaveSlotUI : MonoBehaviour
    {
        public static SaveSlotUI Instance;

        [Header("Required References")]
        public OOPMapGenerator map;
        public GameObject panel;           // กรอบหน้าต่างที่เปิด/ปิด
        public Button[] slotButtons;       // ปุ่มช่องเซฟ เรียงช่อง 1 ถึง 4
        public Button cancelButton;        // ปุ่มข้าม (ไม่เซฟ)

        private CheckpointData pending;    // ข้อมูลที่ถ่ายไว้ตอนชน Checkpoint รอผู้เล่นเลือกช่อง

        public bool IsOpen
        {
            get { return panel != null && panel.activeSelf; }
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(this); return; }

            if (panel != null) panel.SetActive(false);
        }

        private void Start()
        {
            if (Instance != this) return;

            for (int i = 0; i < slotButtons.Length; i++)
            {
                int slot = i + 1;   // คัดลอกค่าไว้ ไม่งั้นทุกปุ่มจะได้เลขสุดท้ายเหมือนกัน
                slotButtons[i].onClick.AddListener(() => OnSlotClicked(slot));
            }

            if (cancelButton != null)
                cancelButton.onClick.AddListener(Close);
        }

        // เรียกจาก Checkpoint.Hit() พร้อมข้อมูลที่ถ่ายไว้ ณ ตอนที่ชน
        public void Open(CheckpointData data)
        {
            pending = data;
            RefreshSlots();
            panel.SetActive(true);

            // หยุดรับปุ่มเดินระหว่างเลือกช่อง (ปิด Update() ของผู้เล่น)
            if (map != null && map.player != null)
                map.player.enabled = false;
        }

        // เขียนข้อความบนปุ่มว่าแต่ละช่องว่างหรือมีเซฟอะไรอยู่
        private void RefreshSlots()
        {
            for (int i = 0; i < slotButtons.Length; i++)
            {
                int slot = i + 1;
                TMP_Text label = slotButtons[i].GetComponentInChildren<TMP_Text>();
                if (label == null) continue;

                CheckpointData d = null;
                try
                {
                    if (SaveManager.HasSave(slot)) d = SaveManager.Load(slot);
                }
                catch (System.Exception)
                {
                    d = null;   // ไฟล์เสีย ถือว่าว่าง (เซฟใหม่จะทับ)
                }

                label.text = d == null
                    ? "Slot " + slot + " - Empty"
                    : "Slot " + slot + " - " + d.saveName + "  (" + d.saveTime + ")";
            }
        }

        private void OnSlotClicked(int slot)
        {
            if (pending == null) return;

            SaveManager.currentSlot = slot;      // จำช่องล่าสุดที่ใช้
            SaveManager.Save(slot, pending);     // ถ้าช่องนั้นมีเซฟเดิม จะถูกทับ
            Close();
        }

        public void Close()
        {
            pending = null;
            panel.SetActive(false);

            if (map != null && map.player != null)
                map.player.enabled = true;
        }
    }
}