using UnityEngine;

namespace Solution
{
    public class Trap : Obstacle
    {
        public int trapDamage = 2;

        private void Awake()
        {
            kind = ObstacleKind.Trap;
            blocksPath = false;
        }

        public override bool Hit()
        {
            mapGenerator.player.TakeDamage(trapDamage);
            return mapGenerator.player.energy > 0;
        }
    }
}

