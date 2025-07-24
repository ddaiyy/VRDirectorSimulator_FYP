using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using NativeFilePickerNamespace;

public class VRFilePicker : MonoBehaviour
{
    public void OpenModelPicker()
    {
        string[] types = { ".glb", ".gltf", ".fbx", ".obj" };
        NativeFilePicker.PickFile((path) =>
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("用户取消选择");
                return;
            }
            Debug.Log("选择的模型路径: " + path);
            StartCoroutine(LoadModel(path));
        }, types);
    }

    IEnumerator LoadModel(string path)
    {
        byte[] data = null;
#if UNITY_ANDROID
        using (var uwr = UnityWebRequest.Get(path))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("加载失败: " + uwr.error);
                yield break;
            }
            data = uwr.downloadHandler.data;
        }
#else
        data = System.IO.File.ReadAllBytes(path);
#endif
        // 你可以在这里把 data 传给加载器，比如 GLTFast
        Debug.Log($"加载完成，字节数: {data.Length}");
    }
}