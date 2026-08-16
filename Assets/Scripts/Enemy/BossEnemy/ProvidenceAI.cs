using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EProvidenceState { None, Intro, Combat, PhaseTransition, Dead }
public enum EProvidenceCombatState { Approach, Optimal, TooClose, Attack, Recover }

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class ProvidenceAI : EnemyBase
{
    [Header("Target & Stats")]
    public Transform Player;
    [SerializeField] private float m_BaseDamage = 20f;
    protected float m_Damage;
    protected override bool CanReceiveKnockback => false;

    [Header("Movement")]
    [SerializeField] private float m_MoveSpeed = 4f;
    [SerializeField] private float m_Acceleration = 15f;

    [Header("Combat Patterns & Distances")]
    [SerializeField] private float m_SlashCooldown = 3.0f;
    [Tooltip("이 거리보다 멀어지면 텔레포트 내리찍기(Z2) 발동")]
    [SerializeField] private float m_TeleportThresholdDistance = 3.0f;
    [Tooltip("근거리(Z1)와 중거리(X1/C1)를 나누는 기준 거리")]
    [SerializeField] private float m_MeleeRangeLimit = 1.5f;

    [Header("Environment & Effects")]
    [SerializeField] private LayerMask m_GroundLayer;
    [SerializeField] private LayerMask m_TargetLayer;
    [SerializeField] private TrailRenderer m_TrailRenderer;

    private float m_SlashCooldownTimer = 0f;
    private string m_LastAttackName = "";

    [Header("Hitbox")]
    [SerializeField] private Vector2 m_SlashHitboxOffset = new Vector2(1.5f, 0f);
    [SerializeField] private Vector2 m_SlashHitboxSize = new Vector2(2f, 2f);

    protected Animator m_Animator;
    protected BoxCollider2D m_Collider;
    [SerializeField] private EProvidenceState m_CurrentState = EProvidenceState.None;
    [SerializeField] private EProvidenceCombatState m_CombatState = EProvidenceCombatState.Approach;

    private float m_ActionFailsafeTimer = 0f;
    private const float k_MaxActionTime = 3.0f;
    protected bool m_IsBattleStarted = false;
    protected float m_DistanceXToPlayer;
    protected int m_Direction = 1;
    protected bool m_IsFacingRight = true;
    private readonly Collider2D[] m_HitBuffer = new Collider2D[5];

    protected virtual void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Collider = GetComponent<BoxCollider2D>();
        m_Animator = GetComponent<Animator>();
        if (m_TrailRenderer != null) m_TrailRenderer.emitting = false;
    }

    public override void SetTarget(Transform target) => Player = target;
    public override void OnSpawn() { base.OnSpawn(); m_IsBattleStarted = false; m_SlashCooldownTimer = 0f; ChangeState(EProvidenceState.None); }

    public void StartIntro() { ChangeState(EProvidenceState.Intro); StartCoroutine(CoIntroRoutine()); }
    public void IntroComplete() { if (BossBattleController.Instance != null) BossBattleController.Instance.OnIntroFinished(); }
    public void EnterPhase1() { m_IsBattleStarted = true; ChangeCombatState(EProvidenceCombatState.Approach); ChangeState(EProvidenceState.Combat); }

    // 💡 페이즈 2/3 전환 메서드 (여기에 있어야 함!)
    public void ExitArenaForPhase2()
    {
        m_IsBattleStarted = false;
        ChangeState(EProvidenceState.PhaseTransition);
        if (m_TrailRenderer != null) m_TrailRenderer.emitting = false;
        gameObject.SetActive(false);
    }

    public void ReturnToArenaForPhase3()
    {
        gameObject.SetActive(true);
        m_IsBattleStarted = true;
        ChangeCombatState(EProvidenceCombatState.Approach);
        ChangeState(EProvidenceState.Combat);
    }

    private void Update()
    {
        if (!m_IsBattleStarted || Player == null || m_CurrentHp <= 0f) return;
        CheckHPForTransition();
        StateMachine();
    }

    private void CheckHPForTransition()
    {
        if (BossBattleController.Instance != null)
            BossBattleController.Instance.CheckPhaseTransition(GetHpRatio());
    }

    private IEnumerator CoIntroRoutine() { yield return new WaitForSeconds(1.0f); IntroComplete(); }
    private void StateMachine() { if (m_CurrentState == EProvidenceState.Combat) UpdateCombatFSM(); }

    private void UpdateCombatFSM()
    {
        UpdateTimers();
        if (m_CombatState == EProvidenceCombatState.Attack || m_CombatState == EProvidenceCombatState.Recover)
        {
            ApplyMovement();
            if (m_ActionFailsafeTimer <= 0f) FinishAction();
            return;
        }
        UpdateTargetInfo();
        CheckAttackConditions();
        ApplyMovement();
    }

    private void UpdateTargetInfo()
    {
        m_DistanceXToPlayer = Mathf.Abs(Player.position.x - transform.position.x);
        if (m_DistanceXToPlayer > 0.2f)
        {
            int newDir = Player.position.x > transform.position.x ? 1 : -1;
            m_Direction = newDir;
            if ((m_Direction == 1 && !m_IsFacingRight) || (m_Direction == -1 && m_IsFacingRight)) Flip();
        }
    }

    private void CheckAttackConditions()
    {
        if (m_SlashCooldownTimer > 0f) return;
        if (m_DistanceXToPlayer < m_MeleeRangeLimit) ExecuteNormalSlash();
        else if (m_DistanceXToPlayer <= m_TeleportThresholdDistance) ExecuteMediumRangePattern();
        else ExecuteTeleportSlash();
    }

    private void ExecuteMediumRangePattern()
    {
        float weightX1 = (m_LastAttackName == "ShootX1") ? 20f : 60f;
        float weightC1 = (m_LastAttackName == "ShootC1") ? 20f : 40f;
        if (UnityEngine.Random.Range(0f, weightX1 + weightC1) < weightX1) ExecuteX1Attack();
        else ExecuteC1Attack();
    }

    private void ExecuteNormalSlash() { m_LastAttackName = "ShootZ1"; StartAction("ShootZ1"); }
    private void ExecuteX1Attack() { m_LastAttackName = "ShootX1"; StartAction("ShootX1"); }
    private void ExecuteC1Attack() { m_LastAttackName = "ShootC1"; StartAction("ShootC1"); }
    private void ExecuteTeleportSlash() { m_LastAttackName = "ShootZ2"; StartCoroutine(CoTeleportDownwardSlashRoutine()); }

    private void ApplyMovement()
    {
        if (m_CombatState == EProvidenceCombatState.Attack || m_CombatState == EProvidenceCombatState.Recover)
        {
            if (m_Rigidbody != null) m_Rigidbody.linearVelocity = new Vector2(0f, m_Rigidbody.linearVelocity.y);
            return;
        }
        float speed = (m_DistanceXToPlayer > m_MeleeRangeLimit) ? m_MoveSpeed : 0f;
        m_Rigidbody.linearVelocity = new Vector2(Mathf.MoveTowards(m_Rigidbody.linearVelocity.x, m_Direction * speed, m_Acceleration * Time.deltaTime), m_Rigidbody.linearVelocity.y);
    }

    private void Flip()
    {
        m_IsFacingRight = !m_IsFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private IEnumerator CoTeleportDownwardSlashRoutine()
    {
        ChangeCombatState(EProvidenceCombatState.Attack);
        m_Rigidbody.bodyType = RigidbodyType2D.Kinematic;
        m_Rigidbody.linearVelocity = Vector2.zero;
        m_SlashCooldownTimer = m_SlashCooldown;
        m_ActionFailsafeTimer = k_MaxActionTime;

        if (m_TrailRenderer != null) m_TrailRenderer.emitting = true;
        transform.position = new Vector2(Player.position.x, Player.position.y + 2.5f);
        if (m_Animator != null) m_Animator.Play("ShootZ2");

        float targetY = Player.position.y;
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(Player.position.x, transform.position.y), Vector2.down, 20f, m_GroundLayer);
        if (hit.collider != null) targetY = hit.point.y + m_Collider.bounds.extents.y;

        Vector3 targetPos = new Vector3(Player.position.x, targetY, transform.position.z);
        float elapsed = 0f;
        while (elapsed < 0.35f)
        {
            if (m_CombatState != EProvidenceCombatState.Attack) yield break;
            elapsed += Time.deltaTime;
            Vector3 nextPos = Vector3.Lerp(transform.position, targetPos, elapsed / 0.35f);
            if (nextPos.y < targetY) nextPos.y = targetY;
            transform.position = nextPos;
            yield return null;
        }
        transform.position = targetPos;
    }

    private void StartAction(string clipName)
    {
        ChangeCombatState(EProvidenceCombatState.Attack);
        m_Rigidbody.bodyType = RigidbodyType2D.Kinematic;
        m_Rigidbody.linearVelocity = Vector2.zero;
        m_SlashCooldownTimer = m_SlashCooldown;
        m_ActionFailsafeTimer = k_MaxActionTime;
        if (m_Animator != null) m_Animator.Play(clipName);
    }

    public void PerformSlash()
    {
        Vector2 hitCenter = (Vector2)transform.position + new Vector2(m_Direction * m_SlashHitboxOffset.x, m_SlashHitboxOffset.y);
        int hitCount = Physics2D.OverlapBoxNonAlloc(hitCenter, m_SlashHitboxSize, 0f, m_HitBuffer, m_TargetLayer);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D col = m_HitBuffer[i];
            IDamageable targetDamageable = col.GetComponent<IDamageable>() ?? col.GetComponentInParent<IDamageable>();

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
                break;
            }
        }
    }

    public void EnterRecovery()
    {
        ChangeCombatState(EProvidenceCombatState.Recover);
        m_ActionFailsafeTimer = k_MaxActionTime;
        if (m_TrailRenderer != null) m_TrailRenderer.emitting = false;
    }

    public void FinishAction()
    {
        m_ActionFailsafeTimer = 0f;
        if (m_TrailRenderer != null) m_TrailRenderer.emitting = false;
        m_Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        ChangeCombatState(EProvidenceCombatState.Approach);
    }

    protected virtual void ChangeState(EProvidenceState newState)
    {
        if (m_CurrentState == newState) return;
        m_CurrentState = newState;
    }

    protected virtual void ChangeCombatState(EProvidenceCombatState newCombatState)
    {
        if (m_CombatState == newCombatState) return;
        m_CombatState = newCombatState;
    }

    protected override void Die()
    {
        m_IsBattleStarted = false;
        ChangeState(EProvidenceState.Dead);
        m_Rigidbody.linearVelocity = Vector2.zero;
        if (m_TrailRenderer != null) m_TrailRenderer.emitting = false;
        m_Rigidbody.bodyType = RigidbodyType2D.Dynamic;

        if (m_Animator != null) m_Animator.Play("Death");
        if (BossBattleController.Instance != null) BossBattleController.Instance.OnBossDead();
    }

    public void DeathComplete() { gameObject.SetActive(false); }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        int dir = Application.isPlaying ? m_Direction : (m_IsFacingRight ? 1 : -1);
        Vector2 hitCenter = (Vector2)transform.position + new Vector2(dir * m_SlashHitboxOffset.x, m_SlashHitboxOffset.y);
        Gizmos.DrawCube(hitCenter, m_SlashHitboxSize);
    }

    private void UpdateTimers()
    {
        if (m_SlashCooldownTimer > 0f) m_SlashCooldownTimer -= Time.deltaTime;
        if (m_ActionFailsafeTimer > 0f) m_ActionFailsafeTimer -= Time.deltaTime;
    }
}