using System.Collections.Generic;
using UnityEngine;

namespace Solution
{
    public partial class OOPMapGenerator
    {
        private bool[,] BuildBlocked()
        {
            bool[,] blocked = new bool[mapWidth, mapHeight];
            for (int x = 0; x < mapWidth; x++)
                for (int y = 0; y < mapHeight; y++)
                    blocked[x, y] = GetAt(MapData.Obstacle, x, y) != null;
            return blocked;
        }

        private void CarvePath()
        {
            CarvePath(BuildBlocked());
        }

        private void CarvePath(bool[,] blocked)
        {
            if (TryCarve(blocked)) return;
            TryCarve(new bool[mapWidth, mapHeight]);
        }

        private bool TryCarve(bool[,] blocked)
        {
            pathMap = new bool[mapWidth, mapHeight];
            pathCells = new List<Vector2Int>();
            bool[,] visited = new bool[mapWidth, mapHeight];

            Stack<Vector2Int> stack = new Stack<Vector2Int>();
            visited[startCell.x, startCell.y] = true;
            stack.Push(startCell);
            bool reached = false;

            while (stack.Count > 0)
            {
                Vector2Int currentCell = stack.Peek();
                if (currentCell == exitCell) { reached = true; break; }

                List<Vector2Int> neighbors = GetUnvisitedNeighbors(currentCell, visited, blocked);

                if (neighbors.Count > 0)
                {
                    Vector2Int next = neighbors[seedrng.Next(neighbors.Count)];

                    if (seedrng.NextDouble() < goalBias)
                    {
                        int bestWay = int.MaxValue;
                        foreach (Vector2Int n in neighbors)
                        {
                            int distanceN = Mathf.Abs(n.x - exitCell.x) + Mathf.Abs(n.y - exitCell.y);
                            if (distanceN < bestWay) { bestWay = distanceN; next = n; }
                        }
                    }

                    visited[next.x, next.y] = true;
                    stack.Push(next);
                }
                else
                {
                    stack.Pop();
                }
            }

            if (!reached) return false;
            pathCells.AddRange(stack);
            pathCells.Reverse();
            foreach (Vector2Int cell in pathCells)
                pathMap[cell.x, cell.y] = true;
            return true;
        }

        private List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell, bool[,] visited, bool[,] blocked)
        {
            List<Vector2Int> result = new List<Vector2Int>();
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (Vector2Int direction in directions)
            {
                Vector2Int n = cell + direction;
                bool inside = n.x >= 0 && n.x < mapWidth && n.y >= 0 && n.y < mapHeight;
                if (!inside) continue;
                if (visited[n.x, n.y]) continue;
                if (blocked[n.x, n.y] && n != exitCell) continue;

                result.Add(n);
            }
            return result;
        }
        public bool HasWalkablePath()
        {
            bool[,] seen = new bool[mapWidth, mapHeight];
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            queue.Enqueue(startCell);
            seen[startCell.x, startCell.y] = true;

            while (queue.Count > 0)
            {
                Vector2Int c = queue.Dequeue();
                if (c == exitCell) return true;

                foreach (Vector2Int d in dirs)
                {
                    Vector2Int n = c + d;
                    if (n.x < 0 || n.x >= mapWidth || n.y < 0 || n.y >= mapHeight) continue;
                    if (seen[n.x, n.y]) continue;

                    // เช็กเฉพาะแผ่น Obstacle: ไอเทมกับตัวละครไม่ขวาง / ทางออกคือเป้าหมาย จึงผ่านได้
                    if (n != exitCell && GetAt(MapData.Obstacle, n.x, n.y) != null) continue;

                    seen[n.x, n.y] = true;
                    queue.Enqueue(n);
                }
            }
            return false;   //ทางตัน
        }
    }   
}
