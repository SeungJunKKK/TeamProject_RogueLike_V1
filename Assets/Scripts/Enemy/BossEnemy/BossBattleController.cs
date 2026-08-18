using System.Linq.Expressions;
using UnityEngine;

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

    // 💡 이제 수동으로 등록 안 해도 코드가 알아서 좌우 위치를 잡습니다 (선택 사항으로 남겨둠)
    [SerializeField] private Transform m_SpawnPointLeft;
    [SerializeField] private Transform m_SpawnPointRight;

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

        // 💡 1. 수동 지정된 포인트가 있으면 그거 쓰고, 없으면 현재 보스/컨트롤러 위치 기준으로 좌우 자동 계산 (간격은 필요시 숫자 조절 가능)
        Vector3 centerPos = m_ProvidenceBoss != null ? m_ProvidenceBoss.transform.position : transform.position;

        Vector3 leftPos = m_SpawnPointLeft != null ? m_SpawnPointLeft.position : centerPos + new Vector3(-7f, 0f, 0f);
        Vector3 rightPos = m_SpawnPointRight != null ? m_SpawnPointRight.position : centerPos + new Vector3(7f, 0f, 0f);

        SpawnWurmAt(m_RedWurmPrefab, leftPos);
        SpawnWurmAt(m_BlueWurmPrefab, rightPos);
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