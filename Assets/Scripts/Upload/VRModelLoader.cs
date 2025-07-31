using UnityEngine;
using GLTFast;
using System.Threading.Tasks;
using System.IO;
using System;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.Collections;

public class VRModelLoader : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text vrMessageText;

    [Header("Material Options")]
    public bool keepOriginalMaterial = true; // 控制是否保留 glTF 原材质

    void Start()
    {// 一开始隐藏
        if (vrMessageText != null)
            vrMessageText.gameObject.SetActive(false);

        // if (vrMessagePanel != null)
        //    vrMessagePanel.SetActive(false);
    }

    public async void OpenModelPicker()
    {
        Debug.Log("[测试] OpenModelPicker 被调用了");
        string[] types = { ".glb", ".gltf", ".fbx", ".obj", ".zip" };
        NativeFilePicker.PickFile(async (path) =>
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("取消选择");
                return;
            }

            Debug.Log("选择路径: " + path);

            if (path.StartsWith("content://"))
            {
                string copiedPath = CopyFileToPersistentPath(path);
                if (string.IsNullOrEmpty(copiedPath))
                {
                    Debug.LogError("拷贝文件失败，无法加载");
                    return;
                }
                await LoadByExtension(copiedPath);
            }
            else
            {
                await LoadByExtension(path);
            }
        }, null);
    }

    public void ShowVRMessage(string message, float duration = 3f)
    {
        if (vrMessageText == null) return;

        // 激活并设文本
        vrMessageText.gameObject.SetActive(true);
        vrMessageText.text = message;

        StopAllCoroutines();
        StartCoroutine(HideAfterDelay(duration));
    }
    private async Task LoadByExtension(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        if (ext == ".zip")
        {
            await LoadZipModel(path);
        }
        else if (ext == ".glb")
        {
            await LoadModel(path);
        }
        else
        {
            Debug.LogError("❌ Currently only .glb files are supported.");
            ShowVRMessage("Currently only .glb files are supported.", 5f);
        }
    }

    IEnumerator HideAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        vrMessageText.gameObject.SetActive(false);
        // if (vrMessagePanel != null)
        //    vrMessagePanel.SetActive(false);
    }

    public async Task LoadModel(string path)
    {
        path = path.Replace("\\", "/");
        Debug.Log($"[加载开始] 路径: {path}");

        if (!File.Exists(path))
        {
            Debug.LogError("❌ 文件不存在！");
            return;
        }

        GameObject parent = new GameObject("UploadedModel");

        GltfImport gltf = null;
        bool loaded = false;

        try
        {
            string directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory))
            {
                Debug.LogError("❌ 无效的文件路径，无法获取目录");
                Destroy(parent);
                return;
            }
            directory = directory.Replace("\\", "/");
            Debug.Log($"[路径] gltf 文件目录: {directory}");

            if (path.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
            {
                gltf = new GltfImport();
                loaded = await gltf.Load(path);
            }
            else
            {
                Destroy(parent);
                return;
            }

        }
        catch (Exception ex)
        {
            Destroy(parent);
            return;
        }

        if (!loaded)
        {
            Debug.LogError("❌ [失败] GLTF 加载失败！");
            Destroy(parent);
            return;
        }

        bool instantiated = await gltf.InstantiateMainSceneAsync(parent.transform);
        if (!instantiated)
        {
            Debug.LogError("❌ [失败] GLTF 实例化失败！");
            Destroy(parent);
            return;
        }

        // ✅ 文件目录验证
        string modelDir = Path.GetDirectoryName(path)?.Replace("\\", "/");
        Debug.Log($"[验证] 模型目录：{modelDir}");



        // ✅ 自动归一化缩放
        NormalizeModelScale(parent, 1f);

        if (instantiated)
        {
            AddAccurateBoxCollider(parent);
            
            SetupRigidbodyAndGrab(parent);
        }

        //ReplaceShadersToStandard(parent);
        PrintLoadedMaterialsAndTextures(parent);
        ForceAssignWhiteTexture(parent);
        // 在模型正面中部添加抓取锚点
        AddFrontCenterGrabAnchor(parent);


        // ✅ 设置位置
        if (Camera.main != null)
        {
            var cam = Camera.main.transform;
            parent.transform.position = cam.position + cam.forward * 2f;

            // ✅ 让模型朝向用户
            Vector3 lookAtPosition = new Vector3(cam.position.x, parent.transform.position.y, cam.position.z);
            parent.transform.LookAt(lookAtPosition);

        }
        else
        {
            Debug.LogWarning("⚠️ 无主摄像机，模型不会自动定位");
        }

        Debug.Log("✅ 模型加载完成");
        ShowVRMessage("Model loaded successfully.", 5f);

        // ✅ 添加 Canvas 触发组件
        var trigger = parent.AddComponent<VRModelCanvasTrigger>();
        trigger.canvasPrefab = Resources.Load<GameObject>("TriggerCanvas"); // 你的 Canvas 预制体放到 Resources 文件夹



    }


    public async Task LoadZipModel(string zipPath)
    {
        string extractPath = Path.Combine(Application.temporaryCachePath, "unzipped_model");

        if (Directory.Exists(extractPath))
            Directory.Delete(extractPath, true);

        Directory.CreateDirectory(extractPath);

        System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);

        // 查找 glb 或 gltf 文件，优先选择 .glb
        string[] glbFiles = Directory.GetFiles(extractPath, "*.glb", SearchOption.AllDirectories);
        string[] gltfFiles = Directory.GetFiles(extractPath, "*.gltf", SearchOption.AllDirectories);

        string modelPath = null;

        if (glbFiles.Length > 0)
        {
            modelPath = glbFiles[0]; // 优先使用 .glb 文件
        }
        else if (gltfFiles.Length > 0)
        {
            modelPath = gltfFiles[0];
        }

        if (modelPath == null)
        {
            string errorMsg = "❌ No .glb or .gltf file found in the Zip archive.";
            Debug.LogError(errorMsg);
            ShowVRMessage(errorMsg, 5f); // ✅ 在 VR 头显中显示错误提示
            return;
        }

        await LoadModel(modelPath);
    }


    void NormalizeModelScale(GameObject model, float maxSize = 1f)
    {
        var renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers)
            bounds.Encapsulate(r.bounds);

        float maxDimension = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
        if (maxDimension <= 0) return;

        float scaleFactor = maxSize / maxDimension;
        model.transform.localScale *= scaleFactor;
    }

    void AddAccurateBoxCollider(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning("No renderer found, skipping collider.");
            return;
        }

        Bounds worldBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            worldBounds.Encapsulate(renderers[i].bounds);

        BoxCollider boxCollider = go.GetComponent<BoxCollider>();
        if (boxCollider == null)
            boxCollider = go.AddComponent<BoxCollider>();

        Vector3 localCenter = go.transform.InverseTransformPoint(worldBounds.center);
        boxCollider.center = localCenter;

        Vector3 worldSize = worldBounds.size;
        Vector3 localSize = go.transform.InverseTransformVector(worldSize);
        boxCollider.size = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
    }

    void SetupRigidbodyAndGrab(GameObject go)
    {
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;

        var grab = go.GetComponent<XRGrabInteractable>();
        if (grab == null) grab = go.AddComponent<XRGrabInteractable>();

        grab.colliders.Clear();
        var col = go.GetComponent<Collider>();
        if (col != null)
            grab.colliders.Add(col);

        var manager = UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>();
        if (manager != null)
            grab.interactionManager = manager;

        // 重置组件启用状态，防止不响应
        grab.enabled = false;
        grab.enabled = true;
    }

    /*void ReplaceShadersToStandard(GameObject go)
    {
        Shader standardShader = Shader.Find("Standard");
        if (standardShader == null)
        {
            Debug.LogError("❌ 找不到 Standard Shader");
            return;
        }

        foreach (var renderer in go.GetComponentsInChildren<Renderer>())
        {
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                var mat = materials[i];
                if (mat == null) continue;

                string shaderName = mat.shader.name.ToLower();

                // 判断是否是 glTF 材质（保留），否则替换
                if (shaderName.Contains("gltf") || shaderName.Contains("pbrmetallicroughness"))
                {
                    Debug.Log($"✅ 保留 glTF 材质: {mat.name} ({mat.shader.name})");
                }
                else
                {
                    Debug.Log($"🔄 替换非 glTF 材质: {mat.name} ({mat.shader.name}) → Standard");

                    var newMat = new Material(standardShader);

                    // 尝试复制常见贴图属性
                    if (mat.HasProperty("_MainTex"))
                        newMat.SetTexture("_MainTex", mat.GetTexture("_MainTex"));

                    if (mat.HasProperty("_BaseMap")) // 一些非标准材质使用 _BaseMap
                        newMat.SetTexture("_MainTex", mat.GetTexture("_BaseMap")); // 转到 Standard 的 _MainTex

                    if (mat.HasProperty("_Color"))
                        newMat.SetColor("_Color", mat.GetColor("_Color"));

                    // 可选：复制法线贴图
                    if (mat.HasProperty("_BumpMap"))
                        newMat.SetTexture("_BumpMap", mat.GetTexture("_BumpMap"));

                    materials[i] = newMat;
                }
            }
            renderer.sharedMaterials = materials;
        }
    }*/


    void ForceAssignWhiteTexture(GameObject go)
    {
        var whiteTex = Texture2D.whiteTexture;
        foreach (var renderer in go.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.materials)
            {
                if (mat == null) continue;
                if (mat.mainTexture == null)
                {
                    mat.mainTexture = whiteTex;
                }
            }
        }
    }

    void PrintLoadedMaterialsAndTextures(GameObject go)
    {
        foreach (var renderer in go.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.materials)
            {
                if (mat == null) continue;
                Debug.Log($"材质: {mat.name}, Shader: {mat.shader.name}, 贴图: {mat.mainTexture?.name ?? "无"}");
            }
        }
    }

    string CopyFileToPersistentPath(string contentUri)
    {
        string fileName = Path.GetFileName(contentUri); // 取文件名
        string persistentPath = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(persistentPath))
            return persistentPath;

