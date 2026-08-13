using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<GameObject, Queue<GameObject>> m_Pools = new Dictionary<GameObject, Queue<GameObject>>();

    public GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!m_Pools.ContainsKey(prefab))
        {
            m_Pools[prefab] = new Queue<GameObject>();
        }

        GameObject obj;

        if (m_Pools[prefab].Count > 0)
        {
            obj = m_Pools[prefab].Dequeue();

            Debug.Log($"<color=orange>[PoolManager] 기존 객체 재사용: {obj.name}</color>");
        }
        else
        {
            obj = Instantiate(prefab);

            if (!obj.TryGetComponent(out PooledObject pooledObj))
            {
                pooledObj = obj.AddComponent<PooledObject>();
            }

            pooledObj.Init(prefab);

            Debug.Log($"<color=yellow>[PoolManager] 새 객체 생성: {obj.name}</color>");
        }

        obj.transform.SetPositionAndRotation(pos, rot);
        obj.SetActive(true);

        IPoolable poolable = obj.GetComponent<IPoolable>();

        if (poolable == null)
        {
            Debug.LogError($"<color=red>[PoolManager] {obj.name}에서 IPoolable을 찾지 못했습니다!</color>");
        }
        else
        {
            Debug.Log($"<color=green>[PoolManager] OnSpawn 호출 직전: {obj.name}</color>");

            poolable.OnSpawn();

            Debug.Log($"<color=cyan>[PoolManager] OnSpawn 호출 완료: {obj.name}</color>");
        }

        return obj;
    }

    public void Return(GameObject prefab, GameObject obj)
    {
        if (!m_Pools.ContainsKey(prefab))
        {
            Destroy(obj);
            return;
        }

        obj.GetComponent<IPoolable>()?.OnDespawn();
        obj.SetActive(false);
        m_Pools[prefab].Enqueue(obj);
    }

    public void ClearAll()
    {
        foreach (var q in m_Pools.Values)
        {
            while (q.Count > 0)
            {
                Destroy(q.Dequeue());
            }
        }
        m_Pools.Clear();
    }
}
