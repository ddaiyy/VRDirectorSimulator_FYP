using System.Linq;
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
            Debug.LogWarning("originalPrefab 未设置！");
            return;
        }
        Quaternion rotation = Quaternion.Euler(0, 180f, 0);  // Y轴旋转180度
        GameObject newCharacter = Instantiate(originalPrefab, position, rotation);
        TimelineTrack tempTimelineTrack = newCharacter.GetComponent<TimelineTrack>();
        // 注册到场景对象管理器用于保存
        if (tempTimelineTrack)
        {
            newCharacter.name = GetCharacterNameWithIndex(originalPrefab.name);
        }
        SceneObjectManager.Instance?.RegisterObject(newCharacter);


        
        
        Debug.Log("✅ 在 " + position + " 生成角色: " + newCharacter.name);
        //注册Timeline
        TimelineManager.Instance.RegisterTrack(newCharacter.GetComponent<TimelineTrack>());
        // 添加 XRGrabInteractable
        if (newCharacter.GetComponent<XRGrabInteractable>() == null)
            newCharacter.AddComponent<XRGrabInteractable>();

        // 添加 Rigidbody
        Rigidbody rb = newCharacter.GetComponent<Rigidbody>();
        if (rb == null)
            rb = newCharacter.AddComponent<Rigidbody>();
        rb.isKinematic = false;

        // **重新绑定 Animator，刷新姿态**
        Animator animator = newCharacter.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        // 如果在 VR 中就立即被抓取
        if (interactor != null)
        {
            interactor.interactionManager.SelectEnter(interactor, newCharacter.GetComponent<IXRSelectInteractable>());
        }
    }

    private string GetCharacterNameWithIndex(string originalPrefabName)
    {
        var allObjects = GameObject.FindObjectsOfType<Transform>(true)
            .Select(t => t.gameObject)
            .Where(go => go.name.StartsWith(originalPrefabName))
            .ToList();
        int maxIndex = 0;

        foreach (var obj in allObjects)
        {
            string suffix = obj.name.Substring(originalPrefabName.Length); // 去掉前缀
            if (int.TryParse(suffix, out int index))
            {
                if (index > maxIndex)
                    maxIndex = index;
            }
        }

        int nextIndex = maxIndex + 1;
        return originalPrefabName + nextIndex;
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




