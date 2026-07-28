using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class BasicEnemyAI : EnemyBase // IPoolable, IDamageable은 EnemyBase에 있으므로 생략
{
    [Header("Target Setting")]
    public Transform Player;

    [Header("Stats Setting (고유 스탯)")]
    [SerializeField] private float m_BaseDamage = 12f;
    private float m_Damage;

    [Header("Movement Setting")]
    public float MoveSpeed = 3f;
    public float Acceleration = 15f;
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

    private bool m_IsFacingRight = true;
    private bool m_IsGrounded = true;
    private bool m_IsAttacking = false;

    private const float k_GroundCheckDistance = 0.1f;
    private const float k_AttackWindupDelay = 0.5f;
    private const float k_SeparationThreshold = 0.01f;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Collider = GetComponent<CapsuleCollider2D>();
    }

    public override void SetTarget(Transform target)
    {
        Player = target;
    }

    // ✅ 부모의 OnSpawn을 override 하여 사용
    public override void OnSpawn()
    {
        base.OnSpawn(); // EnemyBase의 HP 스케일링 호출

        ResetState();
        ResetPhysics();

        var difficulty = DifficultyManager.Instance;
        m_Damage = difficulty != null ? m_BaseDamage * difficulty.Coefficient : m_BaseDamage;
    }

    private void ResetState()
    {
        m_IsFacingRight = true;
        m_IsAttacking = false;
        m_IsGrounded = true;
        m_LastJumpTime = 0f;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    private void ResetPhysics()
    {
        m_Rigidbody.linearVelocity = Vector2.zero;
        m_Rigidbody.angularVelocity = 0f;
        m_Rigidbody.rotation = 0f;
    }

    // ✅ 부모의 OnDespawn을 override 하여 사용
    public override void OnDespawn()
    {
        base.OnDespawn(); // EnemyBase의 코루틴 정지 호출
        m_Rigidbody.linearVelocity = Vector2.zero;
        m_IsAttacking = false;
    }

    private void Update()
    {
        // ✅ m_IsDead, m_IsKnockback 플래그 대신 EnemyBase의 체력을 기준으로 정지 상태 판별
        if (Player == null || m_CurrentHp <= 0f) return;

        Vector2 bottomCenter = new Vector2(m_Collider.bounds.center.x, m_Collider.bounds.min.y);
        m_IsGrounded = Physics2D.Raycast(bottomCenter, Vector2.down, k_GroundCheckDistance, GroundLayer);

        float distanceToPlayer = Vector2.Distance(transform.position, Player.position);
        int direction = Player.position.x > transform.position.x ? 1 : -1;

        if (m_IsAttacking)
        {
            float desiredDistance = AttackRange * 0.8f;
            float error = distanceToPlayer - desiredDistance;
            float attackMove = Mathf.Clamp(error, -MoveSpeed * 0.4f, MoveSpeed * 0.4f);

            Move(direction * attackMove);
            return;
        }

        if (distanceToPlayer > AttackRange)
        {
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
        float targetVelocityX = desiredMoveX + separationX;
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

        if (Player == null || m_CurrentHp <= 0f)
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