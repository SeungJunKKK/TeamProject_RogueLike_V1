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
    [Tooltip("근거리(Z1/Z2/X1)와 원거리(Z2/X1)를 나누는 기준 거리")]
    [SerializeField] private float m_MeleeRangeLimit = 1.5f;
    [SerializeField] private float m_RecoveryDelay = 0.2f;

    [Header("Combat: X1 Cascading Explosion Setting")]
    [Tooltip("통합된 폭발 이펙트 프리팹 (하나만 등록)")]
    [SerializeField] private GameObject m_ExplosionPrefab;
    [Tooltip("폭발 생성 간격 (거리)")]
    [SerializeField] private float m_ExplosionStep = 1.2f;
    [Tooltip("폭발 간 딜레이 (초)")]
    [SerializeField] private float m_ExplosionStepDelay = 0.06f;

    [Header("Combat: Pattern Counter")]
    [SerializeField] private int m_SlashCount = 0;
    [SerializeField] private int m_X1Count = 0;
    private const int k_MaxSlashBeforeX1 = 3;
    private const int k_MaxX1BeforeC1 = 2;

    [Header("Combat: C1 Purple Circle Setting")]
    [Tooltip("플레이어 위치에 생성될 보라색 원 프리팹")]
    [SerializeField] private GameObject m_PurpleCirclePrefab;
    [Tooltip("원 연속 소환 횟수")]
    [SerializeField] private int m_CircleCount = 4;
    [Tooltip("원 소환 간격 (초)")]
    [SerializeField] private float m_CircleSpawnInterval = 0.4f;

    [Header("Environment & Effects")]
    [SerializeField] private LayerMask m_GroundLayer;
    [SerializeField] private LayerMask m_TargetLayer;
    [SerializeField] private TrailRenderer m_TrailRenderer;

    private float m_SlashCooldownTimer = 0f;
    private string m_LastAttackName = "";

    [Header("Hitbox (Z1 Slash)")]
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

    public override void OnSpawn()
    {
        base.OnSpawn();
        var difficulty = DifficultyManager.Instance;
        m_Damage = difficulty != null ? m_BaseDamage * difficulty.Coefficient : m_BaseDamage;

        m_IsBattleStarted = false;
        m_IsFacingRight = true;
        m_SlashCooldownTimer = 0f;
        if (m_TrailRenderer != null) m_TrailRenderer.emitting = false;
        ChangeState(EProvidenceState.None);
    }

    // ==========================================
    // [Lifecycle & Phase Transitions]
    // ==========================================
    public void StartIntro() { ChangeState(EProvidenceState.Intro); StartCoroutine(CoIntroRoutine()); }
    public void IntroComplete() { if (BossBattleController.Instance != null) BossBattleController.Instance.OnIntroFinished(); }
    public void EnterPhase1() { m_IsBattleStarted = true; ChangeCombatState(EProvidenceCombatState.Approach); ChangeState(EProvidenceState.Combat); }

    public void ExitArenaForPhase2()
    {
        m_IsBattleStarted = false;
        StopAllCoroutines();
        m_Rigidbody.linearVelocity = Vector2.zero;
        m_Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        ChangeState(EProvidenceState.PhaseTransition);

        if (m_TrailRenderer != null) m_TrailRenderer.emitting = false;

        // 💡 즉시 숨기지 않고 사망("Death") 애니메이션 재생
        if (m_Animator != null) m_Animator.Play("Death");
    }

    public void ReturnToArenaForPhase3()
    {
        gameObject.SetActive(true);
        m_IsBattleStarted = true;
        m_Rigidbody.bodyType = RigidbodyType2D.Dynamic;
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

    // ==========================================
    // [Combat FSM Logic]
    // ==========================================
    private void UpdateCombatFSM()
    {
        UpdateTimers();

        if (m_CombatState == EProvidenceCombatState.Attack)
        {
            ApplyMovement();
            if (m_ActionFailsafeTimer <= 0f) FinishAction();
            return;
        }
        else if (m_CombatState == EProvidenceCombatState.Recover)
        {
            ApplyMovement();
            if (m_ActionFailsafeTimer <= 0f)
            {
                ChangeCombatState(EProvidenceCombatState.Approach);
            }
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

        if (m_SlashCount >= k_MaxSlashBeforeX1)
        {
            ExecuteX1Attack();
            return;
        }

        if (m_DistanceXToPlayer < m_MeleeRangeLimit)
        {
            ExecuteNearRangePattern(); // 근접: Z1 / Z2 / X1 랜덤
        }
        else
        {
            ExecuteFarRangePattern();  // 원거리: Z2 / X1 랜덤
        }
    }

    private void ExecuteNearRangePattern()
    {
        // 💡 랜덤 풀에서 X1을 제거하고 Z1(일반 베기), Z2(순간이동 베기)만 남김
        float weightZ1 = (m_LastAttackName == "ShootZ1") ? 20f : 60f;
        float weightZ2 = (m_LastAttackName == "ShootZ2") ? 20f : 40f;

        float total = weightZ1 + weightZ2;
        float roll = UnityEngine.Random.Range(0f, total);

        if (roll < weightZ1)
        {
            ExecuteNormalSlash();
        }
        else
        {
            ExecuteTeleportSlash();
        }
    }

    private void ExecuteFarRangePattern()
    {
            ExecuteTeleportSlash();
    }

    // ==========================================
    // [Attack Execution Methods]
    // ==========================================
    private void ExecuteNormalSlash()
    {
        m_LastAttackName = "ShootZ1";
        m_SlashCount++;
        StartAction("ShootZ1");
    }

    private void ExecuteX1Attack()
    {
        m_LastAttackName = "ShootX1";
        m_X1Count++;
        m_SlashCount = 0;
        StartAction("ShootX1");
    }

    private void ExecuteC1Attack()
    {
        m_LastAttackName = "ShootC1";
        StartCoroutine(CoPurpleCircleRoutine());
    }

    private void ExecuteTeleportSlash()
    {
        m_LastAttackName = "ShootZ2";
        m_SlashCount++;
        StartCoroutine(CoTeleportDownwardSlashRoutine());
    }

    // ==========================================
    // [Attack Coroutines & Events]
    // ==========================================
    public void PerformExplosion()
    {
        StartCoroutine(CoCascadingExplosionRoutine(1f));
        StartCoroutine(CoCascadingExplosionRoutine(-1f));
    }

    private IEnumerator CoCascadingExplosionRoutine(float waveDirection)
    {
        float currentX = transform.position.x + (waveDirection * 1.5f);
        bool isHitWall = false;

        int maxSteps = 20;
        int currentStep = 0;

        while (!isHitWall && currentStep < maxSteps)
        {
            float groundY = transform.position.y;
            RaycastHit2D groundHit = Physics2D.Raycast(new Vector2(currentX, transform.position.y + 1f), Vector2.down, 5f, m_GroundLayer);
            if (groundHit.collider != null) groundY = groundHit.point.y;

            RaycastHit2D wallHit = Physics2D.Raycast(new Vector2(currentX, groundY + 0.5f), Vector2.right * waveDirection, m_ExplosionStep, m_GroundLayer);
            if (wallHit.collider != null && !wallHit.collider.isTrigger)
            {
                isHitWall = true;
                break;
            }

            Vector2 checkCenter = new Vector2(currentX, groundY + 1f);
            Vector2 checkSize = new Vector2(m_ExplosionStep, 3f);
            Collider2D playerHit = Physics2D.OverlapBox(checkCenter, checkSize, 0f, m_TargetLayer);

            // 💡 초기 기획대로 플레이어를 만나면 BigWave(큰 기둥)를 생성하고 루프를 즉시 종료
            if (playerHit != null)
            {
                SpawnExplosionEffect(currentX, groundY, waveDirection, true);
                break;
            }

            // 플레이어를 못 만났으면 평소대로 SmallWave 생성 후 전진 계속
            SpawnExplosionEffect(currentX, groundY, waveDirection, false);

            currentX += waveDirection * m_ExplosionStep;
            currentStep++;
            yield return new WaitForSeconds(m_ExplosionStepDelay);
        }
    }

    private void SpawnExplosionEffect(float x, float y, float dir, bool isBigPillar)
    {
        if (m_ExplosionPrefab != null)
        {
            GameObject fx = Instantiate(m_ExplosionPrefab, new Vector2(x, y), Quaternion.identity);
            if (fx.TryGetComponent(out BossSpreadingEffect effect))
            {
                float dmg = isBigPillar ? m_Damage * 1.5f : m_Damage * 0.8f;
                effect.Setup(dmg, (int)dir, gameObject, isBigPillar);
            }
        }
    }

    private IEnumerator CoPurpleCircleRoutine()
    {
        ChangeCombatState(EProvidenceCombatState.Attack);
        m_Rigidbody.bodyType = RigidbodyType2D.Kinematic;
        m_Rigidbody.linearVelocity = Vector2.zero;

        m_ActionFailsafeTimer = 6.0f;

        if (m_Animator != null)
        {
            m_Animator.Play("ShootC1");
        }
        yield return new WaitForSeconds(1.0f);

        if (m_Animator != null)
        {
            m_Animator.Play("ShootC1_2");
        }

        float duration = 4.0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (Player != null && m_PurpleCirclePrefab != null)
            {
                Vector2 targetPos = Player.GetComponent<Collider2D>() != null
                    ? Player.GetComponent<Collider2D>().bounds.center
                    : Player.position;

                for (int i = 0; i < 2; i++)
                {
                    GameObject circle = Instantiate(m_PurpleCirclePrefab, targetPos, Quaternion.identity);
                    if (circle.TryGetComponent(out PurpleCircleEffect effect))
                    {
                        float direction = (i == 0) ? 1f : -1f;
                        effect.Setup(m_Damage * 0.7f, gameObject, direction);
                    }
                }
            }

            yield return new WaitForSeconds(m_CircleSpawnInterval);
            elapsed += m_CircleSpawnInterval;
        }

        FinishAction();
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

    public void PerformSlash()
    {
        Debug.Log("베기 판정");
        Vector2 hitCenter = (Vector2)transform.position + new Vector2(m_Direction * m_SlashHitboxOffset.x, m_SlashHitboxOffset.y);
        int hitCount = Physics2D.OverlapBoxNonAlloc(hitCenter, m_SlashHitboxSize, 0f, m_HitBuffer, m_TargetLayer);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D col = m_HitBuffer[i];

            // 💡 IDamageable 대신 PlayerController를 찾도록 수정
            PlayerController player = col.GetComponent<PlayerController>() ?? col.GetComponentInParent<PlayerController>();

            if (player != null)
            {
                // 💡 PlayerController에 직접 m_Damage(숫자) 전달
                player.TakeDamage(m_Damage);
                break;
            }
        }
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

    public void EnterRecovery()
    {
        ChangeCombatState(EProvidenceCombatState.Recover);
        m_ActionFailsafeTimer = k_MaxActionTime;
        if (m_TrailRenderer != null) m_TrailRenderer.emitting = false;
    }

    public void FinishAction()
    {
        if (m_TrailRenderer != null) m_TrailRenderer.emitting = false;

        m_Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        if (m_X1Count >= k_MaxX1BeforeC1)
        {
            m_X1Count = 0;
            ExecuteC1Attack();
            return;
        }

        ChangeCombatState(EProvidenceCombatState.Recover);
        m_ActionFailsafeTimer = m_RecoveryDelay;
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

    private void UpdateTimers()
    {
        if (m_SlashCooldownTimer > 0f) m_SlashCooldownTimer -= Time.deltaTime;
        if (m_ActionFailsafeTimer > 0f) m_ActionFailsafeTimer -= Time.deltaTime;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        int dir = Application.isPlaying ? m_Direction : (m_IsFacingRight ? 1 : -1);
        Vector2 hitCenter = (Vector2)transform.position + new Vector2(dir * m_SlashHitboxOffset.x, m_SlashHitboxOffset.y);
        Gizmos.DrawCube(hitCenter, m_SlashHitboxSize);
    }
}