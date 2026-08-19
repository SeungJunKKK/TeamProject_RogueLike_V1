using System.Collections;
using UnityEngine;

public class ProvidenceUmbraAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer m_SpriteRenderer;
    [SerializeField] private Animator m_Animator;
    [SerializeField] private BoxCollider2D m_Collider;
    [SerializeField] private TrailRenderer m_TrailRenderer;

    [Header("Umbra Combat Settings")]
    [Tooltip("분신이 공격을 시도하는 최소/최대 주기 (본체와 엇박자를 만들기 위함)")]
    [SerializeField] private float m_MinAttackInterval = 2.5f;
    [SerializeField] private float m_MaxAttackInterval = 4.5f;

    [Header("Hitbox Settings")]
    [SerializeField] private Vector2 m_SlashHitboxOffset = new Vector2(1.5f, 0f);
    [SerializeField] private Vector2 m_SlashHitboxSize = new Vector2(2f, 2f);
    [SerializeField] private LayerMask m_TargetLayer;
    [SerializeField] private float m_Damage = 15f;

    [Header("Environment")]
    [Tooltip("본체와 동일한 Ground 레이어를 지정해주세요.")]
    [SerializeField] private LayerMask m_GroundLayer;

    private Transform m_Player;
    private bool m_IsActive = false;
    private Coroutine m_UmbraRoutine;
    private readonly Collider2D[] m_HitBuffer = new Collider2D[5];
    private int m_Direction = 1;

    private void Awake()
    {
        if (m_SpriteRenderer == null) m_SpriteRenderer = GetComponent<SpriteRenderer>();
        if (m_Animator == null) m_Animator = GetComponent<Animator>();
        if (m_Collider == null) m_Collider = GetComponent<BoxCollider2D>();
        if (m_TrailRenderer != null) m_TrailRenderer.emitting = false;

        // 스프라이트 색상을 완벽한 검은색(실루엣)으로 고정
        if (m_SpriteRenderer != null)
        {
            m_SpriteRenderer.color = Color.black;
        }

        // 평소에는 꺼두기
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 3페이즈 진입 시 분신 활성화 및 독립 타이머 시작
    /// </summary>
    public void ActivateUmbra(Transform player, float damageMultiplier)
    {
        if (m_IsActive)
        {
            DeactivateUmbra();
        }

        m_Player = player;
        m_Damage *= damageMultiplier;
        m_IsActive = true;
        gameObject.SetActive(true);

        // 등장 직후엔 숨어있다가 타이머 시작
        if (m_SpriteRenderer != null) m_SpriteRenderer.enabled = false;
        if (m_TrailRenderer != null) m_TrailRenderer.emitting = false;

        if (m_UmbraRoutine != null) StopCoroutine(m_UmbraRoutine);
        m_UmbraRoutine = StartCoroutine(CoUmbraIndependentLoop());
    }

    /// <summary>
    /// 분신 비활성화 및 정리
    /// </summary>
    public void DeactivateUmbra()
    {
        m_IsActive = false;
        if (m_UmbraRoutine != null) StopCoroutine(m_UmbraRoutine);
        if (m_TrailRenderer != null) m_TrailRenderer.emitting = false;
        gameObject.SetActive(false);
    }

    private IEnumerator CoUmbraIndependentLoop()
    {
        // 페이즈 3 진입 직후 너무 칼같이 때리지 않도록 첫 딜레이 부여
        yield return new WaitForSeconds(1.5f);

        while (m_IsActive)
        {
            // 본체와는 별개로 도는 독립적인 쿨타ım (엇박자 유도)
            float waitTime = Random.Range(m_MinAttackInterval, m_MaxAttackInterval);
            yield return new WaitForSeconds(waitTime);

            if (m_Player == null) continue;

            // 불쑥 나타나서 공격 수행
            yield return StartCoroutine(CoExecuteUmbraAttack());
        }
    }

    private IEnumerator CoExecuteUmbraAttack()
    {
        bool isTeleport = Random.value > 0.5f;
        string clipName = isTeleport ? "Umbra_ShootZ2" : "Umbra_ShootZ1";

        if (m_SpriteRenderer != null)
            m_SpriteRenderer.enabled = true;

        if (isTeleport)
        {
            // 공격 시작 순간의 플레이어 위치를 고정
            Vector3 attackStartPlayerPosition = m_Player.position;

            // 고정된 위치를 기준으로 텔레포트
            if (m_TrailRenderer != null)
                m_TrailRenderer.emitting = true;

            transform.position = new Vector2(
                attackStartPlayerPosition.x,
                attackStartPlayerPosition.y + 2.5f
            );

            if (m_Animator != null)
            {
                m_Animator.Play(clipName, -1, 0f);
            }

            // 고정된 플레이어 X 위치에서 바닥 탐색
            float targetY = attackStartPlayerPosition.y;

            RaycastHit2D hit = Physics2D.Raycast(
                new Vector2(
                    attackStartPlayerPosition.x,
                    transform.position.y
                ),
                Vector2.down,
                20f,
                m_GroundLayer
            );

            if (hit.collider != null)
            {
                float extentsY = m_Collider != null
                    ? m_Collider.bounds.extents.y
                    : 1f;

                targetY = hit.point.y + extentsY;
            }

            // 공격 시작 시점의 플레이어 X를 고정
            Vector3 targetPos = new Vector3(
                attackStartPlayerPosition.x,
                targetY,
                transform.position.z
            );

            float elapsed = 0f;

            while (elapsed < 0.35f)
            {
                elapsed += Time.deltaTime;

                Vector3 nextPos = Vector3.Lerp(
                    transform.position,
                    targetPos,
                    elapsed / 0.35f
                );

                if (nextPos.y < targetY)
                    nextPos.y = targetY;

                transform.position = nextPos;

                yield return null;
            }

            transform.position = targetPos;

            if (m_TrailRenderer != null)
                m_TrailRenderer.emitting = false;
        }
        else
        {
            // Z1 역시 공격 시작 위치를 고정해서 사용
            Vector3 attackStartPlayerPosition = m_Player.position;

            int side = Random.value > 0.5f ? 1 : -1;

            Vector3 spawnPos =
                attackStartPlayerPosition +
                new Vector3(side * 1.8f, 0f, 0f);

            m_Direction = -side;

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * m_Direction;
            transform.localScale = scale;

            transform.position = spawnPos;

            if (m_Animator != null)
            {
                m_Animator.Play(clipName, -1, 0f);
            }
        }
    }


    // 애니메이션 클립 안의 'PerformUmbraSlash' 이벤트 마커가 호출할 경우를 대비한 오버로드 함수
    public void PerformUmbraSlash()
    {
        Vector2 hitCenter = (Vector2)transform.position + new Vector2(m_Direction * m_SlashHitboxOffset.x, m_SlashHitboxOffset.y);
        int hitCount = Physics2D.OverlapBoxNonAlloc(hitCenter, m_SlashHitboxSize, 0f, m_HitBuffer, m_TargetLayer);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D col = m_HitBuffer[i];
            PlayerController player = col.GetComponent<PlayerController>() ?? col.GetComponentInParent<PlayerController>();

            if (player != null)
            {
                player.TakeDamage(m_Damage);
                break;
            }
        }
    }

    // 애니메이션 클립의 마지막 프레임 'FinishAction' 이벤트 마커가 호출할 함수
    public void FinishAction()
    {
        if (m_SpriteRenderer != null)
        {
            m_SpriteRenderer.enabled = false;
        }
        if (m_TrailRenderer != null)
        {
            m_TrailRenderer.emitting = false;
        }
    }
}