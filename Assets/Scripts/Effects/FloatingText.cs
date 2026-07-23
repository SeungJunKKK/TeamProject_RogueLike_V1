using UnityEngine;

public class FloatingText : MonoBehaviour, IPoolable
{
    public float MoveSpeed = 2f;
    public float LifeTime = 1f;

    private float m_Timer;
    private PooledObject m_Pooled;

    public void OnSpawn()
    {
        m_Timer = 0f;
    }

    public void OnDespawn()
    {
    }

    private void Update()
    {
        transform.Translate(Vector3.up * MoveSpeed * Time.deltaTime, Space.World);

        m_Timer += Time.deltaTime;

        if (m_Timer >= LifeTime)
        {
            if (m_Pooled == null)
            {
                m_Pooled = GetComponent<PooledObject>();
            }

            m_Pooled.Return();
        }
    }
}