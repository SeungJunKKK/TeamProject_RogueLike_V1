using UnityEngine;
using System.Collections;

public class RedGildedWurm : GildedWurmBase
{
    [Header("Laser Animation Settings")]
    [Tooltip("화면에 표시될 레이저 이펙트(애니메이션이 포함된) 오브젝트")]
    [SerializeField] private GameObject m_LaserObject;

    [Tooltip("레이저가 뿜어져 나오는 웜의 입 위치 (기준점)")]
    [SerializeField] private Transform m_MouthPoint;

    [Tooltip("레이저 스프라이트 이미지의 기본 방향을 맞추기 위한 회전 보정값")]
    [SerializeField] private float m_LaserSpriteRotationOffset = -90f;

    [Tooltip("레이저가 플레이어의 위치를 부드럽게 따라가며 회전하는 속도")]
    [SerializeField] private float m_LaserRotationSpeed = 180f;

    [Header("Laser Damage Settings")]
    [Tooltip("레이저 최대 사거리 (이 거리 안에서만 공격 조건 만족)")]
    [SerializeField] private float m_LaserRange = 20f;

    [Header("Attack Conditions")]
    [Range(0.5f, 1.0f)]
    [Tooltip("웜이 이동하는 방향과 플레이어 방향이 일치해야 공격하는 임계값")]
    [SerializeField] private float m_FacingThreshold = 0.94f;

    [Range(0f, 1f)]
    [Tooltip("공격 조건을 만족했을 때 실제로 레이저를 쏠 확률")]
    [SerializeField] private float m_AttackChance = 0.4f;

    [Tooltip("한 번 레이저를 발사하고 유지하는 총 지속 시간 (초)")]
    [SerializeField] private float m_AttackDuration = 3.0f;

    [Tooltip("레이저 공격이 끝난 후 다음 공격까지 대기하는 쿨타임 (초)")]
    [SerializeField] private float m_AttackCooldown = 3.0f;

    [Header("Attack Movement Settings")]
    [Range(0.1f, 1.0f)]
    [Tooltip("레이저 발사 중일 때 이동 속도가 평소의 몇 배로 줄어드는지 비율")]
    [SerializeField] private float m_AttackSpeedMultiplier = 0.5f;

    [Range(0.1f, 1.0f)]
    [Tooltip("레이저 발사 중일 때 몸체가 회전하는 속도가 평소의 몇 배로 느려지는지 비율")]
    [SerializeField] private float m_AttackTurnMultiplier = 0.6f;

    private bool m_IsAttacking = false;

    private float m_CurrentCooldown = 2.0f;
    private float m_OriginalMaxSpeed;
    private float m_OriginalTurnSpeed;

    // 현재 레이저의 실제 회전 각도
    private float m_CurrentLaserAngle;


    // =========================================================
    // Setup
    // =========================================================

    public override void Setup(Transform player)
    {
        base.Setup(player);

        m_OriginalMaxSpeed = m_MaxSpeed;
        m_OriginalTurnSpeed = m_TurnSpeed;

        m_IsAttacking = false;
        m_CurrentCooldown = 2.0f;

        if (m_LaserObject != null)
        {
            m_LaserObject.SetActive(false);
        }
    }


    // =========================================================
    // Update
    // =========================================================

    protected override void Update()
    {
        base.Update();

        if (m_Player == null)
            return;

        if (!m_IsAttacking)
        {
            m_CurrentCooldown -= Time.deltaTime;

            if (m_CurrentCooldown <= 0f)
            {
                CheckAttackConditions();
            }
        }
    }


    // =========================================================
    // Movement
    // =========================================================

    protected override void UpdateMovement()
    {
        if (m_IsAttacking)
        {
            m_MaxSpeed = Mathf.Lerp(
                m_MaxSpeed,
                m_OriginalMaxSpeed * m_AttackSpeedMultiplier,
                Time.deltaTime * 4f
            );

            m_TurnSpeed = Mathf.Lerp(
                m_TurnSpeed,
                m_OriginalTurnSpeed * m_AttackTurnMultiplier,
                Time.deltaTime * 4f
            );
        }
        else
        {
            m_MaxSpeed = Mathf.Lerp(
                m_MaxSpeed,
                m_OriginalMaxSpeed,
                Time.deltaTime * 2f
            );

            m_TurnSpeed = Mathf.Lerp(
                m_TurnSpeed,
                m_OriginalTurnSpeed,
                Time.deltaTime * 2f
            );
        }

        base.UpdateMovement();
    }


    // =========================================================
    // Attack Condition
    // =========================================================

