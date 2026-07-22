using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    private float m_Speed;
    private float m_Damage;
    private Vector2 m_Direction;
    private bool m_IsPiercing;

    private PooledObject m_Pooled;
    private float m_Timer;
    public float LifeTime = 2f;


    public void Setup(Vector2 direction, float speed, float damage, bool isPiercing)
    {
        m_Direction = direction;
        m_Speed = speed;
        m_Damage = damage;
        m_IsPiercing = isPiercing; 
    }

    public void OnSpawn()
    {
        m_Timer = 0f;
    }

    public void OnDespawn() { }

    private void Update()
    {
        transform.Translate(m_Direction * m_Speed * Time.deltaTime, Space.World);

        m_Timer += Time.deltaTime;
        if (m_Timer >= LifeTime)
        {
            m_Pooled ??= GetComponent<PooledObject>();
            m_Pooled.Return(); 
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            TestDummyHealth dummy = other.GetComponent<TestDummyHealth>();
            if (dummy != null)
            {
                dummy.TakeDamage(m_Damage, m_Direction, 5f);
            }

            if (!m_IsPiercing)
            {
                m_Pooled ??= GetComponent<PooledObject>();
                m_Pooled.Return();
            }
           
        }
    }
}