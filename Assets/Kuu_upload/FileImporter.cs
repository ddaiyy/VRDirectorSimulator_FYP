using UnityEngine;
using SimpleFileBrowser;
using System.Collections;
using System.Threading.Tasks;
using GLTFast; //  你需要确保 GLTFast 已正确安装（用 Package Manager 安装）

public class FileImporter : MonoBehaviour
{
    public void OpenFileBrowser()
    {
        StartCoroutine(ShowFileBrowser());
    }

    IEnumerator ShowFileBrowser()
    {
        yield return FileBrowser.WaitForLoadDialog(
            pickMode: FileBrowser.PickMode.Files,
            allowMultiSelection: false,
            initialPath: null,
            title: "Select Model",
            loadButtonText: "Load"
        );

        if (FileBrowser.Success)
        {
            string path = FileBrowser.Result[0];
            Debug.Log("Selected file: " + path);

            // 启动异步加载模型，注意这里不阻塞协程
            _ = LoadModelAsync(path);
        }
    }

    private async Task LoadModelAsync(string path)
    {
        var gltf = new GltfImport();

        bool success = await gltf.Load(path);
        if (success)
        {
            GameObject model = new GameObject("ImportedModel");

            // 用异步新版 Instantiate 方法
            await gltf.InstantiateMainSceneAsync(model.transform);

            Debug.Log("Model loaded successfully.");
        }
        else
        {
            Debug.LogError("Failed to load model.");
        }
    }
}