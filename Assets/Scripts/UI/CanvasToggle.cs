using UnityEngine;

public class CanvasToggle : MonoBehaviour
{
    public GameObject canvasPrefab; // 拖入Canvas预制体
    private GameObject spawnedCanvas;

    private bool isCanvasSpawned = false;

    // 这个方法给按钮绑定，点击时切换生成/销毁Canvas
    public void OnButtonClick()
    {
        if (canvasPrefab == null)
        {
            Debug.LogWarning("Canvas prefab 未设置！");
            return;
        }

        isCanvasSpawned = !isCanvasSpawned;

        if (isCanvasSpawned)
        {
            // 生成，使用 prefab 原位置和旋转
            Vector3 prefabPos = canvasPrefab.transform.position;
            Quaternion prefabRot = canvasPrefab.transform.rotation;

            spawnedCanvas = Instantiate(canvasPrefab, prefabPos, prefabRot);
            Debug.Log("Canvas已生成，位置为预制体默认位置");
        }
        else
        {
            if (spawnedCanvas != null)
            {
                Destroy(spawnedCanvas);
                Debug.Log("Canvas已销毁");
            }
        }
    }
}
