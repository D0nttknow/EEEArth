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
        public override void SetUP()
        {
            base.SetUP();
            PrintInfo();
            GetRemainEnergy();
            txtHp.text = energy.ToString();
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

        protected override void CheckDead()
        {
            if (energy <= 0)
            {
                mapGenerator.EnemysOnMap.Remove(this);
                Despawn();
            }
        }

        public void RandomMove()
        {
            int toX = positionX;
            int toY = positionY;
            int random = Random.Range(0, 4);
            switch (random)
            {
                case 0:
                    // up
                    toY += 1;
                    break;
                case 1:
                    // down 
                    toY -= 1;
                    break;
                case 2:
                    // left
                    toX -= 1;
                    break;
                case 3:
                    // right
                    toX += 1;
                    break;
            }
            if (!HasPlacement(toX, toY))
            {
                UpdatePosition(toX, toY);
            }
        }
        
    }
}