using System.Collections.Generic;
using UnityEngine;

namespace Solution
{
    public partial class OOPMapGenerator
    {
        public void LoadCheckpoint()
        {
            LoadCheckpoint(SaveManager.currentSlot);
        }

        public void LoadCheckpoint(int slot)
        {
            CheckpointData d = null;
            try { d = SaveManager.Load(slot); }
            catch (System.Exception e) { Debug.LogError("อ่านเซฟช่อง " + slot + " ไม่ได้: " + e.Message); return; }

            if (d == null) { Debug.Log("ไม่มีไฟล์เซฟช่อง " + slot); return; }

            SaveManager.currentSlot = slot;
            d.Restore(player, this);
        }

        public List<Vector2Int> GetRemovedCells()
        {
            List<Vector2Int> list = new List<Vector2Int>();
            foreach (KeyValuePair<Vector2Int, Identity> kv in generatedObjects)
                if (kv.Value == null || !kv.Value.gameObject.activeInHierarchy)
                    list.Add(kv.Key);
            return list;
        }

        public void ApplyRemovedCells(List<CellPos> cells)
        {
            foreach (CellPos c in cells)
            {
                Identity id;
                if (generatedObjects.TryGetValue(new Vector2Int(c.x, c.y), out id) && id != null)
                    id.Despawn();
            }
        }

        public void ApplyEnemyStates(List<EnemyState> states)
        {
            for (int i = 0; i < allEnemies.Count; i++)
            {
                OOPEnemy e = allEnemies[i];
                if (e != null) RemoveFromMap(e);
            }

            HashSet<int> alive = new HashSet<int>();
            foreach (EnemyState s in states) alive.Add(s.index);
            for (int i = 0; i < allEnemies.Count; i++)
            {
                if (alive.Contains(i) || allEnemies[i] == null) continue;
                EnemysOnMap.Remove(allEnemies[i]);
                allEnemies[i].gameObject.SetActive(false);
            }

            foreach (EnemyState s in states)
            {
                if (s.index < 0 || s.index >= allEnemies.Count) continue;
                OOPEnemy e = allEnemies[s.index];
                if (e == null) continue;
                MoveIdentityTo(e, s.x, s.y);
                e.energy = s.energy;
                if (e.txtHp != null) e.txtHp.text = e.energy.ToString();
            }
        }

        public string GetId(Identity obj)
        {
            if (obj == null) return "";
            if (obj == player) return "P";

            OOPEnemy e = obj as OOPEnemy;
            if (e != null)
            {
                int i = allEnemies.IndexOf(e);
                return i >= 0 ? "E:" + i : "";
            }

            foreach (KeyValuePair<Vector2Int, Identity> kv in generatedObjects)
                if (kv.Value == obj) return "G:" + kv.Key.x + "," + kv.Key.y;

            return "";
        }

        public Identity FindById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (id == "P") return player;

            if (id.StartsWith("E:"))
            {
                int i;
                if (int.TryParse(id.Substring(2), out i) && i >= 0 && i < allEnemies.Count)
                    return allEnemies[i];
                return null;
            }

            if (id.StartsWith("G:"))
            {
                string[] p = id.Substring(2).Split(',');
                int x, y;
                if (p.Length == 2 && int.TryParse(p[0], out x) && int.TryParse(p[1], out y))
                {
                    Identity found;
                    if (generatedObjects.TryGetValue(new Vector2Int(x, y), out found)) return found;
                }
            }
            return null;
        }
    }
}

