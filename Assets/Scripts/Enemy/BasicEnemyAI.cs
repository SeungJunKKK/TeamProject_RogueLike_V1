using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
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
    public float Acceleration = 15f; // 가속도
    public float JumpForce = 6f;
    public float JumpCooldown = 0.5f;
    private float m_LastJumpTime;
    public float AttackRange = 1.5f;
    public float AttackCooldown = 2f;

    [Header("Raycast Sensors (자동 계산)")]
    public float GroundRayLength = 1.5f;
    public float WallRayLength = 0.2f;
    public LayerMask GroundLayer;

    [Header("Steering & Separation Setting")]
    public float SeparationRadius = 0.8f;
    public float SeparationForce = 2.5f;
    [Range(0f, 1f)] public float AllyBlockPenalty = 0.3f;
    public float ArrivalRadius = 1.2f;
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
        m_KnockbackCoroutine = null;
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
        yield return new WaitForSeconds(k_KnockbackDuration);
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

        if (m_Pooled != null)
        {
            m_Pooled.Return();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Player == null || m_IsDead || m_IsKnockback) return;

        Vector2 bottomCenter = new Vector2(m_Collider.bounds.center.x, m_Collider.bounds.min.y);
        m_IsGrounded = Physics2D.Raycast(bottomCenter, Vector2.down, k_GroundCheckDistance, GroundLayer);

        float distanceToPlayer = Vector2.Distance(transform.position, Player.position);
        int direction = Player.position.x > transform.position.x ? 1 : -1;

        if (m_IsAttacking)
        {
            // Error Steering (공격 중 앞뒤 미세 이동)
            float desiredDistance = AttackRange * 0.8f;
            float error = distanceToPlayer - desiredDistance;
            float attackMove = Mathf.Clamp(error, -MoveSpeed * 0.4f, MoveSpeed * 0.4f);

            Move(direction * attackMove);
            return;
        }

        if (distanceToPlayer > AttackRange)
        {
            // Arrival Steering (목표 접근 시 감속)
            float distanceToAttack = distanceToPlayer - AttackRange;
            float desiredSpeed = MoveSpeed;

            if (distanceToAttack < ArrivalRadius)
            {
                float arrivalRatio = distanceToAttack / ArrivalRadius;
                desiredSpeed = MoveSpeed * Mathf.Max(arrivalRatio, 0.2f);
            }

            ChasePlayer(direction, desiredSpeed);
        }
        else if (m_IsGrounded)
        {
            StartCoroutine(AttackRoutine());
        }
        else
        {
            ChasePlayer(direction, MoveSpeed);
        }
    }

    private void Move(float desiredMoveX)
    {
        float frontX = m_IsFacingRight ? m_Collider.bounds.max.x : m_Collider.bounds.min.x;
        Vector2 forwardCheckPos = new Vector2(frontX, m_Collider.bounds.center.y);
        Vector2 forwardDir = m_IsFacingRight ? Vector2.right : Vector2.left;

        // 전방 아군 감속 (방향 체크)
        RaycastHit2D allyAhead = Physics2D.Raycast(forwardCheckPos, forwardDir, 0.4f, EnemyLayer);
        if (allyAhead.collider != null && allyAhead.collider.gameObject != gameObject)
        {
            bool isMovingForward = (m_IsFacingRight && desiredMoveX > 0f) || (!m_IsFacingRight && desiredMoveX < 0f);
            if (isMovingForward)
            {
                desiredMoveX *= AllyBlockPenalty;
            }
        }

        float separationX = CalculateSeparation();

        // 최종적으로 목표하는 속도
        float targetVelocityX = desiredMoveX + separationX;

        // 부드러운 가속도 적용
        float currentVelocityX = m_Rigidbody.linearVelocity.x;
        float smoothedVelocityX = Mathf.MoveTowards(currentVelocityX, targetVelocityX, Acceleration * Time.deltaTime);

        m_Rigidbody.linearVelocity = new Vector2(smoothedVelocityX, m_Rigidbody.linearVelocity.y);
    }

    private void ChasePlayer(int direction, float currentSpeed)
    {
        if ((direction == 1 && !m_IsFacingRight) || (direction == -1 && m_IsFacingRight))
        {
            Flip();
        }

        if (!m_IsGrounded)
        {
            Move(direction * currentSpeed);
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

        Move(direction * currentSpeed);
    }

    private void JumpIfNeeded()
    {
        if (Time.time - m_LastJumpTime >= JumpCooldown)
        {
            Jump();
        }
        else
        {
            // 점프 대기 시에도 서서히 감속되도록 유도
            float currentVelocityX = m_Rigidbody.linearVelocity.x;
            float smoothedVelocityX = Mathf.MoveTowards(currentVelocityX, 0f, Acceleration * Time.deltaTime);
            m_Rigidbody.linearVelocity = new Vector2(smoothedVelocityX, m_Rigidbody.linearVelocity.y);
        }
    }

    private void Jump()
    {
        m_LastJumpTime = Time.time;
        m_IsGrounded = false;
        m_Rigidbody.linearVelocity = new Vector2(m_Rigidbody.linearVelocity.x, 0f);
        m_Rigidbody.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
    }

    private float CalculateSeparation()
    {
        float separationForceX = 0f;
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, SeparationRadius, m_NearbyEnemies, EnemyLayer);

        for (int i = 0; i < count; i++)
        {
            Collider2D enemy = m_NearbyEnemies[i];
            if (enemy.gameObject == gameObject) continue;

            Vector2 diff = transform.position - enemy.transform.position;
            float distance = diff.magnitude;

            if (distance < k_SeparationThreshold)
            {
                diff = new Vector2(
                    Random.Range(-k_SeparationThreshold, k_SeparationThreshold),
                    Random.Range(-k_SeparationThreshold, k_SeparationThreshold)
                );
                distance = diff.magnitude;
            }

            float ratio = 1f - (distance / SeparationRadius);
            ratio = Mathf.Clamp01(ratio);

            float pushForce = SeparationForce * (ratio * ratio);

            Vector2 pushVector = (diff / distance) * pushForce;
            separationForceX += pushVector.x;
        }

        return Mathf.Clamp(separationForceX, -MoveSpeed * 1.5f, MoveSpeed * 1.5f);
    }

    private IEnumerator AttackRoutine()
    {
        m_IsAttacking = true;

        yield return new WaitForSeconds(k_AttackWindupDelay);

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