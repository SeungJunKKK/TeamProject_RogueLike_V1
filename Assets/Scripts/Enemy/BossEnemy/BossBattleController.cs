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
    public ProvidenceAI ProvidenceBoss;
    public Transform Player;

    [Header("Battle State")]
    [SerializeField] private EBossPhase m_CurrentPhase = EBossPhase.None;

    // 💡 중복 실행 방지용 플래그
    private bool m_IsBattleStarted = false;

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
        if (ProvidenceBoss != null && Player != null)
        {
            // 💡 보스가 씬에 미리 배치되어 있다면 풀매니저를 안 거치므로 여기서 수동 초기화!
            ProvidenceBoss.OnSpawn();

            StartBattle();
        }
    }

    public void StartBattle()
    {
        // 💡 트리거 중복 터짐 방지
        if (m_IsBattleStarted) return;
        m_IsBattleStarted = true;

        m_CurrentPhase = EBossPhase.Intro;
        Debug.Log("<color=magenta>[BossBattleController] 보스전 시작 - Providence 등장 (Intro)</color>");

        ProvidenceBoss.SetTarget(Player);
        ProvidenceBoss.StartIntro();
    }

    public void OnIntroFinished()
    {
        m_CurrentPhase = EBossPhase.Phase1;
        ProvidenceBoss.EnterPhase1();
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
        ProvidenceBoss.ExitArenaForPhase2();
        // TODO: Gilded Wurm 소환
    }

    public void OnBossDead()
    {
        m_CurrentPhase = EBossPhase.Dead;
    }
}