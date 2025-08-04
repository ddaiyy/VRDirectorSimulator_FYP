using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CameraRecorder : MonoBehaviour
{
    public RenderTexture previewRenderTexture;
    public int width = 1920;
    public int height = 1080;
    public int frameRate = 10;
    public int recordLength = 100; // 录制帧数

    private Texture2D screenShot;
    private int frameCount;
    private bool isRecording = false;

    // 外部公共目录（DCIM/Recordings）
    //private string recordingsPath = "/sdcard/Oculus/VideoShots";
    private string recordingsPath = "/storage/emulated/0/DCIM/Recordings";
    //private string recordingsPath = "/storage/emulated/0/Recordings";


    void Start()
    {
        screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        Time.captureFramerate = frameRate;
        // 创建目录
        if (!Directory.Exists(recordingsPath))
        {
            Directory.CreateDirectory(recordingsPath);
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageWrite))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageWrite);
        }
#endif
    }

    void LateUpdate()
    {
        if (!isRecording) return;

        RenderTexture.active = previewRenderTexture;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        string filePath = $"{recordingsPath}/frame_{frameCount:D04}.png";
        File.WriteAllBytes(filePath, screenShot.EncodeToPNG());
        Debug.Log("保存图片: " + filePath);
        #if UNITY_ANDROID && !UNITY_EDITOR
            RefreshMediaFile(filePath);
        #endif
        frameCount++;

        if (frameCount >= recordLength)
        {
            StopRecording();
        }
    }

    [ContextMenu("开始录制")]
    public void StartRecording()
    {
        // 清空 Recordings 文件夹
        if (Directory.Exists(recordingsPath))
        {
            var files = Directory.GetFiles(recordingsPath);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
        else
        {
            Directory.CreateDirectory(recordingsPath);
        }

        frameCount = 0;
        isRecording = true;
        Debug.Log("[CameraRecorder]:开始录制！");
    }

    public void StopRecording()
    {
        isRecording = false;
        Debug.Log("[CameraRecorder]:录制结束！");
        StartCoroutine(CombineImagesToMp4());
    }

    private IEnumerator CombineImagesToMp4()
    {
        yield return null; // 等待一帧，确保所有图片都写入完成

        string outputPath = recordingsPath + "/output.mp4";
        //string ffmpegCmd =
        //    $"-framerate {frameRate} -i {recordingsPath}/frame_%04d.png -c:v libx264 -pix_fmt yuv420p {outputPath}";

        CheckImageFiles();
        string ffmpegCmd = $"-loglevel debug -framerate {frameRate} -i {recordingsPath}/frame_%04d.png -c:v libx264 -pix_fmt yuv420p {outputPath}";
        Debug.Log("开始合成视频: " + ffmpegCmd);

        FFmpegKit.ExecuteAsync(ffmpegCmd);
        Debug.Log("已调用FFmpegKit合成命令：" + ffmpegCmd);
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        RefreshMediaFile(outputPath);
        #endif
    }

    #if UNITY_ANDROID && !UNITY_EDITOR
    public static void RefreshMediaFile(string filePath)
    {
        using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
            {
                using (AndroidJavaClass mediaScanner = new AndroidJavaClass("android.media.MediaScannerConnection"))
                {
                    mediaScanner.CallStatic("scanFile", context, new string[] { filePath }, null, null);
                }
            }
        }
    }
    #endif

    [ContextMenu("检查图片文件")]
    public void CheckImageFiles()
    {
        if (Directory.Exists(recordingsPath))
        {
            string[] files = Directory.GetFiles(recordingsPath, "frame_*.png");
            Debug.Log($"在 {recordingsPath} 找到 {files.Length} 个图片文件：");
            foreach (var file in files)
            {
                Debug.Log(file);
            }
        }
        else
        {
            Debug.LogWarning($"目录不存在: {recordingsPath}");
        }
    }
}