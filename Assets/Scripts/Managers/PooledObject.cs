using UnityEngine;

public class PooledObject : MonoBehaviour
{
    private GameObject m_Prefab;

    public void Init(GameObject prefab)
    {
        m_Prefab = prefab;
    }

    public void Return()
    {
        PoolManager.Instance.Return(m_Prefab, gameObject);
    }
}
