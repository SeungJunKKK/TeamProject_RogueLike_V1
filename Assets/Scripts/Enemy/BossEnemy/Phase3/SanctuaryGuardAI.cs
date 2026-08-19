using System.Collections;
using UnityEngine;

public class SanctuaryGuardAI : EnemyBase
{
    [Header("Guard Combat Settings")]
    [SerializeField] private GameObject m_BulletPrefab;
    [SerializeField] private Transform m_FirePoint;
    [SerializeField] private float m_AttackCooldown = 3.0f;
    [SerializeField] private float m_Damage = 10f;

    [Header("Guard Movement Settings")]
    [SerializeField] private float m_MoveSpeed = 3f;
    [SerializeField] private float m_PreferredRange = 5.0f;

    [Header("References")]
    [SerializeField] private Animator m_Animator;

    // 💡 부모 클래스와 충돌 및 접근 권한 문제를 피하기 위해 변수명을 고유하게 변경
    [SerializeField] private SpriteRenderer m_GuardSprite;

    private Transform m_Player;
    private bool m_IsGuardActive = false;
    private bool m_IsAttacking = false;
    private bool m_IsFirstAttack = true;
    private Coroutine m_CombatRoutine;

    private void Awake()
    {
        if (m_Animator == null) m_Animator = GetComponentInChildren<Animator>();
        if (m_GuardSprite == null) m_GuardSprite = GetComponentInChildren<SpriteRenderer>();
    }

    public override void SetTarget(Transform target)
    {
        m_Player = target;
    }

    public void InitGuard(Transform player, float difficultyCoefficient)
    {
        base.OnSpawn();
        SetTarget(player);
        m_Damage *= difficultyCoefficient;
        m_IsGuardActive = true;
        m_IsFirstAttack = true;

        if (m_CombatRoutine != null) StopCoroutine(m_CombatRoutine);
        m_CombatRoutine = StartCoroutine(CoGuardCombatLoop());
    }

    protected void Update()
    {
        if (!m_IsGuardActive || m_Player == null) return;

        if (!m_IsAttacking)
        {
            HandleMovement();
        }
        else
        {
            if (m_Rigidbody != null) m_Rigidbody.linearVelocity = new Vector2(0f, m_Rigidbody.linearVelocity.y);
            if (m_Animator != null) m_Animator.SetBool("IsWalk", false);
        }
    }

    private void HandleMovement()
    {
        float distanceX = m_Player.position.x - transform.position.x;
        int dir = distanceX > 0 ? 1 : -1;

        // 💡 고유 변수명인 m_GuardSprite 사용
        if (m_GuardSprite != null)
        {
            m_GuardSprite.flipX = (dir < 0);
        }

        float currentDist = Mathf.Abs(distanceX);
        float moveInput = 0f;

        if (currentDist > m_PreferredRange + 1f) moveInput = dir;
        else if (currentDist < m_PreferredRange - 1f) moveInput = -dir;

        if (m_Rigidbody != null)
        {
            m_Rigidbody.linearVelocity = new Vector2(moveInput * m_MoveSpeed, m_Rigidbody.linearVelocity.y);
        }

        if (m_Animator != null)
        {
            bool isActuallyMoving = Mathf.Abs(moveInput) > 0.1f && Mathf.Abs(m_Rigidbody.linearVelocity.x) > 0.1f;
            m_Animator.SetBool("IsWalk", isActuallyMoving);
        }
    }

    private IEnumerator CoGuardCombatLoop()
    {
        yield return new WaitForSeconds(1.0f);

        while (m_IsGuardActive)
        {
            yield return new WaitForSeconds(m_AttackCooldown);

            if (m_Player == null) continue;

            m_IsAttacking = true;
            if (m_Animator != null)
            {
                m_Animator.SetBool("IsWalk", false);
                m_Animator.SetTrigger("ShootTrigger");
            }

            if (m_IsFirstAttack)
            {
                ShootTriangleBurst();
                m_IsFirstAttack = false;
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    ShootSingleBullet();
                    yield return new WaitForSeconds(0.18f);
                }
            }

            yield return new WaitForSeconds(0.5f);
            m_IsAttacking = false;
        }
    }

    private void ShootTriangleBurst()
    {
        if (m_BulletPrefab == null || m_Player == null) return;

        Vector3 spawnPos = m_FirePoint != null ? m_FirePoint.position : transform.position;
        Vector2 baseDir = ((Vector2)m_Player.position - (Vector2)spawnPos).normalized;

        float[] angleOffsets = { -15f, 0f, 15f };

        foreach (float offsetAngle in angleOffsets)
        {
            Vector2 spreadDir = Quaternion.Euler(0, 0, offsetAngle) * baseDir;
            GameObject bulletObj = Instantiate(m_BulletPrefab, spawnPos, Quaternion.identity);

            if (bulletObj.TryGetComponent(out EnemyBullet bullet))
            {
                bullet.Setup(spreadDir, 1f);
            }
        }
    }

    private void ShootSingleBullet()
    {
        if (m_BulletPrefab == null || m_Player == null) return;

        Vector3 spawnPos = m_FirePoint != null ? m_FirePoint.position : transform.position;
        GameObject bulletObj = Instantiate(m_BulletPrefab, spawnPos, Quaternion.identity);

        if (bulletObj.TryGetComponent(out EnemyBullet bullet))
        {
            Vector2 dir = ((Vector2)m_Player.position - (Vector2)spawnPos).normalized;
            bullet.Setup(dir, 1f);
        }
    }

    protected override void Die()
    {
        if (!m_IsGuardActive) return; // 중복 호출 방지
        m_IsGuardActive = false;

        if (m_CombatRoutine != null) StopCoroutine(m_CombatRoutine);

        // 💡 1. 죽는 순간 이동 및 물리 멈추기 (그 자리에서 굳기)
        if (m_Rigidbody != null)
        {
            m_Rigidbody.linearVelocity = Vector2.zero;
            m_Rigidbody.simulated = false; // 더 이상 물리 작용을 받지 않음
        }

        // 💡 2. 죽은 뒤 플레이어 공격이나 투사체가 시체에 막히지 않도록 콜라이더 끄기
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // 💡 3. 사망 애니메이션 재생
        if (m_Animator != null)
        {
            m_Animator.SetTrigger("Death");
        }

        // 💡 4. 일정 시간(예: 1.2초, 사망 애니메이션 길이에 맞춤) 동안 대기 후 오브젝트 파괴
        StartCoroutine(CoDestroyRoutine(1.2f));

        base.Die();
    }

    private IEnumerator CoDestroyRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject); // 시체 오브젝트 삭제
    }
}