using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string saveDir => Application.persistentDataPath + "/saves/";


    public static void DeleteAllSaves()
    {
        if (Directory.Exists(saveDir))
        {
            Directory.Delete(saveDir, true);
            Debug.Log("所有存档已清空");
        }
    }

    public static void Save(SaveData data)
    {
        if (!Directory.Exists(saveDir))
            Directory.CreateDirectory(saveDir);

        data.SyncSaveTimeToString();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveDir + data.saveId + ".json", json);
    }

    public static SaveData Load(string saveId)
    {
        string path = saveDir + saveId + ".json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            /*return JsonUtility.FromJson<SaveData>(json);*/
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            data.SyncSaveTimeFromString(); // 👈 添加这一行
            return data;
        }
        return null;
    }

    public static List<SaveData> LoadAll()
    {
        List<SaveData> all = new();
        if (Directory.Exists(saveDir))
        {
            foreach (var file in Directory.GetFiles(saveDir, "*.json"))
            {
                string json = File.ReadAllText(file);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                data.SyncSaveTimeFromString();
                all.Add(data);
            }
        }
        return all;
    }

    public static void Delete(string saveId)
    {
        string path = saveDir + saveId + ".json";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"存档 {saveId} 已删除");
        }
    }

}
