/*using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RoleSpawnProxy : MonoBehaviour
{
    public GameObject originalPrefab;  // 拖入角色 prefab
    private XRBaseInteractor interactor;

    public void OnGrab(SelectEnterEventArgs args)
    {
        interactor = args.interactorObject as XRBaseInteractor;

        if (interactor != null && originalPrefab != null)
        {
            // 在手的位置生成一个复制体
            GameObject newCharacter = Instantiate(originalPrefab, interactor.transform.position, Quaternion.identity);

            // 添加交互组件
            var grab = newCharacter.GetComponent<XRGrabInteractable>();
            if (grab == null)
                grab = newCharacter.AddComponent<XRGrabInteractable>();

            // 添加刚体，防止穿模
            if (newCharacter.GetComponent<Rigidbody>() == null)
                newCharacter.AddComponent<Rigidbody>();

            // 把新角色交给玩家抓
            interactor.interactionManager.SelectEnter(interactor, grab);
        }
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
        Debug.Log("你需要在运行时使用手柄抓取角色，才能触发复制逻辑。");
    }

#endif
}
*/



using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RoleSpawnProxy : MonoBehaviour
{
    public GameObject originalPrefab;  // 拖入角色 prefab

    public void OnGrab(SelectEnterEventArgs args)
    {
        var interactor = args.interactorObject as XRBaseInteractor;
        if (interactor != null)
        {
            SpawnNewCharacter(interactor.transform.position, interactor);
        }
    }

    private void SpawnNewCharacter(Vector3 position, XRBaseInteractor interactor = null)
    {
        if (originalPrefab == null)
        {
            Debug.LogWarning("❌ originalPrefab 未设置！");
            return;
        }

        GameObject newCharacter = Instantiate(originalPrefab, position, Quaternion.identity);
        Debug.Log("✅ 在 " + position + " 生成角色: " + newCharacter.name);

        // 添加 XRGrabInteractable
        if (newCharacter.GetComponent<XRGrabInteractable>() == null)
            newCharacter.AddComponent<XRGrabInteractable>();

        // 添加 Rigidbody
        Rigidbody rb = newCharacter.GetComponent<Rigidbody>();
        if (rb == null)
            rb = newCharacter.AddComponent<Rigidbody>();
        rb.isKinematic = false;

        // 如果在 VR 中就立即被抓取
        if (interactor != null)
        {
            interactor.interactionManager.SelectEnter(interactor, newCharacter.GetComponent<IXRSelectInteractable>());
        }
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
        SpawnNewCharacter(spawnPos);
    }
#endif
}
