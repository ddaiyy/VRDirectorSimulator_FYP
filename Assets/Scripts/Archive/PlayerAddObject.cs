using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAddObject : MonoBehaviour
{
    public SceneObjectManager sceneObjectManager;

    public void AddObjectByName(string prefabName, Vector3 pos)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/" + prefabName);
        if (prefab != null)
        {
            GameObject go = Instantiate(prefab, pos, Quaternion.identity);
            sceneObjectManager.RegisterObject(go);
        }
        else
        {
            Debug.LogWarning("�Ҳ���Ԥ���壺" + prefabName);
        }
    }
}
