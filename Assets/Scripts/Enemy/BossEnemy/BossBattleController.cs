using System.Linq.Expressions;
using UnityEngine;
using System.Collections;

public enum EBossPhase
{
    None,
    Intro,
    Phase1,
    Phase2_Wurms,
    Phase3,
    Dead
}

public class BossBattleController : MonoBehaviour
{
    public static BossBattleController Instance { get; private set; }

    [Header("Boss References")]
    [SerializeField] private ProvidenceAI m_ProvidenceBoss;
    [SerializeField] private Transform m_Player;

    [Header("Phase 2: Gilded Wurms References")]
    [SerializeField] private GameObject m_RedWurmPrefab;
    [SerializeField] private GameObject m_BlueWurmPrefab;

    [Header("Phase 2 Crosshair Attack Settings")]
    [SerializeField] private GameObject m_CrosshairPrefab; // TargetCrosshair 프리팹
    [SerializeField] private GameObject m_LaserPrefab;       // Laser 프리팹
    [SerializeField] private float m_CrosshairMinInterval = 3.0f;
    [SerializeField] private float m_CrosshairMaxInterval = 6.0f;
    [SerializeField] private float m_CrosshairDuration = 1.2f;

    [Header("Crosshair Spawn Offset Settings")]
    [SerializeField] private float m_SpawnHeightOffset = 5.0f;     // 플레이어 기준 위쪽으로 떨어질 높이
    [SerializeField] private float m_SpawnHorizontalRange = 4.0f;  // 플레이어 기준 좌우 랜덤 퍼짐 범위
    private Coroutine m_CrosshairSchedulerRoutine;

    private int m_AliveWurmCount = 0;

    [Header("Battle State & Debug")]
    [SerializeField] private EBossPhase m_CurrentPhase = EBossPhase.None;
    [SerializeField] private bool m_EnableDebugKeys = true;

    private bool m_IsBattleStarted = false;

    public ProvidenceAI ProvidenceBoss => m_ProvidenceBoss;
    public Transform Player => m_Player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (m_ProvidenceBoss != null && m_Player != null)
        {
            m_ProvidenceBoss.OnSpawn();
            StartBattle();
        }
    }

    private void Update()
    {
        if (m_EnableDebugKeys && Input.GetKeyDown(KeyCode.T))
        {
            if (m_CurrentPhase == EBossPhase.Phase1)
            {
                Debug.Log("<color=cyan>[BossBattleController] 디버그: T키 입력으로 페이즈 2 강제 진입</color>");
                TransitionToPhase2();
            }
        }
    }

    public void StartBattle()
    {
        if (m_IsBattleStarted) return;
        m_IsBattleStarted = true;

        m_CurrentPhase = EBossPhase.Intro;
        m_ProvidenceBoss.SetTarget(m_Player);
        m_ProvidenceBoss.StartIntro();
    }

    public void OnIntroFinished()
    {
        m_CurrentPhase = EBossPhase.Phase1;
        m_ProvidenceBoss.EnterPhase1();
    }

    public void CheckPhaseTransition(float hpPercentage)
    {
        if (m_CurrentPhase == EBossPhase.Phase1 && hpPercentage <= 0.66f)
        {
            TransitionToPhase2();
        }
    }

    private void TransitionToPhase2()
    {
        m_CurrentPhase = EBossPhase.Phase2_Wurms;
        Debug.Log("<color=orange>[BossBattleController] 페이즈 2 진입 - 프로비던스 퇴장 및 웜 자동 소환</color>");
        m_ProvidenceBoss.ExitArenaForPhase2();

        m_AliveWurmCount = 2;

        // 중심 위치 기준으로 좌우 자동 계산
        Vector3 centerPos = m_ProvidenceBoss != null ? m_ProvidenceBoss.transform.position : transform.position;
        Vector3 leftPos = centerPos + new Vector3(-7f, 0f, 0f);
        Vector3 rightPos = centerPos + new Vector3(7f, 0f, 0f);

        SpawnWurmAt(m_RedWurmPrefab, leftPos);
        SpawnWurmAt(m_BlueWurmPrefab, rightPos);

        // 페이즈 2 크로스헤어 독립 스케줄러 시작
        if (m_CrosshairSchedulerRoutine != null) StopCoroutine(m_CrosshairSchedulerRoutine);
        m_CrosshairSchedulerRoutine = StartCoroutine(CrosshairAttackScheduler());
    }

    private IEnumerator CrosshairAttackScheduler()
    {
        yield return new WaitForSeconds(2.0f);

        while (m_CurrentPhase == EBossPhase.Phase2_Wurms)
        {
            float waitTime = Random.Range(m_CrosshairMinInterval, m_CrosshairMaxInterval);
            yield return new WaitForSeconds(waitTime);

            if (m_Player != null && m_CrosshairPrefab != null)
            {
                // 플레이어 기준 위쪽 지정한 오프셋 위치에 고정 생성
                float randomX = Random.Range(-m_SpawnHorizontalRange, m_SpawnHorizontalRange);
                Vector2 spawnPos = (Vector2)m_Player.position + new Vector2(randomX, m_SpawnHeightOffset);

                GameObject crosshairObj = Instantiate(m_CrosshairPrefab, spawnPos, Quaternion.identity);
                if (crosshairObj.TryGetComponent(out TargetCrosshair crosshairScript))
                {
                    crosshairScript.Init(
                        spawnPos,
                        m_CrosshairDuration,
                        m_Player,
                        m_LaserPrefab
                    );
                }
            }
        }
    }

    private void SpawnWurmAt(GameObject wurmPrefab, Vector3 spawnPos)
    {
        if (wurmPrefab == null) return;

        GameObject wurmObj = Instantiate(wurmPrefab, spawnPos, Quaternion.identity);
        if (wurmObj.TryGetComponent(out GildedWurmBase wurmBase))
        {
            wurmBase.OnSpawn();
            wurmBase.Setup(m_Player);
        }
    }

    public void OnWurmDied()
    {
        m_AliveWurmCount--;
        Debug.Log($"<color=orange>[BossBattleController] 웜 처치됨. 남은 웜: {m_AliveWurmCount}</color>");

        if (m_AliveWurmCount <= 0)
        {
            TransitionToPhase3();
        }
    }

    private void TransitionToPhase3()
    {
        m_CurrentPhase = EBossPhase.Phase3;

        // 페이즈 종료 시 크로스헤어 스케줄러 중지
        if (m_CrosshairSchedulerRoutine != null)
        {
            StopCoroutine(m_CrosshairSchedulerRoutine);
            m_CrosshairSchedulerRoutine = null;
        }

        Debug.Log("<color=magenta>[BossBattleController] 페이즈 3 진입 - 프로비던스 복귀</color>");
        m_ProvidenceBoss.ReturnToArenaForPhase3();
    }

    public void OnBossDead()
    {
        m_CurrentPhase = EBossPhase.Dead;

        EventBus.Publish(new BossDiedEvent
        {
            Boss = m_ProvidenceBoss != null ? m_ProvidenceBoss.gameObject : null,
            Position = m_ProvidenceBoss != null ? (Vector2)m_ProvidenceBoss.transform.position : Vector2.zero
        });
    }
}