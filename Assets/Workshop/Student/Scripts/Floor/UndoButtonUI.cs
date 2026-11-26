using UnityEngine;
using UnityEngine.UI;

namespace Solution
{
    public class UndoButtonUI : MonoBehaviour
    {
        [SerializeField] private Button button;

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
            if (HistoryManager.Instance != null) HistoryManager.Instance.Undo();
        }

        public void Refresh()
        {
            button.interactable = HistoryManager.Instance != null && HistoryManager.Instance.CanUndo();
        }
    }
}