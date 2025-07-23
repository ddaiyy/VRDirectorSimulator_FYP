using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DragSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn;

    private XRBaseInteractor currentInteractor;
    private GameObject spawnedObject;

    [ContextMenu("Test OnGrab")]
    public void TestOnGrab()
    {
        // 构造一个假的 XRBaseInteractor，或传 null 测试
        Debug.Log("Running TestOnGrab()");
        OnGrab(null);
    }


    [ContextMenu("Test OnRelease")]
    public void TestOnRelease()
    {
        Debug.Log("Running TestOnRelease()");
        OnRelease();
    }

    public void OnGrab(XRBaseInteractor interactor)
    {
        if (prefabToSpawn == null)
        {
            Debug.LogError("❌ prefabToSpawn is not assigned.");
            return;
        }

        // 1️⃣ 如果是测试模式（interactor 为 null），只生成对象
        if (interactor == null)
        {
            Debug.Log("⚠️ No interactor passed in — Test mode.");
            spawnedObject = Instantiate(prefabToSpawn, transform.position + Vector3.forward, Quaternion.identity);
            return;
        }

        // 2️⃣ 正常 XR 交互逻辑
        currentInteractor = interactor;
        spawnedObject = Instantiate(prefabToSpawn, interactor.transform.position, Quaternion.identity);

        interactor.attachTransform.position = spawnedObject.transform.position;
        interactor.attachTransform.rotation = spawnedObject.transform.rotation;

        interactor.StartManualInteraction(spawnedObject.GetComponent<XRGrabInteractable>());
    }

    public void OnRelease()
    {
        currentInteractor = null;
        spawnedObject = null;
    }
}


