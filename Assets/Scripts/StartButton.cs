using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  // ���Ҫ�г���

public class StartButton : MonoBehaviour
{
    public void OnSelected()
    {
        Debug.Log("Start button pressed.");
        GameManager.Instance.StartGame();
        Debug.Log("Start game with:");
        Debug.Log("Character: " + GameManager.Instance.GetSelectedCharacterID());
        Debug.Log("Action: " + GameManager.Instance.GetSelectedAction());
        Debug.Log("Environment: " + GameManager.Instance.GetSelectedEnvironment());

        // ������Լ�����Ϸ������
        // SceneManager.LoadScene("GameScene");
    }

    [ContextMenu("Test Select Action")]
    private void TestSelect()
    {
        OnSelected();
    }
}

