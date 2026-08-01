using UnityEngine;

public class Teleporter : MonoBehaviour, IInteractable
{
    [SerializeField]
    private float m_ChargeDuration = 90f;
    [SerializeField]
    private string m_NextStageName;

    private float m_ChargeTimer;
    private bool m_IsBossDead; // 실제 보스 사망 여부 (BossDiedEvent 구독으로 업데이트)

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private bool m_DebugBossDead;   // 치트: 가짜 보스 사망 (테스트용)
#endif

    public ETeleporterState State { get; private set; }
    public float ChargeProgress { get; private set; }
    public void Interact(GameObject interactor)
    {
        if (State == ETeleporterState.Idle)
        {
            ChangeState(ETeleporterState.Charging);
        }
        else if (State == ETeleporterState.Cleared)
        {
            SceneLoader.Instance.LoadScene(m_NextStageName);
        }
    }

    private void Awake()
    {
        EventBus.Subscribe<BossDiedEvent>(OnBossDied);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<BossDiedEvent>(OnBossDied);
    }

    private void Update()
    {
        switch (State)
        {
            case ETeleporterState.Charging:
                UpdateCharging();
                break;

            case ETeleporterState.WaitingForClear:
                UpdateWaitingForClear();
                break;

            default:
                break;      // Idle, Cleared는 매프레임 할 일 없음
        }
    }

    private void UpdateCharging()
    {
        m_ChargeTimer += Time.deltaTime;
        ChargeProgress = Mathf.Clamp01(m_ChargeTimer / m_ChargeDuration);

        // 웨이브 스폰은 EnemySpawner가 TeleporterStateChangedEvent를 구독해 처리
        //   Charging: 스폰 강화 + 보스 등장 / WaitingForClear: 신규 스폰 중단

        if (m_ChargeTimer >= m_ChargeDuration)
        {
            ChangeState(ETeleporterState.WaitingForClear);
        }
    }

    private void UpdateWaitingForClear()
    {
        // 스포너가 없는 씬(단독 테스트 등)에서는 적 0으로 간주
        int activeEnemies = EnemySpawner.Instance != null ? EnemySpawner.Instance.ActiveEnemyCount : 0;
        bool bossDead = IsBossDead();

        if (activeEnemies == 0 && bossDead)     // AND 두 조건 (RoR1: 전멸 + 보스처치)
        {
            ChangeState(ETeleporterState.Cleared);
        }
    }

    private bool IsBossDead()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        return m_DebugBossDead || m_IsBossDead;                 // 치트값
#else
        return m_IsBossDead;   // TODO: BossDiedEvent 구독으로 실제 상태 반영
#endif
    }

    private void ChangeState(ETeleporterState next)
    {
        if (State == next)
        {
            return;
        }

        State = next;
        EventBus.Publish(new TeleporterStateChangedEvent { State = State });
    }

    private void OnBossDied(BossDiedEvent e)
    {
        m_IsBossDead = true;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public void DebugForceChargeComplete()      // 치트: 충전 즉시 완료
    {
        m_ChargeTimer = m_ChargeDuration;
    }

    public void DebugKillBoss()                 // 치트: 가짜 보스 처치
    {
        m_DebugBossDead = true;
    }
#endif
}
