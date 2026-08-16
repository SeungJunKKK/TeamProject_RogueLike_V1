using System;
using UnityEngine;

// 1계층: 보스 생명주기
public enum EProvidenceState
{
    None,
    Intro,
    Combat,
    PhaseTransition,
    Dead
}

// 2계층: 전투 중 상세 행동
public enum EProvidenceCombatState
{
    Approach,
    Optimal,
    TooClose,
    Attack,
    Recover
}

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class ProvidenceAI : EnemyBase
{
    [Header("Target Setting")]
    public Transform Player;

    [Header("Stats Setting")]
    [SerializeField] private float m_BaseDamage = 20f;
    protected float m_Damage;

    protected override bool CanReceiveKnockback => false;

    [Header("Movement & Distance Setting")]
    [SerializeField] private float m_MoveSpeed = 4f;
    [SerializeField] private float m_Acceleration = 15f;

    [SerializeField] private float m_TooCloseRange = 1.2f;
    [SerializeField] private float m_OptimalRangeMax = 2.0f;

    [Header("Combat: Slash Setting")]
    [Tooltip("Slash 공격 쿨타임")]
    [SerializeField] private float m_SlashCooldown = 3.0f;
    [Tooltip("Slash 히트박스 오프셋 (보스 중심으로부터의 위치)")]
    [SerializeField] private Vector2 m_SlashHitboxOffset = new Vector2(1.5f, 0f);
    [Tooltip("Slash 히트박스 크기")]
    [SerializeField] private Vector2 m_SlashHitboxSize = new Vector2(2f, 2f);
    [Tooltip("타격 대상 레이어")]
    [SerializeField] private LayerMask m_TargetLayer; // Player 레이어 할당 필요

    private float m_SlashCooldownTimer = 0f;

    // 💡 애니메이션 이벤트 누락 대비 안전장치 (Failsafe)
    private float m_ActionFailsafeTimer = 0f;
    private const float k_MaxActionTime = 3.0f; // 어떤 액션이든 3초가 넘어가면 강제 종료

    protected Animator m_Animator;
    protected BoxCollider2D m_Collider;

    [SerializeField] private EProvidenceState m_CurrentState = EProvidenceState.None;
    protected bool m_IsBattleStarted = false;

    [SerializeField] private EProvidenceCombatState m_CombatState = EProvidenceCombatState.Approach;

    protected int m_Direction = 1;
    protected float m_DistanceXToPlayer;
    protected float m_DistanceToPlayer;
    protected float m_DesiredVelocityX;
    protected bool m_IsFacingRight = true;

    private readonly Collider2D[] m_HitBuffer = new Collider2D[5];

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

        m_IsBattleStarted = false;
        m_IsFacingRight = true;
        m_SlashCooldownTimer = 0f;
        ChangeState(EProvidenceState.None);
    }

    // ==========================================
    // [Lifecycle: 1계층 FSM 흐름 제어]
    // ==========================================
    public void StartIntro() { ChangeState(EProvidenceState.Intro); if (m_Animator != null) m_Animator.Play("Spawn"); }
    public void IntroComplete() { if (BossBattleController.Instance != null) BossBattleController.Instance.OnIntroFinished(); }
    public void EnterPhase1() { m_IsBattleStarted = true; ChangeCombatState(EProvidenceCombatState.Approach); ChangeState(EProvidenceState.Combat); }
    public void ExitArenaForPhase2() { m_IsBattleStarted = false; ChangeState(EProvidenceState.PhaseTransition); gameObject.SetActive(false); }
    public void ReturnToArenaForPhase3() { gameObject.SetActive(true); m_IsBattleStarted = true; ChangeCombatState(EProvidenceCombatState.Approach); ChangeState(EProvidenceState.Combat); }

    // ==========================================
    // [Module: Update Loop]
    // ==========================================
    private void Update()
    {
        if (!m_IsBattleStarted || Player == null || m_CurrentHp <= 0f) return;

        CheckHPForTransition();
        StateMachine();
    }

    private void CheckHPForTransition()
    {
        float hpPercent = m_CurrentHp / m_MaxHp;
        if (BossBattleController.Instance != null) BossBattleController.Instance.CheckPhaseTransition(hpPercent);
    }

    private void StateMachine()
    {
        switch (m_CurrentState)
        {
            case EProvidenceState.Combat:
                UpdateCombatFSM();
                break;
            case EProvidenceState.PhaseTransition:
                break;
        }
    }

    // ==========================================
    // [Module: Combat & Movement (2계층 FSM)]
    // ==========================================
    private void UpdateCombatFSM()
    {
        UpdateTimers();

        // 행동 중일 때는 이동 잠금 및 Failsafe 검사
        if (m_CombatState == EProvidenceCombatState.Attack || m_CombatState == EProvidenceCombatState.Recover)
        {
            ApplyMovement(); // 이동 정지 유지

            // 💡 Failsafe: 애니메이션 이벤트가 모종의 이유로 누락되어 무한 대기하는 것 방지
            if (m_ActionFailsafeTimer <= 0f)
            {
                Debug.LogWarning("[ProvidenceAI] Action Failsafe Triggered! 강제로 상태를 복구합니다.");
                FinishAction();
            }
            return;
        }

        UpdateTargetInfo();
        CalculateDistanceState();
        ApplyMovement();
        CheckAttackConditions();
    }

    private void UpdateTimers()
    {
        if (m_SlashCooldownTimer > 0f) m_SlashCooldownTimer -= Time.deltaTime;

        // Failsafe 타이머 감소
        if (m_ActionFailsafeTimer > 0f) m_ActionFailsafeTimer -= Time.deltaTime;
    }

    private void UpdateTargetInfo()
    {
        m_DistanceXToPlayer = Mathf.Abs(Player.position.x - transform.position.x);

        if (m_DistanceXToPlayer > 0.2f)
        {
            m_Direction = Player.position.x > transform.position.x ? 1 : -1;
            if ((m_Direction == 1 && !m_IsFacingRight) || (m_Direction == -1 && m_IsFacingRight))
            {
                Flip();
            }
        }
    }

    private void CalculateDistanceState()
    {
        if (m_DistanceXToPlayer > m_OptimalRangeMax)
        {
            ChangeCombatState(EProvidenceCombatState.Approach);
            m_DesiredVelocityX = m_Direction * m_MoveSpeed;
        }
        else if (m_DistanceXToPlayer >= m_TooCloseRange)
        {
            ChangeCombatState(EProvidenceCombatState.Optimal);
            m_DesiredVelocityX = 0f;
        }
        else
        {
            ChangeCombatState(EProvidenceCombatState.TooClose);
            m_DesiredVelocityX = 0f;
        }
    }

    private void CheckAttackConditions()
    {
        if (m_CombatState == EProvidenceCombatState.Optimal && m_SlashCooldownTimer <= 0f)
        {
            ExecuteSlashAttack();
        }
    }

    private void ApplyMovement()
    {
        float currentVelocityX = m_Rigidbody.linearVelocity.x;
        float smoothedVelocityX = Mathf.MoveTowards(currentVelocityX, m_DesiredVelocityX, m_Acceleration * Time.deltaTime);
        m_Rigidbody.linearVelocity = new Vector2(smoothedVelocityX, m_Rigidbody.linearVelocity.y);

        if (m_Animator != null) m_Animator.SetFloat("Speed", Mathf.Abs(m_Rigidbody.linearVelocity.x));
    }

    private void Flip()
    {
        m_IsFacingRight = !m_IsFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // ==========================================
    // [Action: Slash Attack Module]
    // ==========================================
    private void ExecuteSlashAttack()
    {
        ChangeCombatState(EProvidenceCombatState.Attack);
        m_DesiredVelocityX = 0f;
        m_SlashCooldownTimer = m_SlashCooldown;
        m_ActionFailsafeTimer = k_MaxActionTime; // 안전장치 온!

        if (m_Animator != null) m_Animator.SetTrigger("Slash");
    }

    /// <summary>
    /// ★ 애니메이션 이벤트: 검을 휘두르는 정확한 프레임(Hit)에서 호출!
    /// </summary>
    public void PerformSlash()
    {
        // 1. 월드 좌표 기준의 정확한 히트박스 중심점 계산
        Vector2 hitCenter = (Vector2)transform.position + new Vector2(m_Direction * m_SlashHitboxOffset.x, m_SlashHitboxOffset.y);

        // 2. 오버랩 검사
        int hitCount = Physics2D.OverlapBoxNonAlloc(hitCenter, m_SlashHitboxSize, 0f, m_HitBuffer, m_TargetLayer);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D col = m_HitBuffer[i];

            // 💡 실전 최적화: IDamageable이 Player 최상단에 있을 수도, Hurtbox에 있을 수도 있음을 대비
            IDamageable targetDamageable = col.GetComponent<IDamageable>();
            if (targetDamageable == null)
            {
                targetDamageable = col.GetComponentInParent<IDamageable>();
            }

            if (targetDamageable != null)
            {
                DamageInfo info = new DamageInfo
                {
                    Amount = m_Damage,
                    HitPoint = col.ClosestPoint(hitCenter),
                    HitDirection = new Vector2(m_Direction, 0f),
                    KnockbackForce = 10f,
                    Attacker = gameObject,
                    IsCrit = false,
                    CanProc = true
                };

                targetDamageable.TakeDamage(info);

                // 💡 핵심: 플레이어 단일 타겟이므로, "한 명이라도 맞췄으면" 이번 칼질의 판정은 즉시 종료!
                // (플레이어가 다중 콜라이더를 가져서 데미지가 중복으로 들어가는 버그 완벽 차단)
                break;
            }
        }
    }

    /// <summary>
    /// 애니메이션 이벤트: 타격 이후 후딜레이(Recover) 시작 프레임에서 호출
    /// </summary>
    public void EnterRecovery()
    {
        ChangeCombatState(EProvidenceCombatState.Recover);
        m_ActionFailsafeTimer = k_MaxActionTime;
    }

    /// <summary>
    /// 애니메이션 이벤트: 액션 완전 종료
    /// </summary>
    public void FinishAction()
    {
        m_ActionFailsafeTimer = 0f; // 안전장치 초기화
        ChangeCombatState(EProvidenceCombatState.Approach);
    }

    // ==========================================
    // [State Changers & Death]
    // ==========================================
    protected virtual void ChangeState(EProvidenceState newState) { if (m_CurrentState == newState) return; m_CurrentState = newState; }
    protected virtual void ChangeCombatState(EProvidenceCombatState newCombatState) { if (m_CombatState == newCombatState) return; m_CombatState = newCombatState; }

    protected override void Die()
    {
        m_IsBattleStarted = false;
        ChangeState(EProvidenceState.Dead);
        m_Rigidbody.linearVelocity = Vector2.zero;
        if (m_Animator != null) m_Animator.SetTrigger("Death");
        if (BossBattleController.Instance != null) BossBattleController.Instance.OnBossDead();
    }
    public void DeathComplete() { gameObject.SetActive(false); }

    // ==========================================
    // [Debug: Gizmos]
    // ==========================================
    private void OnDrawGizmosSelected()
    {
        // Slash 히트박스를 에디터에서 볼 수 있도록 그려줌
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        int dir = Application.isPlaying ? m_Direction : (m_IsFacingRight ? 1 : -1);
        Vector2 hitCenter = (Vector2)transform.position + new Vector2(dir * m_SlashHitboxOffset.x, m_SlashHitboxOffset.y);
        Gizmos.DrawCube(hitCenter, m_SlashHitboxSize);
    }
}