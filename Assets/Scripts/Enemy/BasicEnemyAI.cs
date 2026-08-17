using System;
using UnityEngine;

public enum EnemyState
{
    None,
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
    protected float m_Damage;

    [Header("Movement Setting")]
    public float MoveSpeed = 3f;
    public float Acceleration = 15f;
    public float JumpForce = 6f;
    public float JumpCooldown = 0.5f;
    protected float m_LastJumpTime;

    [Header("Combat Setting")]
    public float AttackRange = 1.5f;
    public float AttackWindup = 0.5f;  // 공격 선딜레이
    public float AttackCooldown = 2f;  // 공격 쿨타임
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

    protected BoxCollider2D m_Collider;
    protected Animator m_Animator;

    protected bool m_IsFacingRight = true;

    // --- Sensor Data ---
    protected bool m_IsGrounded = true;
    protected bool m_IsWallAhead = false;
    protected bool m_IsGroundAhead = true;

    // --- Target Info ---
    protected float m_DistanceToPlayer;
    protected int m_Direction;
    protected bool m_IsAlignedVertically = false;

    // --- Combat Timer ---
    protected float m_AttackCooldownTimer = 0f;

    // --- FSM & Movement ---
    protected EnemyState m_CurrentState = EnemyState.None;
    protected float m_StateTimer = 0f;
    protected float m_DesiredVelocityX = 0f;
    protected bool m_IsSpawnFinished = false;

    private const float k_GroundCheckDistance = 0.1f;

    protected virtual void Awake()
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

        m_IsSpawnFinished = false;

