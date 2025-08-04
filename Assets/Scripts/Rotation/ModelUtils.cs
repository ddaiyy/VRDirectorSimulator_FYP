using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public static class ModelUtils
{
    /// <summary>
    /// 为 GameObject 添加或更新一个精确的 BoxCollider
    /// </summary>
    public static void AddAccurateBoxCollider(GameObject go)
    {
        var meshFilters = go.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length == 0)
        {
            Debug.LogWarning("⚠️ 无 MeshFilter，跳过碰撞体添加");
            return;
        }

        Transform visualRoot = meshFilters[0].transform;
        foreach (var mf in meshFilters)
            visualRoot = FindCommonRoot(visualRoot, mf.transform);

        if (visualRoot == null)
        {
            Debug.LogWarning("❌ 无法找到公共父节点");
            return;
        }

        Bounds localBounds = new Bounds();

        bool hasInit = false;
        foreach (var mf in meshFilters)
        {
            if (mf.sharedMesh == null) continue;

            // mesh 的 bounds 是局部空间的
            Bounds meshBounds = mf.sharedMesh.bounds;
            Vector3 worldCenter = mf.transform.TransformPoint(meshBounds.center);
            Vector3 worldExtents = mf.transform.TransformVector(meshBounds.extents);

            Bounds worldBounds = new Bounds(worldCenter, worldExtents * 2);

            if (!hasInit)
            {
                localBounds = new Bounds(visualRoot.InverseTransformPoint(worldBounds.center), visualRoot.InverseTransformVector(worldBounds.size));
                hasInit = true;
            }
            else
            {
                Bounds temp = new Bounds(visualRoot.InverseTransformPoint(worldBounds.center), visualRoot.InverseTransformVector(worldBounds.size));
                localBounds.Encapsulate(temp);
            }
        }

        BoxCollider boxCollider = visualRoot.GetComponent<BoxCollider>();
        if (boxCollider == null)
            boxCollider = visualRoot.gameObject.AddComponent<BoxCollider>();

        boxCollider.center = localBounds.center;
        boxCollider.size = localBounds.size;
        boxCollider.isTrigger = false;

        Debug.Log($"✅ BoxCollider 添加到: {visualRoot.name}");

        var grab = go.GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            if (!grab.colliders.Contains(boxCollider))
            {
                grab.colliders.Clear();
                grab.colliders.Add(boxCollider);
                Debug.Log("✅ Collider 注册到 XRGrabInteractable");
            }
        }
    }

    static Transform FindCommonRoot(Transform a, Transform b)
    {
        var ancestors = new HashSet<Transform>();
        while (a != null)
        {
            ancestors.Add(a);
            a = a.parent;
        }

        while (b != null)
        {
            if (ancestors.Contains(b))
                return b;
            b = b.parent;
        }

        return null;
    }


}
