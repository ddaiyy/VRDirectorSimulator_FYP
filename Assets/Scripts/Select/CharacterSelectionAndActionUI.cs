using UnityEngine;
using MyGame.Selection;

public class CharacterSelectionAndActionUI : MonoBehaviour, ICustomSelectable
{
    [Header("角色控制器")]
    public CharacterActionController controllerForThisCharacter;
    public GameObject propUI;

    /*[Header("动作选择 Canvas 预制体")]
    public GameObject actionSelectionCanvasPrefab;
    */

    [Header("角色 Transform")]
    public Transform characterTransform;

    private GameObject currentCanvasInstance;
    /*private bool isCanvasVisible = false;*/

    // UI按钮调用此方法：选中角色并显示/隐藏动作Canvas
    public void OnCharacterButtonClicked()
    {
        SelectCharacter();
        //ToggleActionCanvas();
        OnSelect();
        

    }

    // ✅ 设置当前选中角色
    private void SelectCharacter()
    {
        if (controllerForThisCharacter == null)
        {
            Debug.LogWarning("未设置 controllerForThisCharacter！");
            return;
        }

        SelectedCharacterManager.CurrentSelectedCharacter = controllerForThisCharacter;
        Debug.Log($"✅ 选中了角色: {controllerForThisCharacter.gameObject.name}");
    }

    // 🔧 编辑器右键测试
    [ContextMenu("测试：选中角色并切换Canvas")]
    private void TestSelectAndToggleCanvas()
    {
        OnCharacterButtonClicked();
    }
    public void OnSelect()
    {
        SelectCharacter();

        if (propUI != null)
        {
            propUI.SetActive(true);
            Debug.Log("道具被选中，显示 UI");
            TimelineTrack track = gameObject.GetComponent<TimelineTrack>();
            if (track != null)
            {
                Debug.Log("[CharacterSelect]:显示对应UI");
                track.showUI();
            }
        }
        
        if (currentCanvasInstance == null)
        {
            if (propUI == null || characterTransform == null)
            {
                Debug.LogWarning("缺少 Canvas 预制体 或 Character Transform！");
                return;
            }

            currentCanvasInstance = Instantiate(propUI);

            Debug.Log("道具被选中，显示 UI");

            Vector3 offset = new Vector3(2f, 2f, 0);
            currentCanvasInstance.transform.position = characterTransform.position + offset;

            if (Camera.main != null)
            {
                currentCanvasInstance.transform.LookAt(Camera.main.transform);
                currentCanvasInstance.transform.Rotate(0, 180f, 0);
            }

            currentCanvasInstance.transform.SetParent(characterTransform);
            /*isCanvasVisible = true;*/
        }
        else
        {
            Destroy(currentCanvasInstance);
            currentCanvasInstance = null;
            /*isCanvasVisible = false;*/
        }
        
    }
    public void OnDeselect()
    {
        if (propUI != null)
        {
            propUI.SetActive(false);
            Debug.Log("取消选中，隐藏 UI");
        }
    }
}
