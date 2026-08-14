using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<GameObject, Queue<GameObject>> m_Pools = new Dictionary<GameObject, Queue<GameObject>>();

    /// <summary>
    /// 풀에서 오브젝트를 꺼낸다. 자식 포함 모든 IPoolable의 OnSpawn이 호출
    /// </summary>
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
        }
        else
        {
            obj = Instantiate(prefab);

            if (!obj.TryGetComponent(out PooledObject pooledObj))
            {
                pooledObj = obj.AddComponent<PooledObject>();
            }
            pooledObj.Init(prefab);
        }

        obj.transform.SetPositionAndRotation(pos, rot);
        obj.SetActive(true);

        // 자식 포함 모든 IPoolable의 OnSpawn 호출 (비활성 자식도 포함)
        IPoolable[] poolables = obj.GetComponentsInChildren<IPoolable>(true);
        foreach (IPoolable poolable in poolables)
        {
            poolable.OnSpawn();
        }

        return obj;
    }

    /// <summary>
    /// 프리팹을 미리 생성하여 풀에 넣는다. 자식 포함 모든 IPoolable의 OnSpawn이 호출
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="count"></param>
    public void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null || count == 0)
        {
            return;
        }

        if (!m_Pools.ContainsKey(prefab))
        {
            m_Pools[prefab] = new Queue<GameObject>();
        }

        for (int i = 0; i < count; ++i)
        {
            GameObject obj = Instantiate(prefab);
            if (!obj.TryGetComponent(out PooledObject pooledObj))
            {
                pooledObj = obj.AddComponent<PooledObject>();
            }
            pooledObj.Init(prefab);
            obj.SetActive(false);
            m_Pools[prefab].Enqueue(obj);
        }
    }

    /// <summary>
    /// 오브젝트를 풀로 반납한다. 자식 포함 모든 IPoolable의 OnDespawn이 호출
    /// </summary>
    public void Return(GameObject prefab, GameObject obj)
    {
        if (!m_Pools.ContainsKey(prefab))
        {
            Destroy(obj);
            return;
        }

        IPoolable[] poolables = obj.GetComponentsInChildren<IPoolable>(true);
        foreach (IPoolable poolable in poolables)
        {
            poolable.OnDespawn();
        }

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