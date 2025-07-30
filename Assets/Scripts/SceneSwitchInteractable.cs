using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class SceneSwitchInteractable : MonoBehaviour
{
    public string targetSceneName = "TargetScene"; // ����������ת�ĳ�����

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

    [ContextMenu("�����л���Ŀ�곡��")]
    private void TestLoadScene()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Debug.LogWarning("ֻ��������ʱʹ�ó����л���");
            return;
        }
#endif
        Debug.Log("[ContextMenu] �ֶ��л�������: " + targetSceneName);
        SceneManager.LoadScene(targetSceneName);
    }
}
