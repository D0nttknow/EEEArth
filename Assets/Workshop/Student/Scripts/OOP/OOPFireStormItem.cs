using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solution
{

    public class OOPFireStormItem : Identity
    {
        public override bool Hit()
        {
            mapGenerator.player.inventory.AddItem("FireStorm",1);
            mapGenerator.RemoveFromMap(this);
            Despawn();
            return true;
        }
    }

}