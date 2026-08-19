using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class RockShockwave : MonoBehaviour
{
    [Header("Shockwave Settings")]
    [SerializeField] private float m_Speed = 8f;         // 충격파 전진 속도
    [SerializeField] private float m_Lifetime = 1.2f;    // 충격파 유지 시간 (사거리 결정)
    [SerializeField] private float m_KnockbackForce = 15f; // 넉백 세기

    [Header("Collision Setting")]
    [SerializeField] private LayerMask m_GroundLayer;    // 지형 레이어

    [Header("Terrain Follow Settings")]
    [Tooltip("파동이 평지에서도 공중에 떠 있다면 이 값을 조절하세요!")]
    [SerializeField] private float m_HeightOffset = 0f;

    private float m_Damage;
    private Vector2 m_Direction;
    private GameObject m_Attacker;
    private float m_LifeTimer;

    private Rigidbody2D m_Rigidbody;
    private BoxCollider2D m_Collider;
    private SpriteRenderer m_SpriteRenderer;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Collider = GetComponent<BoxCollider2D>();
        m_SpriteRenderer = GetComponent<SpriteRenderer>();

        m_Collider.isTrigger = true;
        m_Rigidbody.bodyType = RigidbodyType2D.Kinematic;
    }

    public void Init(float damage, Vector2 direction, GameObject attacker, bool isElite)
    {
        m_Damage = damage;
        m_Direction = direction.normalized;
        m_Attacker = attacker;
        m_LifeTimer = 0f;

        transform.localScale = new Vector3(Mathf.Sign(direction.x) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        if (m_SpriteRenderer != null)
        {
            m_SpriteRenderer.color = isElite ?  Color.red : Color.white;
        }
    }

    private void Update()
    {
        m_LifeTimer += Time.deltaTime;

        if (m_LifeTimer >= m_Lifetime)
        {
            ReturnToPool();
            return;
        }

        Vector2 currentPos = transform.position;

        Vector2 forwardRayOrigin = currentPos + Vector2.up * 0.5f;
        int wallLayer = LayerMask.GetMask("Wall");
        RaycastHit2D wallHit = Physics2D.Raycast(forwardRayOrigin, m_Direction, 0.5f, wallLayer);

        if (wallHit.collider != null)
        {
            ReturnToPool();
            return;
        }

        Vector2 nextPos = currentPos + (m_Direction * m_Speed * Time.deltaTime);
        Vector2 downRayOrigin = nextPos + Vector2.up * 1.5f;
        // int groundLayer = LayerMask.GetMask("Ground", "OneWayGround");
        //RaycastHit2D groundHit = Physics2D.Raycast(downRayOrigin, Vector2.down, 3f, groundLayer);
        RaycastHit2D groundHit = Physics2D.Raycast(downRayOrigin, Vector2.down, 3f, m_GroundLayer);

        if (groundHit.collider != null)
        {
            nextPos.y = groundHit.point.y + m_HeightOffset;
            transform.up = groundHit.normal;
        }
        else
        {
            ReturnToPool();
            return;
        }

        transform.position = nextPos;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int hitLayer = collision.gameObject.layer;

        if (hitLayer == LayerMask.NameToLayer("Wall") || hitLayer == LayerMask.NameToLayer("Default"))
        {
            ReturnToPool();
            return;
        }

        if (collision.gameObject == m_Attacker ||
            hitLayer == LayerMask.NameToLayer("Enemy") ||
            hitLayer == LayerMask.NameToLayer("FlyingEnemy"))
        {
            return;
        }

        if (collision.TryGetComponent(out IDamageable target))
        {
            DamageInfo info = new DamageInfo
            {
                Amount = m_Damage,
                HitPoint = collision.bounds.center,
                HitDirection = m_Direction,
                KnockbackForce = m_KnockbackForce,
                Attacker = m_Attacker,
                IsCrit = false,
                CanProc = false
            };

            target.TakeDamage(info);

            //ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        m_Rigidbody.linearVelocity = Vector2.zero;

        if (TryGetComponent(out PooledObject pooledObj))
        {
            pooledObj.Return();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}