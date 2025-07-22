using UnityEngine;
using SimpleFileBrowser;
using System.Collections;
using System.Threading.Tasks;
using GLTFast; //  ����Ҫȷ�� GLTFast ����ȷ��װ���� Package Manager ��װ��

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

            // �����첽����ģ�ͣ�ע�����ﲻ����Э��
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

            // ���첽�°� Instantiate ����
            await gltf.InstantiateMainSceneAsync(model.transform);

            Debug.Log("Model loaded successfully.");
        }
        else
        {
            Debug.LogError("Failed to load model.");
        }
    }
}