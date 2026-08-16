using UnityEngine;

public enum ERockGolemState
{
    None,
    Chase,
    Attack
}

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class RockGolemAI : EnemyBase
{
    // --- Configurable Settings ---
    [Header("Stats Setting")]
    [SerializeField] private float m_BaseDamage = 34f;

    [Header("Movement Setting")]
    [SerializeField] private float m_MoveSpeed = 1.8f;
    [SerializeField] private float m_Acceleration = 6.0f;

    [Header("Combat Setting")]
    [SerializeField] private float m_AttackStartRange = 3.5f;
    [SerializeField] private float m_AttackCooldown = 3.5f;
    [SerializeField] private Transform m_ShockwaveSpawnPos;
    [SerializeField] private GameObject m_ShockwavePrefab;

    [Header("Raycast Sensors")]
    [SerializeField] private float m_CliffRayLength = 1.5f;
    [SerializeField] private float m_WallRayLength = 0.3f;
    [SerializeField] private LayerMask m_GroundLayer;

    [Header("Separation Setting")]
    [SerializeField] private float m_SeparationRadius = 1.2f;
    [SerializeField] private float m_SeparationForce = 1.0f;
    [SerializeField] private LayerMask m_EnemyLayer;

    // --- Components & Internal States ---
    private BoxCollider2D m_Collider;
    private Animator m_Animator;

    private Transform m_Target;
    private bool m_IsFacingRight = true;
    private bool m_IsSpawnFinished = false;

    private float m_Damage;
    private float m_AttackCooldownTimer = 0f;

    // --- Sensor Data ---
    private bool m_IsGrounded = true;
    private bool m_IsWallAhead = false;
    private bool m_IsGroundAhead = true;

    // --- Target Info ---
    private int m_Direction;
    private int m_LockedAttackDirection;

    // --- FSM & Movement ---
    private ERockGolemState m_CurrentState = ERockGolemState.None;
    private float m_DesiredVelocityX = 0f;

    private readonly Collider2D[] m_NearbyEnemies = new Collider2D[10];
    private const float k_GroundCheckDistance = 0.1f;
    private const float k_SeparationThreshold = 0.01f;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Collider = GetComponent<BoxCollider2D>();
        m_Animator = GetComponent<Animator>();
    }

    public override void SetTarget(Transform target)
    {
        m_Target = target;

        Debug.Log($"[RockGolem] ★ SetTarget 호출됨 / Target = {m_Target}");
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        Debug.Log($"[RockGolem] OnSpawn 호출 / Target = {m_Target}");

        var difficulty = DifficultyManager.Instance;
        m_Damage = difficulty != null ? m_BaseDamage * difficulty.Coefficient : m_BaseDamage;

        ResetState();
        ResetPhysics();

        m_IsSpawnFinished = false;

        if (m_Animator != null)
        {
            Debug.Log("[RockGolem] Spawn 애니메이션 재생");

            m_Animator.Rebind();
            m_Animator.Update(0f);
            m_Animator.Play("Spawn", -1, 0f);
        }
        else
        {
            Debug.Log("[RockGolem] Animator 없음 → SpawnComplete 직접 호출");
            SpawnComplete();
        }
    }


    private void ResetState()
    {
        gameObject.layer = LayerMask.NameToLayer("Enemy");

        m_IsFacingRight = true;
        m_IsGrounded = true;
        m_AttackCooldownTimer = 0f;

        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x);
        transform.localScale = localScale;

        m_CurrentState = ERockGolemState.None;
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
        Debug.Log("[RockGolem] SpawnComplete 호출됨");

        m_IsSpawnFinished = true;
        m_CurrentState = ERockGolemState.None;

        ChangeState(ERockGolemState.Chase);
    }

    private void Update()
    {
        if (!m_IsSpawnFinished || m_Target == null || m_CurrentHp <= 0f) return;


        m_DesiredVelocityX = 0f;

        UpdateTargetInfo();
        UpdateSensors();
        UpdateCombat();

        StateMachine();
        ApplyMovement();
    }

    private void UpdateTargetInfo()
    {
        m_Direction = m_Target.position.x > transform.position.x ? 1 : -1;
    }

    private void UpdateSensors()
    {
        float extentsX = m_Collider.bounds.extents.x;
        float checkX = m_Collider.bounds.center.x + (extentsX * m_Direction);

        float safeStartY = m_Collider.bounds.min.y + 0.1f;

        Vector2 bottomCenter = new Vector2(m_Collider.bounds.center.x, safeStartY);
        m_IsGrounded = Physics2D.Raycast(bottomCenter, Vector2.down, k_GroundCheckDistance + 0.1f, m_GroundLayer);

        Vector2 cliffCheckPos = new Vector2(checkX, safeStartY);

        RaycastHit2D cliffHit = Physics2D.Raycast(cliffCheckPos, Vector2.down, m_CliffRayLength + 0.1f, m_GroundLayer);
        m_IsGroundAhead = (cliffHit.collider != null);

        Debug.DrawRay(cliffCheckPos, Vector2.down * (m_CliffRayLength + 0.1f), m_IsGroundAhead ? Color.green : Color.red);

        if (!m_IsGroundAhead)
        {
            Debug.LogWarning("[RockGolemAI] 앞쪽 바닥을 인식하지 못했습니다! (절벽으로 판정되어 멈춤)");
        }

        Vector2 wallCheckPos = new Vector2(checkX, m_Collider.bounds.center.y - (m_Collider.bounds.extents.y * 0.5f));
        Vector2 wallDir = m_Direction == 1 ? Vector2.right : Vector2.left;
        m_IsWallAhead = Physics2D.Raycast(wallCheckPos, wallDir, m_WallRayLength, m_GroundLayer);

        if (m_Animator != null)
        {
            m_Animator.SetFloat("Speed", Mathf.Abs(m_Rigidbody.linearVelocity.x));
            m_Animator.SetBool("IsGrounded", m_IsGrounded);
        }
    }

    private void UpdateCombat()
    {
        if (m_AttackCooldownTimer > 0f)
        {
            m_AttackCooldownTimer -= Time.deltaTime;
        }
    }

    private bool CanAttack()
    {
        float horizontalDistance = Mathf.Abs(m_Target.position.x - transform.position.x);
        return horizontalDistance <= m_AttackStartRange && m_AttackCooldownTimer <= 0f && m_IsGrounded && !m_IsWallAhead;
    }

    private void ChangeState(ERockGolemState newState)
    {
        if (m_CurrentState == newState) return;

        m_CurrentState = newState;

        switch (newState)
        {
            case ERockGolemState.None:
            case ERockGolemState.Chase:
                break;
            case ERockGolemState.Attack:
                m_LockedAttackDirection = m_IsFacingRight ? 1 : -1;
                if (m_Animator != null) m_Animator.SetTrigger("Attack");
                break;
            default:
                break;
        }
    }

    private void StateMachine()
    {
        if (m_CurrentState != ERockGolemState.Attack)
        {
            if ((m_Direction == 1 && !m_IsFacingRight) || (m_Direction == -1 && m_IsFacingRight))
            {
                Flip();
            }
        }

        switch (m_CurrentState)
        {
            case ERockGolemState.None:
                break;
            case ERockGolemState.Chase:
                UpdateChase();
                break;
            case ERockGolemState.Attack:
                UpdateAttack();
                break;
        }
    }

    private void UpdateChase()
    {
        if (CanAttack())
        {
            Debug.Log($"[RockGolem] 공격 전환 | Distance={Mathf.Abs(m_Target.position.x - transform.position.x):F2}");
            ChangeState(ERockGolemState.Attack);
            return;
        }

        if (m_IsWallAhead || !m_IsGroundAhead)
        {
            m_DesiredVelocityX = 0f;
        }
        else
        {
            m_DesiredVelocityX = m_Direction * m_MoveSpeed;
        }
    }

    private void UpdateAttack()
    {
        m_DesiredVelocityX = 0f;
    }

    public void SpawnShockwave()
    {
        if (!m_IsSpawnFinished || m_CurrentState != ERockGolemState.Attack || m_CurrentHp <= 0f) return;

        if (m_ShockwavePrefab == null)
        {
            Debug.LogError("[RockGolemAI] 충격파 프리팹이 연결되지 않았습니다.");
            return;
        }

        if (PoolManager.Instance == null)
        {
            Debug.LogError("[RockGolemAI] PoolManager.Instance가 없습니다.");
            return;
        }

        Vector2 spawnPos = m_ShockwaveSpawnPos != null ? (Vector2)m_ShockwaveSpawnPos.position : (Vector2)transform.position;
        Vector2 attackDir = m_LockedAttackDirection == 1 ? Vector2.right : Vector2.left;

        GameObject shockwaveObj = PoolManager.Instance.Get(m_ShockwavePrefab, spawnPos, Quaternion.identity);

        if (shockwaveObj == null)
        {
            Debug.LogError("[RockGolemAI] 충격파 생성에 실패했습니다.");
            return;
        }

        if (shockwaveObj.TryGetComponent(out RockShockwave shockwave))
        {
            shockwave.Init(m_Damage, attackDir, gameObject);
        }
        else
        {
            Debug.LogError("[RockGolemAI] 생성된 충격파에 RockShockwave가 없습니다.");
        }
    }

    public void EndAttack()
    {
        if (m_CurrentState != ERockGolemState.Attack || m_CurrentHp <= 0f) return;

        m_AttackCooldownTimer = m_AttackCooldown;
        ChangeState(ERockGolemState.Chase);
    }

    private void ApplyMovement()
    {
        float separationX = 0f;

        if (m_CurrentState != ERockGolemState.Attack)
        {
            separationX = CalculateSeparation();
        }

        float finalTargetVelocityX = m_DesiredVelocityX + separationX;

        float currentVelocityX = m_Rigidbody.linearVelocity.x;
        float smoothedVelocityX = Mathf.MoveTowards(currentVelocityX, finalTargetVelocityX, m_Acceleration * Time.deltaTime);

        m_Rigidbody.linearVelocity = new Vector2(smoothedVelocityX, m_Rigidbody.linearVelocity.y);

   //     Debug.Log(
   //    $"[RockGolem] 실제 이동 | " +
   //    $"Desired={m_DesiredVelocityX:F2} | " +
   //    $"Separation={separationX:F2} | " +
   //    $"Final={finalTargetVelocityX:F2} | " +
   //    $"VelocityX={m_Rigidbody.linearVelocity.x:F2}"
   //);
    }

    private float CalculateSeparation()
    {
        float separationForceX = 0f;
        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = m_EnemyLayer;
        int count = Physics2D.OverlapCircle(transform.position, m_SeparationRadius, filter, m_NearbyEnemies);

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

            float ratio = 1f - (distance / m_SeparationRadius);
            ratio = Mathf.Clamp01(ratio);

            float pushForce = m_SeparationForce * (ratio * ratio);
            Vector2 pushVector = (diff / distance) * pushForce;
            separationForceX += pushVector.x;
        }

        return Mathf.Clamp(separationForceX, -m_MoveSpeed * 1.5f, m_MoveSpeed * 1.5f);
    }

    private void Flip()
    {
        m_IsFacingRight = !m_IsFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    protected override void Die()
    {
        m_CurrentState = ERockGolemState.None;
        m_IsSpawnFinished = false;
        m_DesiredVelocityX = 0f;

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

    private void OnDrawGizmosSelected()
    {
        if (m_Collider == null) m_Collider = GetComponent<BoxCollider2D>();
        if (m_Collider == null) return;

        int gizmoDir = Application.isPlaying ? m_Direction : (m_IsFacingRight ? 1 : -1);
        if (gizmoDir == 0) gizmoDir = 1;

        float extentsX = m_Collider.bounds.extents.x;
        float checkX = m_Collider.bounds.center.x + (extentsX * gizmoDir);

        Gizmos.color = Color.red;
        Vector2 cliffCheckPos = new Vector2(checkX, m_Collider.bounds.min.y);
        Gizmos.DrawLine(cliffCheckPos, cliffCheckPos + Vector2.down * m_CliffRayLength);

        Gizmos.color = Color.blue;
        Vector2 wallCheckPos = new Vector2(checkX, m_Collider.bounds.center.y - (m_Collider.bounds.extents.y * 0.5f));
        Vector2 wallDir = gizmoDir == 1 ? Vector2.right : Vector2.left;
        Gizmos.DrawLine(wallCheckPos, wallCheckPos + wallDir * m_WallRayLength);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_SeparationRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, m_AttackStartRange);
    }
}