using UnityEngine;

namespace Solution
{
    public enum ObstacleKind { Solid, Ice, Trap, Checkpoint }

    public class Obstacle : Identity
    {
        [Header("Obstacle")]
        public ObstacleKind kind = ObstacleKind.Solid;
        public bool blocksPath = true;
        public override MapData Data { get { return MapData.Obstacle; } }
    }
}