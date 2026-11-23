#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Solution
{
    public partial class OOPMapGenerator
    {
        [Header("Gizmos (ดูในหน้า Scene)")]
        public bool showGizmos = true;
        public bool showGrid = true;
        public bool showPath = true;
        public bool showLayers = true;
        public bool showLabels = false;

        private Vector3 GizmoPos(int x, int y) { return new Vector3(x, y, -0.2f); }

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;
            if (showGrid) DrawGridGizmo();
            if (showPath) DrawPathGizmo();
            if (showLayers) DrawLayerGizmo();
        }

        // เส้นตาราง: เห็นได้แม้ยังไม่กด Play (อ่าน X, Y จาก Inspector)
        private void DrawGridGizmo()
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
            for (int x = 0; x <= X; x++)
                Gizmos.DrawLine(new Vector3(x - 0.5f, -0.5f, -0.2f), new Vector3(x - 0.5f, Y - 0.5f, -0.2f));
            for (int y = 0; y <= Y; y++)
                Gizmos.DrawLine(new Vector3(-0.5f, y - 0.5f, -0.2f), new Vector3(X - 0.5f, y - 0.5f, -0.2f));
        }

        // เส้นทางที่ขุด (เหลือง) + จุดเริ่ม (ขาว) + ทางออก (ม่วง)
        private void DrawPathGizmo()
        {
            if (pathCells == null || pathCells.Count == 0) return;

            Gizmos.color = Color.yellow;
            for (int i = 0; i < pathCells.Count; i++)
            {
                Vector3 p = GizmoPos(pathCells[i].x, pathCells[i].y);
                Gizmos.DrawSphere(p, 0.08f);
                if (i > 0) Gizmos.DrawLine(GizmoPos(pathCells[i - 1].x, pathCells[i - 1].y), p);
            }
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(GizmoPos(startCell.x, startCell.y), Vector3.one * 0.9f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(GizmoPos(exitCell.x, exitCell.y), Vector3.one * 0.9f);
        }

        // สีตามเลเยอร์: Obstacle = กล่องแดง, Character = วงกลมฟ้า, Item = ลูกบาศก์เขียว
        private void DrawLayerGizmo()
        {
            foreach (Identity id in AllIdentities())
            {
                if (!id.gameObject.activeInHierarchy) continue;
                Vector3 p = GizmoPos(id.positionX, id.positionY);
                switch (id.Data)
                {
                    case MapData.Obstacle:
                        Gizmos.color = new Color(1f, 0.3f, 0.3f);
                        Gizmos.DrawWireCube(p, Vector3.one * 0.8f); break;
                    case MapData.Character:
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawWireSphere(p, 0.35f); break;
                    case MapData.Item:
                        Gizmos.color = Color.green;
                        Gizmos.DrawCube(p, Vector3.one * 0.3f); break;
                }
                if (showLabels) Handles.Label(p + Vector3.up * 0.4f, GizmoLabel(id));
            }
        }

        private string GizmoLabel(Identity id)
        {
            Obstacle o = id as Obstacle;
            if (o != null) return o.kind.ToString();
            OOPEnemy e = id as OOPEnemy;
            if (e != null) return e.type + " " + e.energy;
            return id.Name;
        }
    }
}
#endif