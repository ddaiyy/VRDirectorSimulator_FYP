using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialVideoController : MonoBehaviour
{
    public GameObject tutorialCanvas;        // Tutorial UI Canvas
    public VideoPlayer videoPlayer;          // VideoPlayer 组件
    public RenderTexture renderTexture;      // 用于显示的视频纹理
    public RawImage rawImage;                // 显示视频的UI组件

    void Start()
    {
        tutorialCanvas.SetActive(false);     // 初始不显示
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

