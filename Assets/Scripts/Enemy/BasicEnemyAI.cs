using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(PooledObject))]
public class BasicEnemyAI : EnemyBase, IPoolable, IDamageable
{
    [Header("Target Setting")]
    public Transform Player;

    [Header("Stats Setting (기본 스탯)")]
    [SerializeField] private float m_BaseHp = 80f;
    [SerializeField] private float m_BaseDamage = 12f;
    [SerializeField] private int m_BaseGold = 2;

    private float m_MaxHp;
    private float m_CurrentHp;
    private float m_Damage;

    [Header("Movement Setting")]
    public float MoveSpeed = 3f;
    public float JumpForce = 6f;
    public float JumpCooldown = 0.5f;
    private float m_LastJumpTime;
    public float AttackRange = 1.5f;
    public float AttackCooldown = 2f;

    [Header("Raycast Sensors (자동 계산)")]
    public float GroundRayLength = 1.5f;
    public float WallRayLength = 0.2f;
    public LayerMask GroundLayer;

    [Header("Separation Setting (로컬 회피)")]
    public float SeparationRadius = 0.8f;
    public float SeparationForce = 2.5f;
    public LayerMask EnemyLayer;

    private readonly Collider2D[] m_NearbyEnemies = new Collider2D[10];

    private Rigidbody2D m_Rigidbody;
    private CapsuleCollider2D m_Collider;
    private PooledObject m_Pooled;

    private bool m_IsFacingRight = true;
    private bool m_IsGrounded = true;
    private bool m_IsAttacking = false;
    private bool m_IsDead = false;
    private bool m_IsKnockback = false;

    private Coroutine m_KnockbackCoroutine;

