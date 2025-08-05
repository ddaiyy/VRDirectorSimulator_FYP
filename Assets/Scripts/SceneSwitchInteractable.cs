using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class SceneSwitchInteractable : MonoBehaviour
{
    public string targetSceneName = "TargetScene"; // ����������ת�ĳ�����

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
        Debug.Log("XR �������������л�: " + targetSceneName);
        LoadTargetScene();
    }

    // ���������� UI Button ���
    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("Ŀ�곡����Ϊ�գ��޷����أ�");
            return;
        }

        Debug.Log("���س���: " + targetSceneName);
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
