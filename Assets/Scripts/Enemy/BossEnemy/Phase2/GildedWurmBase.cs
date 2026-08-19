using UnityEngine;
using System.Collections.Generic;

public abstract class GildedWurmBase : EnemyBase
{
    [Header("Target")]
    [SerializeField] protected Transform m_Player;

    [Header("Movement (Steering)")]
    [SerializeField] protected float m_MaxSpeed = 10f;
    [SerializeField] protected float m_TurnSpeed = 90f;
    [SerializeField] protected float m_OvershootDistance = 8f;

    [Header("Body Tracking")]
    [SerializeField] private float m_SegmentSpacing = 0.8f;
    [SerializeField] private int m_MaxPathPoints = 500;

    protected Vector2 m_Velocity;
    protected bool m_IsActive = false;
    protected bool m_IsAlreadyDead = false;

    protected enum EMoveState { Cruising, Overshooting }
    protected EMoveState m_CurrentMoveState = EMoveState.Cruising;

    private readonly List<Vector2> m_Path = new List<Vector2>();

    // 💡 [추가] 부모를 떠날 몸통들을 영구적으로 기억해둘 배열
    protected WurmSegmentMover[] m_CachedMovers;

    public float SegmentSpacing => m_SegmentSpacing;
    public bool IsActive => m_IsActive;

    public override void SetTarget(Transform target)
    {
        m_Player = target;
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        ResetPath();

        // 💡 [수정] 1. 자식들이 독립하기 '전'에 모든 컴포넌트를 미리 찾아 저장합니다.
        m_CachedMovers = GetComponentsInChildren<WurmSegmentMover>(true);
        WurmSegment[] segments = GetComponentsInChildren<WurmSegment>(true);

        // 💡 [수정] 2. 피격 판정(WurmSegment)을 먼저 초기화합니다.
        foreach (WurmSegment segment in segments)
        {
            segment.Init(this);
        }

        // 💡 [수정] 3. 가장 마지막에 Mover를 초기화하며 부모 관계를 끊습니다.
        for (int i = 0; i < m_CachedMovers.Length; i++)
        {
            m_CachedMovers[i].Init(this, i + 1);
        }
    }

    public virtual void Setup(Transform player)
    {
        m_Player = player;
        m_IsActive = true;

        // 1. 머리를 화면 밖 랜덤 위치로 순간이동
        transform.position = GetRandomOffScreenPosition();

        // 2. 방향 결정
        if (m_Player != null)
        {
            Vector2 toPlayer = (m_Player.position - transform.position).normalized;
            m_Velocity = toPlayer * m_MaxSpeed;
        }
        else
        {
            m_Velocity = Vector2.left * m_MaxSpeed;
        }

        // 3. 쫙 펴진 상태의 경로 초기화
        ResetPath();

        // 💡 [수정] 4. 머리가 이동한 즉시, 저장해둔 배열을 꺼내 몸통들도 다 같이 머리 위치로 강제 소환!
        if (m_CachedMovers != null)
        {
            for (int i = 0; i < m_CachedMovers.Length; i++)
            {
                m_CachedMovers[i].transform.position = transform.position;
            }
        }
    }

    protected Vector2 GetRandomOffScreenPosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return transform.position;

        float randomX = 0f;
        float randomY = 0f;
        float offset = 1.0f;

        int edge = Random.Range(0, 4);
        switch (edge)
        {
            case 0: randomX = Random.Range(-0.2f, 1.2f); randomY = 1f + offset; break;
            case 1: randomX = Random.Range(-0.2f, 1.2f); randomY = 0f - offset; break;
            case 2: randomX = 0f - offset; randomY = Random.Range(-0.2f, 1.2f); break;
            case 3: randomX = 1f + offset; randomY = Random.Range(-0.2f, 1.2f); break;
        }

