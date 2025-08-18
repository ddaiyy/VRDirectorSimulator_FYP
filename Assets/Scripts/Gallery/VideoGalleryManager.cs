using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

public class VideoGalleryManager : MonoBehaviour
{
    public string videoFolderPath = "/sdcard/DCIM/Recordings/";
    public GameObject videoItemPrefab; // 你的新 prefab：含 RawImage + Button
    public Transform contentContainer; // ScrollView/Content

    /*void Start()
    {
        LoadVideoList();
    }*/

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame() ;
        LoadVideoList();
    }

    void LoadVideoList()
    {
        if (!Directory.Exists(videoFolderPath))
        {
            Debug.LogWarning("未找到路径: " + videoFolderPath);
            return;
        }

        string[] videoFiles = Directory.GetFiles(videoFolderPath, "*.mp4");
        foreach (string videoPath in videoFiles)
        {
            GameObject newItem = Instantiate(videoItemPrefab, contentContainer);
            VideoItem videoItem = newItem.GetComponent<VideoItem>();
            if (videoItem != null)
            {
                RenderTexture tex = new RenderTexture(1920, 1080, 0); // 为每个视频准备一张独立贴图
                videoItem.renderTexture = tex;
                videoItem.Setup(videoPath);              
            }
        }
    }
}
