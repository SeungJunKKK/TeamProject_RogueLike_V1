using UnityEngine;

public enum EnemyState
{
    Chase,
    Attack
}

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class BasicEnemyAI : EnemyBase
{
    [Header("Target Setting")]
    public Transform Player;

    [Header("Stats Setting")]
    [SerializeField] private float m_BaseDamage = 12f;
    private float m_Damage;

    [Header("Movement Setting")]
    public float MoveSpeed = 3f;
    public float Acceleration = 15f;
    public float JumpForce = 6f;
    public float JumpCooldown = 0.5f;
    private float m_LastJumpTime;

    [Header("Combat Setting")]
    public float AttackRange = 1.5f;
    public float AttackWindup = 0.5f;  // 공격 선딜레이 (State 진행 시간)
    public float AttackCooldown = 2f;  // 공격 쿨타임 (Combat 시간)
    [Range(0f, 1f)] public float AttackMoveRatio = 0.3f;

    [Header("Raycast Sensors (자동 계산)")]
    public float GroundRayLength = 1.5f;
    public float WallRayLength = 0.2f;
    public LayerMask GroundLayer;

    [Header("Separation Setting")]
    public float SeparationRadius = 0.8f;
    public float SeparationForce = 2.5f;
    public LayerMask EnemyLayer;
    private readonly Collider2D[] m_NearbyEnemies = new Collider2D[10];
    private const float k_SeparationThreshold = 0.01f;

    private Rigidbody2D m_Rigidbody;
    private BoxCollider2D m_Collider;
    private Animator m_Animator;

    private bool m_IsFacingRight = true;

    // --- Sensor Data ---
    private bool m_IsGrounded = true;
    private bool m_IsWallAhead = false;
    private bool m_IsGroundAhead = true;

    // --- Target Info ---
    private float m_DistanceToPlayer;
    private int m_Direction;

    // --- Combat Timer ---
    private float m_AttackCooldownTimer = 0f;

    // --- FSM & Movement ---
    private EnemyState m_CurrentState;
    private float m_StateTimer = 0f;
    private float m_DesiredVelocityX = 0f;

    private const float k_GroundCheckDistance = 0.1f;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Collider = GetComponent<BoxCollider2D>();
        m_Animator = GetComponent<Animator>();
    }

    public override void SetTarget(Transform target)
    {
        Player = target;
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        var difficulty = DifficultyManager.Instance;
        m_Damage = difficulty != null ? m_BaseDamage * difficulty.Coefficient : m_BaseDamage;

        ResetState();
        ResetPhysics();
    }


    private void ResetState()
    {
        m_IsFacingRight = true;
        m_IsGrounded = true;
        m_LastJumpTime = 0f;
        m_AttackCooldownTimer = 0f; // 스폰 시 쿨타임 초기화
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        if (m_Animator != null) m_Animator.Play("Spawn", -1, 0f);

        // 초기 상태를 더미 값으로 둔 뒤 확실하게 Chase로 전환
        m_CurrentState = (EnemyState)(-1);
        ChangeState(EnemyState.Chase);
    }

    private void ResetPhysics()
    {
        m_Rigidbody.linearVelocity = Vector2.zero;
        m_Rigidbody.angularVelocity = 0f;
        m_Rigidbody.rotation = 0f;
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        m_Rigidbody.linearVelocity = Vector2.zero;
    }

    public void ChangeState(EnemyState newState)
    {
        if (m_CurrentState == newState) return;

        // 애니메이터 동기화 방어 코드: 기존 상태가 Attack이었다면 모션 캔슬 트리거 등을 넣을 수 있음
        // if (m_CurrentState == EnemyState.Attack && m_Animator != null) m_Animator.ResetTrigger("Attack");

        m_CurrentState = newState;
        m_StateTimer = 0f;

        if (newState == EnemyState.Attack)
        {
            if (m_Animator != null) m_Animator.SetTrigger("Attack");
        }
    }

    private void Update()
    {
        if (Player == null || m_CurrentHp <= 0f) return;

        // 매 프레임 속도 초기화
        m_DesiredVelocityX = 0f;

        // 통합 정보 갱신 및 논리 블록 실행
        UpdateTargetInfo();
        UpdateSensors();
        UpdateCombat();    // 시간에 종속된 모듈

        StateMachine();    // 행동에 종속된 모듈

        ApplyMovement();   // 물리 적용 모듈
    }

    private void UpdateTargetInfo()
    {
        m_DistanceToPlayer = Vector2.Distance(transform.position, Player.position);
        m_Direction = Player.position.x > transform.position.x ? 1 : -1;
    }

    private void UpdateSensors()
    {
        // 시각적(Flip) 방향이 아닌, 논리적 타겟 방향(m_Direction)을 기준으로 센서 발사
        float extentsX = m_Collider.bounds.extents.x;
        float checkX = m_Collider.bounds.center.x + (extentsX * m_Direction);

        Vector2 bottomCenter = new Vector2(m_Collider.bounds.center.x, m_Collider.bounds.min.y);
        m_IsGrounded = Physics2D.Raycast(bottomCenter, Vector2.down, k_GroundCheckDistance, GroundLayer);

        Vector2 cliffCheckPos = new Vector2(checkX, m_Collider.bounds.min.y);
        m_IsGroundAhead = Physics2D.Raycast(cliffCheckPos, Vector2.down, GroundRayLength, GroundLayer);

        Vector2 wallCheckPos = new Vector2(checkX, m_Collider.bounds.center.y - (m_Collider.bounds.extents.y * 0.5f));
        Vector2 wallDir = m_Direction == 1 ? Vector2.right : Vector2.left;
        m_IsWallAhead = Physics2D.Raycast(wallCheckPos, wallDir, WallRayLength, GroundLayer);

        if (m_Animator != null)
        {
            m_Animator.SetFloat("Speed", Mathf.Abs(m_Rigidbody.linearVelocity.x));
            m_Animator.SetBool("IsGrounded", m_IsGrounded);
        }

        Debug.Log($"바닥에 닿았는가? : {m_IsGrounded}");
    }

    private void UpdateCombat()
    {
        // 상태와 무관하게 쿨타임은 세상의 시간처럼 흐름
        if (m_AttackCooldownTimer > 0f)
        {
            m_AttackCooldownTimer -= Time.deltaTime;
        }
    }

    private bool CanAttack()
    {
        return m_DistanceToPlayer <= AttackRange && m_AttackCooldownTimer <= 0f && m_IsGrounded;
    }

    private void StateMachine()
    {
        // 타겟 방향으로 시선 변경 (매 프레임 추적)
        if ((m_Direction == 1 && !m_IsFacingRight) || (m_Direction == -1 && m_IsFacingRight))
        {
            Flip();
        }

        switch (m_CurrentState)
        {
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Attack:
                UpdateAttack();
                break;
        }
    }

    private void UpdateChase()
    {
        // Combat 모듈에게 공격 가능 여부만 물어봄
        if (CanAttack())
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        if (!m_IsGrounded)
        {
            m_DesiredVelocityX = m_Direction * MoveSpeed;
            return;
        }

        if (m_IsWallAhead || !m_IsGroundAhead)
        {
            JumpIfNeeded();
        }     
        else
        {
            m_DesiredVelocityX = m_Direction * MoveSpeed;
        }
    }

    private void UpdateAttack()
    {
        m_StateTimer += Time.deltaTime;

        // 공격 중에는 천천히 플레이어 방향으로 이동
        m_DesiredVelocityX = m_Direction * MoveSpeed * AttackMoveRatio;

        if (m_StateTimer >= AttackWindup)
        {
            // 선딜레이가 끝나는 시점에 플레이어가 사거리 안에 있으면 데미지 판정
            if (Player != null && m_DistanceToPlayer <= AttackRange)
            {
                if (Player.TryGetComponent(out IDamageable targetDamageable))
                {
                    Debug.Log($"플레이어를 {m_Damage} 의 데미지로 공격.");
                    DamageInfo info = new DamageInfo
                    {
                        Amount = m_Damage,                           // 내 공격력
                        HitPoint = Player.position,                  // 타격 지점
                        HitDirection = new Vector2(m_Direction, 0f), // 내가 바라보는 방향
                        KnockbackForce = 0f,
                        Attacker = gameObject,
                        IsCrit = false,
                        CanProc = false
                    };
                    targetDamageable.TakeDamage(info);
                }
                else if (Player.TryGetComponent(out PlayerController playerTarget))
                {
                    playerTarget.TakeDamage(m_Damage);
                }
            }

            m_AttackCooldownTimer = AttackCooldown;
            ChangeState(EnemyState.Chase);
        }
    }

    private void JumpIfNeeded()
    {
        if (Time.time - m_LastJumpTime >= JumpCooldown)
        {
            m_LastJumpTime = Time.time;
            m_IsGrounded = false;
            if (m_Animator != null) m_Animator.SetTrigger("Jump");

            // Impulse 즉시 적용
            m_Rigidbody.linearVelocity = new Vector2(m_Rigidbody.linearVelocity.x, 0f);
            m_Rigidbody.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
        }
    }

    private void ApplyMovement()
    {
        float separationX = CalculateSeparation();
        float finalTargetVelocityX = m_DesiredVelocityX + separationX;

        float currentVelocityX = m_Rigidbody.linearVelocity.x;
        float smoothedVelocityX = Mathf.MoveTowards(currentVelocityX, finalTargetVelocityX, Acceleration * Time.deltaTime);

        m_Rigidbody.linearVelocity = new Vector2(smoothedVelocityX, m_Rigidbody.linearVelocity.y);
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

    private void Flip()
    {
        m_IsFacingRight = !m_IsFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void OnDrawGizmos()
    {
        if (m_Collider == null) m_Collider = GetComponent<BoxCollider2D>();
        if (m_Collider == null) return;

        int gizmoDir = Application.isPlaying ? m_Direction : (m_IsFacingRight ? 1 : -1);
        if (gizmoDir == 0) gizmoDir = 1;

        float extentsX = m_Collider.bounds.extents.x;
        float checkX = m_Collider.bounds.center.x + (extentsX * gizmoDir);

        Vector2 cliffCheckPos = new Vector2(checkX, m_Collider.bounds.min.y);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(cliffCheckPos, cliffCheckPos + Vector2.down * GroundRayLength);

        Vector2 wallCheckPos = new Vector2(checkX, m_Collider.bounds.center.y - (m_Collider.bounds.extents.y * 0.5f));
        Vector2 wallDir = gizmoDir == 1 ? Vector2.right : Vector2.left;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(wallCheckPos, wallCheckPos + wallDir * WallRayLength);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, SeparationRadius);
    }
}