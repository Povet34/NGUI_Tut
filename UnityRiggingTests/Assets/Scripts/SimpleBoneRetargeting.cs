using System.Collections.Generic;
using UnityEngine;

public class SimpleBoneRetargeting : MonoBehaviour
{
    [Header("Main Avatar")]
    public SkinnedMeshRenderer mainAvatarRenderer;

    [Header("New Clothes")]
    public SkinnedMeshRenderer[] newClothes;

    [Header("Test Controls")]
    public int currentClothIndex = 0;

    // 메인 본 정보 저장
    private Dictionary<string, Transform> mainBoneDict = new Dictionary<string, Transform>();
    private Transform mainRootBone;

    void Start()
    {
        if (mainAvatarRenderer != null)
        {
            SetupMainBones();
        }
    }

    void Update()
    {
        // 테스트용 키 입력
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwapClothes(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwapClothes(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwapClothes(2);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            ResetToOriginal();
        }
    }

    /// <summary>
    /// 메인 아바타의 본 구조를 Dictionary에 저장
    /// </summary>
    void SetupMainBones()
    {
        mainBoneDict.Clear();
        mainRootBone = mainAvatarRenderer.rootBone;

        // 메인 아바타의 모든 본을 Dictionary에 저장
        Transform[] mainBones = mainAvatarRenderer.bones;
        for (int i = 0; i < mainBones.Length; i++)
        {
            if (mainBones[i] != null)
            {
                string boneName = mainBones[i].name;
                if (!mainBoneDict.ContainsKey(boneName))
                {
                    mainBoneDict.Add(boneName, mainBones[i]);
                    Debug.Log($"Main Bone Added: {boneName}");
                }
            }
        }

        Debug.Log($"Main Avatar Setup Complete. Total Bones: {mainBoneDict.Count}");
    }

    /// <summary>
    /// 새 옷으로 교체
    /// </summary>
    public void SwapClothes(int clothIndex)
    {
        if (newClothes == null || clothIndex >= newClothes.Length)
        {
            Debug.LogError("Invalid cloth index or no clothes available");
            return;
        }

        SkinnedMeshRenderer newCloth = newClothes[clothIndex];
        if (newCloth == null)
        {
            Debug.LogError("New cloth is null");
            return;
        }

        Debug.Log($"=== Swapping to Cloth {clothIndex} ===");

        // 1. 새 옷의 메시 정보 가져오기
        Mesh newMesh = newCloth.sharedMesh;
        Material[] newMaterials = newCloth.materials;

        // 2. 새 옷의 본들을 메인 본에 매핑
        Transform[] newBones = RetargetBones(newCloth);

        // 3. 메인 아바타 렌더러에 적용
        mainAvatarRenderer.sharedMesh = newMesh;
        mainAvatarRenderer.materials = newMaterials;
        mainAvatarRenderer.bones = newBones;

        Debug.Log($"Cloth {clothIndex} applied successfully!");
        currentClothIndex = clothIndex;
    }

    /// <summary>
    /// 본 이름 기반으로 새 옷의 본들을 메인 본에 매핑
    /// </summary>
    Transform[] RetargetBones(SkinnedMeshRenderer newCloth)
    {
        Transform[] originalBones = newCloth.bones;
        Transform[] retargetedBones = new Transform[originalBones.Length];

        int successCount = 0;
        int failCount = 0;

        for (int i = 0; i < originalBones.Length; i++)
        {
            if (originalBones[i] != null)
            {
                string boneName = originalBones[i].name;

                // 메인 본에서 같은 이름의 본 찾기
                if (mainBoneDict.ContainsKey(boneName))
                {
                    retargetedBones[i] = mainBoneDict[boneName];
                    successCount++;
                    Debug.Log($"✓ Bone Mapped: {boneName}");
                }
                else
                {
                    // 매핑 실패 시 원본 본 사용 (또는 null)
                    retargetedBones[i] = originalBones[i];
                    failCount++;
                    Debug.LogWarning($"✗ Bone Not Found: {boneName}");
                }
            }
        }

        Debug.Log($"Bone Mapping Result - Success: {successCount}, Failed: {failCount}");
        return retargetedBones;
    }

    /// <summary>
    /// 원본 아바타로 복원
    /// </summary>
    public void ResetToOriginal()
    {
        if (mainAvatarRenderer != null)
        {
            // 원본 데이터가 있다면 복원
            // 실제로는 원본 데이터를 백업해두고 복원해야 함
            Debug.Log("Reset to original avatar");
        }
    }

    /// <summary>
    /// 현재 상태 정보 출력
    /// </summary>
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));

        GUILayout.Label("=== Bone Retargeting Test ===");
        GUILayout.Label($"Current Cloth: {currentClothIndex}");
        GUILayout.Label($"Main Bones: {mainBoneDict.Count}");

        GUILayout.Space(10);
        GUILayout.Label("Controls:");
        GUILayout.Label("1, 2, 3 - Swap Clothes");
        GUILayout.Label("R - Reset to Original");

        GUILayout.Space(10);
        if (GUILayout.Button("Swap Cloth 0")) SwapClothes(0);
        if (GUILayout.Button("Swap Cloth 1")) SwapClothes(1);
        if (GUILayout.Button("Swap Cloth 2")) SwapClothes(2);
        if (GUILayout.Button("Reset")) ResetToOriginal();

        GUILayout.EndArea();
    }

    /// <summary>
    /// 본 매핑 상태 디버그 출력
    /// </summary>
    [ContextMenu("Debug Bone Mapping")]
    public void DebugBoneMapping()
    {
        if (mainAvatarRenderer == null) return;

        Debug.Log("=== Current Bone Mapping ===");
        Transform[] currentBones = mainAvatarRenderer.bones;

        for (int i = 0; i < currentBones.Length; i++)
        {
            if (currentBones[i] != null)
            {
                Debug.Log($"Bone[{i}]: {currentBones[i].name} - {currentBones[i].GetInstanceID()}");
            }
        }
    }
}