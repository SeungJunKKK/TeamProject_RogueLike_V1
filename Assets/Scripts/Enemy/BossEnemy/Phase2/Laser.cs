using UnityEngine;

public class Laser : MonoBehaviour
{
    [Header("Laser Specs")]
    [SerializeField] private float m_Duration = 1.0f;
    [SerializeField] private float m_BeamDamage = 10f;
    [SerializeField] private float m_DamageInterval = 0.08f;
    [SerializeField] private LayerMask m_TargetLayer;

    [Header("Overlap Box Settings")]
    [SerializeField] private Vector2 m_BoxSize = new Vector2(0.5f, 4f); // 레이저의 두께와 길이 설정
    [SerializeField] private Vector2 m_BoxOffset = new Vector2(0f, -2f); // 중심점 기준 오프셋

    [Header("Visual Settings")]
    [SerializeField] private float m_SpriteRotationOffset = -90f;
    [SerializeField] private SpriteRenderer m_SpriteRenderer;

    [Header("Tracking Settings")]
    [SerializeField] private float m_MoveSpeed = 5f;

    private Vector2 m_CurrentFirePosition;
    private float m_DamageTimer = 0f;
    private float m_Elapsed = 0f;
    private Transform m_PlayerTransform;
    private readonly Collider2D[] m_HitBuffer = new Collider2D[5];

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

        Vector2 boxCenter = (Vector2)transform.position + m_BoxOffset;
        int hitCount = Physics2D.OverlapBoxNonAlloc(boxCenter, m_BoxSize, 0f, m_HitBuffer, m_TargetLayer);

        bool isPlayerHit = false;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D col = m_HitBuffer[i];
            if (col != null && col.CompareTag("Player"))
            {
                isPlayerHit = true;
                break;
            }
        }

        if (isPlayerHit)
        {
            m_DamageTimer += Time.deltaTime;

            if (m_DamageTimer >= m_DamageInterval)
            {
                m_DamageTimer = 0f;
                if (hitCount > 0)
                {
                    // 널 체크 및 플레이어 컴포넌트 가져오기
                    for (int i = 0; i < hitCount; i++)
                    {
                        PlayerController player = m_HitBuffer[i].GetComponent<PlayerController>() ?? m_HitBuffer[i].GetComponentInParent<PlayerController>();

                        if (player != null)
                        {
                            player.TakeDamage(m_BeamDamage);
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            m_DamageTimer = 0f;
        }
    }

    // 💡 에디터에서 판정 박스 범위를 눈으로 확인하기 위한 Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Vector2 boxCenter = (Vector2)transform.position + m_BoxOffset;
        Gizmos.DrawCube(boxCenter, m_BoxSize);
    }
}