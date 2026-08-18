using UnityEngine;

public class Laser : MonoBehaviour
{
    [Header("Laser Specs")]
    [SerializeField] private float m_Duration = 1.0f;
    [SerializeField] private float m_BeamDamage = 10f;
    [SerializeField] private float m_DamageInterval = 0.08f;
    [SerializeField] private float m_LaserRange = 40f;
    [SerializeField] private LayerMask m_TargetLayer;

    [Header("Visual Settings")]
    [SerializeField] private float m_SpriteRotationOffset = -90f;
    [SerializeField] private SpriteRenderer m_SpriteRenderer;

    [Header("Tracking Settings")]
    [SerializeField] private float m_MoveSpeed = 5f;

    private Vector2 m_CurrentFirePosition;
    private float m_DamageTimer = 0f;
    private float m_Elapsed = 0f;
    private Transform m_PlayerTransform;

    public void Init(Vector2 firePos, Transform playerTransform)
    {
        m_CurrentFirePosition = firePos;
        transform.position = m_CurrentFirePosition;
        m_PlayerTransform = playerTransform;

        // 위쪽으로 고정
        transform.rotation = Quaternion.Euler(0, 0, 90f + m_SpriteRotationOffset);

        m_DamageTimer = 0f;
        m_Elapsed = 0f;
    }

    private void Update()
    {
        m_Elapsed += Time.deltaTime;

        // 마지막 크로스헤어 지점에서 시작해 플레이어의 X 위치를 향해 천천히 추적
        if (m_PlayerTransform != null)
        {
            float targetX = m_PlayerTransform.position.x;
            float newX = Mathf.MoveTowards(m_CurrentFirePosition.x, targetX, m_MoveSpeed * Time.deltaTime);
            m_CurrentFirePosition = new Vector2(newX, m_CurrentFirePosition.y);
        }

        ExecuteLaserLogic();

        if (m_Elapsed >= m_Duration) Destroy(gameObject);
    }

    private void ExecuteLaserLogic()
    {
        transform.position = m_CurrentFirePosition;

        Vector2 laserDir = Vector2.up;

        RaycastHit2D hit = Physics2D.Raycast(m_CurrentFirePosition, laserDir, m_LaserRange, m_TargetLayer);

        if (hit.collider != null)
        {
            m_DamageTimer += Time.deltaTime;

            if (m_DamageTimer >= m_DamageInterval)
            {
                m_DamageTimer = 0f;

                // IDamageable 대신 PlayerController를 직접 타겟팅
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
}