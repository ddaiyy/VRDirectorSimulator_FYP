using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  // 如果要切场景

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

        // 这里可以加载游戏主场景
        // SceneManager.LoadScene("GameScene");
    }

    [ContextMenu("Test Select Action")]
    private void TestSelect()
    {
        OnSelected();
    }
}

