using UnityEngine;
using System.Collections;

public class RedGildedWurm : GildedWurmBase
{
    [Header("Laser Animation Settings")]
    [Tooltip("애니메이션이 적용된 레이저 오브젝트")]
    [SerializeField] private GameObject m_LaserObject;

    [Tooltip("레이저가 발사될 입 위치")]
    [SerializeField] private Transform m_MouthPoint;

    [Tooltip("레이저 스프라이트의 기본 방향 보정값")]
    [SerializeField] private float m_LaserSpriteRotationOffset = -90f;

    [Tooltip("레이저가 플레이어를 추적하며 회전하는 속도")]
    [SerializeField] private float m_LaserRotationSpeed = 180f;

    [Header("Laser Damage Settings")]
    [SerializeField] private float m_BeamDamage = 15f;
    [SerializeField] private float m_DamageInterval = 0.08f;
    [SerializeField] private LayerMask m_TargetLayer;
    [SerializeField] private float m_LaserRange = 20f;

    [Header("Attack Conditions")]
    [Range(0.5f, 1.0f)]
    [SerializeField] private float m_FacingThreshold = 0.94f;

    [Range(0f, 1f)]
    [SerializeField] private float m_AttackChance = 0.4f;

    [SerializeField] private float m_AttackDuration = 3.0f;
    [SerializeField] private float m_AttackCooldown = 3.0f;

    [Header("Attack Movement Settings")]
    [Range(0.1f, 1.0f)]
    [SerializeField] private float m_AttackSpeedMultiplier = 0.5f;

    [Range(0.1f, 1.0f)]
    [SerializeField] private float m_AttackTurnMultiplier = 0.6f;

    private bool m_IsAttacking = false;

    private float m_CurrentCooldown = 2.0f;
    private float m_DamageTimer = 0f;

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
        m_DamageTimer = 0f;

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

        // 레이저 최대 사거리 밖이면 공격하지 않음
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

        // 현재 이동 방향이 플레이어 방향과 어느 정도 일치하는지 확인
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
        m_DamageTimer = 0f;

        if (m_LaserObject != null && m_MouthPoint != null)
        {
            // 💡 1. 자식 Laser 오브젝트에 있는 Animator를 가져와 isAttacking을 true로 설정
            Animator laserAnimator = m_LaserObject.GetComponent<Animator>();
            if (laserAnimator != null)
            {
                laserAnimator.SetBool("isAttacking", true);
            }

            // 💡 2. 레이저 오브젝트 켜기
            m_LaserObject.SetActive(true);

            Vector2 startPos = m_MouthPoint.position;

            Vector2 initialDirection =
                ((Vector2)m_Player.position - startPos).normalized;

            float initialAngle =
                Mathf.Atan2(
                    initialDirection.y,
                    initialDirection.x
                ) * Mathf.Rad2Deg;

            m_CurrentLaserAngle =
                initialAngle + m_LaserSpriteRotationOffset;

            m_LaserObject.transform.position = startPos;

            m_LaserObject.transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    m_CurrentLaserAngle
                );
        }

        float elapsed = 0f;

        while (elapsed < m_AttackDuration)
        {
            if (m_Player == null)
                break;

            UpdateLaserLogic();

            elapsed += Time.deltaTime;

            yield return null;
        }

        // 💡 3. 공격 종료 시 Animator의 isAttacking을 false로 설정하고 레이저 끄기
        if (m_LaserObject != null)
        {
            Animator laserAnimator = m_LaserObject.GetComponent<Animator>();
            if (laserAnimator != null)
            {
                laserAnimator.SetBool("isAttacking", false);
            }

            m_LaserObject.SetActive(false);
        }

        m_IsAttacking = false;
        m_DamageTimer = 0f;
        m_CurrentCooldown = m_AttackCooldown;

        Debug.Log("🔴 빨간 웜: 레이저 공격 종료");
    }


    // =========================================================
    // Laser Logic
    // =========================================================

    private void UpdateLaserLogic()
    {
        if (m_Player == null ||
            m_MouthPoint == null ||
            m_LaserObject == null)
        {
            return;
        }

        // -----------------------------------------------------
        // 1. 레이저 시작점
        // -----------------------------------------------------

        Vector2 startPos = m_MouthPoint.position;

        m_LaserObject.transform.position = startPos;


        // -----------------------------------------------------
        // 2. 플레이어 방향 계산
        // -----------------------------------------------------

        Vector2 directionToPlayer =
            ((Vector2)m_Player.position - startPos).normalized;

        if (directionToPlayer.sqrMagnitude <= 0.001f)
            return;


        // -----------------------------------------------------
        // 3. 목표 각도 계산
        // -----------------------------------------------------

        float targetAngle =
            Mathf.Atan2(
                directionToPlayer.y,
                directionToPlayer.x
            ) * Mathf.Rad2Deg;

        targetAngle += m_LaserSpriteRotationOffset;


        // -----------------------------------------------------
        // 4. 현재 레이저 각도를 목표 각도로 천천히 회전
        // -----------------------------------------------------

        m_CurrentLaserAngle = Mathf.MoveTowardsAngle(
            m_CurrentLaserAngle,
            targetAngle,
            m_LaserRotationSpeed * Time.deltaTime
        );


        // -----------------------------------------------------
        // 5. 화면에 보이는 레이저 회전
        // -----------------------------------------------------

        m_LaserObject.transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                m_CurrentLaserAngle
            );


        // -----------------------------------------------------
        // 6. 실제 공격 방향 계산
        // -----------------------------------------------------
        //
        // Sprite Rotation Offset은 이미지 방향 보정용이므로
        // 실제 Raycast 방향에서는 다시 제거한다.
        //

        float actualAngle =
            m_CurrentLaserAngle -
            m_LaserSpriteRotationOffset;

        Vector2 laserDirection =
            new Vector2(
                Mathf.Cos(actualAngle * Mathf.Deg2Rad),
                Mathf.Sin(actualAngle * Mathf.Deg2Rad)
            ).normalized;


        // -----------------------------------------------------
        // 7. 레이저 Raycast
        // -----------------------------------------------------

        RaycastHit2D hit = Physics2D.Raycast(
            startPos,
            laserDirection,
            m_LaserRange,
            m_TargetLayer
        );


        // -----------------------------------------------------
        // 8. 플레이어 피격
        // -----------------------------------------------------

        if(hit.collider != null &&
            hit.collider.CompareTag("Player"))
        {
            m_DamageTimer += Time.deltaTime;

            if (m_DamageTimer >= m_DamageInterval)
            {
                m_DamageTimer = 0f;

                if (hit.collider.TryGetComponent(out PlayerController player))
                {
                    player.TakeDamage(m_BeamDamage);
                }
            }
        }
        else
        {
            m_DamageTimer = 0f;
        }
    }


    // =========================================================
    // Debug
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (m_MouthPoint == null)
            return;

        Gizmos.color = Color.red;

        Vector3 start = m_MouthPoint.position;

        Vector2 direction;

        if (Application.isPlaying)
        {
            float actualAngle =
                m_CurrentLaserAngle -
                m_LaserSpriteRotationOffset;

            direction = new Vector2(
                Mathf.Cos(actualAngle * Mathf.Deg2Rad),
                Mathf.Sin(actualAngle * Mathf.Deg2Rad)
            );
        }
        else if (m_Player != null)
        {
            direction =
                ((Vector2)m_Player.position -
                 (Vector2)start).normalized;
        }
        else
        {
            direction = Vector2.right;
        }

        Gizmos.DrawLine(
            start,
            start + (Vector3)direction * m_LaserRange
        );
    }
}
