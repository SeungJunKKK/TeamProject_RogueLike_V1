using System;
using UnityEngine;

public enum EFlyingEnemyState
{
    None,
    Chase,
    Dash,
    Attack
}

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class FlyingEnemyAI : EnemyBase
{
    // --- Configurable Settings ---
    [Header("Stats Setting")]
    [SerializeField] private float m_BaseDamage = 12f;

    [Header("Movement Setting")]
    [SerializeField] private float m_MoveSpeed = 3f; // 이동속도
    [SerializeField] private float m_Acceleration = 5f; // 가속도
    [SerializeField] private float m_DashRange = 6f; // 감지범위
    [SerializeField] private float m_DashSpeed = 8f; // 대쉬 이동속도
    [SerializeField] private float m_DashAcceleration = 15f; // 대쉬 가속도

    [Header("Combat Setting")]
    [SerializeField] private float m_AttackRange = 1.5f;
    [SerializeField] private float m_AttackCooldown = 2f;
    [SerializeField] private float m_AttackWindup = 0.5f;

    [Header("Separation Setting")]
    [SerializeField] private float m_SeparationRadius = 1f;
    [SerializeField] private float m_SeparationStrength = 2f;
    [SerializeField] private LayerMask m_EnemyLayer;

    // --- Components & State ---
    private BoxCollider2D m_Collider;
    private Animator m_Animator;

    [Header("Effect Setting")]
    [SerializeField] private Animator m_EffectAnimator;

    private Transform m_Target;

    private bool m_IsFacingRight = true;
    private bool m_IsSpawnFinished = false;

    private float m_Damage;
    private readonly Collider2D[] m_NearbyEnemies = new Collider2D[10];
    private const float k_SeparationThreshold = 0.01f;

    // --- Target Info (2D) ---
    private float m_DistanceToTarget;
    private Vector2 m_DirectionToTarget;

    // --- Timers & Velocity ---
    private float m_AttackCooldownTimer = 0f;
    private float m_StateTimer = 0f;
    private EFlyingEnemyState m_CurrentState = EFlyingEnemyState.None;
    private Vector2 m_DesiredVelocity = Vector2.zero;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Collider = GetComponent<BoxCollider2D>();
        m_Animator = GetComponent<Animator>();
    }

    public override void SetTarget(Transform target)
    {
        m_Target = target;
    }

    // ==========================================
    // [Module: Lifecycle & Initialization]
    // ==========================================
    public override void OnSpawn()
    {
        base.OnSpawn();

        DifficultyManager difficulty = DifficultyManager.Instance;
        m_Damage = difficulty != null ? m_BaseDamage * difficulty.Coefficient : m_BaseDamage;

        ResetState();
        ResetPhysics();

        if (m_Animator != null)
        {
            m_Animator.Play("Spawn", -1, 0f);
            Invoke("SpawnComplete", 0.5f);
        }
        else
        {
            SpawnComplete();
        }
    }

    private void ResetState()
    {
        gameObject.layer = LayerMask.NameToLayer("FlyingEnemy");

        m_IsFacingRight = true;
        m_IsSpawnFinished = false;
        m_AttackCooldownTimer = 0f;

        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x);
        transform.localScale = localScale;

        m_CurrentState = EFlyingEnemyState.None;
        m_StateTimer = 0f;

        if (m_Animator != null)
        {
            m_Animator.Rebind();
            m_Animator.Update(0f);
        }

        if (m_EffectAnimator != null)
        {
            m_EffectAnimator.Rebind();
            m_EffectAnimator.Update(0f);
        }
    }

    private void ResetPhysics()
    {
        m_Rigidbody.gravityScale = 0f;
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

    public void SpawnComplete()
    {
        m_IsSpawnFinished = true;
        ChangeState(EFlyingEnemyState.Chase);
    }

    // ==========================================
    // [Module: Main Update Flow]
    // ==========================================
    private void Update()
    {
        if (!m_IsSpawnFinished || m_Target == null || m_CurrentHp <= 0f) return;

        m_DesiredVelocity = Vector2.zero;

        UpdateTargetInfo();
        UpdateCombat();

        StateMachine();
        ApplyMovement();
    }

    private void UpdateTargetInfo()
    {
        Vector2 direction = (Vector2)m_Target.position - (Vector2)transform.position;
        m_DistanceToTarget = direction.magnitude;

        if (m_DistanceToTarget > 0f)
        {
            m_DirectionToTarget = direction.normalized;
        }
        else
        {
            m_DirectionToTarget = Vector2.zero;
        }
    }

    private void UpdateCombat()
    {
        if (m_AttackCooldownTimer > 0f)
        {
            m_AttackCooldownTimer -= Time.deltaTime;
        }
    }

    // ==========================================
    // [Module: FSM]
    // ==========================================
    private void ChangeState(EFlyingEnemyState newState)
    {
        if (m_CurrentState == newState) return;

        m_CurrentState = newState;
        m_StateTimer = 0f;

        switch (newState)
        {
            case EFlyingEnemyState.None:
            case EFlyingEnemyState.Chase:
                break;
            case EFlyingEnemyState.Dash:
                break;
            case EFlyingEnemyState.Attack:
                if (m_EffectAnimator != null) m_EffectAnimator.SetTrigger("Attack");
                break;
            default:
                throw new NotImplementedException($"unhandled: {newState}");
        }
    }

    private void StateMachine()
    {
        if (m_DirectionToTarget != Vector2.zero)
        {
            transform.up = m_DirectionToTarget;
        }

        switch (m_CurrentState)
        {
            case EFlyingEnemyState.None:
                break;
            case EFlyingEnemyState.Chase:
                UpdateChase();
                break;
            case EFlyingEnemyState.Dash:
                UpdateDash();
                break;
            case EFlyingEnemyState.Attack:
                UpdateAttack();
                break;
            default:
                throw new NotImplementedException($"unhandled: {m_CurrentState}");
        }
    }

    private void UpdateChase()
    {
        if (m_DistanceToTarget <= m_DashRange)
        {
            ChangeState(EFlyingEnemyState.Dash);
            return;
        }

        m_DesiredVelocity = m_DirectionToTarget * m_MoveSpeed;
    }

    private void UpdateDash()
    {
        // 만약 플레이어가 도망가서 돌진 사거리 밖으로 벗어나면 다시 평범한 Chase로 복귀
        if (m_DistanceToTarget > m_DashRange)
        {
            ChangeState(EFlyingEnemyState.Chase);
            return;
        }

        // 돌진 중에 공격 사거리에 닿으면 공격 상태로 진입
        if (m_DistanceToTarget <= m_AttackRange && m_AttackCooldownTimer <= 0f)
        {
            ChangeState(EFlyingEnemyState.Attack);
            return;
        }

        m_DesiredVelocity = m_DirectionToTarget * m_DashSpeed;
    }


    private void UpdateAttack()
    {
        m_StateTimer += Time.deltaTime;

        if (m_StateTimer >= m_AttackWindup)
        {
            if (m_Target != null && m_DistanceToTarget <= m_AttackRange)
            {
                if (m_Target.TryGetComponent(out IDamageable targetDamageable))
                {
                    DamageInfo info = new DamageInfo
                    {
                        Amount = m_Damage,
                        HitPoint = m_Target.position,
                        HitDirection = m_DirectionToTarget,
                        KnockbackForce = 0f,
                        Attacker = gameObject,
                        IsCrit = false,
                        CanProc = false
                    };
                    targetDamageable.TakeDamage(info);
                }
            }

            m_AttackCooldownTimer = m_AttackCooldown;
            ChangeState(EFlyingEnemyState.Chase);
        }
    }

    // ==========================================
    // [Module: Movement & Separation]
    // ==========================================
    private void ApplyMovement()
    {
        Vector2 separationVector = CalculateSeparation();
        Vector2 finalDesiredVelocity = m_DesiredVelocity + separationVector;

        // 상태에 따라 사용할 가속도를 다르게 설정
        float speedDifference = Vector2.Distance(m_Rigidbody.linearVelocity, finalDesiredVelocity);
        float currentAcceleration = (m_CurrentState == EFlyingEnemyState.Dash) ? m_DashAcceleration : m_Acceleration;

        if (speedDifference > 5f) currentAcceleration *= 0.5f;

        m_Rigidbody.linearVelocity = Vector2.MoveTowards(
            m_Rigidbody.linearVelocity,
            finalDesiredVelocity,
            currentAcceleration * Time.deltaTime // 바뀐 가속도 적용
        );
    }

    private Vector2 CalculateSeparation()
    {
        Vector2 totalSeparation = Vector2.zero;

        int count = Physics2D.OverlapCircleNonAlloc(transform.position, m_SeparationRadius, m_NearbyEnemies, m_EnemyLayer);

        for (int i = 0; i < count; i++)
        {
            Collider2D enemy = m_NearbyEnemies[i];
            if (enemy.gameObject == gameObject) continue;

            Vector2 diff = (Vector2)transform.position - (Vector2)enemy.transform.position;
            float distance = diff.magnitude;

            if (distance < k_SeparationThreshold)
            {
                diff = new Vector2(
                    UnityEngine.Random.Range(-k_SeparationThreshold, k_SeparationThreshold),
                    UnityEngine.Random.Range(-k_SeparationThreshold, k_SeparationThreshold)
                );

                if (diff == Vector2.zero) diff = Vector2.up * k_SeparationThreshold;

                distance = diff.magnitude;
            }

            float ratio = 1f - (distance / m_SeparationRadius);
            ratio = Mathf.Clamp01(ratio);
            float strength = m_SeparationStrength * (ratio * ratio);

            Vector2 pushVector = (diff / distance) * strength;
            totalSeparation += pushVector;
        }

        return Vector2.ClampMagnitude(totalSeparation, m_MoveSpeed * 1.5f);
    }

    private void Flip()
    {
        m_IsFacingRight = !m_IsFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // ==========================================
    // [Module: Death & Pooling]
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

    private void ReturnToPool()
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
}