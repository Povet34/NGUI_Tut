using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SimpleAdreesable : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(BasicTest());
    }

    IEnumerator BasicTest()
    {
        Debug.Log("Starting basic test...");

        // 아무 설정 없이 기본 초기화만
        var handle = Addressables.InitializeAsync();
        yield return handle;

        Debug.Log($"Handle valid: {handle.IsValid()}");
        if (handle.IsValid())
        {
            Debug.Log($"Status: {handle.Status}");
            if (handle.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError($"Error: {handle.OperationException}");
            }
        }
    }
}
