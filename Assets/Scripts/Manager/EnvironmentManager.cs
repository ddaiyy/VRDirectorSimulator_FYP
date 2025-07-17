using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    //可选环境预制体
    public List<GameObject> environmentPrefabs;
    //环境父节点(摆放位置
    public Transform environmentParent;
    //可选择放置物体预制体
    public List<GameObject> placeableObjectPrefabs;

    //加载指定环境
    public void LoadEnvironment(string environmentName) { /* TODO */ }
    
    //在场景中放置物体
    public void PlaceObject(GameObject objPrefab, Vector3 position) { /* TODO */ }
    
    //移除场景中物体
    public void RemoveObject(GameObject obj) { /* TODO */ }
    
    //获取所有已放置物体
    public List<GameObject> GetPlacedObjects() { /* TODO */ return new List<GameObject>(); }
    
    //清除所有已放置物体
    public void ClearAllObjects() { /* TODO */ }
    
    //保存当前场景配置
    public void SaveSceneConfiguration() { /* TODO */ }
    
    //加载场景配置
    public void LoadSceneConfiguration() { /* TODO */ }
} 