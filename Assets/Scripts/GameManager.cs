using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private int selectedCharacterID = -1;
    private string selectedAction = "";
    private string selectedEnvironment;
    public GameObject[] characterPrefabs; // 在 Inspector 拖进去


    public GameObject actionButtonsContainer;

    private void Awake()
    {
        // 保证唯一实例
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 可选：让 GameManager 场景切换不销毁
        DontDestroyOnLoad(gameObject);
    }

    // 角色选择
    public void SelectCharacter(int id)
    {
        selectedCharacterID = id;
        Debug.Log("Selected Character ID: " + id);

        // 显示动作按钮
        if (actionButtonsContainer != null)
            actionButtonsContainer.SetActive(true);
    }

    // 动作选择
    public void SelectAction(string actionName)
    {
        selectedAction = actionName;
        Debug.Log("Selected Action: " + actionName);

        // TODO: 可以在这里加入逻辑，比如播放角色动画，或者进入下一阶段
    }

    public void SelectEnvironment(string envName)
    {
        selectedEnvironment = envName;
        Debug.Log("Selected environment: " + envName);
        // 这里可以做更多，比如UI更新，或者存储状态供后续场景加载用
    }


    // 可选：获取当前选择
    public int GetSelectedCharacterID() => selectedCharacterID;
    public string GetSelectedAction() => selectedAction;
    public string GetSelectedEnvironment() => selectedEnvironment;


    public void StartGame()
    {
        if (string.IsNullOrEmpty(selectedEnvironment))
        {
            Debug.LogError("No environment selected!");
            return;
        }

        Debug.Log("Loading scene: " + selectedEnvironment);
        SceneManager.LoadScene(selectedEnvironment);
    }
}
