using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static AddressableManager Instance { get; private set; }

    // 로드된 에셋들의 핸들(Handle)을 관리하는 딕셔너리 (메모리 해제할 때 주소로 찾기 위함)
    private readonly Dictionary<string, AsyncOperationHandle> _loadedAssets = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Load (에셋 로드)
    public void LoadAssetAsync<T>(string key, Action<T> onComplete) where T : UnityEngine.Object
    {
        // 이미 동일한 키로 로드가 완료된 에셋이 있다면, 새로 로드하지 않고 캐시된 에셋을 바로 반환
        if (_loadedAssets.TryGetValue(key, out var activeHandle))
        {
            if (activeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                onComplete?.Invoke(activeHandle.Result as T);
                return;
            }
        }

        // 새로운 에셋 비동기 로드 시작
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);

        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                // 로드에 성공하면 딕셔너리에 핸들 저장
                if (!_loadedAssets.ContainsKey(key))
                {
                    _loadedAssets.Add(key, op);
                }

                // 콜백 함수 실행 및 로드된 에셋 전달
                onComplete?.Invoke(op.Result);
            }
            else
            {
                Debug.LogError($"[AddressableManager] 에셋 로드 실패: {key}");
                onComplete?.Invoke(null);
            }
        };
    }
    #endregion

    #region Release / Unload (에셋 메모리 해제)
    public void UnloadAsset(string key)
    {
        if (_loadedAssets.TryGetValue(key, out var handle))
        {
            Addressables.Release(handle);
            _loadedAssets.Remove(key);
            Debug.Log($"[AddressableManager] 에셋 해제 완료: {key}");
        }
        else
        {
            Debug.LogWarning($"[AddressableManager] 해제하려는 에셋을 찾을 수 없음 (이미 해제되었거나 로드된 적 없음): {key}");
        }
    }
    
    public void UnloadAllAssets() // 게임 오버, 스테이지 전환 등 모든 캐시된 에셋을 일괄 해제할 때 UnloadAllAssets() 사용
    {
        foreach (var handle in _loadedAssets.Values)
        {
            Addressables.Release(handle);
        }
        _loadedAssets.Clear();
        Debug.Log("[AddressableManager] 모든 에셋 해제 완료");
    }
    #endregion
}