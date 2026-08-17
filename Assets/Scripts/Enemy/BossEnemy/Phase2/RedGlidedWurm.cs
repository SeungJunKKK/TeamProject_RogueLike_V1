using UnityEngine;
using System.Collections;

public class RedGildedWurm : GildedWurmBase
{
    [Header("Movement Settings")]
    [SerializeField] private float m_FloatAmplitude = 0.5f;
    [SerializeField] private float m_FloatFrequency = 2f;
    private float m_RandomOffset;

    [Header("Laser Pattern Settings")]
    [SerializeField] private LineRenderer m_LaserRenderer;
    [SerializeField] private GameObject m_CrosshairPrefab;
    [SerializeField] private float m_BeamDamage = 15f;
    [SerializeField] private float m_BeamRotationSpeed = 90f;
    [SerializeField] private LayerMask m_TargetLayer;
    [SerializeField] private float m_LaserRange = 20f;

    private bool m_IsAttacking = false;
    private Vector2 m_CurrentLaserDir;

    protected void Awake()
    {
        m_RandomOffset = Random.Range(0f, Mathf.PI * 2f);
        if (m_LaserRenderer != null) m_LaserRenderer.enabled = false;
    }

    public override void Setup(Transform player)
    {
        base.Setup(player);
        StartCoroutine(AttackLoop());
    }

    // 부모 클래스의 위치 기록을 실행하기 위해 override 및 base 호출 필수 적용
    protected override void Update()
    {
        base.Update();

        if (m_IsEmerging || m_Player == null) return;

        float currentSpeed = m_IsAttacking ? m_MoveSpeed * 0.4f : m_MoveSpeed;
        float floatY = Mathf.Sin(Time.time * m_FloatFrequency + m_RandomOffset) * m_FloatAmplitude;
        Vector2 targetPos = new Vector2(m_Player.position.x, m_Player.position.y + floatY);

        transform.position = Vector2.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

        Vector2 direction = (m_Player.position - transform.position).normalized;
        if (direction.x != 0)
        {
            transform.localScale = new Vector3(direction.x > 0 ? 1 : -1, 1, 1);
        }
    }

    private IEnumerator AttackLoop()
    {
        while (m_IsEmerging) yield return null;

        yield return new WaitForSeconds(1.0f);

        while (true)
        {
            yield return new WaitForSeconds(2.0f);

            m_IsAttacking = true;
            GameObject crosshair = null;
            if (m_CrosshairPrefab != null)
            {
                crosshair = Instantiate(m_CrosshairPrefab, m_Player.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(0.6f);
            if (crosshair != null) Destroy(crosshair);

            if (m_LaserRenderer != null) m_LaserRenderer.enabled = true;
            m_CurrentLaserDir = (m_Player.position - transform.position).normalized;

            float elapsed = 0f;
            while (elapsed < 2.0f)
            {
                UpdateLaserBeam();
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (m_LaserRenderer != null) m_LaserRenderer.enabled = false;
            m_IsAttacking = false;

            yield return new WaitForSeconds(1.5f);
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