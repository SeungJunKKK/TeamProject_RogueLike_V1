using UnityEngine;

public class WurmSegmentMover : MonoBehaviour
{
    private GildedWurmBase m_Wurm;
    private int m_MyIndex;

    [Header("Follow Settings")]
    [SerializeField] private float m_FollowSpeed = 25f;
    [SerializeField] private float m_AdditionalSpacing = 0f;

    [Header("Sprite Rotation Offset")]
    [Tooltip("스프라이트가 누워있다면 이 값을 -90 또는 90으로 맞춰주세요")]
    [SerializeField] private float m_RotationOffset = -90f; // 기본적으로 위를 보고 그린 스프라이트 보정용

    public void Init(GildedWurmBase wurm, int index)
    {
        m_Wurm = wurm;
        m_MyIndex = index;

        // 💡 마법의 코드: 스폰 직후 머리의 자식 상태에서 스스로 탈출하여 독립!
        // 이렇게 하면 프리팹 구조는 깔끔하게 유지하면서, 인게임에서는 완벽히 분리됩니다.
        transform.SetParent(null);
    }

    private void Update()
    {
        // 💡 머리(보스 본체)가 죽어서 사라졌다면, 독립된 몸통들도 스스로 파괴
        if (m_Wurm == null)
        {
            Destroy(gameObject);
            return;
        }

        if (!m_Wurm.IsActive) return;

        float distanceBehindHead = (m_MyIndex * m_Wurm.SegmentSpacing) + m_AdditionalSpacing;
        Vector2 targetPosition = m_Wurm.GetPathPosition(distanceBehindHead);

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, m_FollowSpeed * Time.deltaTime);
        UpdateRotation(distanceBehindHead);
    }

    private void UpdateRotation(float distanceBehindHead)
    {
        Vector2 direction = m_Wurm.GetPathDirection(distanceBehindHead);
        if (direction.sqrMagnitude <= 0.001f) return;

        // 💡 몸통이 바라보는 각도 계산 + 스프라이트 원본 방향 보정(Offset) 적용
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, angle + m_RotationOffset), 15f * Time.deltaTime);
    }
}