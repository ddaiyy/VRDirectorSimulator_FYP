using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private int selectedCharacterID = -1;
    private string selectedAction = "";
    private string selectedEnvironment;
    public GameObject[] characterPrefabs; // �� Inspector �Ͻ�ȥ


    public GameObject actionButtonsContainer;

    private void Awake()
    {
        // ��֤Ψһʵ��
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // ��ѡ���� GameManager �����л�������
        DontDestroyOnLoad(gameObject);
    }

    // ��ɫѡ��
    public void SelectCharacter(int id)
    {
        selectedCharacterID = id;
        Debug.Log("Selected Character ID: " + id);

        // ��ʾ������ť
        if (actionButtonsContainer != null)
            actionButtonsContainer.SetActive(true);
    }

    // ����ѡ��
    public void SelectAction(string actionName)
    {
        selectedAction = actionName;
        Debug.Log("Selected Action: " + actionName);

        // TODO: ��������������߼������粥�Ž�ɫ���������߽�����һ�׶�
    }

    public void SelectEnvironment(string envName)
    {
        selectedEnvironment = envName;
        Debug.Log("Selected environment: " + envName);
        // ������������࣬����UI���£����ߴ洢״̬����������������
    }


    // ��ѡ����ȡ��ǰѡ��
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
