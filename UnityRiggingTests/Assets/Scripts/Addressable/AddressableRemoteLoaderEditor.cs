#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(AddressableRemoteLoader))]
public class AddressableRemoteLoaderEditor : Editor
{
    private AddressableRemoteLoader loader;
    private bool showLoadedAssets = false;
    private bool showBundleInfo = false;

    private void OnEnable()
    {
        loader = (AddressableRemoteLoader)target;
    }

    public override void OnInspectorGUI()
    {
        // 기본 Inspector 표시
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Addressable Remote Loader Controls", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 초기화 버튼
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Initialize Addressable System", GUILayout.Height(30)))
        {
            if (Application.isPlaying)
            {
                loader.StartCoroutine(loader.InitializeAddressableSystem());
            }
            else
            {
                EditorUtility.DisplayDialog("Warning", "This function only works in Play Mode!", "OK");
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 개별 기능 버튼들
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Individual Functions", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Check Catalog Updates"))
        {
            if (Application.isPlaying)
            {
                loader.StartCoroutine(loader.CheckForCatalogUpdates());
            }
            else
            {
                EditorUtility.DisplayDialog("Warning", "This function only works in Play Mode!", "OK");
            }
        }

        if (GUILayout.Button("Check Download Size"))
        {
            if (Application.isPlaying)
            {
                loader.StartCoroutine(loader.CheckDownloadSize());
            }
            else
            {
                EditorUtility.DisplayDialog("Warning", "This function only works in Play Mode!", "OK");
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Download All Bundles"))
        {
            if (Application.isPlaying)
            {
                loader.StartCoroutine(loader.DownloadAndLoadBundles());
            }
            else
            {
                EditorUtility.DisplayDialog("Warning", "This function only works in Play Mode!", "OK");
            }
        }

        if (GUILayout.Button("Release All Assets"))
        {
            if (Application.isPlaying)
            {
                loader.ReleaseAllAssets();
            }
            else
            {
                EditorUtility.DisplayDialog("Warning", "This function only works in Play Mode!", "OK");
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // 개별 라벨 제어
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Individual Label Controls", EditorStyles.boldLabel);

        foreach (string label in loader.labelsToLoad)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(100));

            if (GUILayout.Button($"Download {label}"))
            {
                if (Application.isPlaying)
                {
                    loader.StartCoroutine(loader.DownloadAndLoadBundle(label));
                }
                else
                {
                    EditorUtility.DisplayDialog("Warning", "This function only works in Play Mode!", "OK");
                }
            }

            if (GUILayout.Button($"Release {label}"))
            {
                if (Application.isPlaying)
                {
                    loader.ReleaseAssets(label);
                }
                else
                {
                    EditorUtility.DisplayDialog("Warning", "This function only works in Play Mode!", "OK");
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // 다운로드 진행률 테스트
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Download Progress Test", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Test Progress Download"))
        {
            if (Application.isPlaying && loader.labelsToLoad.Count > 0)
            {
                string firstLabel = loader.labelsToLoad[0];
                loader.StartCoroutine(loader.DownloadBundleWithProgress(firstLabel,
                    (progress) => Debug.Log($"Progress: {progress * 100:F1}%"),
                    (success) => Debug.Log($"Download completed: {success}")
                ));
            }
            else
            {
                EditorUtility.DisplayDialog("Warning", "This function only works in Play Mode and requires at least one label!", "OK");
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // 정보 표시 섹션
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Runtime Information", EditorStyles.boldLabel);

            // 로드된 에셋 정보 표시
            showLoadedAssets = EditorGUILayout.Foldout(showLoadedAssets, "Loaded Assets");
            if (showLoadedAssets)
            {
                var loadedAssets = GetLoadedAssets();
                if (loadedAssets.Count > 0)
                {
                    foreach (var kvp in loadedAssets)
                    {
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField($"Label: {kvp.Key}", EditorStyles.boldLabel);

                        foreach (GameObject asset in kvp.Value)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField($"  - {asset.name}");

                            if (GUILayout.Button("Instantiate", GUILayout.Width(80)))
                            {
                                loader.InstantiateAsset(kvp.Key, asset.name);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No assets loaded yet");
                }
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();

        // 유틸리티 버튼들
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Utility Functions", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Console"))
        {
            ClearConsole();
        }

        if (GUILayout.Button("Force Repaint"))
        {
            Repaint();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        // 자동 업데이트를 위해 플레이 모드에서 지속적으로 Repaint
        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    private Dictionary<string, List<GameObject>> GetLoadedAssets()
    {
        // Reflection을 사용해서 private field에 접근
        var field = typeof(AddressableRemoteLoader).GetField("loadedAssets",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            return (Dictionary<string, List<GameObject>>)field.GetValue(loader);
        }

        return new Dictionary<string, List<GameObject>>();
    }

    private void ClearConsole()
    {
        // Console 클리어 (Editor 전용)
        var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
}

// 추가적인 에디터 윈도우 (선택사항)
public class AddressableLoaderWindow : EditorWindow
{
    private AddressableRemoteLoader selectedLoader;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Addressable Remote Loader")]
    public static void ShowWindow()
    {
        GetWindow<AddressableLoaderWindow>("Addressable Loader");
    }

    void OnGUI()
    {
        GUILayout.Label("Addressable Remote Loader Control Panel", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        // 로더 선택
        selectedLoader = (AddressableRemoteLoader)EditorGUILayout.ObjectField(
            "Target Loader", selectedLoader, typeof(AddressableRemoteLoader), true);

        if (selectedLoader == null)
        {
            EditorGUILayout.HelpBox("Please select an AddressableRemoteLoader from the scene", MessageType.Warning);
            return;
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Controls only work in Play Mode", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // 메인 컨트롤
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Main Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Initialize System", GUILayout.Height(30)))
        {
            selectedLoader.StartCoroutine(selectedLoader.InitializeAddressableSystem());
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Check Updates"))
        {
            selectedLoader.StartCoroutine(selectedLoader.CheckForCatalogUpdates());
        }
        if (GUILayout.Button("Check Size"))
        {
            selectedLoader.StartCoroutine(selectedLoader.CheckDownloadSize());
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Download All"))
        {
            selectedLoader.StartCoroutine(selectedLoader.DownloadAndLoadBundles());
        }
        if (GUILayout.Button("Release All"))
        {
            selectedLoader.ReleaseAllAssets();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // 개별 라벨 컨트롤
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Label Controls", EditorStyles.boldLabel);

        foreach (string label in selectedLoader.labelsToLoad)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(100));

            if (GUILayout.Button("Download"))
            {
                selectedLoader.StartCoroutine(selectedLoader.DownloadAndLoadBundle(label));
            }

            if (GUILayout.Button("Release"))
            {
                selectedLoader.ReleaseAssets(label);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
    }

    void Update()
    {
        if (Application.isPlaying)
        {
            Repaint();
        }
    }
}

#endif