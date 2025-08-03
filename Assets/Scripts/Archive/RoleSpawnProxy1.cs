using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class RoleSpawnProxy1 : MonoBehaviour
{
    public GameObject originalPrefab;  // 拖入的预制体

    public void OnGrab(SelectEnterEventArgs args)
    {
        var interactor = args.interactorObject as XRBaseInteractor;
        if (interactor != null)
        {
            SpawnNewObject(interactor.transform.position);
        }
    }

    private void SpawnNewObject(Vector3 position)
    {
        if (originalPrefab == null)
        {
            Debug.LogWarning("❌ originalPrefab 未设置！");
            return;
        }

        Quaternion rotation = Quaternion.Euler(0, 180f, 0); // 可选：默认旋转
        GameObject obj = Instantiate(originalPrefab, position, rotation);

        // 注册到场景对象管理器用于保存
        SceneObjectManager.Instance?.RegisterObject(obj);

        Debug.Log("✅ 生成新物体: " + obj.name);
    }

    void OnEnable()
    {
        var grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
            grab.selectEntered.AddListener(OnGrab);
    }

    void OnDisable()
    {
        var grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
            grab.selectEntered.RemoveListener(OnGrab);
    }

#if UNITY_EDITOR
    [ContextMenu("测试：模拟抓取")]
    public void TestGrab()
    {
        // 在面板旁边生成角色
        Vector3 spawnPos = transform.position + Vector3.right * 1.5f;
        SpawnNewObject(spawnPos);
    }
#endif
}
