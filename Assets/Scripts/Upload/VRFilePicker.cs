using UnityEngine;
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
        string[] types = { ".glb", ".gltf", ".fbx", ".obj", ".zip" };
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
                string destPath = CopyFileToPersistentPath(path);
                await LoadByExtension(destPath);
                Debug.LogError("路径是content URI，建议选择文件管理器里的文件复制到缓存目录。");
                return;
            }
            else
            {
                await LoadByExtension(path);
            }
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
                string directory = Path.GetDirectoryName(path);
                var import = new GltfImport(new LocalDownloadProvider(directory));

                loaded = await import.Load(path);  // ← 不要重新声明 loaded，只赋值

                if (!loaded)
                {
                    Debug.LogError("[失败] GLTF 加载失败！");
                    return;
                }

                gltf = import; // 把 import 赋值回外部的 gltf
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

        bool instantiated = await gltf.InstantiateMainSceneAsync(parent.transform);
        if (!instantiated)
        {
            Debug.LogError("[失败] 实例化失败！");
            return;
        }
        // Step 2: 自动调整缩放（可选，统一模型大小）
        NormalizeModelScale(parent);

        if (instantiated)
        {
            // Step 3: 添加精确 BoxCollider
            AddAccurateBoxCollider(parent);

            // Step 4: 确保材质贴图正常显示
            ForceAssignWhiteTexture(parent);

            // Step 5: 设置 XRGrabInteractable / RigidBody
            SetupRigidbodyAndGrab(parent);
        }

        Debug.Log("[完成] 模型加载完成");
        ReplaceShadersToStandard(parent);
        PrintLoadedMaterialsAndTextures(parent);
        

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
    void ForceAssignWhiteTexture(GameObject go)
    {
        Texture2D defaultTex = Texture2D.whiteTexture;
        var renderers = go.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.sharedMaterials)
            {
                if (mat != null && mat.shader.name == "Standard")
                {
                    if (!mat.HasProperty("_MainTex") || mat.mainTexture == null)
                    {
                        Debug.LogWarning($"材质 {mat.name} 没有贴图，强制赋白贴图");
                        mat.mainTexture = defaultTex;
                    }
                }
            }
        }
    }


    // 新增简化碰撞体方法，替代 AddMeshCollidersRecursive
    /* void AddColliderToParentOnly(GameObject root)
     {
         if (root.GetComponent<Collider>() != null)
             return; // 已有碰撞体则不重复添加

         var meshFilter = root.GetComponent<MeshFilter>();
         if (meshFilter != null && meshFilter.sharedMesh != null)
         {
             var meshCollider = root.AddComponent<MeshCollider>();
             meshCollider.convex = true;  // 父物体加凸包碰撞体
         }
         else
         {
             root.AddComponent<BoxCollider>();
         }
     }*/
    void AddAccurateBoxCollider(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning("No renderer found, skipping collider.");
            return;
        }

        // 计算世界空间下的包围盒
        Bounds worldBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            worldBounds.Encapsulate(renderers[i].bounds);
        }

        // 将中心从世界坐标转换到本地坐标
        Vector3 localCenter = go.transform.InverseTransformPoint(worldBounds.center);

        // 将包围盒 size 从世界空间转换到本地空间（考虑缩放）
        Vector3 localSize = go.transform.InverseTransformVector(worldBounds.size + Vector3.zero); // +0 防止引用错误

        // 防止负值
        localSize = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));

        BoxCollider box = go.GetComponent<BoxCollider>();
        if (box == null)
            box = go.AddComponent<BoxCollider>();

        box.center = localCenter;
        box.size = localSize;
    }



    // 修改 SetupRigidbodyAndGrab 只给父物体添加刚体和抓取组件
    void SetupRigidbodyAndGrab(GameObject go)
    {
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;

        var grab = go.GetComponent<XRGrabInteractable>();
        if (grab == null) grab = go.AddComponent<XRGrabInteractable>();

        // 抓取组件的colliders只添加根物体的碰撞器即可
        grab.colliders.Clear();
        var col = go.GetComponent<Collider>();
        if (col != null)
        {
            grab.colliders.Add(col);
        }

        var manager = UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>();
        if (manager != null)
        {
            grab.interactionManager = manager;
        }

        grab.enabled = false;
        grab.enabled = true;
    }

    public static string CopyFileToPersistentPath(string srcPath)
    {
        string fileName = Path.GetFileName(srcPath);
        string destPath = Path.Combine(Application.persistentDataPath, fileName);

        File.Copy(srcPath, destPath, true);
        return destPath;
    }
    public async Task LoadZipModel(string zipPath)
    {
        string extractPath = Path.Combine(Application.temporaryCachePath, "unzipped_model");
        if (Directory.Exists(extractPath))
            Directory.Delete(extractPath, true);

        Directory.CreateDirectory(extractPath);

        // 使用 SharpZipLib 或 System.IO.Compression 解压
        System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);

        // 查找 gltf 或 glb 文件
        string[] gltfFiles = Directory.GetFiles(extractPath, "*.gltf", SearchOption.AllDirectories);
        string[] glbFiles = Directory.GetFiles(extractPath, "*.glb", SearchOption.AllDirectories);
        string modelPath = (gltfFiles.Length > 0) ? gltfFiles[0] :
                           (glbFiles.Length > 0) ? glbFiles[0] : null;

        if (modelPath == null)
        {
            Debug.LogError("Zip 文件中未找到 gltf 或 glb 文件");
            return;
        }

        if (modelPath != null)
        {
            string modelDir = Path.GetDirectoryName(modelPath);

            // 支持的贴图格式
            string[] imageExts = new[] { ".png", ".jpg", ".jpeg", ".webp", ".bmp" };
            string[] allFiles = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories);

            foreach (var file in allFiles)
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (Array.Exists(imageExts, e => e == ext))
                {
                    string destPath = Path.Combine(modelDir, Path.GetFileName(file));
                    if (!File.Exists(destPath))  // 避免重复拷贝
                    {
                        File.Copy(file, destPath);
                        Debug.Log($"已移动贴图到模型目录: {destPath}");
                    }
                }
            }
        }

        await LoadModel(modelPath);
    }

    private async Task LoadByExtension(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();

        if (ext == ".zip")
        {
            await LoadZipModel(path);
        }
        else if (ext == ".glb" || ext == ".gltf")
        {
            await LoadModel(path);
        }
        else
        {
            Debug.LogError($"不支持的文件格式: {ext}");
        }
    }

    void NormalizeModelScale(GameObject model, float maxSize = 1f)
    {
        // 计算模型的包围盒
        var renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        // 计算最大边长
        float maxDimension = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
        if (maxDimension <= 0) return;

        // 计算缩放比例
        float scaleFactor = maxSize / maxDimension;

        model.transform.localScale = model.transform.localScale * scaleFactor;
    }

}
