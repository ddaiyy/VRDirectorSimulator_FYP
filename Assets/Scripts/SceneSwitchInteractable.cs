using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class SceneSwitchInteractable : MonoBehaviour
{
    public string targetSceneName = "TargetScene"; // 设置你想跳转的场景名

    private void OnEnable()
    {
        var interactable = GetComponent<XRBaseInteractable>();
        if (interactable != null)
            interactable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDisable()
    {
        var interactable = GetComponent<XRBaseInteractable>();
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log("XR 交互触发场景切换: " + targetSceneName);
        LoadTargetScene();
    }

    // 新增：用于 UI Button 点击
    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("目标场景名为空，无法加载！");
            return;
        }

        Debug.Log("加载场景: " + targetSceneName);
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
