using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoPanelController : MonoBehaviour
{
    public GameObject[] videoPanels;
    public Button nextButton;
    public Button prevButton;
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

        // 初始状态
        prevButton.gameObject.SetActive(false); // 第一页直接隐藏
        nextButton.interactable = videoPanels.Length > 1;

        UpdatePageIndicator();
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
        // 第1页隐藏 Prev 按钮
        prevButton.gameObject.SetActive(currentIndex > 0);

        // 最后一页隐藏 Next 按钮
        nextButton.gameObject.SetActive(currentIndex < videoPanels.Length - 1);

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
