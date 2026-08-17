using UnityEngine;

public class WurmHead : GildedWurmBase
{
    [Header("Movement Settings")]
    [SerializeField] private float m_FloatAmplitude = 0.5f;
    [SerializeField] private float m_FloatFrequency = 2f;

    private float m_RandomOffset;

    protected void Awake()
    {
        m_RandomOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    // 부모 클래스의 위치 기록을 실행하기 위해 override 및 base 호출 필수 적용
    protected override void Update()
    {
        base.Update();

        if (m_Player == null) return;

        Vector2 direction = (m_Player.position - transform.position).normalized;

        float floatY = Mathf.Sin(Time.time * m_FloatFrequency + m_RandomOffset) * m_FloatAmplitude;
        Vector2 targetPos = new Vector2(m_Player.position.x, m_Player.position.y + floatY);

        transform.position = Vector2.MoveTowards(transform.position, targetPos, m_MoveSpeed * Time.deltaTime);

        if (direction.x != 0)
        {
            transform.localScale = new Vector3(direction.x > 0 ? 1 : -1, 1, 1);
        }
    }
}