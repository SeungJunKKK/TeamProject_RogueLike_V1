using UnityEngine;
using System.Collections.Generic;

public abstract class GildedWurmBase : EnemyBase
{
    [Header("Target")]
    [SerializeField] protected Transform m_Player;

    [Header("Movement (Steering)")]
    [SerializeField] protected float m_MaxSpeed = 10f;
    [SerializeField] protected float m_TurnSpeed = 90f; // 초당 회전 각도 (낮을수록 선회 반경이 커짐)
    [SerializeField] protected float m_OvershootDistance = 8f; // 플레이어를 스쳐 지나갈 때, 돌진을 유지할 거리

    [Header("Body Tracking")]
    [SerializeField] private float m_SegmentSpacing = 0.8f;
    [SerializeField] private int m_MaxPathPoints = 500;

    protected Vector2 m_Velocity;
    protected bool m_IsActive = false;

    // AI 이동 상태
    protected enum EMoveState { Cruising, Overshooting }
    protected EMoveState m_CurrentMoveState = EMoveState.Cruising;

    private readonly List<Vector2> m_Path = new List<Vector2>();

    public float SegmentSpacing => m_SegmentSpacing;
    public bool IsActive => m_IsActive; // 외부(몸통)에서 접근할 상태 프로퍼티

    public override void SetTarget(Transform target)
    {
        m_Player = target;
    }

    public virtual void Setup(Transform player)
    {
        m_Player = player;
        m_IsActive = true;

        // 스폰 즉시 플레이어 방향으로 초기 속도 부여
        if (m_Player != null)
        {
            Vector2 toPlayer = (m_Player.position - transform.position).normalized;
            m_Velocity = toPlayer * m_MaxSpeed;
        }
        else
        {
            m_Velocity = Vector2.left * m_MaxSpeed;
        }
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        ResetPath();

        WurmSegmentMover[] movers = GetComponentsInChildren<WurmSegmentMover>(true);
        for (int i = 0; i < movers.Length; i++)
        {
            movers[i].Init(this, i + 1);
        }

        WurmSegment[] segments = GetComponentsInChildren<WurmSegment>(true);
        foreach (WurmSegment segment in segments)
        {
            segment.Init(this);
        }
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

    // =========================================================
    // Steering AI Logic
    // =========================================================
    protected virtual void UpdateMovement()
    {
        Vector2 toPlayer = (Vector2)m_Player.position - (Vector2)transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        if (m_CurrentMoveState == EMoveState.Cruising)
        {
            // 플레이어를 향해 서서히 기수를 돌림
            SteerTowards(toPlayer.normalized);

            // 플레이어와 충분히 가까워지면 조향을 멈추고 관성으로 지나침 (Overshoot)
            if (distanceToPlayer < 2.5f)
            {
                m_CurrentMoveState = EMoveState.Overshooting;
            }
        }
        else if (m_CurrentMoveState == EMoveState.Overshooting)
        {
            // 방향을 틀지 않고 현재 속도(m_Velocity) 방향 그대로 직진
            // 플레이어로부터 일정 거리 이상 멀어지면 다시 선회 시작
            if (distanceToPlayer > m_OvershootDistance)
            {
                m_CurrentMoveState = EMoveState.Cruising;
            }
        }

        // 최종 이동 적용
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

    }
    // =========================================================
    // Path Recording (이전 기획의 완벽한 거리 추적 로직 승계)
    // =========================================================
    private void ResetPath()
    {
        m_Path.Clear();
        Vector2 startPosition = transform.position;
        for (int i = 0; i < 30; i++) m_Path.Add(startPosition);
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
        base.Die();
        if (BossBattleController.Instance != null) BossBattleController.Instance.OnWurmDied();
    }
}