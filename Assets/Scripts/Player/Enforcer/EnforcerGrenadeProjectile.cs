using UnityEngine;

public class EnforcerGrenadeProjectile : MonoBehaviour, IPoolable
{
    private Rigidbody2D m_Rb;
    private Animator m_Anim;
    private PooledObject m_PooledObj;

    private float m_Damage;
    private bool m_IsExploded = false;
    private bool m_IsStrengthened;

    private Vector2 m_StartPosition;
    private float m_MaxFlightDistance = 11.25f; // 360픽셀 / 32 PPU = 11.25 유닛
    [SerializeField] private float m_MaxLifetime = 3f;
    private float m_LifeTimer = 0f;

    [SerializeField]
    private string m_ExplosionSFXAddress = "";
    private int m_BounceCount = 0;
    
    [SerializeField] private GameObject m_DustPrefab;
    private float m_DustSpawnTimer = 0f;

    private void Awake()
    {
        m_Rb = GetComponent<Rigidbody2D>();
        m_Anim = GetComponentInChildren<Animator>();
        m_PooledObj = GetComponent<PooledObject>();
    }

    public void OnSpawn()
    {
        m_IsExploded = false;
        m_BounceCount = 0;
        m_DustSpawnTimer = 0f;
        m_LifeTimer = 0f;
    }

    public void OnDespawn()
    {
        m_Rb.linearVelocity = Vector2.zero; 
    }

    public void Initialize(Vector2 direction, float damage, bool isStrengthened)
    {
        m_Damage = damage;
        m_IsStrengthened = isStrengthened;
        m_StartPosition = transform.position;

        float throwForceX = direction.x > 0 ? 18f : -18f;
        float throwForceY = 1f;
        m_Rb.linearVelocity = new Vector2(throwForceX, throwForceY);
    }

    private void Update()
    {
        if (m_IsExploded) return;

        m_DustSpawnTimer += Time.deltaTime;
        if (m_DustSpawnTimer >= 0.03f)
        {
            m_DustSpawnTimer = 0f;
            SpawnDustEffect();
        }
        m_LifeTimer += Time.deltaTime;

        bool reachedMaxDistance = Vector2.Distance(m_StartPosition, transform.position) >= m_MaxFlightDistance;
        bool reachedMaxLifetime = m_LifeTimer >= m_MaxLifetime;

        if (reachedMaxDistance || reachedMaxLifetime)
        {
            StartCoroutine(ExplodeRoutine());
        }
    }
    private void SpawnDustEffect()
    {
        if (m_DustPrefab != null)
        {
            PoolManager.Instance.Get(m_DustPrefab, transform.position, Quaternion.identity);
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_IsExploded)
        {
            return;
        }
            
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (m_BounceCount == 0)
            {
                m_BounceCount++;
                float currentDirX = Mathf.Sign(m_Rb.linearVelocity.x);

                m_Rb.linearVelocity = new Vector2(currentDirX * 15f, 5.5f);
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") ||
                  collision.gameObject.layer == LayerMask.NameToLayer("FlyingEnemy"))
        {
            // 적에게 직격 시 즉시 폭발
            StartCoroutine(ExplodeRoutine());
        }
    }

    private System.Collections.IEnumerator ExplodeRoutine()
    {
        m_IsExploded = true;
        m_Rb.linearVelocity = Vector2.zero;
        m_Rb.simulated = false;

        if (m_Anim != null)
        {
            m_Anim.Play("Grenade_Explosion");
        }

        if (!string.IsNullOrEmpty(m_ExplosionSFXAddress))
        {
            AddressableManager.Instance.LoadAssetAsync<AudioClip>(m_ExplosionSFXAddress, (clip) =>
            {
                if (clip != null)
                {
                    SoundManager.Instance.PlaySFX(clip);
                }
                else
                {
                    Debug.LogError($"[어드레서블 에러] '{m_ExplosionSFXAddress}' 효과음을 찾을 수 없습니다!");
                }
            });
        }

        float explosionRadius = 2.5f;

        int enemyLayer = LayerMask.GetMask("Enemy", "FlyingEnemy");
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);

        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                DamageInfo info = new DamageInfo
                {
                    Amount = m_Damage,
                    HitPoint = hit.transform.position,
                    HitDirection = (hit.transform.position - transform.position).normalized,
                    KnockbackForce = 1.5f,
                    Attacker = gameObject,
                    IsCrit = false,
                    CanProc = true
                };

                damageable.TakeDamage(info);

                //if (m_IsStrengthened)
                //    Debug.Log($"<color=purple>[최루탄 강화] {hit.name}에게 공포 효과 부여!</color>");
                //else
                //    Debug.Log($"<color=orange>[최루탄 일반] {hit.name} 마비!</color>");
            }
        }

        yield return new WaitForSeconds(0.5f);

        if (m_PooledObj != null)
        {
            m_PooledObj.Return();
        }
    }

    private void ApplyCrowdControl(GameObject enemy)
    {
        //if (m_IsStrengthened)
        //    Debug.Log($"<color=purple>[최루탄 강화] {enemy.name}에게 공포 효과 부여!</color>");
        //else
        //    Debug.Log($"<color=orange>[최루탄 일반] {enemy.name} 마비!</color>");
    }
}