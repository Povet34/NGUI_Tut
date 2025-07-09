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
        // �⺻ Inspector ǥ��
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Addressable Remote Loader Controls", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // �ʱ�ȭ ��ư
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

        // ���� ��� ��ư��
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

        // ���� �� ����
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

        // �ٿ�ε� ����� �׽�Ʈ
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

        // ���� ǥ�� ����
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Runtime Information", EditorStyles.boldLabel);

            // �ε�� ���� ���� ǥ��
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

        // ��ƿ��Ƽ ��ư��
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

        // �ڵ� ������Ʈ�� ���� �÷��� ��忡�� ���������� Repaint
        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    private Dictionary<string, List<GameObject>> GetLoadedAssets()
    {
        // Reflection�� ����ؼ� private field�� ����
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
        // Console Ŭ���� (Editor ����)
        var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
}

// �߰����� ������ ������ (���û���)
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

        // �δ� ����
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

        // ���� ��Ʈ��
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

        // ���� �� ��Ʈ��
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