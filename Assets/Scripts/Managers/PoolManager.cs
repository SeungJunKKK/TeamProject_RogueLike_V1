using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<GameObject, Queue<GameObject>> m_Pools = new Dictionary<GameObject, Queue<GameObject>>();

    public GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!m_Pools.ContainsKey(prefab))
            m_Pools[prefab] = new Queue<GameObject>();

        GameObject obj;
        if (m_Pools[prefab].Count > 0)
            obj = m_Pools[prefab].Dequeue();
        else
        {
            obj = Instantiate(prefab);
            obj.AddComponent<PooledObject>().Init(prefab);
        }


        obj.transform.SetPositionAndRotation(pos, rot);
        obj.SetActive(true);


        obj.GetComponent<IPoolable>()?.OnSpawn();

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
                Destroy(q.Dequeue());
        }
        m_Pools.Clear();
    }
}
