using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialVideoController : MonoBehaviour
{
    public GameObject tutorialCanvas;        // Tutorial UI Canvas
    public VideoPlayer videoPlayer;          // VideoPlayer ���
    public RenderTexture renderTexture;      // ������ʾ����Ƶ����
    public RawImage rawImage;                // ��ʾ��Ƶ��UI���

    void Start()
    {
        tutorialCanvas.SetActive(false);     // ��ʼ����ʾ
    }

    public void ShowTutorial()
    {
        tutorialCanvas.SetActive(true);
        videoPlayer.targetTexture = renderTexture;
        rawImage.texture = renderTexture;
        videoPlayer.Play();
    }

    public void HideTutorial()
    {
        videoPlayer.Stop();
        tutorialCanvas.SetActive(false);
    }
}

