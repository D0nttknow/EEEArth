using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum EnemyType { Wanderer, Stationary, Chaser }
namespace Solution
{

    public class OOPEnemy : Character
    {
        public TMP_Text txtHp;
        public EnemyType type = EnemyType.Wanderer;
        [HideInInspector] public bool isFollower;
        public override void SetUP()
        {
            base.SetUP();
            PrintInfo();
            GetRemainEnergy();
            if (txtHp != null) txtHp.text = energy.ToString();
        }

        public override bool Hit()
        {
            mapGenerator.player.Attack(this);
            this.Attack(mapGenerator.player);
            return false;
        }

        public void Attack(OOPPlayer _player)
        {
            _player.TakeDamage(AttackPoint);
        }
        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            if (txtHp != null) {
                txtHp.text = energy.ToString();
            }
        }

        public override void SetEnergy(int value)
        {
            base.SetEnergy(value);
            if (txtHp != null) txtHp.text = energy.ToString();
        }

        protected override void CheckDead()
        {
            if (energy <= 0)
            {
                mapGenerator.EnemysOnMap.Remove(this);
                Despawn();
            }
        }

        public virtual void RandomMove()
        {
            if (isFollower) return;
            switch (type)
            {
                case EnemyType.Stationary:
                    AttackIfAdjacent();
                    break;
                case EnemyType.Chaser:
                    if (!AttackIfAdjacent()) ChasePlayer();
                    break;
                default:
                    WanderStep();
                    break;
            }
        }

        private void WanderStep()
        {
            int toX = positionX, toY = positionY;
            switch (Random.Range(0, 4))
            {
                case 0: toY += 1; break;
                case 1: toY -= 1; break;
                case 2: toX -= 1; break;
                case 3: toX += 1; break;
            }
            if (!HasPlacement(toX, toY)) UpdatePosition(toX, toY);
        }

        private bool AttackIfAdjacent()
        {
            OOPPlayer pl = mapGenerator.player;
            if (pl == null || !pl.gameObject.activeInHierarchy) return false;

            int dist = Mathf.Abs(pl.positionX - positionX) + Mathf.Abs(pl.positionY - positionY);
            if (dist != 1) return false;

            Attack(pl);
            return true;
        }

        private void ChasePlayer()
        {
            OOPPlayer pl = mapGenerator.player;
            if (pl == null || !pl.gameObject.activeInHierarchy) return;

            int dx = pl.positionX - positionX;
            int dy = pl.positionY - positionY;
            Vector2Int stepX = new Vector2Int(System.Math.Sign(dx), 0);
            Vector2Int stepY = new Vector2Int(0, System.Math.Sign(dy));

            if (Mathf.Abs(dx) >= Mathf.Abs(dy)) { if (!TryStep(stepX)) TryStep(stepY); }
            else { if (!TryStep(stepY)) TryStep(stepX); }
        }

        private bool TryStep(Vector2Int step)
        {
            if (step == Vector2Int.zero) return false;
            int toX = positionX + step.x, toY = positionY + step.y;
            if (HasPlacement(toX, toY)) return false;
            UpdatePosition(toX, toY);
            return true;
        }

    }
}