using System.Collections;
using UnityEngine;

public class BlueGildedWurm : GildedWurmBase
{
    [Header("Blue Gilded Wurm Combat Settings")]
    [SerializeField] private GameObject m_ProjectilePrefab;
    [SerializeField] private Transform m_ProjectileSpawnPoint; // Mouth의 SpawnPoint
    [SerializeField] private float m_BaseDamage = 15f;
    private float m_Damage;

    [Header("Spread Pattern Settings")]
    [SerializeField] private float m_SpreadAngle = 55f;       // 전체 부채꼴 각도
    [SerializeField] private int m_BulletsPerBurst = 5;       // 1회 발사 당 탄 수
    [SerializeField] private int m_BurstCount = 3;            // 총 반복 횟수 (3회)
    [SerializeField] private float m_BurstInterval = 0.3f;    // 회차 간 딜레이
    [SerializeField] private float m_ProjectileSpeed = 12f;
    [SerializeField] private float m_ProjectileLifetime = 3f;

    [Header("Cooldown & Range")]
    [SerializeField] private float m_AttackCooldown = 3.0f;
    [SerializeField] private float m_AttackRange = 8.0f;

    private float m_CooldownTimer = 0f;
    private bool m_IsAttacking = false;

    public override void Setup(Transform player)
    {
        base.Setup(player);
        m_Damage = m_BaseDamage;
        m_CooldownTimer = 0f;
        m_IsAttacking = false;
    }

    protected override void Update()
    {
        base.Update();

        // 💡 RedGildedWurm과 동일하게 m_Player 사용
        if (m_Player == null) return;

        if (m_CooldownTimer > 0f)
        {
            m_CooldownTimer -= Time.deltaTime;
        }

        if (!m_IsAttacking && m_CooldownTimer <= 0f)
        {
            // 💡 m_Player 위치 기준 거리 계산
            float distToPlayer = Vector2.Distance(transform.position, m_Player.position);
            if (distToPlayer <= m_AttackRange)
            {
                StartCoroutine(CoBlueWurmAttackPattern());
            }
        }
    }

    private IEnumerator CoBlueWurmAttackPattern()
    {
        m_IsAttacking = true;

        for (int burst = 0; burst < m_BurstCount; burst++)
        {
            // 💡 발사 순간마다 m_Player 방향으로 재조준
            Vector2 targetDir = (m_Player != null) ? ((Vector2)(m_Player.position - (m_ProjectileSpawnPoint != null ? m_ProjectileSpawnPoint.position : transform.position))).normalized : Vector2.left;

            FireSpreadBullets(targetDir);

            if (burst < m_BurstCount - 1)
            {
                yield return new WaitForSeconds(m_BurstInterval);
            }
        }

        m_CooldownTimer = m_AttackCooldown;
        m_IsAttacking = false;
    }

    private void FireSpreadBullets(Vector2 baseDirection)
    {
        if (m_ProjectilePrefab == null) return;
        Vector3 spawnPos = m_ProjectileSpawnPoint != null ? m_ProjectileSpawnPoint.position : transform.position;

        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - (m_SpreadAngle / 2f);
        float angleStep = m_BulletsPerBurst > 1 ? m_SpreadAngle / (m_BulletsPerBurst - 1) : 0f;

        for (int i = 0; i < m_BulletsPerBurst; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Quaternion rot = Quaternion.Euler(0f, 0f, currentAngle);
            Vector2 dir = rot * Vector3.right;

            GameObject projObj = Instantiate(m_ProjectilePrefab, spawnPos, Quaternion.identity);
            if (projObj.TryGetComponent(out BlueWurmProjectile projectile))
            {
                projectile.Setup(m_Damage, dir, m_ProjectileSpeed, m_ProjectileLifetime);
            }
        }
    }
}