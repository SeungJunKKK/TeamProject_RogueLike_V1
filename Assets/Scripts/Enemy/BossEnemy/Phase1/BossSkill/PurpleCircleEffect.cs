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

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_Collider = GetComponent<Collider2D>();
        m_Collider.enabled = false;
    }

    public void Setup(float damage, GameObject attacker, float direction)
    {
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

    // 💡 [중요] 애니메이션 3번째 프레임(폭발 모션)에 Animation Event 마커로 이 함수를 등록하세요!
    public void PerformExplosionHit()
    {
        m_Collider.enabled = true; // 폭발 타격 판정 활성화
        
        // 판정 프레임 유지 후 오브젝트 파괴
        Destroy(gameObject, 0.2f);
    }
}