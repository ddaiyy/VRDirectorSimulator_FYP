using UnityEngine;
using UnityEngine.UI;

public class ButtonAutoLinker : MonoBehaviour
{
    private void Start()
    {
        // 获取当前 Canvas 下的所有 Button
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });
        }
    }
}
