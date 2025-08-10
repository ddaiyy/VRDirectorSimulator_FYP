using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoPanelController : MonoBehaviour
{
    public GameObject[] videoPanels;
    public Button nextButton;
    public Button prevButton;
    public Button closeButton;
    public TMP_Text pageIndicator;

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

        // 初始状态
        closeButton.interactable = false;
        prevButton.interactable = false;  // 第一页不能点上一页
        nextButton.interactable = videoPanels.Length > 1; // 如果只有1页就禁用Next

        UpdatePageIndicator(); // 初始化分页显示
    }

    void ShowNext()
    {
        if (currentIndex < videoPanels.Length - 1)
        {
            videoPanels[currentIndex].SetActive(false);
            currentIndex++;
            videoPanels[currentIndex].SetActive(true);

            UpdateButtonStates();
        }
    }

    void ShowPrev()
    {
        if (currentIndex > 0)
        {
            videoPanels[currentIndex].SetActive(false);
            currentIndex--;
            videoPanels[currentIndex].SetActive(true);

            UpdateButtonStates();
        }
    }

    void UpdateButtonStates()
    {
        // 第一页禁用Prev
        prevButton.interactable = currentIndex > 0;

        // 最后一页禁用Next
        nextButton.interactable = currentIndex < videoPanels.Length - 1;

        // Close按钮：只在最后一页才可用
        closeButton.interactable = currentIndex == videoPanels.Length - 1;

        UpdatePageIndicator();
    }

    void UpdatePageIndicator()
    {
        if (pageIndicator != null)
        {
            pageIndicator.text = $"{currentIndex + 1} / {videoPanels.Length}";
        }
    }

    void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
