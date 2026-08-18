using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(Collider2D))]
public class PurpleCircleEffect : MonoBehaviour
{
    private Animator m_Animator;
    private Collider2D m_Collider;
    private float m_RotationDirection = 1f;
    private bool m_IsSpinning = false;
    [SerializeField] private float m_SpinDuration = 2.0f; // 2프레임째에 고정되어 회전하는 지속 시간

    private float m_Damage;
    private GameObject m_Attacker;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_Collider = GetComponent<Collider2D>();
        m_Collider.enabled = false;
    }

    public void Setup(float damage, GameObject attacker, float direction)
    {
        m_Damage = damage;
        m_Attacker = attacker;
        m_RotationDirection = direction;
        StartCoroutine(SequenceRoutine());
    }

    private IEnumerator SequenceRoutine()
    {
        // 1. 애니메이션이 재생되다가 2번째 프레임(루프 구간)에 도달할 때까지 잠시 대기
        yield return new WaitForSeconds(0.15f);

        // 2. 애니메이션 속도를 0으로 만들어 2번째 프레임에 딱 멈춰 세우기
        if (m_Animator != null)
        {
            m_Animator.speed = 0f;
        }
        m_IsSpinning = true;

        // 3. 정해진 시간 동안 그 자리에서 반대 방향으로 뱅글뱅글 회전 대기
        yield return new WaitForSeconds(m_SpinDuration);

        // 4. 회전 종료 및 애니메이션 속도 복구 (3번째 프레임인 폭발 모션으로 진행)
        m_IsSpinning = false;
        if (m_Animator != null)
        {
            m_Animator.speed = 1f;
        }
    }

    private void Update()
    {
        if (m_IsSpinning)
        {
            // 2번째 프레임 고정 상태에서 지정된 방향(1 or -1)으로 회전
            transform.Rotate(0, 0, 120f * m_RotationDirection * Time.deltaTime);
        }
    }

    // 💡 애니메이션 이벤트가 실행되는 순간 호출
    public void PerformExplosionHit()
    {
        m_Collider.enabled = true;

        // 💡 [핵심] OnTriggerEnter2D에 의존하지 않고, 콜리더가 켜진 순간 내부를 강제 스캔
        Collider2D[] hits = new Collider2D[5];
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter(); // 모든 레이어 대상 (필요시 Player 레이어만 잡도록 최적화 가능)

        int hitCount = Physics2D.OverlapCollider(m_Collider, filter, hits);

        for (int i = 0; i < hitCount; i++)
        {
            if (hits[i] == null) continue;

            PlayerController player = hits[i].GetComponent<PlayerController>() ?? hits[i].GetComponentInParent<PlayerController>();

            if (player != null)
            {
                Debug.Log("강제 스캔으로 타격 성공!");
                player.TakeDamage(m_Damage);
                break; // 한 번만 데미지를 주도록 반복문 탈출
            }
        }

        // 판정 프레임 유지 후 오브젝트 파괴
        Destroy(gameObject, 0.2f);
    }
}