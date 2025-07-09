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
        // Addressable 시스템 초기화
        var initHandle = Addressables.InitializeAsync();
        yield return initHandle;

        if (initHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Addressable System Initialized Successfully");

            // 카탈로그 업데이트 확인
            yield return StartCoroutine(CheckForCatalogUpdates());

            // 번들 다운로드 크기 확인
            yield return StartCoroutine(CheckDownloadSize());

            // 번들 다운로드 및 로드
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

                // 카탈로그 업데이트 다운로드
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

        // 모든 로드된 에셋 이름 출력
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

        // 번들 다운로드
        var downloadHandle = Addressables.DownloadDependenciesAsync(label);
        yield return downloadHandle;

        if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log($"Bundle downloaded successfully for label: {label}");

            // 해당 라벨의 모든 에셋 로드
            yield return StartCoroutine(LoadAssetsByLabel(label));
        }
        else
        {
            Debug.LogError($"Failed to download bundle for label: {label}");
        }

        // 다운로드 핸들 정리
        Addressables.Release(downloadHandle);
    }

    IEnumerator LoadAssetsByLabel(string label)
    {
        Debug.Log($"Loading assets for label: {label}");

        // 라벨로 에셋 로드
        var loadHandle = Addressables.LoadAssetsAsync<GameObject>(label, null);
        yield return loadHandle;

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            IList<GameObject> result = loadHandle.Result;
            List<GameObject> assets = new List<GameObject>(result);
            loadedAssets[label] = assets;

            Debug.Log($"Loaded {assets.Count} assets for label: {label}");

            // 로드된 에셋 정보 출력
            foreach (GameObject asset in assets)
            {
                Debug.Log($"  - {asset.name}");
            }

            // 핸들 저장 (나중에 해제를 위해)
            bundleHandles[label] = loadHandle;
        }
        else
        {
            Debug.LogError($"Failed to load assets for label: {label}");
            Addressables.Release(loadHandle);
        }
    }

    // 특정 라벨의 특정 에셋 가져오기
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

    // 특정 라벨의 모든 에셋 가져오기
    public List<GameObject> GetAssetsByLabel(string label)
    {
        if (loadedAssets.ContainsKey(label))
        {
            return loadedAssets[label];
        }

        Debug.LogWarning($"No assets found for label '{label}'");
        return new List<GameObject>();
    }

    // 에셋 인스턴스 생성
    public GameObject InstantiateAsset(string label, string assetName, Vector3 position = default, Quaternion rotation = default)
    {
        GameObject asset = GetAssetByName(label, assetName);
        if (asset != null)
        {
            return Instantiate(asset, position, rotation);
        }
        return null;
    }

    // 비동기로 단일 에셋 로드
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

    // 메모리 정리
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

    // 모든 에셋 해제
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

    // 바이트 크기 포맷팅
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

    // 프로그레스 콜백과 함께 다운로드하는 버전
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