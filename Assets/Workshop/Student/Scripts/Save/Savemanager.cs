using System.IO;
using UnityEngine;

namespace Solution
{
    public static class SaveManager
    {
        public static int currentSlot = 1;

        private static string PathOf(int slot)
        {
            return Path.Combine(Application.persistentDataPath, "checkpoint_" + slot + ".json");
        }

        public static bool HasSave(int slot)
        {
            return File.Exists(PathOf(slot));
        }

        public static void Save(int slot, CheckpointData data)
        {
            File.WriteAllText(PathOf(slot), JsonUtility.ToJson(data, true));
            Debug.Log("Saved slot " + slot + " -> " + PathOf(slot));
        }

        public static CheckpointData Load(int slot)
        {
            if (!HasSave(slot)) return null;
            return JsonUtility.FromJson<CheckpointData>(File.ReadAllText(PathOf(slot)));
        }

        public static void Delete(int slot)
        {
            if (HasSave(slot)) File.Delete(PathOf(slot));
        }
    }
}