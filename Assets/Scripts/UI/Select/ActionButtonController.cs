using UnityEngine;

public class ActionButtonController : MonoBehaviour
{
    public GameObject actionSelectionCanvasPrefab; // 动作选择Canvas预制体
    private GameObject currentCanvasInstance;

    // 角色Transform，方便定位
    public Transform characterTransform;

    // 正常点击时调用
    public void OnActionButtonClicked()
    {
        ToggleActionCanvas();
    }

    // ⬇️ 右键测试：在 Inspector 中右键组件运行此方法
    [ContextMenu("Test Show")]
    private void TestToggleCanvas()
    {
        ToggleActionCanvas();
    }

    // 公用的 Canvas 显示/隐藏方法
    private void ToggleActionCanvas()
    {
        if (currentCanvasInstance == null)
        {
            if (actionSelectionCanvasPrefab == null || characterTransform == null)
            {
                Debug.LogWarning("缺少Canvas预制体或Character Transform引用！");
                return;
            }

            // 生成Canvas实例
            currentCanvasInstance = Instantiate(actionSelectionCanvasPrefab);

            // 设置Canvas为World Space并放在角色右侧
            Vector3 offset = new Vector3(2f, 2f, 0); // 根据角色大小调整偏移
            currentCanvasInstance.transform.position = characterTransform.position + offset;

            // 让Canvas朝向玩家
            if (Camera.main != null)
            {
                currentCanvasInstance.transform.LookAt(Camera.main.transform);
                currentCanvasInstance.transform.Rotate(0, 180f, 0);
            }

            // 父对象设为角色，保持相对位置
            currentCanvasInstance.transform.SetParent(characterTransform);
        }
        else
        {
            Destroy(currentCanvasInstance);
            currentCanvasInstance = null;
        }
    }
}
