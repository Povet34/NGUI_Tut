using UnityEngine;
using System.Collections.Generic;

public class TestSetupHelper : MonoBehaviour
{
    [Header("Test Setup")]
    public Transform avatarRoot;
    public SkinnedMeshRenderer[] testClothes;

    [Header("Debug")]
    public bool showBoneHierarchy = true;

    /// <summary>
    /// ������ �׽�Ʈ�� �� ���� ����
    /// </summary>
    [ContextMenu("Create Test Bone Structure")]
    public void CreateTestBoneStructure()
    {
        if (avatarRoot == null)
        {
            GameObject avatarObj = new GameObject("Test Avatar");
            avatarRoot = avatarObj.transform;
        }

        // ������ �� ���� ����
        Transform root = CreateBone("Root", avatarRoot);
        Transform spine = CreateBone("Spine", root);
        Transform chest = CreateBone("Chest", spine);
        Transform leftShoulder = CreateBone("LeftShoulder", chest);
        Transform rightShoulder = CreateBone("RightShoulder", chest);
        Transform leftArm = CreateBone("LeftArm", leftShoulder);
        Transform rightArm = CreateBone("RightArm", rightShoulder);

        // �׽�Ʈ�� �޽� ����
        CreateTestMesh(avatarRoot.gameObject, "Original Avatar");

        Debug.Log("Test bone structure created!");
    }

    Transform CreateBone(string name, Transform parent)
    {
        GameObject bone = new GameObject(name);
        bone.transform.SetParent(parent);
        bone.transform.localPosition = Vector3.zero;
        bone.transform.localRotation = Quaternion.identity;
        bone.transform.localScale = Vector3.one;
        return bone.transform;
    }

    void CreateTestMesh(GameObject parent, string name)
    {
        GameObject meshObj = new GameObject(name + "_Mesh");
        meshObj.transform.SetParent(parent.transform);

        SkinnedMeshRenderer smr = meshObj.AddComponent<SkinnedMeshRenderer>();

        // �⺻ ť�� �޽� ���
        smr.sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

        // �⺻ ��Ƽ���� ���
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Random.ColorHSV();
        smr.material = mat;

        // �� �迭 ���� (�����δ� �� ������ �°� �����ؾ� ��)
        List<Transform> bones = new List<Transform>();
        Transform[] allBones = parent.GetComponentsInChildren<Transform>();
        foreach (Transform bone in allBones)
        {
            if (bone.name.Contains("Root") || bone.name.Contains("Spine") ||
                bone.name.Contains("Chest") || bone.name.Contains("Shoulder") ||
                bone.name.Contains("Arm"))
            {
                bones.Add(bone);
            }
        }

        smr.bones = bones.ToArray();
        if (bones.Count > 0)
        {
            smr.rootBone = bones[0];
        }
    }

    /// <summary>
    /// ���� �׽�Ʈ �� ����
    /// </summary>
    [ContextMenu("Create Test Clothes")]
    public void CreateTestClothes()
    {
        if (avatarRoot == null)
        {
            Debug.LogError("Avatar root not set!");
            return;
        }

        List<SkinnedMeshRenderer> clothesList = new List<SkinnedMeshRenderer>();

        // 3���� �ٸ� ������ �� ����
        Color[] colors = { Color.red, Color.green, Color.blue };
        string[] names = { "Red Shirt", "Green Shirt", "Blue Shirt" };

        for (int i = 0; i < 3; i++)
        {
            GameObject clothObj = new GameObject($"TestCloth_{i}");
            clothObj.transform.SetParent(transform);

            // ������ �� ���� ����
            CopyBoneStructure(avatarRoot, clothObj.transform);

            // �޽� ����
            CreateTestMesh(clothObj, names[i]);
            SkinnedMeshRenderer smr = clothObj.GetComponentInChildren<SkinnedMeshRenderer>();
            smr.material.color = colors[i];

            clothesList.Add(smr);
        }

        testClothes = clothesList.ToArray();
        Debug.Log($"Created {testClothes.Length} test clothes!");
    }

    void CopyBoneStructure(Transform original, Transform copy)
    {
        // �� ���� ���� (���� ����)
        foreach (Transform child in original)
        {
            if (child.name.Contains("Root") || child.name.Contains("Spine") ||
                child.name.Contains("Chest") || child.name.Contains("Shoulder") ||
                child.name.Contains("Arm"))
            {
                GameObject newBone = new GameObject(child.name);
                newBone.transform.SetParent(copy);
                newBone.transform.localPosition = child.localPosition;
                newBone.transform.localRotation = child.localRotation;
                newBone.transform.localScale = child.localScale;

                CopyBoneStructure(child, newBone.transform);
            }
        }
    }

    /// <summary>
    /// �� ���� ���
    /// </summary>
    [ContextMenu("Debug Bone Structure")]
    public void DebugBoneStructure()
    {
        if (avatarRoot == null) return;

        Debug.Log("=== Bone Structure ===");
        PrintBoneHierarchy(avatarRoot, 0);
    }

    void PrintBoneHierarchy(Transform bone, int depth)
    {
        string indent = new string(' ', depth * 2);
        Debug.Log($"{indent}{bone.name}");

        foreach (Transform child in bone)
        {
            PrintBoneHierarchy(child, depth + 1);
        }
    }

    void OnGUI()
    {
        if (!showBoneHierarchy) return;

        GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 300));
        GUILayout.Label("=== Test Setup ===");

        if (GUILayout.Button("Create Bone Structure"))
        {
            CreateTestBoneStructure();
        }

        if (GUILayout.Button("Create Test Clothes"))
        {
            CreateTestClothes();
        }

        if (GUILayout.Button("Debug Bone Structure"))
        {
            DebugBoneStructure();
        }

        GUILayout.Space(10);
        GUILayout.Label($"Avatar Root: {(avatarRoot ? "Set" : "None")}");
        GUILayout.Label($"Test Clothes: {(testClothes != null ? testClothes.Length : 0)}");

        GUILayout.EndArea();
    }
}