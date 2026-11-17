using System.Collections.Generic;
using UnityEngine;

namespace Solution
{
    // คลาสแม่ของทุกการกระทำที่ย้อนได้
    public abstract class GameAction
    {
        public abstract void Undo();
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
            obj.positionX = x;
            obj.positionY = y;
            map.mapdata[x, y] = obj;
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
            enemy.positionX = x;
            enemy.positionY = y;
            map.mapdata[x, y] = enemy;
            if (!map.EnemysOnMap.Contains(enemy)) map.EnemysOnMap.Add(enemy);
            enemy.energy = energyBefore;
            if (enemy.txtHp != null) enemy.txtHp.text = enemy.energy.ToString();
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
    }
}