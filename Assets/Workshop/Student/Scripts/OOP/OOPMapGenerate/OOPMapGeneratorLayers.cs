using System.Collections.Generic;
using UnityEngine;

namespace Solution
{
    public partial class OOPMapGenerator
    {
        private Identity[][,] mapdata;

        private void ResetLayers()
        {
            int count = System.Enum.GetValues(typeof(MapData)).Length;
            mapdata = new Identity[count][,];
            for (int i = 0; i < count; i++)
                mapdata[i] = new Identity[X, Y];
        }

        private Identity[,] LayerOf(MapData data) { return mapdata[(int)data]; }

        public Identity GetAt(MapData Data, int x, int y)
        {
            if (x < 0 || x >= X || y < 0 || y >= Y) return null;
            return LayerOf(Data)[x, y];
        }

        public bool IsCellEmpty(int x, int y)
        {
            if (mapdata == null || x < 0 || x >= X || y < 0 || y >= Y) return false;
            for (int i = 0; i < mapdata.Length; i++)
                if (mapdata[i][x, y] != null) return false;
            return true;
        }

        public Identity GetMapData(int x, int y)
        {
            if (x >= X || x < 0 || y >= Y || y < 0) return Wall;
            Identity found = mapdata[(int)MapData.Character][x, y];
            if (found != null) return found;
            found = mapdata[(int)MapData.Obstacle][x, y];
            if (found != null) return found;
            return mapdata[(int)MapData.Item][x, y];
        }

        public void Register(Identity obj, int x, int y)
        {
            Identity old = LayerOf(obj.Data)[x, y];
            if (old != null && old != obj)
                Debug.LogWarning("เลเยอร์ " + obj.Data + " ช่อง " + x + "," + y + " มี " + old.name + " อยู่แล้ว ถูกทับด้วย " + obj.name);

            LayerOf(obj.Data)[x, y] = obj;
            obj.positionX = x;
            obj.positionY = y;
            obj.mapGenerator = this;
        }

        public void RemoveFromMap(Identity obj)
        {
            Identity[,] layer = LayerOf(obj.Data);
            if (layer[obj.positionX, obj.positionY] == obj)
                layer[obj.positionX, obj.positionY] = null;
        }

        public void MoveIdentityTo(Identity obj, int x, int y)
        {
            if (obj == null) return;
            RemoveFromMap(obj);
            Register(obj, x, y);
            obj.transform.position = new Vector3(x, y, obj.transform.position.z);
        }

        public IEnumerable<Identity> AllIdentities()
        {
            if (mapdata == null) yield break;
            foreach (Identity[,] layer in mapdata)
                foreach (Identity id in layer)
                    if (id != null) yield return id;
        }
    }   
}
