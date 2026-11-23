using System;
using System.Collections.Generic;
using UnityEngine;

namespace Solution
{
    [Serializable]
    public class ActionRecord
    {
        public string type;
        public string target;
        public int x;
        public int y;
        public int value;
        public List<ItemEntry> inventory = new List<ItemEntry>();
    }


    [Serializable]
    public class TurnRecord
    {
        public List<ActionRecord> actions = new List<ActionRecord>();
    }


    public abstract class GameAction
    {
        public abstract void Undo();

        public abstract ActionRecord ToRecord(OOPMapGenerator m);
    }

    public class MoveAction : GameAction
    {
        private readonly OOPMapGenerator map;
        private readonly Identity who;
        private readonly int fromX, fromY;

        public MoveAction(OOPMapGenerator map, Identity who, int fromX, int fromY)
        {
            this.map = map; this.who = who; this.fromX = fromX; this.fromY = fromY;
        }

        public override void Undo()
        {
            map.MoveIdentityTo(who, fromX, fromY);
        }

        public override ActionRecord ToRecord(OOPMapGenerator m)
        {
            string id = m.GetId(who);
            if (string.IsNullOrEmpty(id)) return null;
            return new ActionRecord { type = "Move", target = id, x = fromX, y = fromY };
        }
    }

    public class PickUpAction : GameAction
    {
        private readonly OOPMapGenerator map;
        private readonly Identity obj;
        private readonly int x, y;

        public PickUpAction(OOPMapGenerator map, Identity obj, int x, int y)
        {
            this.map = map; this.obj = obj; this.x = x; this.y = y;
        }

        public override void Undo()
        {
            if (obj == null) { Debug.LogWarning("Undo ไม่ได้: วัตถุถูก Destroy ไปแล้ว ให้เปลี่ยนเป็น Despawn()"); return; }
            obj.gameObject.SetActive(true);
            map.Register(obj, x, y);
        }

        public override ActionRecord ToRecord(OOPMapGenerator m)
        {
            string id = m.GetId(obj);
            if (string.IsNullOrEmpty(id)) return null;
            return new ActionRecord { type = "PickUp", target = id, x = x, y = y };
        }
    }

    public class KillEnemyAction : GameAction
    {
        private readonly OOPMapGenerator map;
        private readonly OOPEnemy enemy;
        private readonly int x, y, energyBefore;

        public KillEnemyAction(OOPMapGenerator map, OOPEnemy enemy, int x, int y, int energyBefore)
        {
            this.map = map; this.enemy = enemy; this.x = x; this.y = y; this.energyBefore = energyBefore;
        }

        public override void Undo()
        {
            if (enemy == null) { Debug.LogWarning("Undo ไม่ได้: ศัตรูถูก Destroy ไปแล้ว ให้เปลี่ยนเป็น Despawn()"); return; }
            enemy.gameObject.SetActive(true);
            map.Register(enemy, x, y);
            if (!map.EnemysOnMap.Contains(enemy)) map.EnemysOnMap.Add(enemy);
            enemy.energy = energyBefore;
            if (enemy.txtHp != null) enemy.txtHp.text = enemy.energy.ToString();
        }

        public override ActionRecord ToRecord(OOPMapGenerator m)
        {
            string id = m.GetId(enemy);
            if (string.IsNullOrEmpty(id)) return null;
            return new ActionRecord { type = "Kill", target = id, x = x, y = y, value = energyBefore };
        }
    }

    public class UseItemAction : GameAction
    {
        private readonly OOPPlayer player;
        private readonly Dictionary<string, int> before;

        public UseItemAction(OOPPlayer player, Dictionary<string, int> before)
        {
            this.player = player;
            this.before = new Dictionary<string, int>(before);
        }

        public override void Undo()
        {
            Dictionary<string, int> inv = player.inventory.inventory;
            inv.Clear();
            foreach (KeyValuePair<string, int> kv in before)
                inv[kv.Key] = kv.Value;
        }

        public override ActionRecord ToRecord(OOPMapGenerator m)
        {
            ActionRecord r = new ActionRecord { type = "Inventory" };
            foreach (KeyValuePair<string, int> kv in before)
                r.inventory.Add(new ItemEntry { itemName = kv.Key, itemAmount = kv.Value });
            return r;
        }
    }

    public class EnergyAction : GameAction
    {
        private readonly Character who;
        private readonly int before;

        public EnergyAction(Character who, int before)
        {
            this.who = who; this.before = before;
        }

        public override void Undo()
        {
            if (who == null) return;
            who.energy = before;
            OOPEnemy e = who as OOPEnemy;
            if (e != null && e.txtHp != null) e.txtHp.text = e.energy.ToString();
        }

        public override ActionRecord ToRecord(OOPMapGenerator m)
        {
            string id = m.GetId(who);
            if (string.IsNullOrEmpty(id)) return null;
            return new ActionRecord { type = "Energy", target = id, value = before };
        }
    }

    public class TurnAction : GameAction
    {
        private readonly List<GameAction> actions = new List<GameAction>();

        public int Count { get { return actions.Count; } }
        public void Add(GameAction a) { actions.Add(a); }
        public override void Undo()
        {
            foreach (GameAction a in actions) a.Undo();
        }

        public override ActionRecord ToRecord(OOPMapGenerator m)
        {
            return null;
        }

        public TurnRecord ToTurnRecord(OOPMapGenerator m)
        {
            TurnRecord tr = new TurnRecord();
            foreach (GameAction a in actions)
            {
                ActionRecord r = a.ToRecord(m);
                if (r != null) tr.actions.Add(r);
                else Debug.LogWarning("เซฟลงไฟล์ไม่ได้ (หารหัสวัตถุไม่เจอ) Undo ของตานั้นอาจไม่ครบ");
            }
            return tr;
        }

        public static TurnAction FromRecord(OOPMapGenerator m, OOPPlayer player, TurnRecord tr)
        {
            TurnAction t = new TurnAction();
            foreach (ActionRecord r in tr.actions)
            {
                GameAction a = ActionFactory.FromRecord(m, player, r);
                if (a != null) t.Add(a);
            }
            return t;
        }
    }

    public static class ActionFactory
    {
        public static GameAction FromRecord(OOPMapGenerator m, OOPPlayer player, ActionRecord r)
        {
            Identity target = string.IsNullOrEmpty(r.target) ? null : m.FindById(r.target);

            switch (r.type)
            {
                case "Move":
                    if (target == null) return null;
                    return new MoveAction(m, target, r.x, r.y);

                case "PickUp":
                    if (target == null) return null;
                    return new PickUpAction(m, target, r.x, r.y);

                case "Kill":
                    OOPEnemy enemy = target as OOPEnemy;
                    if (enemy == null) return null;
                    return new KillEnemyAction(m, enemy, r.x, r.y, r.value);

                case "Energy":
                    Character c = target as Character;
                    if (c == null) return null;
                    return new EnergyAction(c, r.value);

                case "Inventory":
                    Dictionary<string, int> inv = new Dictionary<string, int>();
                    foreach (ItemEntry it in r.inventory) inv[it.itemName] = it.itemAmount;
                    return new UseItemAction(player, inv);
            }
            return null;
        }
    }
}