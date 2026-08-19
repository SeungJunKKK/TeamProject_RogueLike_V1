using UnityEngine;

public enum ERockGolemState
{
    None,
    Idle,
    Chase,
    Attack
}

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class RockGolemAI : EnemyBase
{
    [Header("Rock Golem Unique Setting")]
    [SerializeField] private Transform m_ShockwaveSpawnPos;
    [SerializeField] private GameObject m_ShockwavePrefab;

    [SerializeField] private GameObject m_EliteShockwavePrefab;


    [Header("Layer Setting")]
    [SerializeField] private LayerMask m_GroundLayer;
    [SerializeField] private LayerMask m_EnemyLayer;

    protected GroundEnemyDataSO m_GroundData;

    // --- Components & Internal States ---
    private BoxCollider2D m_Collider;
    private Animator m_Animator;

    private Transform m_Target;
    private bool m_IsFacingRight = true;
    private bool m_IsSpawnFinished = false;

    private float m_Damage;
    private float m_AttackCooldownTimer = 0f;
    private float m_LastJumpTime = 0f;

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

        m_GroundData = m_Data as GroundEnemyDataSO;
    }

    public override void SetTarget(Transform target)
    {
        m_Target = target;
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        m_GroundData ??= m_Data as GroundEnemyDataSO;


        var difficulty = DifficultyManager.Instance;
        m_Damage = difficulty != null ? m_GroundData.BaseDamage * difficulty.Coefficient : m_GroundData.BaseDamage;
        

        ResetState();
        ResetPhysics();

        m_IsSpawnFinished = false;

        if (m_Animator != null)
        {
            m_Animator.Rebind();
            m_Animator.Update(0f);
            m_Animator.Play("Spawn", -1, 0f);
        }
        else
        {
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
        //Debug.Log("[RockGolem] SpawnComplete 호출됨");

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

        RaycastHit2D cliffHit = Physics2D.Raycast(cliffCheckPos, Vector2.down, m_GroundData.GroundRayLength + 0.1f, m_GroundLayer);
        m_IsGroundAhead = (cliffHit.collider != null);

        Debug.DrawRay(cliffCheckPos, Vector2.down * (m_GroundData.GroundRayLength + 0.1f), m_IsGroundAhead ? Color.green : Color.red);

        if (!m_IsGroundAhead)
        {
            //Debug.LogWarning("[RockGolemAI] 앞쪽 바닥을 인식하지 못했습니다! (절벽으로 판정되어 멈춤)");
        }

        Vector2 wallCheckPos = new Vector2(checkX, m_Collider.bounds.center.y - (m_Collider.bounds.extents.y * 0.5f));
        Vector2 wallDir = m_Direction == 1 ? Vector2.right : Vector2.left;
        m_IsWallAhead = Physics2D.Raycast(wallCheckPos, wallDir, m_GroundData.WallRayLength, m_GroundLayer);

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

        return horizontalDistance <= m_GroundData.AttackRange
            && m_AttackCooldownTimer <= 0f
            && m_IsGrounded
            && !m_IsWallAhead;
    }
 

    private void ChangeState(ERockGolemState newState)
    {
        if (m_CurrentState == newState) return;

        m_CurrentState = newState;

        switch (newState)
        {
            case ERockGolemState.Idle:
                m_DesiredVelocityX = 0f;
                break;

            case ERockGolemState.None:
            case ERockGolemState.Chase:
                break;
            case ERockGolemState.Attack:
                m_LockedAttackDirection = m_IsFacingRight ? 1 : -1;
                if (m_Animator != null)
                { 
                    m_Animator.SetTrigger("Attack");
                }
                
                if (m_GroundData != null && !string.IsNullOrEmpty(m_GroundData.AttackSoundAddress))
                {
                    PlayAddressableSFX(m_GroundData.AttackSoundAddress);
                }

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
            case ERockGolemState.Idle:   
                UpdateIdle();
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
            ChangeState(ERockGolemState.Attack);
            return;
        }

        if (m_IsGrounded && !m_IsGroundAhead)
        {
            ChangeState(ERockGolemState.Idle);
            return;
        }

        if (m_IsWallAhead && m_IsGroundAhead)
        {
            JumpIfNeeded();
        }

        m_DesiredVelocityX = m_Direction * m_GroundData.MoveSpeed;
    }

    private void JumpIfNeeded()
    {
        if (Time.time - m_LastJumpTime >= m_GroundData.JumpCooldown && m_IsGrounded)
        {
            m_LastJumpTime = Time.time;
            m_IsGrounded = false;

             if (m_Animator != null)
            {
                m_Animator.SetTrigger("Jump");
            }

            m_Rigidbody.linearVelocity = new Vector2(m_Rigidbody.linearVelocity.x, 0f);
            m_Rigidbody.AddForce(Vector2.up * m_GroundData.JumpForce, ForceMode2D.Impulse);
        }
    }

    private void UpdateAttack()
    {
        m_DesiredVelocityX = 0f;
    }

    private void UpdateIdle()
    {
        m_DesiredVelocityX = 0f;

        if (CanAttack())
        {
            ChangeState(ERockGolemState.Attack);
            return;
        }
        if (m_IsGroundAhead)
        {
            ChangeState(ERockGolemState.Chase);
        }

        //float distanceToPlayer = Vector2.Distance(transform.position, m_Target.position);

        //if (m_IsGroundAhead || distanceToPlayer <= m_GroundData.AttackRange * 1.5f)
        //{
        //    ChangeState(ERockGolemState.Chase);
        //}
    }


    public void SpawnShockwave()
    {
        if (!m_IsSpawnFinished || m_CurrentState != ERockGolemState.Attack || m_CurrentHp <= 0f) return;

        GameObject prefabToFire = (IsElite && m_EliteShockwavePrefab != null) ? m_EliteShockwavePrefab : m_ShockwavePrefab;

        if (prefabToFire == null)
        {
            return;
        }

        if (PoolManager.Instance == null) return;

        Vector2 spawnPos = m_ShockwaveSpawnPos != null ? (Vector2)m_ShockwaveSpawnPos.position : (Vector2)transform.position;
        Vector2 attackDir = m_LockedAttackDirection == 1 ? Vector2.right : Vector2.left;

        GameObject shockwaveObj = PoolManager.Instance.Get(prefabToFire, spawnPos, Quaternion.identity);

        if (shockwaveObj == null) return;

        if (shockwaveObj.TryGetComponent(out RockShockwave shockwave))
        {
            shockwave.Init(m_Damage, attackDir, gameObject, IsElite);
        }
    }

    //public void SpawnShockwave()
    //{
    //    if (!m_IsSpawnFinished || m_CurrentState != ERockGolemState.Attack || m_CurrentHp <= 0f) return;


    //    GameObject prefabToFire = (IsElite && m_EliteShockwavePrefab != null) ? m_EliteShockwavePrefab : m_ShockwavePrefab;

    //    if (m_ShockwavePrefab == null)
    //    {
    //       // Debug.LogError("[RockGolemAI] 충격파 프리팹이 연결되지 않았습니다.");
    //        return;
    //    }

    //    if (PoolManager.Instance == null)
    //    {
    //        //Debug.LogError("[RockGolemAI] PoolManager.Instance가 없습니다.");
    //        return;
    //    }

    //    Vector2 spawnPos = m_ShockwaveSpawnPos != null ? (Vector2)m_ShockwaveSpawnPos.position : (Vector2)transform.position;
    //    Vector2 attackDir = m_LockedAttackDirection == 1 ? Vector2.right : Vector2.left;

    //    GameObject shockwaveObj = PoolManager.Instance.Get(m_ShockwavePrefab, spawnPos, Quaternion.identity);

    //    if (shockwaveObj == null)
    //    {
    //        Debug.LogError("[RockGolemAI] 충격파 생성에 실패했습니다.");
    //        return;
    //    }

    //    if (shockwaveObj.TryGetComponent(out RockShockwave shockwave))
    //    {
    //        shockwave.Init(m_Damage, attackDir, gameObject);
    //    }
    //    else
    //    {
    //        //Debug.LogError("[RockGolemAI] 생성된 충격파에 RockShockwave가 없습니다.");
    //    }
    //}

    public void EndAttack()
    {
        if (m_CurrentState != ERockGolemState.Attack || m_CurrentHp <= 0f) return;

        m_AttackCooldownTimer = m_GroundData.AttackCooldown;
        ChangeState(ERockGolemState.Chase);
    }

    public override void MakeElite(EliteBuff buff)
    {
        base.MakeElite(buff);
        m_Damage *= buff.DamageMultiplier;
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
        float smoothedVelocityX = Mathf.MoveTowards(currentVelocityX, finalTargetVelocityX, m_GroundData.Acceleration * Time.deltaTime);

        m_Rigidbody.linearVelocity = new Vector2(smoothedVelocityX, m_Rigidbody.linearVelocity.y);
    }

    private float CalculateSeparation()
    {
        float separationForceX = 0f;
        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = m_EnemyLayer;
        int count = Physics2D.OverlapCircle(transform.position, m_GroundData.SeparationRadius, filter, m_NearbyEnemies);

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

            float ratio = 1f - (distance / m_GroundData.SeparationRadius);
            ratio = Mathf.Clamp01(ratio);

            float pushForce = m_GroundData.SeparationForce * (ratio * ratio);
            Vector2 pushVector = (diff / distance) * pushForce;
            separationForceX += pushVector.x;
        }

        return Mathf.Clamp(separationForceX, -m_GroundData.MoveSpeed * 1.5f, m_GroundData.MoveSpeed * 1.5f);
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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (m_Collider == null) m_Collider = GetComponent<BoxCollider2D>();
        if (m_Collider == null) return;
        if (m_GroundData == null) return;

        int gizmoDir = Application.isPlaying ? m_Direction : (m_IsFacingRight ? 1 : -1);
        if (gizmoDir == 0) gizmoDir = 1;

        float extentsX = m_Collider.bounds.extents.x;
        float checkX = m_Collider.bounds.center.x + (extentsX * gizmoDir);

        Gizmos.color = Color.red;
        Vector2 cliffCheckPos = new Vector2(checkX, m_Collider.bounds.min.y);
        Gizmos.DrawLine(cliffCheckPos, cliffCheckPos + Vector2.down * m_GroundData.GroundRayLength);

        Gizmos.color = Color.blue;
        Vector2 wallCheckPos = new Vector2(checkX, m_Collider.bounds.center.y - (m_Collider.bounds.extents.y * 0.5f));
        Vector2 wallDir = gizmoDir == 1 ? Vector2.right : Vector2.left;
        Gizmos.DrawLine(wallCheckPos, wallCheckPos + wallDir * m_GroundData.WallRayLength);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_GroundData.SeparationRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, m_GroundData.AttackRange);
    }
#endif
}
