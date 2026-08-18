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

    private float m_Damage;
    private Vector2 m_Direction;
    private GameObject m_Attacker;
    private float m_LifeTimer;

    private Rigidbody2D m_Rigidbody;
    private BoxCollider2D m_Collider;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Collider = GetComponent<BoxCollider2D>();

        m_Collider.isTrigger = true;
        m_Rigidbody.isKinematic = true;
    }

    public void Init(float damage, Vector2 direction, GameObject attacker)
    {
        m_Damage = damage;
        m_Direction = direction.normalized;
        m_Attacker = attacker;
        m_LifeTimer = 0f;

        transform.localScale = new Vector3(Mathf.Sign(direction.x) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    private void Update()
    {
        m_LifeTimer += Time.deltaTime;

        if (m_LifeTimer >= m_Lifetime)
        {
            ReturnToPool();
            return;
        }

        // 등속도 전진
        m_Rigidbody.linearVelocity = m_Direction * m_Speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(
       $"[RockShockwave] 충돌: {collision.name} / " +
       $"Layer: {LayerMask.LayerToName(collision.gameObject.layer)} / " +
       $"IDamageable: {collision.GetComponent<IDamageable>() != null}"
   );

        if (((1 << collision.gameObject.layer) & m_GroundLayer) != 0)
        {
            ReturnToPool();
            return;
        }

        if (collision.gameObject == m_Attacker || collision.gameObject.layer == LayerMask.NameToLayer("Enemy")) return;


        //if (collision.TryGetComponent(out PlayerController player))
        //{
        //    player.TakeDamage(m_Damage);
        //    ReturnToPool();
        //    return;
        //}

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
            ReturnToPool();
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