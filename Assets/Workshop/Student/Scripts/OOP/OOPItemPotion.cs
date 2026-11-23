using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solution
{

    public class OOPItemPotion : Identity
    {
        public int healPoint = 10;
        public bool isBonues;

        private void Start()
        {
            isBonues = Random.Range(0, 100) < 20 ? true : false;
            if (isBonues)
            {
                GetComponent<SpriteRenderer>().color = Color.blue;
            }
        }
        public override bool Hit()
        {
            if (isBonues)
            {
                mapGenerator.player.Heal(healPoint, isBonues);
                Debug.Log("You got " + Name + " Bonues : " + healPoint * 2);
            }
            else
            {
                mapGenerator.player.Heal(healPoint);
                Debug.Log("You got " + Name + " : " + healPoint);
            }


            mapGenerator.RemoveFromMap(this);

            mapGenerator.player.UpdatePosition(positionX, positionY);
            Despawn();
            return true;
        }
    }
}