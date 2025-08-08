using UnityEngine;

public class LightSpawner : MonoBehaviour
{
    public GameObject lightPrefab;  // 灯光预制体
    public Transform xrCamera;      // 用户头部摄像机Transform

    public float heightOffset = 5f; // 高度偏移，比如2米

    public void SpawnLightAboveUserHead()
    {
        if (lightPrefab == null || xrCamera == null)
        {
            Debug.LogWarning("请确保 lightPrefab 和 xrCamera 已设置！");
            return;
        }

        Vector3 spawnPos = xrCamera.position + Vector3.up * heightOffset;
        GameObject newLight = Instantiate(lightPrefab, spawnPos, Quaternion.identity);

        SceneObjectManager.Instance.RegisterObject(newLight);
    }
}