        Vector3 worldPos = cam.ViewportToWorldPoint(new Vector3(randomX, randomY, Mathf.Abs(cam.transform.position.z)));
        return new Vector2(worldPos.x, worldPos.y);
    }

    protected virtual void Update()
    {
        if (!m_IsActive || m_Player == null) return;
        UpdateMovement();
    }

    protected virtual void LateUpdate()
    {
        if (!m_IsActive) return;
        RecordHeadPosition(transform.position);
    }

    protected virtual void UpdateMovement()
    {
        Vector2 toPlayer = (Vector2)m_Player.position - (Vector2)transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        if (m_CurrentMoveState == EMoveState.Cruising)
        {
            SteerTowards(toPlayer.normalized);

            if (distanceToPlayer < 2.5f)
            {
                m_CurrentMoveState = EMoveState.Overshooting;
            }
        }
        else if (m_CurrentMoveState == EMoveState.Overshooting)
        {
            if (distanceToPlayer > m_OvershootDistance)
            {
                m_CurrentMoveState = EMoveState.Cruising;
            }
        }

        transform.position += (Vector3)(m_Velocity * Time.deltaTime);
        UpdateVisualDirection();
    }

    private void SteerTowards(Vector2 targetDir)
    {
        float currentAngle = Mathf.Atan2(m_Velocity.y, m_Velocity.x) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, m_TurnSpeed * Time.deltaTime);
        Vector2 newDir = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad));

        m_Velocity = newDir * m_MaxSpeed;
    }

    private void UpdateVisualDirection()
    {
        if (m_Velocity.sqrMagnitude <= 0.001f)
            return;

        float angle = Mathf.Atan2(m_Velocity.y, m_Velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void ResetPath()
    {
        m_Path.Clear();
        Vector2 headPos = transform.position;
        Vector2 backwardDir = m_Velocity.sqrMagnitude > 0 ? -m_Velocity.normalized : Vector2.right;

        float pointSpacing = 0.05f;
        int preSpawnPointsCount = 60;

        for (int i = preSpawnPointsCount - 1; i >= 0; i--)
        {
            m_Path.Add(headPos + backwardDir * (i * pointSpacing));
        }
    }

    private void RecordHeadPosition(Vector2 newPosition)
    {
        if (m_Path.Count == 0)
        {
            m_Path.Add(newPosition);
            return;
        }

        Vector2 lastPosition = m_Path[m_Path.Count - 1];
        if (Vector2.Distance(lastPosition, newPosition) < 0.03f) return;

        m_Path.Add(newPosition);
        if (m_Path.Count > m_MaxPathPoints) m_Path.RemoveAt(0);
    }

    public Vector2 GetPathPosition(float distanceBehindHead)
    {
        if (m_Path.Count == 0) return transform.position;
        if (distanceBehindHead <= 0f) return m_Path[m_Path.Count - 1];

        float accumulatedDistance = 0f;
        for (int i = m_Path.Count - 1; i > 0; i--)
        {
            Vector2 newerPoint = m_Path[i];
            Vector2 olderPoint = m_Path[i - 1];

            float segmentDistance = Vector2.Distance(newerPoint, olderPoint);
            if (segmentDistance <= 0.001f) continue;

            if (accumulatedDistance + segmentDistance >= distanceBehindHead)
            {
                float t = (distanceBehindHead - accumulatedDistance) / segmentDistance;
                return Vector2.Lerp(newerPoint, olderPoint, t);
            }
            accumulatedDistance += segmentDistance;
        }
        return m_Path[0];
    }

    public Vector2 GetPathDirection(float distanceBehindHead)
    {
        Vector2 current = GetPathPosition(distanceBehindHead);
        Vector2 previous = GetPathPosition(distanceBehindHead + 0.15f);
        Vector2 direction = current - previous;
        return direction.sqrMagnitude <= 0.001f ? Vector2.right : direction.normalized;
    }

    protected override void Die()
    {
        if (m_IsAlreadyDead) return;
        m_IsAlreadyDead = true;

        // 1. 움직임 정지
        m_IsActive = false;

        // 2. 부모의 Die() 호출 -> 여기서 골드, 경험치를 주고 사망 사운드를 비동기로 부름!
        base.Die();

        if (BossBattleController.Instance != null)
        {
            BossBattleController.Instance.OnWurmDied();
        }


        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        if (m_CachedMovers != null)
        {
            foreach (var mover in m_CachedMovers)
            {
                if (mover != null) Destroy(mover.gameObject);
            }
        }

        Destroy(gameObject, 2.0f);
    }
}