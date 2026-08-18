using UnityEngine;

public class TargetCrosshair : MonoBehaviour
{
    [Header("Original Tracking Settings")]
    [Tooltip("플레이어를 따라다니는 시간")]
    [SerializeField] private float m_TrackingDuration = 1.5f;
    [Tooltip("추적을 멈추고 발사 대기하는 시간 (회피할 틈)")]
    [SerializeField] private float m_LockDuration = 0.5f;
    [SerializeField] private float m_RotationSpeed = 200f;

    private Transform m_Player;
    private GameObject m_LaserPrefab;
    private float m_Elapsed = 0f;
    private bool m_IsLocked = false;

    public void Init(Vector2 spawnPos, float duration, Transform player, GameObject laserPrefab)
    {
        m_Player = player;
        m_LaserPrefab = laserPrefab;
        m_Elapsed = 0f;
        m_IsLocked = false;

        // 스폰 위치 오프셋 무시하고 플레이어 정중앙으로 강제 이동
        if (m_Player != null) transform.position = m_Player.position;
    }

    private void Update()
    {
        m_Elapsed += Time.deltaTime;
        transform.Rotate(0f, 0f, m_RotationSpeed * Time.deltaTime);

        // 1. 추적 단계: 플레이어 몸을 정확히 따라다님
        if (m_Elapsed < m_TrackingDuration)
        {
            if (m_Player != null)
            {
                transform.position = m_Player.position;
            }
        }
        // 2. 락온 단계: 추적을 멈추고 정지함 (플레이어가 피할 수 있는 타이밍)
        else if (m_Elapsed < m_TrackingDuration + m_LockDuration)
        {
            if (!m_IsLocked)
            {
                m_IsLocked = true;
                m_RotationSpeed *= 3f; // 멈추면서 회전 속도를 확 높여 경고 연출
            }
        }
        // 3. 발사 단계: 수직 레이저 발사 후 소멸
        else
        {
            SpawnLaserAndDestroy();
        }
    }

    private void SpawnLaserAndDestroy()
    {
        if (m_LaserPrefab != null)
        {
            Vector2 spawnPos = transform.position;
            GameObject laserObj = Instantiate(m_LaserPrefab, spawnPos, Quaternion.identity);

            if (laserObj.TryGetComponent(out Laser laserScript))
            {
                // 크로스헤어 위치와 플레이어 트랜스폼 전달
                laserScript.Init(spawnPos, m_Player);
            }
        }
        Destroy(gameObject);
    }
}