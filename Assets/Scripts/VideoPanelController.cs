using UnityEngine;
using UnityEngine.UI;

public class VideoPanelController : MonoBehaviour
{
    public GameObject[] videoPanels;
    public Button nextButton;
    public Button prevButton;
    public Button closeButton;

    private int currentIndex = 0;

    void Start()
    {
        // 初始化：只显示第一个视频
        for (int i = 0; i < videoPanels.Length; i++)
        {
            videoPanels[i].SetActive(i == 0);
        }

        prevButton.onClick.AddListener(ShowPrev);
        nextButton.onClick.AddListener(ShowNext);
        closeButton.onClick.AddListener(ClosePanel);

        closeButton.interactable = false; // 初始不可点击
    }

    void ShowNext()
    {
        if (currentIndex < videoPanels.Length - 1)
        {
            videoPanels[currentIndex].SetActive(false);
            currentIndex++;
            videoPanels[currentIndex].SetActive(true);

            // 如果已经是最后一页，就让 Close 按钮可用
            if (currentIndex == videoPanels.Length - 1)
            {
                closeButton.interactable = true;
            }
        }
    }

    void ShowPrev()
    {
        if (currentIndex > 0)
        {
            videoPanels[currentIndex].SetActive(false);
            currentIndex--;
            videoPanels[currentIndex].SetActive(true);

            // 只要不是最后一页，Close 按钮就禁用
            if (currentIndex != videoPanels.Length - 1)
            {
                closeButton.interactable = false;
            }
        }
    }

    void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
