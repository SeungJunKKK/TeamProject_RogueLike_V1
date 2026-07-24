using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BasicEnemyAI : MonoBehaviour, IPoolable
{
    [Header("Target Setting")]
    public Transform Player;

    [Header("Movement Setting")]
    public float MoveSpeed = 3f;
    public float JumpForce = 6f;
    public float JumpCooldown = 0.5f; // 점프 쿨타임
    private float m_LastJumpTime;
    public float AttackRange = 1.5f;
    public float AttackCooldown = 2f; // 공격 후 대기 시간

    [Header("Raycast Sensors")]
    public Transform GroundSensor;
    public Transform WallSensor;
    public float RayLength = 1f;
    public LayerMask GroundLayer;

    [Header("Separation Setting (로컬 회피)")]
    public float SeparationRadius = 0.8f; // 적들을 밀어낼 반경
    public float SeparationForce = 2.5f;  // 밀어내는 힘의 세기
    public LayerMask EnemyLayer;

    // 멤버 변수
    private Rigidbody2D m_Rigidbody;
    private PooledObject m_Pooled;

    private bool m_IsFacingRight = true;
    private bool m_IsGrounded = true;
    private bool m_IsAttacking = false; // 현재 공격 중인지 상태 체크

    // OnSpawn(): 풀에서 꺼내질 때마다 자동 호출
    public void OnSpawn()
    {
        // 몬스터가 재사용될 때 이전 상태가 남아있지 않도록 전부 초기화
        m_IsFacingRight = true;
        m_IsAttacking = false;
        m_IsGrounded = true;
        m_LastJumpTime = 0f;

        // 방향을 기본 상태로 리셋
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    // OnDespawn(): 풀로 돌아갈 때 자동 호출
    public void OnDespawn()
    {
        // 진행 중이던 공격 코루틴 등이 백그라운드에서 계속 돌지 않도록 강제 종료
        StopAllCoroutines();
    }

    // 사망 처리 로직 추가
    public void Die()
    {
        if (m_Pooled != null)
        {
            m_Pooled.Return(); // 풀 매니저로 안전하게 반납
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start() 대신 Awake()에서 컴포넌트 사전 캐싱
    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Pooled = GetComponent<PooledObject>(); // 매번 GetComponent 하지 않도록 캐싱
    }

    private void Update()
    {
        // 플레이어가 없거나, 현재 공격/쿨타임 중이면 이동 로직 중지
        if (Player == null || m_IsAttacking)
        {
            return;
        }

        // 발끝 센서를 Update로 빼서 매 프레임 무조건 바닥을 확인하도록 변경
        m_IsGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, GroundLayer);

        float distanceToPlayer = Vector2.Distance(transform.position, Player.position);

        if (distanceToPlayer > AttackRange)
        {
            ChasePlayer();
        }
        else
        {
            // [핵심] 사거리 내에 들어왔더라도, '땅에 발이 닿아있을 때만' 멈춰서 공격!
            if (m_IsGrounded)
            {
                StartCoroutine(AttackRoutine());
            }
            else
            {
                // 공중이라면 허공에서 멈추지 말고 마저 이동 진행
                ChasePlayer();
            }
        }
    }

    private void ChasePlayer()
    {
        int direction = Player.position.x > transform.position.x ? 1 : -1;

        // 방향 전환
        if ((direction == 1 && !m_IsFacingRight) ||
            (direction == -1 && m_IsFacingRight))
        {
            Flip();
        }

        float separationX = CalculateSeparation();
        float finalVelocityX = direction * MoveSpeed + separationX;

        // 공중에서는 장애물 판단을 하지 않고 계속 이동만
        if (!m_IsGrounded)
        {
            m_Rigidbody.linearVelocity = new Vector2(finalVelocityX, m_Rigidbody.linearVelocity.y);
            return;
        }

        // 착지 상태에서만 센서 검사
        bool isGroundAhead = Physics2D.Raycast(
            GroundSensor.position,
            Vector2.down,
            RayLength,
            GroundLayer);

        Vector2 wallDir = m_IsFacingRight ? Vector2.right : Vector2.left;

        bool isWallAhead = Physics2D.Raycast(
            WallSensor.position,
            wallDir,
            RayLength,
            GroundLayer);

        // 장애물 또는 낭떠러지
        if (isWallAhead || !isGroundAhead)
        {
            if (Time.time - m_LastJumpTime >= JumpCooldown)
            {
                Jump();
            }
            else
            {
                m_Rigidbody.linearVelocity = new Vector2(0f, m_Rigidbody.linearVelocity.y);
            }

            return;
        }

        // 평지 이동
        m_Rigidbody.linearVelocity = new Vector2(finalVelocityX, m_Rigidbody.linearVelocity.y);
    }

    private float CalculateSeparation()
    {
        float separationForceX = 0f;
        // SeparationRadius 반경 내의 모든 EnemyLayer 오브젝트 검출
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, SeparationRadius, EnemyLayer);

        foreach (Collider2D enemy in nearbyEnemies)
        {
            // 자기 자신은 밀어내기 연산에서 제외
            if (enemy.gameObject == gameObject)
            {
                continue;
            }

            // X축 거리를 계산
            float diffX = transform.position.x - enemy.transform.position.x;

            // 좌표가 완벽하게 똑같아 0이 되는 것을 방지
            if (Mathf.Abs(diffX) < 0.01f)
            {
                diffX = Random.Range(-0.01f, 0.01f);
            }

            // 거리가 가까울수록 더 강한 반발력 적용
            float force = Mathf.Sign(diffX) * (SeparationRadius - Mathf.Abs(diffX)) * SeparationForce;
            separationForceX += force;
        }

        return separationForceX;
    }

    private void Jump()
    {
        m_LastJumpTime = Time.time; // 점프 뛰는 순간 시간 기록
        m_Rigidbody.linearVelocity = new Vector2(m_Rigidbody.linearVelocity.x, 0f);
        m_Rigidbody.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
    }

    private void StopMoving()
    {
        m_Rigidbody.linearVelocity = new Vector2(0f, m_Rigidbody.linearVelocity.y);
    }

    private IEnumerator AttackRoutine()
    {
        // 공격 시작
        m_IsAttacking = true;
        StopMoving();

        // 공격 애니메이션 재생
        Debug.Log("플레이어를 공격.");

        yield return new WaitForSeconds(0.5f);

        // 데미지 판정 로직 추가

        yield return new WaitForSeconds(AttackCooldown);

        // 쿨타임 종료시, 다시 추적 가능 상태로 복귀
        m_IsAttacking = false;
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

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, SeparationRadius);
    }
}