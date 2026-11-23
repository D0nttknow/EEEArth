using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solution
{

    public class OOPWall : Identity
    {
        public int Damage;
        public bool IsIceWall;

        public void Setup(int damage, bool isIce)
        {
            Damage = damage;
            IsIceWall = isIce;
            if (IsIceWall)
                GetComponent<SpriteRenderer>().color = Color.blue;
        }
        public override bool Hit()
        {
            if (IsIceWall)
            {
                mapGenerator.player.TakeDamage(Damage, IsIceWall);
            }
            else
            {
                mapGenerator.player.TakeDamage(Damage);
            }
            mapGenerator.RemoveFromMap(this);
            Despawn();
            return false;
        }
    }
}