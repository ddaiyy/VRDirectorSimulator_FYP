using UnityEngine;
using GLTFast;
using System.Threading.Tasks;
using System.IO;
using System;
using UnityEngine.XR.Interaction.Toolkit;

public class VRModelLoader : MonoBehaviour
{
    public async void OpenModelPicker()
    {
        Debug.Log("[测试] OpenModelPicker 被调用了");
        string[] types = { ".glb", ".gltf", ".fbx", ".obj" };
        NativeFilePicker.PickFile(async (path) =>
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("取消选择");
                return;
            }

            Debug.Log("选择路径: " + path);

            // 这里加判断，如果是content://，报个警告或者直接提示用户选其他路径
            if (path.StartsWith("content://"))
            {
                Debug.LogError("路径是content URI，不能直接用File API读取，建议选择文件管理器里的文件复制到缓存目录。");
                // 你也可以这里调用其他处理content uri的方法，但NativeFilePicker通常会自动复制，直接用缓存路径更好。
                return;
            }
            // 这里才可以用 await 了
            await LoadModel(path);
        }, null);

    }
    public async Task LoadModel(string path)
    {
        Debug.Log($"[加载开始] 路径: {path}");

        if (!File.Exists(path))
        {
            Debug.LogError("文件不存在！");
            return;
        }

        GameObject parent = new GameObject("UploadedModel");
        // 模型实例化完成后，给所有MeshRenderer添加Collider和XRGrabInteractable（如果需要抓取整个模型，建议只在parent加即可）
        var meshRenderers = parent.GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in meshRenderers)
        {
            var go = mr.gameObject;
            if (go.GetComponent<Collider>() == null)
            {
                var meshFilter = go.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    var meshCollider = go.AddComponent<MeshCollider>();
                    meshCollider.convex = true;  // XR交互需要凸包Collider
                }
                else
                {
                    // 如果没网格，给BoxCollider作为兜底
                    go.AddComponent<BoxCollider>();
                }
            }
            // 只给parent加XRGrabInteractable即可，或者根据需求加在需要抓取的物体上
        }

        var rb = parent.GetComponent<Rigidbody>();
        if (rb == null) rb = parent.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;  // XR抓取通常要非kinematic

        var grab = parent.GetComponent<XRGrabInteractable>();
        if (grab == null) grab = parent.AddComponent<XRGrabInteractable>();

        var renderers = parent.GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
        {
            foreach (var mat in r.sharedMaterials)
            {
                if (mat == null)
                    Debug.LogWarning($"{r.name} 有空材质");
                else if (mat.shader == null)
                    Debug.LogWarning($"{r.name} 材质没shader");
                else
                    Debug.Log($"{r.name} 材质: {mat.name}, Shader: {mat.shader.name}");
            }
        }

        if (Camera.main == null)
        {
            Debug.LogError("[错误] 找不到主摄像机");
            return;
        }

        Transform cam = Camera.main.transform;
        Vector3 spawnPos = cam.position + cam.forward * 2f;
        parent.transform.position = spawnPos;
        parent.transform.localScale = Vector3.one * 0.2f;

        var gltf = new GltfImport();

        try
        {
            byte[] fileData = File.ReadAllBytes(path);

            bool loaded = false;
            if (path.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
            {
                loaded = await gltf.LoadGltfBinary(fileData);
            }
            else if (path.EndsWith(".gltf", StringComparison.OrdinalIgnoreCase))
            {
                string json = System.Text.Encoding.UTF8.GetString(fileData);
                // 这里简单示例，假设没有外部依赖文件，如果有需实现 getExternalFile 代理
                loaded = await gltf.LoadGltfJson(json, null);
            }
            else
            {
                Debug.LogError("不支持的文件格式（不是 glb 或 gltf）");
                return;
            }

            if (!loaded)
            {
                Debug.LogError("[失败] GLTF 加载失败！");
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"GLTF 加载异常: {ex.Message}");
            return;
        }

        // 模型实例化完成后
        // 模型实例化完成后
        bool instantiated = await gltf.InstantiateMainSceneAsync(parent.transform);
        if (!instantiated)
        {
            Debug.LogError("[失败] 实例化失败！");
            return;
        }

        // 给所有子物体添加 MeshCollider 并设置 convex = true
        AddMeshCollidersRecursive(parent);

        if (instantiated)
        {
            AddMeshCollidersRecursive(parent);
            SetupRigidbodyAndGrab(parent); // 必须在 AddMeshColliders 后
        }

        Debug.Log("[完成] 模型加载完成");
        ReplaceShadersToStandard(parent);
        PrintLoadedMaterialsAndTextures(parent);
        ForceAssignTestTexture(parent);

    }

    void PrintLoadedMaterialsAndTextures(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            Debug.Log($"物体：{renderer.gameObject.name} 有 {renderer.sharedMaterials.Length} 个材质");
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                var mat = renderer.sharedMaterials[i];
                if (mat == null)
                {
                    Debug.LogWarning($"材质 #{i} 为空");
                    continue;
                }
                Debug.Log($"材质 #{i} 名字：{mat.name}，Shader：{mat.shader.name}");
                var mainTex = mat.mainTexture;
                if (mainTex == null)
                {
                    Debug.LogWarning($"材质 {mat.name} 没有主贴图 (mainTexture为空)");
                }
                else
                {
                    Debug.Log($"材质 {mat.name} 主贴图名称：{mainTex.name}，大小：{mainTex.width}x{mainTex.height}");
                }
            }
        }

    }
    void ReplaceShadersToStandard(GameObject go)
    {
        foreach (var renderer in go.GetComponentsInChildren<Renderer>())
        {
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                var mat = materials[i];
                if (mat == null || mat.shader == null)
                    continue;

                // 如果是 glTF 系列的 Shader，就替换为 Standard
                if (mat.shader.name.Contains("glTF"))
                {
                    var newMat = new Material(Shader.Find("Standard"));

                    // 拷贝主贴图
                    if (mat.mainTexture != null)
                    {
                        newMat.mainTexture = mat.mainTexture;
                        newMat.SetTexture("_MainTex", mat.mainTexture);
                    }

                    // 拷贝颜色（如果有）
                    if (mat.HasProperty("_Color"))
                        newMat.color = mat.color;

                    materials[i] = newMat;
                }
            }
            renderer.sharedMaterials = materials;
        }
    }
    void ForceAssignTestTexture(GameObject root)
    {
        Texture2D fallbackTex = Texture2D.whiteTexture;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.materials)
            {
                if (mat != null && mat.mainTexture == null)
                {
                    Debug.LogWarning($"材质 {mat.name} 没有贴图，强行赋值白贴图");
                    mat.mainTexture = fallbackTex;
                }
            }
        }
    }

    void AddMeshCollidersRecursive(GameObject root)
    {
        var meshRenderers = root.GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in meshRenderers)
        {
            var go = mr.gameObject;

            var collider = go.GetComponent<Collider>();
            if (collider == null)
            {
                var meshFilter = go.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    var meshCollider = go.AddComponent<MeshCollider>();
                    meshCollider.convex = true; // 必须是凸包，XR交互要求
                }
                else
                {
                    // 没有Mesh则用BoxCollider兜底
                    go.AddComponent<BoxCollider>();
                }
            }
            else if (collider is MeshCollider mc)
            {
                mc.convex = true; // 确保是凸包
            }
        }
    }
    void SetupRigidbodyAndGrab(GameObject go)
    {
        // 添加 Rigidbody
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;

        // 添加 XRGrabInteractable
        var grab = go.GetComponent<XRGrabInteractable>();
        if (grab == null) grab = go.AddComponent<XRGrabInteractable>();

        // 清空并重新添加 colliders
        grab.colliders.Clear();
        var colliders = go.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            grab.colliders.Add(col);
        }

        // 设置 Interaction Manager（确保存在）
        var manager = UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>();
        if (manager != null)
        {
            grab.interactionManager = manager;
        }

        // 强制刷新
        grab.enabled = false;
        grab.enabled = true;
    }



}