        if (m_Animator != null)
        {
            m_Animator.Rebind();
            m_Animator.Update(0f);
            m_Animator.Play("Spawn", -1, 0f);
        }
    }

    protected virtual void ResetState()
    {
        gameObject.layer = LayerMask.NameToLayer("Enemy");

        m_IsFacingRight = true;
        m_IsGrounded = true;
        m_LastJumpTime = 0f;
        m_AttackCooldownTimer = 0f;

        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        m_CurrentState = EnemyState.None;
        m_StateTimer = 0f;
    }

    protected virtual void ResetPhysics()
    {
        m_Rigidbody.linearVelocity = Vector2.zero;
        m_Rigidbody.angularVelocity = 0f;
        m_Rigidbody.rotation = 0f;
    }

    public override void OnDespawn()
    {
        base.OnDespawn();

        m_IsSpawnFinished = false;
        m_Rigidbody.linearVelocity = Vector2.zero;

        if (m_Animator != null)
        {
            m_Animator.Rebind();
            m_Animator.Update(0f);
        }
    }

    protected virtual void ChangeState(EnemyState newState)
    {
        if (m_CurrentState == newState) return;

        m_CurrentState = newState;
        m_StateTimer = 0f;

        switch (newState)
        {
            case EnemyState.None:
            case EnemyState.Chase:
                break;
            case EnemyState.Attack:
                if (m_Animator != null) m_Animator.SetTrigger("Attack");
                break;
            default:
                throw new NotImplementedException($"unhandled: {newState}");
        }
    }

    private void Update()
    {
        if (!m_IsSpawnFinished || Player == null || m_CurrentHp <= 0f) return;

        m_DesiredVelocityX = 0f;

        UpdateTargetInfo();
        UpdateSensors();
        UpdateCombat();

        StateMachine();
        ApplyMovement();
    }

    protected virtual void UpdateTargetInfo()
    {
        m_DistanceToPlayer = Vector2.Distance(transform.position, Player.position);

        float xDiff = Player.position.x - transform.position.x;

        if (Mathf.Abs(xDiff) <= 0.1f)
        {
            m_IsAlignedVertically = true;
        }
        else
        {
            m_IsAlignedVertically = false;
            m_Direction = xDiff > 0 ? 1 : -1;
        }
    }

    protected virtual void UpdateSensors()
    {
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
    }

    protected virtual void UpdateCombat()
    {
        if (m_AttackCooldownTimer > 0f)
        {
            m_AttackCooldownTimer -= Time.deltaTime;
        }
    }

    public void SpawnComplete()
    {
        m_IsSpawnFinished = true;
        m_CurrentState = EnemyState.None;
        ChangeState(EnemyState.Chase);
    }

    protected virtual bool CanAttack()
    {
        return m_DistanceToPlayer <= AttackRange && m_AttackCooldownTimer <= 0f && m_IsGrounded;
    }

    // ==========================================
    // [Module: FSM (Behavior Management)]
    // ==========================================
    protected virtual void StateMachine()
    {
        //if ((m_Direction == 1 && !m_IsFacingRight) || (m_Direction == -1 && m_IsFacingRight))
        //{
        //    Flip();
        //}

        if (m_CurrentState != EnemyState.Attack)
        {
            if ((m_Direction == 1 && !m_IsFacingRight) || (m_Direction == -1 && m_IsFacingRight))
            {
                Flip();
            }
        }

        switch (m_CurrentState)
        {
            case EnemyState.None:
                break;
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Attack:
                UpdateAttack();
                break;
            default:
                throw new NotImplementedException($"unhandled: {m_CurrentState}");
        }
    }

    protected virtual void UpdateChase()
    {
        if (CanAttack())
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        if (m_IsAlignedVertically)
        {
            m_DesiredVelocityX = 0f;
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

    protected virtual void UpdateAttack()
    {
        m_StateTimer += Time.deltaTime;

        //if (m_DistanceToPlayer > AttackRange)
        //{
        //    ChangeState(EnemyState.Chase);
        //    return;
        //}
    }


    public void TriggerAttack()
    {
        if (Player != null && m_DistanceToPlayer <= AttackRange)
        {
            if (Player.TryGetComponent(out PlayerController playerController))
            {
                playerController.TakeDamage(m_Damage);
            }
            else if (Player.TryGetComponent(out IDamageable targetDamageable))
            {
                DamageInfo info = new DamageInfo
                {
                    Amount = m_Damage,
                    HitPoint = Player.position,
                    HitDirection = new Vector2(m_Direction, 0f),
                    KnockbackForce = 0f,
                    Attacker = gameObject,
                    IsCrit = false,
                    CanProc = false
                };
                targetDamageable.TakeDamage(info);
            }
        }
    }
            

    public void FinishAttack()
    {
        m_AttackCooldownTimer = AttackCooldown;
        ChangeState(EnemyState.Chase);
    }

    protected virtual void JumpIfNeeded()
    {
        if (Time.time - m_LastJumpTime >= JumpCooldown)
        {
            m_LastJumpTime = Time.time;
            m_IsGrounded = false;
            if (m_Animator != null) m_Animator.SetTrigger("Jump");

            m_Rigidbody.linearVelocity = new Vector2(m_Rigidbody.linearVelocity.x, 0f);
            m_Rigidbody.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
        }
    }

    /// <summary>
    /// 최대 낙하 속도 -20f를 적용하고, 목표 속도와 현재 속도를 기반으로 가속도를 적용하여 이동을 처리합니다.
    /// </summary>
    protected virtual void ApplyMovement()
    {
        float separationX = CalculateSeparation();
        float finalTargetVelocityX = m_DesiredVelocityX + separationX;

        float currentVelocityX = m_Rigidbody.linearVelocity.x;
        float smoothedVelocityX = Mathf.MoveTowards(currentVelocityX, finalTargetVelocityX, Acceleration * Time.deltaTime);

        float clampedFallY = Mathf.Max(m_Rigidbody.linearVelocity.y, -20f);
        m_Rigidbody.linearVelocity = new Vector2(smoothedVelocityX, clampedFallY);
    }

    protected virtual float CalculateSeparation()
    {
        float separationForceX = 0f;
        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = EnemyLayer;
        int count = Physics2D.OverlapCircle(transform.position, SeparationRadius, filter, m_NearbyEnemies);

        for (int i = 0; i < count; i++)
        {
            Collider2D enemy = m_NearbyEnemies[i];
            if (enemy.gameObject == gameObject) continue;

            Vector2 diff = transform.position - enemy.transform.position;
            float distance = diff.magnitude;

            if (distance < k_SeparationThreshold)
            {
                diff = new Vector2(
                    UnityEngine.Random.Range(-k_SeparationThreshold, k_SeparationThreshold),
                    UnityEngine.Random.Range(-k_SeparationThreshold, k_SeparationThreshold)
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

    protected virtual void Flip()
    {
        m_IsFacingRight = !m_IsFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // ==========================================
    // [Module: Death Animation & Pooling]
    // ==========================================
    protected override void Die()
    {
        base.Die();
        gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
        m_Rigidbody.linearVelocity = Vector2.zero;

        if (m_Animator != null)
        {
            m_Animator.SetTrigger("Death");
        }
        else
        {
            ReturnToPool();
        }
    }

    public void DeathComplete()
    {
        ReturnToPool();
    }

    protected virtual void ReturnToPool()
    {
        if (TryGetComponent(out PooledObject pooledObj))
        {
            pooledObj.Return();
        }
        else
        {
            Destroy(gameObject);
        }
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