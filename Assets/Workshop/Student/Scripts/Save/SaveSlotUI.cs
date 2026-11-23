using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Solution
{
    public class SaveSlotUI : MonoBehaviour
    {
        public static SaveSlotUI Instance;

        private enum Mode { Save, Load }

        [Header("Required References")]
        public OOPMapGenerator map;
        public GameObject panel;
        public Button[] slotButtons;
        public Button cancelButton;

        [Header("Optional")]
        public TMP_Text titleLabel;

        private Mode mode = Mode.Save;
        private CheckpointData pending;

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
                int slot = i + 1;
                slotButtons[i].onClick.AddListener(() => OnSlotClicked(slot));
            }

            if (cancelButton != null)
                cancelButton.onClick.AddListener(Close);
        }

        public void Open(CheckpointData data)
        {
            if (IsOpen) return;
            mode = Mode.Save;
            pending = data;
            Show();
        }

  
        public void OpenForLoad()
        {
            if (IsOpen) return;
            mode = Mode.Load;
            pending = null;
            Show();
        }

        private void Show()
        {
            if (titleLabel != null)
                titleLabel.text = mode == Mode.Save ? "Select Save Slot" : "Select Slot to Load";

            RefreshSlots();
            panel.SetActive(true);


            if (map != null && map.player != null)
                map.player.enabled = false;
        }

        private void RefreshSlots()
        {
            for (int i = 0; i < slotButtons.Length; i++)
            {
                int slot = i + 1;

                CheckpointData d = null;
                try
                {
                    if (SaveManager.HasSave(slot)) d = SaveManager.Load(slot);
                }
                catch (System.Exception)
                {
                    d = null;
                }

                slotButtons[i].interactable = (mode == Mode.Save) || (d != null);

                TMP_Text label = slotButtons[i].GetComponentInChildren<TMP_Text>();
                if (label == null) continue;

                label.text = d == null
                    ? "Slot " + slot + " - Empty"
                    : "Slot " + slot + " - " + d.saveName + "  (" + d.saveTime + ")";
            }
        }

        private void OnSlotClicked(int slot)
        {
            if (mode == Mode.Save)
            {
                if (pending == null) return;

                SaveManager.currentSlot = slot;
                SaveManager.Save(slot, pending);
                Close();
            }
            else
            {
                Close();
                map.LoadCheckpoint(slot);
            }
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