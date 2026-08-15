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

        // 물리 충돌이 아닌 트리거(Trigger) 판정으로 설정
        m_Collider.isTrigger = true;
        m_Rigidbody.isKinematic = true;
    }

    // 골렘의 Animation Event(SpawnShockwave)에서 호출되어 데이터 주입
    public void Init(float damage, Vector2 direction, GameObject attacker)
    {
        m_Damage = damage;
        m_Direction = direction.normalized;
        m_Attacker = attacker;
        m_LifeTimer = 0f;

        // 방향에 따른 이펙트 스프라이트 좌우 반전
        transform.localScale = new Vector3(Mathf.Sign(direction.x) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    private void Update()
    {
        m_LifeTimer += Time.deltaTime;

        // 수명이 다하면 소멸 (최대 사거리 도달)
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

        // 지형 충돌 검사: 지형에 닿으면 즉시 소멸
        if (((1 << collision.gameObject.layer) & m_GroundLayer) != 0)
        {
            ReturnToPool();
            return;
        }

        // 데미지 대상 검사 (IDamageable 인터페이스 활용)
        if (collision.TryGetComponent(out IDamageable target))
        {
            // 공격자 자신이나 다른 적들은 맞추지 않음
            if (collision.gameObject == m_Attacker || collision.gameObject.layer == LayerMask.NameToLayer("Enemy")) return;

            // DamageInfo 구조체 생성 및 데이터 주입
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

            // 타겟에게 데미지 정보 전달
            target.TakeDamage(info);

            // 한 번 타격하면 즉시 소멸 (단발성 직선 공격)
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        m_Rigidbody.linearVelocity = Vector2.zero;

        // PoolManager 시스템이 붙어있다면 풀로 반환, 아니면 파괴
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