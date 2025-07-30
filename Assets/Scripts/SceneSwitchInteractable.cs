using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class SceneSwitchInteractable : MonoBehaviour
{
    public string targetSceneName = "TargetScene"; // 设置你想跳转的场景名

    private void OnEnable()
    {
        var interactable = GetComponent<XRBaseInteractable>();
        interactable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDisable()
    {
        var interactable = GetComponent<XRBaseInteractable>();
        interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log("Clicked! Loading scene: " + targetSceneName);
        SceneManager.LoadScene(targetSceneName);
    }

    [ContextMenu("立即切换到目标场景")]
    private void TestLoadScene()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Debug.LogWarning("只能在运行时使用场景切换！");
            return;
        }
#endif
        Debug.Log("[ContextMenu] 手动切换到场景: " + targetSceneName);
        SceneManager.LoadScene(targetSceneName);
    }
}
