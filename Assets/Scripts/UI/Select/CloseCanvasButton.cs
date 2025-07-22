using UnityEngine;
using UnityEngine.UI;

public class CloseCanvasButton : MonoBehaviour
{
    public Button closeButton;  // 拖拽你的Close按钮到这里

    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }
        else
        {
            Debug.LogWarning("请给 CloseCanvasButton 脚本绑定 closeButton 按钮！");
        }
    }

    void OnCloseClicked()
    {
        /*// 关闭Canvas，可以Destroy，也可以SetActive(false)，这里用Destroy
        Destroy(gameObject);
        // 如果你想隐藏而不是销毁：*/
        gameObject.SetActive(false);
    }
}
