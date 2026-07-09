using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BasicEnemyAI : MonoBehaviour
{
    [Header("Target Setting")]
    public Transform Player;

    [Header("Movement Setting")]
    public float MoveSpeed = 3f;
    public float AttackRange = 1.5f; // 공격 사거리

    [Header("Raycast Sensors (자식 오브젝트 연결)")]
    public Transform GroundSensor; // 발 앞쪽 낭떠러지 감지
    public Transform WallSensor;   // 정면 벽 감지
    public float RayLength = 1f;
    public LayerMask GroundLayer;  // 바닥 레이어

    private Rigidbody2D m_Rigidbody;
    private bool m_IsFacingRight = true;

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, Player.position);

        if (distanceToPlayer > AttackRange)
        {
            ChasePlayer();
        }
        else
        {
            StopMoving();
        }
    }

    private void ChasePlayer()
    {
        int direction = Player.position.x > transform.position.x ? 1 : -1;

        if ((direction == 1 && !m_IsFacingRight) || (direction == -1 && m_IsFacingRight))
        {
            Flip();
        }

        bool isGroundAhead = Physics2D.Raycast(GroundSensor.position, Vector2.down, RayLength, GroundLayer);
        bool isWallAhead = Physics2D.Raycast(WallSensor.position, m_IsFacingRight ? Vector2.right : Vector2.left, RayLength, GroundLayer);

        if (isGroundAhead && !isWallAhead)
        {
            m_Rigidbody.linearVelocity = new Vector2(direction * MoveSpeed, m_Rigidbody.linearVelocity.y);
        }
        else
        {
            StopMoving();
        }
    }

    private void StopMoving()
    {
        m_Rigidbody.linearVelocity = new Vector2(0, m_Rigidbody.linearVelocity.y);
    }

    private void Flip()
    {
        m_IsFacingRight = !m_IsFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (GroundSensor != null)
        {
            Gizmos.DrawLine(GroundSensor.position, GroundSensor.position + Vector3.down * RayLength);
        }

        Gizmos.color = Color.blue;
        if (WallSensor != null)
        {
            Vector3 wallDir = m_IsFacingRight ? Vector3.right : Vector3.left;
            Gizmos.DrawLine(WallSensor.position, WallSensor.position + wallDir * RayLength);
        }
    }
}