    private void CheckAttackConditions()
    {
        if (m_Player == null)
            return;

        Vector2 toPlayer = m_Player.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance > m_LaserRange)
            return;

        if (m_Velocity.sqrMagnitude <= 0.001f)
        {
            m_CurrentCooldown = 0.5f;
            return;
        }

        Vector2 velocityDirection = m_Velocity.normalized;
        Vector2 playerDirection = toPlayer.normalized;

        float dotProduct = Vector2.Dot(
            velocityDirection,
            playerDirection
        );

        if (dotProduct > m_FacingThreshold)
        {
            if (Random.value <= m_AttackChance)
            {
                Debug.Log("🔴 빨간 웜: 레이저 공격 시작!");
                StartCoroutine(AttackRoutine());
            }
            else
            {
                m_CurrentCooldown = 1.0f;
            }
        }
    }


    // =========================================================
    // Attack Routine
    // =========================================================

    private IEnumerator AttackRoutine()
    {
        m_IsAttacking = true;

        if (m_LaserObject != null && m_MouthPoint != null)
        {
            // 💡 애니메이션 켜기
            Animator laserAnimator = m_LaserObject.GetComponent<Animator>();
            if (laserAnimator != null)
            {
                laserAnimator.SetBool("isAttacking", true);
            }

            m_LaserObject.SetActive(true);

            // 💡 빔의 최초 각도 계산
            Vector2 startPos = m_MouthPoint.position;
            Vector2 initialDirection = ((Vector2)m_Player.position - startPos).normalized;

            float initialAngle = Mathf.Atan2(initialDirection.y, initialDirection.x) * Mathf.Rad2Deg;
            m_CurrentLaserAngle = initialAngle + m_LaserSpriteRotationOffset;

            // 💡 2D 방식 완벽 위치 및 회전 적용
            m_LaserObject.transform.rotation = Quaternion.Euler(0f, 0f, m_CurrentLaserAngle);
            float beamLength = m_LaserObject.transform.localScale.y; // 주의: 레이저 이미지가 가로로 길면 y 대신 x로 변경!
            m_LaserObject.transform.position = startPos + (Vector2)(m_LaserObject.transform.up * (beamLength * 0.5f));
        }

        float elapsed = 0f;

        while (elapsed < m_AttackDuration)
        {
            if (m_Player == null)
                break;

            UpdateLaserTracking();
            elapsed += Time.deltaTime;

            yield return null;
        }

        if (m_LaserObject != null)
        {
            // 💡 애니메이션 끄기
            Animator laserAnimator = m_LaserObject.GetComponent<Animator>();
            if (laserAnimator != null)
            {
                laserAnimator.SetBool("isAttacking", false);
            }

            m_LaserObject.SetActive(false);
        }

        m_IsAttacking = false;
        m_CurrentCooldown = m_AttackCooldown;

        Debug.Log("🔴 빨간 웜: 레이저 공격 종료");
    }


    // =========================================================
    // Laser Tracking (위치 및 회전 추적 전용)
    // =========================================================

    private void UpdateLaserTracking()
    {
        if (m_Player == null || m_MouthPoint == null || m_LaserObject == null)
        {
            return;
        }

        Vector2 startPos = m_MouthPoint.position;
        Vector2 directionToPlayer = ((Vector2)m_Player.position - startPos).normalized;

        if (directionToPlayer.sqrMagnitude <= 0.001f)
            return;

        // 💡 부드러운 회전 추적 알고리즘
        float targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        targetAngle += m_LaserSpriteRotationOffset;

        m_CurrentLaserAngle = Mathf.MoveTowardsAngle(
            m_CurrentLaserAngle,
            targetAngle,
            m_LaserRotationSpeed * Time.deltaTime
        );

        // 💡 2D 방식 완벽 위치 및 회전 적용
        m_LaserObject.transform.rotation = Quaternion.Euler(0f, 0f, m_CurrentLaserAngle);
        float beamLength = m_LaserObject.transform.localScale.y; // 주의: 레이저 이미지가 가로로 길면 y 대신 x로 변경!
        m_LaserObject.transform.position = startPos + (Vector2)(m_LaserObject.transform.up * (beamLength * 0.5f));
    }


    // =========================================================
    // Debug (Gizmos)
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (m_MouthPoint == null)
            return;

        Gizmos.color = Color.red;
        Vector3 start = m_MouthPoint.position;
        Vector2 direction;

        if (Application.isPlaying && m_Player != null)
        {
            direction = ((Vector2)m_Player.position - (Vector2)start).normalized;
        }
        else
        {
            direction = Vector2.right;
        }

        Gizmos.DrawLine(start, start + (Vector3)direction * m_LaserRange);
    }
}