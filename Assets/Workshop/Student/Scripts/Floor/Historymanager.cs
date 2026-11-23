using System.Collections.Generic;
using UnityEngine;

namespace Solution
{
    public class HistoryManager : MonoBehaviour
    {

        public static HistoryManager Instance { get; private set; }
        public OOPMapGenerator map;
        private readonly Stack<GameAction> undoStack = new Stack<GameAction>();
        private Snapshot before;
        private void Awake() { Instance = this; }
        private void OnEnable() { map.FloorLoaded += Clear; }
        private void OnDisable() { map.FloorLoaded -= Clear; }

        private class Entry
        {
            public Identity obj;
            public int x, y;
            public int energy;
        }

        private class Snapshot
        {
            public List<Entry> entries = new List<Entry>();
            public Dictionary<string, int> inventory;
        }

        public void BeginTurn()
        {
            before = new Snapshot();
            before.inventory = new Dictionary<string, int>(map.player.inventory.inventory);

            foreach (Identity id in map.AllIdentities())
            {
                if (id == null || !id.gameObject.activeInHierarchy) continue;

                Character c = id as Character;
                before.entries.Add(new Entry
                {
                    obj = id,
                    x = id.positionX,
                    y = id.positionY,
                    energy = c != null ? c.energy : 0
                });
            }
        }

        public void EndTurn()
        {
            if (before == null) return;

            TurnAction turn = new TurnAction();
            List<GameAction> removed = new List<GameAction>();
            List<GameAction> moved = new List<GameAction>();
            List<GameAction> stats = new List<GameAction>();

            foreach (Entry e in before.entries)
            {
                if (e.obj == null)
                {
                    Debug.LogWarning("Undo กู้ไม่ได้: มีวัตถุถูก Destroy() ให้เปลี่ยนเป็น Despawn()");
                    continue;
                }

                bool gone = !e.obj.gameObject.activeInHierarchy;
                if (gone)
                {
                    OOPEnemy enemy = e.obj as OOPEnemy;
                    if (enemy != null) removed.Add(new KillEnemyAction(map, enemy, e.x, e.y, e.energy));
                    else removed.Add(new PickUpAction(map, e.obj, e.x, e.y));
                    continue;
                }

                if (e.obj.positionX != e.x || e.obj.positionY != e.y)
                    moved.Add(new MoveAction(map, e.obj, e.x, e.y));

                Character c = e.obj as Character;
                if (c != null && c.energy != e.energy)
                    stats.Add(new EnergyAction(c, e.energy));
            }

            foreach (GameAction a in removed) turn.Add(a);
            foreach (GameAction a in moved) turn.Add(a);
            foreach (GameAction a in stats) turn.Add(a);

            if (!SameInventory(before.inventory, map.player.inventory.inventory))
                turn.Add(new UseItemAction(map.player, before.inventory));

            if (turn.Count > 0) Push(turn);
            before = null;
        }

        public void Push(GameAction action)
        {
            undoStack.Push(action);
        }

        public bool CanUndo()
        {
            bool saveUiOpen = SaveSlotUI.Instance != null && SaveSlotUI.Instance.IsOpen;
            return undoStack.Count > 0 && !map.player.isAutoMoving && !saveUiOpen;
        }

        public void Undo()
        {
            if (!CanUndo()) return;
            undoStack.Pop().Undo();
        }

        public void Clear()
        {
            undoStack.Clear();
            before = null;
        }
        [Header("Save")]
        public int maxSavedTurns = 50;

        public List<TurnRecord> Export()
        {
            List<TurnRecord> list = new List<TurnRecord>();

            GameAction[] items = undoStack.ToArray();
            int count = Mathf.Min(items.Length, maxSavedTurns);
            for (int i = count - 1; i >= 0; i--)
            {
                TurnAction t = items[i] as TurnAction;
                if (t == null) continue;

                TurnRecord r = t.ToTurnRecord(map);
                if (r.actions.Count > 0) list.Add(r);
            }
            return list;
        }

        public void Import(List<TurnRecord> records)
        {
            undoStack.Clear();
            before = null;
            if (records == null) return;

            foreach (TurnRecord r in records)
            {
                TurnAction t = TurnAction.FromRecord(map, map.player, r);
                if (t.Count > 0) undoStack.Push(t);
            }
        }
        private static bool SameInventory(Dictionary<string, int> a, Dictionary<string, int> b)
        {
            if (a.Count != b.Count) return false;
            foreach (KeyValuePair<string, int> kv in a)
            {
                int v;
                if (!b.TryGetValue(kv.Key, out v) || v != kv.Value) return false;
            }
            return true;
        }
    }
}