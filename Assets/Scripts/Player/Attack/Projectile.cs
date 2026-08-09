using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    private SpawnProjectileEvent m_Data;
    private PooledObject m_Pooled;
    private float m_Timer;
    public float LifeTime = 2f;

    public void Setup(SpawnProjectileEvent data)
    {
        m_Data = data;
    }

    public void OnSpawn() { m_Timer = 0f; }
    public void OnDespawn() { }

    private void Update()
    {
        transform.Translate(m_Data.Direction * m_Data.Speed * Time.deltaTime, Space.World);

        m_Timer += Time.deltaTime;
        if (m_Timer >= LifeTime)
        {
            m_Pooled ??= GetComponent<PooledObject>();
            m_Pooled.Return();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"총알 충돌 감지됨! 부딪힌 대상: {other.gameObject.name} / 태그: {other.tag}");
        if (other.CompareTag("Enemy"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null) 
            {
                DamageInfo finalInfo = m_Data.AttackData; 
                finalInfo.HitPoint = transform.position; 

                damageable.TakeDamage(finalInfo);

                if (finalInfo.Attacker != null)
                {
                    PlayerController player = finalInfo.Attacker.GetComponent<PlayerController>();
                    if (player != null)
                    {
                        player.OnEnemyHit(other.gameObject, finalInfo.Amount);
                    }
                }
            }

            if (!m_Data.IsPiercing) //[cite: 6]
            {
                m_Pooled ??= GetComponent<PooledObject>(); //[cite: 6]
                m_Pooled.Return(); //[cite: 6]
            }
        }
    }
}