#if UNITY_ANDROID && !UNITY_EDITOR
    try
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");
            AndroidJavaObject inputStream = contentResolver.Call<AndroidJavaObject>("openInputStream", new AndroidJavaObject("android.net.Uri", contentUri));
            using (var fileStream = new FileStream(persistentPath, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = inputStream.Call<int>("read", buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                }
            }
            inputStream.Call("close");
        }
        return persistentPath;
    }
    catch (Exception e)
    {
        Debug.LogError($"复制 contentUri 文件失败: {e.Message}");
        return null;
    }
#else
        Debug.LogWarning("CopyFileToPersistentPath 在非Android平台未实现");
        return null;
#endif
    }

    void AddFrontCenterGrabAnchor(GameObject model)
    {
        var grab = model.GetComponent<XRGrabInteractable>();
        if (grab == null) return;

        var renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        // 计算整体包围盒
        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers)
            bounds.Encapsulate(r.bounds);

        Vector3 center = bounds.center;
        Vector3 size = bounds.size;

        // 模型正面是 forward 方向，取包围盒中心 + forward * 半深度
        Vector3 frontCenterWorld = center + model.transform.forward * (size.z / 2f);
        frontCenterWorld.y = center.y; // 保持中间高度

        // 转换为本地坐标
        Vector3 localFrontCenter = model.transform.InverseTransformPoint(frontCenterWorld);

        // 创建锚点空物体
        GameObject grabAnchor = new GameObject("GrabAnchor");
        grabAnchor.transform.SetParent(model.transform, false);
        grabAnchor.transform.localPosition = localFrontCenter;

        // ✅ 设置锚点旋转，使 forward 一致
        grabAnchor.transform.rotation = Quaternion.LookRotation(model.transform.forward, model.transform.up);
        grabAnchor.transform.localRotation = Quaternion.Inverse(model.transform.rotation) * grabAnchor.transform.rotation;


        grab.attachTransform = grabAnchor.transform;
    }


}