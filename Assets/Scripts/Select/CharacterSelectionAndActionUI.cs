using UnityEngine;

public class CharacterSelectionAndActionUI : MonoBehaviour
{
    [Header("角色控制器")]
    public CharacterActionController controllerForThisCharacter;

    [Header("动作选择 Canvas 预制体")]
    public GameObject actionSelectionCanvasPrefab;

    [Header("角色 Transform")]
    public Transform characterTransform;

    private GameObject currentCanvasInstance;
    private bool isCanvasVisible = false;

    // 👉 UI按钮调用此方法：选中角色并显示/隐藏动作Canvas
    public void OnCharacterButtonClicked()
    {
        SelectCharacter();
        ToggleActionCanvas();
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

    // ✅ 显示/隐藏Canvas
    private void ToggleActionCanvas()
    {
        if (currentCanvasInstance == null)
        {
            if (actionSelectionCanvasPrefab == null || characterTransform == null)
            {
                Debug.LogWarning("❌ 缺少 Canvas 预制体 或 Character Transform！");
                return;
            }

            currentCanvasInstance = Instantiate(actionSelectionCanvasPrefab);

            Vector3 offset = new Vector3(2f, 2f, 0);
            currentCanvasInstance.transform.position = characterTransform.position + offset;

            if (Camera.main != null)
            {
                currentCanvasInstance.transform.LookAt(Camera.main.transform);
                currentCanvasInstance.transform.Rotate(0, 180f, 0);
            }

            currentCanvasInstance.transform.SetParent(characterTransform);
            isCanvasVisible = true;
        }
        else
        {
            Destroy(currentCanvasInstance);
            currentCanvasInstance = null;
            isCanvasVisible = false;
        }
    }

    // 🔧 编辑器右键测试
    [ContextMenu("测试：选中角色并切换Canvas")]
    private void TestSelectAndToggleCanvas()
    {
        OnCharacterButtonClicked();
    }
}
