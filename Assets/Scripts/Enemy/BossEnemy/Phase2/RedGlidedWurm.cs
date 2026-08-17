using UnityEngine;
using System.Collections;

public class RedGildedWurm : GildedWurmBase
{
    [Header("Laser Pattern Settings")]
    [SerializeField] private LineRenderer m_LaserRenderer;
    [SerializeField] private GameObject m_CrosshairPrefab;
    [SerializeField] private float m_BeamDamage = 15f;
    [SerializeField] private float m_BeamRotationSpeed = 90f;
    [SerializeField] private LayerMask m_TargetLayer;
    [SerializeField] private float m_LaserRange = 20f;

    private bool m_IsAttacking = false;
    private Vector2 m_CurrentLaserDir;

    // 공격 시 속도 감속을 위한 원본 수치 저장
    private float m_OriginalMaxSpeed;
    private float m_OriginalTurnSpeed;

    public override void Setup(Transform player)
    {
        base.Setup(player);
        m_OriginalMaxSpeed = m_MaxSpeed;
        m_OriginalTurnSpeed = m_TurnSpeed;

        if (m_LaserRenderer != null) m_LaserRenderer.enabled = false;
        StartCoroutine(AttackRoutine());
    }

    protected override void UpdateMovement()
    {
        // 💡 공격과 이동의 완전한 분리: 이동 수치(Parameter)만 간섭
        if (m_IsAttacking)
        {
            // 공격 중: 속도는 크게 줄이고 회전력은 극도로 둔화시켜 레이저 쏘는 폼을 잡음
            m_MaxSpeed = Mathf.Lerp(m_MaxSpeed, m_OriginalMaxSpeed * 0.15f, Time.deltaTime * 4f);
            m_TurnSpeed = Mathf.Lerp(m_TurnSpeed, m_OriginalTurnSpeed * 0.2f, Time.deltaTime * 4f);
        }
        else
        {
            // 회복 중: 원래 속도와 회전력을 부드럽게 되찾으며 관성 비행 재개
            m_MaxSpeed = Mathf.Lerp(m_MaxSpeed, m_OriginalMaxSpeed, Time.deltaTime * 2f);
            m_TurnSpeed = Mathf.Lerp(m_TurnSpeed, m_OriginalTurnSpeed, Time.deltaTime * 2f);
        }

        // 실제 이동 로직은 Base의 Steering AI가 100% 처리
        base.UpdateMovement();
    }

    private IEnumerator AttackRoutine()
    {
        // 스폰 후 초기 진입 비행을 위한 대기 시간
        yield return new WaitForSeconds(3.0f);

        while (true)
        {
            // ==========================================
            // 1. Telegraph (조준)
            // ==========================================
            m_IsAttacking = true; // 이동 속도 감소 시작
            GameObject crosshair = null;

            if (m_CrosshairPrefab != null && m_Player != null)
            {
                crosshair = Instantiate(m_CrosshairPrefab, m_Player.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(1.2f);
            if (crosshair != null) Destroy(crosshair);

            // ==========================================
            // 2. Fire (발사)
            // ==========================================
            if (m_LaserRenderer != null) m_LaserRenderer.enabled = true;
            if (m_Player != null) m_CurrentLaserDir = (m_Player.position - transform.position).normalized;

            float fireDuration = 2.0f;
            float elapsed = 0f;

            while (elapsed < fireDuration)
            {
                UpdateLaserBeam();
                elapsed += Time.deltaTime;
                yield return null;
            }

            // ==========================================
            // 3. Recovery (회복)
            // ==========================================
            if (m_LaserRenderer != null) m_LaserRenderer.enabled = false;
            m_IsAttacking = false; // 이동 속도 복구 시작

            // 다음 공격까지 쿨타임 (이 동안 자유롭게 아레나를 휘젓고 다님)
            yield return new WaitForSeconds(5.0f);
        }
    }
    
    private void UpdateLaserBeam()
    {
        if (m_Player == null || m_LaserRenderer == null) return;

        Vector2 targetDir = (m_Player.position - transform.position).normalized;
        float currentAngle = Mathf.Atan2(m_CurrentLaserDir.y, m_CurrentLaserDir.x) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

        float smoothedAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, m_BeamRotationSpeed * Time.deltaTime);
        m_CurrentLaserDir = new Vector2(Mathf.Cos(smoothedAngle * Mathf.Deg2Rad), Mathf.Sin(smoothedAngle * Mathf.Deg2Rad)).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, m_CurrentLaserDir, m_LaserRange, m_TargetLayer);

        m_LaserRenderer.SetPosition(0, transform.position);
        m_LaserRenderer.SetPosition(1, hit.collider != null ? (Vector3)hit.point : (transform.position + (Vector3)m_CurrentLaserDir * m_LaserRange));

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            if (hit.collider.TryGetComponent(out IDamageable player))
            {
                DamageInfo info = new DamageInfo
                {
                    Amount = m_BeamDamage * Time.deltaTime,
                    Attacker = gameObject,
                    HitPoint = hit.point
                };
                player.TakeDamage(info);
            }
        }
    }
}