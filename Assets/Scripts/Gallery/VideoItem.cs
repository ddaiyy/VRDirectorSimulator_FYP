using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.IO;

public class VideoItem : MonoBehaviour
{
    public RawImage rawImage;
    public Button playButton;
    public TMP_Text videoLabel;
    public RenderTexture renderTexture;

    private string videoPath;
    private VideoPlayer videoPlayer; // 独立播放器

    public void Setup(string path)
    {
        videoPath = path;

        if (videoLabel != null)
            videoLabel.text = Path.GetFileName(videoPath);

        // 使用外部传入的 renderTexture（不要再 new）
        if (renderTexture == null)
            Debug.LogError("RenderTexture 未设置！");

        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoPath;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        if (rawImage != null)
            rawImage.texture = renderTexture;

        playButton.onClick.AddListener(() =>
        {
            videoPlayer.Play();
        });
    }

}

