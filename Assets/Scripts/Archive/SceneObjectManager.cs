using System.Collections.Generic;
using UnityEngine;

public class SceneObjectManager : MonoBehaviour
{
    public static SceneObjectManager Instance { get; private set; }

    public List<GameObject> spawnedObjects = new List<GameObject>();

    private const string prefabPath = "Prefabs/";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /*public void RegisterObject(GameObject go)
    {
        if (!spawnedObjects.Contains(go))
            spawnedObjects.Add(go);
    }*/

    public void RegisterObject(GameObject go)
    {
        if (!spawnedObjects.Contains(go))
            spawnedObjects.Add(go);

        var info = go.GetComponent<SceneObjectInfo>();
        if (info == null)
        {
            info = go.AddComponent<SceneObjectInfo>();
            info.prefabOriginalName = go.name.Replace("(Clone)", "").Trim();
        }
    }


    public void ClearAll()
    {
        foreach (var go in spawnedObjects)
        {
            Destroy(go);
        }
        spawnedObjects.Clear();
    }

    /*public void SaveObjects(SaveData data)
    {
        data.placedObjects.Clear();
        foreach (var go in spawnedObjects)
        {
            if (go == null) continue;

            var sod = new SceneObjectData
            {
                prefabName = go.name.Replace("(Clone)", "").Trim(),
                position = go.transform.position,
                rotation = go.transform.rotation,
                scale = go.transform.localScale
            };
            data.placedObjects.Add(sod);
        }
    }*/

    public void SaveObjects(SaveData data)
    {
        data.placedObjects.Clear();
        foreach (var go in spawnedObjects)
        {
            if (go == null) continue;

            var info = go.GetComponent<SceneObjectInfo>();
            string prefabName = info != null ? info.prefabOriginalName : go.name.Replace("(Clone)", "").Trim();

            var sod = new SceneObjectData
            {
                prefabName = prefabName,
                position = go.transform.position,
                rotation = go.transform.rotation,
                scale = go.transform.localScale
            };
            data.placedObjects.Add(sod);
        }
    }


    public void LoadObjects(SaveData data)
    {
        ClearAll();
        foreach (var sod in data.placedObjects)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabPath + sod.prefabName);
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab, sod.position, sod.rotation);
                go.transform.localScale = sod.scale;
                RegisterObject(go);
            }
            else
            {
                Debug.LogWarning("❌ 加载失败，未找到 prefab：" + sod.prefabName);
            }
        }
    }
}
