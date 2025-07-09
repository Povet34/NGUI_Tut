using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableRemoteLoader : MonoBehaviour
{
    [Header("Labels to Load")]
    public List<string> labelsToLoad = new List<string> { "Characters", "Weapons", "UI" };

    private Dictionary<string, AsyncOperationHandle> bundleHandles = new Dictionary<string, AsyncOperationHandle>();
    private Dictionary<string, List<GameObject>> loadedAssets = new Dictionary<string, List<GameObject>>();

    void Start()
    {
        StartCoroutine(InitializeAddressableSystem());
    }

    public IEnumerator InitializeAddressableSystem()
    {
        // Addressable �ý��� �ʱ�ȭ
        var initHandle = Addressables.InitializeAsync();
        yield return initHandle;

        if (initHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Addressable System Initialized Successfully");

            // īŻ�α� ������Ʈ Ȯ��
            yield return StartCoroutine(CheckForCatalogUpdates());

            // ���� �ٿ�ε� ũ�� Ȯ��
            yield return StartCoroutine(CheckDownloadSize());

            // ���� �ٿ�ε� �� �ε�
            yield return StartCoroutine(DownloadAndLoadBundles());
        }
        else
        {
            Debug.LogError("Failed to initialize Addressable System");

        }
    }

    public IEnumerator CheckForCatalogUpdates()
    {
        Debug.Log("Checking for catalog updates...");

        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;

        if (checkHandle.Status == AsyncOperationStatus.Succeeded)
        {
            List<string> catalogs = checkHandle.Result;
            if (catalogs.Count > 0)
            {
                Debug.Log($"Found {catalogs.Count} catalog updates");

                // īŻ�α� ������Ʈ �ٿ�ε�
                var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
                yield return updateHandle;

                if (updateHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log("Catalog updated successfully");
                }
                else
                {
                    Debug.LogError("Failed to update catalog");
                }

                Addressables.Release(updateHandle);
            }
            else
            {
                Debug.Log("No catalog updates found");
            }
        }
        else
        {
            Debug.LogError("Failed to check for catalog updates");
        }

        Addressables.Release(checkHandle);
    }

    public IEnumerator CheckDownloadSize()
    {
        Debug.Log("Checking download size for all labels...");

        long totalSize = 0;

        foreach (string label in labelsToLoad)
        {
            var sizeHandle = Addressables.GetDownloadSizeAsync(label);
            yield return sizeHandle;

            if (sizeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                long size = sizeHandle.Result;
                totalSize += size;
                Debug.Log($"Label '{label}' download size: {FormatBytes(size)}");
            }
            else
            {
                Debug.LogError($"Failed to get download size for label: {label}");
            }

            Addressables.Release(sizeHandle);
        }

        Debug.Log($"Total download size: {FormatBytes(totalSize)}");
    }

    public IEnumerator DownloadAndLoadBundles()
    {
        Debug.Log("Starting bundle download and load process...");

        foreach (string label in labelsToLoad)
        {
            yield return StartCoroutine(DownloadAndLoadBundle(label));
        }
        Debug.Log("All bundles loaded successfully!");

        // ��� �ε�� ���� �̸� ���
        foreach (var kvp in loadedAssets)
        {
            Debug.Log($"Label: {kvp.Key}");
            foreach (var go in kvp.Value)
            {
                Debug.Log($"  - {go.name}");
            }
        }
    }

    public IEnumerator DownloadAndLoadBundle(string label)
    {
        Debug.Log($"Downloading and loading bundle for label: {label}");

        // ���� �ٿ�ε�
        var downloadHandle = Addressables.DownloadDependenciesAsync(label);
        yield return downloadHandle;

        if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log($"Bundle downloaded successfully for label: {label}");

            // �ش� ���� ��� ���� �ε�
            yield return StartCoroutine(LoadAssetsByLabel(label));
        }
        else
        {
            Debug.LogError($"Failed to download bundle for label: {label}");
        }

        // �ٿ�ε� �ڵ� ����
        Addressables.Release(downloadHandle);
    }

    IEnumerator LoadAssetsByLabel(string label)
    {
        Debug.Log($"Loading assets for label: {label}");

        // �󺧷� ���� �ε�
        var loadHandle = Addressables.LoadAssetsAsync<GameObject>(label, null);
        yield return loadHandle;

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            IList<GameObject> result = loadHandle.Result;
            List<GameObject> assets = new List<GameObject>(result);
            loadedAssets[label] = assets;

            Debug.Log($"Loaded {assets.Count} assets for label: {label}");

            // �ε�� ���� ���� ���
            foreach (GameObject asset in assets)
            {
                Debug.Log($"  - {asset.name}");
            }

            // �ڵ� ���� (���߿� ������ ����)
            bundleHandles[label] = loadHandle;
        }
        else
        {
            Debug.LogError($"Failed to load assets for label: {label}");
            Addressables.Release(loadHandle);
        }
    }

    // Ư�� ���� Ư�� ���� ��������
    public GameObject GetAssetByName(string label, string assetName)
    {
        if (loadedAssets.ContainsKey(label))
        {
            foreach (GameObject asset in loadedAssets[label])
            {
                if (asset.name == assetName)
                {
                    return asset;
                }
            }
        }

        Debug.LogWarning($"Asset '{assetName}' not found in label '{label}'");
        return null;
    }

    // Ư�� ���� ��� ���� ��������
    public List<GameObject> GetAssetsByLabel(string label)
    {
        if (loadedAssets.ContainsKey(label))
        {
            return loadedAssets[label];
        }

        Debug.LogWarning($"No assets found for label '{label}'");
        return new List<GameObject>();
    }

    // ���� �ν��Ͻ� ����
    public GameObject InstantiateAsset(string label, string assetName, Vector3 position = default, Quaternion rotation = default)
    {
        GameObject asset = GetAssetByName(label, assetName);
        if (asset != null)
        {
            return Instantiate(asset, position, rotation);
        }
        return null;
    }

    // �񵿱�� ���� ���� �ε�
    public IEnumerator LoadSingleAssetAsync(string address, System.Action<GameObject> onComplete)
    {
        var loadHandle = Addressables.LoadAssetAsync<GameObject>(address);
        yield return loadHandle;

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            onComplete?.Invoke(loadHandle.Result);
        }
        else
        {
            Debug.LogError($"Failed to load asset: {address}");
            onComplete?.Invoke(null);
        }
    }

    // �޸� ����
    public void ReleaseAssets(string label)
    {
        if (bundleHandles.ContainsKey(label))
        {
            Addressables.Release(bundleHandles[label]);
            bundleHandles.Remove(label);
            loadedAssets.Remove(label);
            Debug.Log($"Released assets for label: {label}");
        }
    }

    // ��� ���� ����
    public void ReleaseAllAssets()
    {
        foreach (var kvp in bundleHandles)
        {
            Addressables.Release(kvp.Value);
        }

        bundleHandles.Clear();
        loadedAssets.Clear();
        Debug.Log("All assets released");
    }

    // ����Ʈ ũ�� ������
    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    void OnDestroy()
    {
        ReleaseAllAssets();
    }

    // ���α׷��� �ݹ�� �Բ� �ٿ�ε��ϴ� ����
    public IEnumerator DownloadBundleWithProgress(string label, System.Action<float> onProgress, System.Action<bool> onComplete)
    {
        var downloadHandle = Addressables.DownloadDependenciesAsync(label);

        while (!downloadHandle.IsDone)
        {
            onProgress?.Invoke(downloadHandle.PercentComplete);
            yield return null;
        }

        bool success = downloadHandle.Status == AsyncOperationStatus.Succeeded;
        onComplete?.Invoke(success);

        Addressables.Release(downloadHandle);
    }
}