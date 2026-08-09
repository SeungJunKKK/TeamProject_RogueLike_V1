using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableManager : Singleton<AddressableManager>
{
    // 로드된 에셋들의 핸들(Handle)을 관리하는 딕셔너리 (메모리 해제할 때 주소로 찾기 위함)
    private readonly Dictionary<string, AsyncOperationHandle> _loadedAssets = new();


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

    #region Instantiate / ReleaseInstance (객체 생성 및 파괴)
    public void InstantiateAsync(string key, Vector3 position, Quaternion rotation, Transform parent = null, Action<GameObject> onComplete = null) // Addressable 키를 이용해 Prefab을 비동기로 로드하고 Instantiate함
    {
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(key, position, rotation, parent);

        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                onComplete?.Invoke(op.Result);
            }
            else
            {
                Debug.LogError($"[AddressableManager] 객체 생성 실패: {key}");
                onComplete?.Invoke(null);
            }
        };
    }

    public void ReleaseInstance(GameObject instance) // InstantiateAsync로 생성된 GameObject를 파괴하고 메모리 참조를 해제
    {
        if (instance == null) return;

        // Addressables.ReleaseInstance를 호출해야 Destroy()와 함께 메모리 참조 카운트가 감소
        bool isReleased = Addressables.ReleaseInstance(instance);

        if (!isReleased)
        {
            Debug.LogWarning($"[AddressableManager] Addressable 인스턴스가 아니거나 이미 해제된 객체입니다: {instance.name}");
            // Addressable로 생성된 객체가 아닐 경우 기본 Destroy 처리
            Destroy(instance);
        }
    }
    #endregion

    #region Load Sprite Sheet (스프라이트 시트 전용 로드)
    public void LoadSpriteSheetAsync(string key, Action<IList<Sprite>> onComplete) // Addressable Key를 받아 슬라이스된 모든 Sprite 목록을 로드
    {
        Addressables.LoadAssetAsync<IList<Sprite>>(key).Completed += (op) =>
        {
            if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                onComplete?.Invoke(op.Result);
            }
            else
            {
                Addressables.LoadResourceLocationsAsync(key, typeof(Sprite)).Completed += (locHandle) =>
                {
                    if (locHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded && locHandle.Result.Count > 0)
                    {
                        Addressables.LoadAssetsAsync<Sprite>(locHandle.Result, null).Completed += (assetsHandle) =>
                        {
                            onComplete?.Invoke(assetsHandle.Result);
                        };
                    }
                    else
                    {
                        Debug.LogError($"[AddressableManager] 스프라이트 시트 로드 실패: {key}");
                        onComplete?.Invoke(null);
                    }
                };
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