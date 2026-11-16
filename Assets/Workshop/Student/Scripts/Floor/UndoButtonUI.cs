using UnityEngine;
using UnityEngine.UI;

namespace Solution
{
    public class UndoButtonUI : MonoBehaviour
    {
        [SerializeField] private Button button;
        public OOPMapGenerator map;

        private void Start()
        {
            button.onClick.AddListener(OnClick);
        }

        private void Update()
        {
            Refresh();
        }

        public void OnClick()
        {
            map.history.Undo();
        }

        // ปิดปุ่มเมื่อกองว่าง หรือผู้เล่นกำลัง auto-move
        public void Refresh()
        {
            button.interactable = map.history.CanUndo();
        }
    }
}