    private const float k_GroundCheckDistance = 0.1f;
    private const float k_KnockbackDuration = 0.25f;
    private const float k_AttackWindupDelay = 0.5f;
    private const float k_SeparationThreshold = 0.01f;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Collider = GetComponent<CapsuleCollider2D>();
        m_Pooled = GetComponent<PooledObject>();
    }

    public override void SetTarget(Transform target)
    {
        Player = target;
    }

    public void OnSpawn()
    {
        ResetState();
        ResetPhysics();
        ApplyDifficulty();
    }

    private void ResetState()
    {
        m_IsFacingRight = true;
        m_IsAttacking = false;
        m_IsGrounded = true;
        m_IsDead = false;
        m_IsKnockback = false;
        m_LastJumpTime = 0f;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    private void ResetPhysics()
    {
        m_Rigidbody.linearVelocity = Vector2.zero;
        m_Rigidbody.angularVelocity = 0f;
        m_Rigidbody.rotation = 0f;
    }

    private void ApplyDifficulty()
    {
        var difficulty = DifficultyManager.Instance;
        if (difficulty != null)
        {
            m_MaxHp = m_BaseHp * difficulty.GetHPMultiplier();
            m_Damage = m_BaseDamage * difficulty.Coefficient;
        }
        else
        {
            m_MaxHp = m_BaseHp;
            m_Damage = m_BaseDamage;
        }

        m_CurrentHp = m_MaxHp;
    }

    public void OnDespawn()
    {
        StopAllCoroutines();
        m_Rigidbody.linearVelocity = Vector2.zero;
        m_IsAttacking = false;
        m_IsKnockback = false;
    }

    public void TakeDamage(DamageInfo info)
    {
        if (m_IsDead) return;

        m_CurrentHp -= info.Amount;

        EventBus.Publish(new MonsterDamagedEvent
        {
            Amount = info.Amount,
            HitPoint = info.HitPoint,
            IsCrit = info.IsCrit
        });

        if (info.KnockbackForce > 0f)
        {
            m_Rigidbody.linearVelocity = Vector2.zero;
            m_Rigidbody.AddForce(info.HitDirection * info.KnockbackForce, ForceMode2D.Impulse);

            if (m_KnockbackCoroutine != null)
            {
                StopCoroutine(m_KnockbackCoroutine);
            }
            m_KnockbackCoroutine = StartCoroutine(KnockbackRoutine());
        }

        if (m_CurrentHp <= 0)
        {
            Die();
        }
    }

    private IEnumerator KnockbackRoutine()
    {
        m_IsKnockback = true;
        yield return new WaitForSeconds(KnockbackDuration);
        m_IsKnockback = false;
        m_KnockbackCoroutine = null;
    }

    public void Die()
    {
        if (m_IsDead) return;
        m_IsDead = true;

        float goldMultiplier = DifficultyManager.Instance != null ? DifficultyManager.Instance.GetGoldMultiplier() : 1f;
        int finalGold = Mathf.Max(1, Mathf.FloorToInt(m_BaseGold * goldMultiplier));
        float finalExp = finalGold * 0.5f;

        EventBus.Publish(new MonsterDiedEvent
        {
            Exp = finalExp,
            Gold = finalGold,
            Position = transform.position
        });

        EnemySpawner.Instance?.UnregisterEnemy(gameObject);
        m_Pooled.Return();
    }

    private void Update()
    {
        if (Player == null || m_IsAttacking || m_IsDead || m_IsKnockback) return;

        Vector2 bottomCenter = new Vector2(m_Collider.bounds.center.x, m_Collider.bounds.min.y);
        m_IsGrounded = Physics2D.Raycast(bottomCenter, Vector2.down, GroundCheckDistance, GroundLayer);

        float distanceToPlayer = Vector2.Distance(transform.position, Player.position);

        if (distanceToPlayer > AttackRange)
        {
            ChasePlayer();
        }
        else if (m_IsGrounded)
        {
            StartCoroutine(AttackRoutine());
        }
        else
        {
            ChasePlayer();
        }
    }

    private void ChasePlayer()
    {
        int direction = Player.position.x > transform.position.x ? 1 : -1;

        if ((direction == 1 && !m_IsFacingRight) || (direction == -1 && m_IsFacingRight))
        {
            Flip();
        }

        float separationX = CalculateSeparation();
        float finalVelocityX = direction * MoveSpeed + separationX;

        if (!m_IsGrounded)
        {
            m_Rigidbody.linearVelocity = new Vector2(finalVelocityX, m_Rigidbody.linearVelocity.y);
            return;
        }

        float frontX = m_IsFacingRight ? m_Collider.bounds.max.x : m_Collider.bounds.min.x;

        Vector2 cliffCheckPos = new Vector2(frontX, m_Collider.bounds.min.y);
        Vector2 wallCheckPos = new Vector2(frontX, m_Collider.bounds.center.y - (m_Collider.bounds.extents.y * 0.5f));

        Vector2 wallDir = m_IsFacingRight ? Vector2.right : Vector2.left;

        bool isGroundAhead = Physics2D.Raycast(cliffCheckPos, Vector2.down, GroundRayLength, GroundLayer);
        bool isWallAhead = Physics2D.Raycast(wallCheckPos, wallDir, WallRayLength, GroundLayer);

        if (isWallAhead || !isGroundAhead)
        {
            JumpIfNeeded();
            return;
        }

        m_Rigidbody.linearVelocity = new Vector2(finalVelocityX, m_Rigidbody.linearVelocity.y);
    }

    private void JumpIfNeeded()
    {
        if (Time.time - m_LastJumpTime >= JumpCooldown)
        {
            Jump();
        }
        else
        {
            StopMoving();
        }
    }

    private void Jump()
    {
        m_LastJumpTime = Time.time;
        m_IsGrounded = false;
        m_Rigidbody.linearVelocity = new Vector2(m_Rigidbody.linearVelocity.x, 0f);
        m_Rigidbody.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
    }

    private void StopMoving()
    {
        m_Rigidbody.linearVelocity = new Vector2(0f, m_Rigidbody.linearVelocity.y);
    }

    private float CalculateSeparation()
    {
        float separationForceX = 0f;

        int count = Physics2D.OverlapCircleNonAlloc(transform.position, SeparationRadius, m_NearbyEnemies, EnemyLayer);

        for (int i = 0; i < count; i++)
        {
            Collider2D enemy = m_NearbyEnemies[i];
            if (enemy.gameObject == gameObject) continue;

            float diffX = transform.position.x - enemy.transform.position.x;
            if (Mathf.Abs(diffX) < SeparationThreshold)
            {
                diffX = Random.Range(-SeparationThreshold, SeparationThreshold);
            }

            float force = Mathf.Sign(diffX) * (SeparationRadius - Mathf.Abs(diffX)) * SeparationForce;
            separationForceX += force;
        }

        return Mathf.Clamp(separationForceX, -MoveSpeed, MoveSpeed);
    }

    private IEnumerator AttackRoutine()
    {
        m_IsAttacking = true;
        StopMoving();

        yield return new WaitForSeconds(AttackWindupDelay);

        if (Player == null || m_IsDead)
        {
            m_IsAttacking = false;
            yield break;
        }

        if (Vector2.Distance(transform.position, Player.position) <= AttackRange)
        {
            Debug.Log($"플레이어를 {m_Damage} 의 데미지로 공격.");
        }

        yield return new WaitForSeconds(AttackCooldown);

        m_IsAttacking = false;
    }

    private void Flip()
    {
        m_IsFacingRight = !m_IsFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void OnDrawGizmos()
    {
        if (m_Collider == null) m_Collider = GetComponent<CapsuleCollider2D>();
        if (m_Collider == null) return;

        float frontX = m_IsFacingRight ? m_Collider.bounds.max.x : m_Collider.bounds.min.x;

        Vector2 cliffCheckPos = new Vector2(frontX, m_Collider.bounds.min.y);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(cliffCheckPos, cliffCheckPos + Vector2.down * GroundRayLength);

        Vector2 wallCheckPos = new Vector2(frontX, m_Collider.bounds.center.y - (m_Collider.bounds.extents.y * 0.5f));
        Vector2 wallDir = m_IsFacingRight ? Vector2.right : Vector2.left;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(wallCheckPos, wallCheckPos + wallDir * WallRayLength);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, SeparationRadius);
    }
}