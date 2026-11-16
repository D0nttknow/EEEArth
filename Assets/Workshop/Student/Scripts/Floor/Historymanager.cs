using System.Collections.Generic;
using UnityEngine;

namespace Solution
{
    // จดสถานะก่อนเริ่มตา (BeginTurn) เทียบกับหลังจบตา (EndTurn)
    // ความต่างที่เจอ = รายการ GameAction ของตานั้น แล้วใส่กองไว้ให้ Undo ย้อน
    // ไม่ต้องไปแก้โค้ดทุกจุดที่เปลี่ยนสถานะ แค่ครอบ OOPPlayer.Move() ด้วย BeginTurn/EndTurn
    public class HistoryManager
    {
        private readonly OOPMapGenerator map;
        private readonly Stack<GameAction> undoStack = new Stack<GameAction>();
        private Snapshot before;

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

        public HistoryManager(OOPMapGenerator map)
        {
            this.map = map;
        }

        // เรียก "ก่อน" ผู้เล่นเริ่มเดิน
        public void BeginTurn()
        {
            before = new Snapshot();
            before.inventory = new Dictionary<string, int>(map.player.inventory.inventory);

            for (int x = 0; x < map.mapdata.GetLength(0); x++)
            {
                for (int y = 0; y < map.mapdata.GetLength(1); y++)
                {
                    Identity id = map.mapdata[x, y];
                    if (id == null) continue;

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
        }

        // เรียก "หลัง" ตาจบ (รวมศัตรูเดินแล้ว)
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

        // เรียกตอนโหลดเซฟหรือขึ้นชั้นใหม่
        public void Clear()
        {
            undoStack.Clear();
            before = null;
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