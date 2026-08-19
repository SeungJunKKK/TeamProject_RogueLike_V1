using UnityEngine;

public class Teleporter : MonoBehaviour, IInteractable
{
    [SerializeField]
    private float m_ChargeDuration = 90f;
    [SerializeField]
    private string m_NextStageName;
    [SerializeField] private AudioClip m_TeleportOutSound;

    [Header("Teleporter Visuals")]
    [SerializeField] private SpriteRenderer m_SpriteRenderer;
    [SerializeField] private Sprite m_ClearedSprite;


    private float m_ChargeTimer;
    private bool m_IsBossDead; // 실제 보스 사망 여부 (BossDiedEvent 구독으로 업데이트)

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private bool m_DebugBossDead;   // 치트: 가짜 보스 사망 (테스트용)
#endif

    public ETeleporterState State { get; private set; }
    public float ChargeProgress { get; private set; }
    public void Interact(GameObject interactor)
    {
        Debug.Log($"<color=yellow>[Teleporter] 상호작용 발생! 현재 상태: {State}</color>");

        if (State == ETeleporterState.Idle)
        {
            ChangeState(ETeleporterState.Charging);
            Debug.Log("<color=green>[Teleporter] 텔레포터 충전이 시작되었습니다! (90초간 적 스폰 )</color>");
        }
        else if (State == ETeleporterState.Cleared)
        {
            Debug.Log($"<color=magenta>[Teleporter]  다음 스테이지({m_NextStageName})로 이동합니다!</color>");
            if (m_TeleportOutSound != null)
            {
                SoundManager.Instance.PlaySFX(m_TeleportOutSound);
            }
            SceneLoader.Instance.LoadScene(m_NextStageName, true, "Shared");
        }
    }

    public string GetPromptText()
    {
        return State switch { ETeleporterState.Idle => "Activate [Up]", ETeleporterState.Cleared => "Next Stage [Up]", _ => "" };
    }

    public void SetNextStage(string nextStageName)
    {
        m_NextStageName = nextStageName;
    }

    private void Awake()
    {
        EventBus.Subscribe<BossDiedEvent>(OnBossDied);
        EventBus.Subscribe<EliteDiedEvent>(OnEliteDied);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<BossDiedEvent>(OnBossDied);
        EventBus.Unsubscribe<EliteDiedEvent>(OnEliteDied);
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

    private float m_LastLoggedPercent = 0f;
    private void UpdateCharging()
    {
        m_ChargeTimer += Time.deltaTime;
        ChargeProgress = Mathf.Clamp01(m_ChargeTimer / m_ChargeDuration);

        float remainingTime = Mathf.Max(0f, m_ChargeDuration - m_ChargeTimer);
        // 웨이브 스폰은 EnemySpawner가 TeleporterStateChangedEvent를 구독해 처리
        //   Charging: 스폰 강화 + 보스 등장 / WaitingForClear: 신규 스폰 중단
        EventBus.Publish(new TeleporterUpdateEvent
        {
            State = ETeleporterState.Charging,
            ProgressPercent = ChargeProgress * 100f,
            TimeLeft = (int)remainingTime
        });


        float currentPercent = Mathf.Floor(ChargeProgress * 100f / 25f) * 25f;
        if (currentPercent > m_LastLoggedPercent)
        {
            m_LastLoggedPercent = currentPercent;
            Debug.Log($"<color=cyan>[Teleporter] 충전 진행 중... {currentPercent}% 완료 (남은 시간: {m_ChargeDuration - m_ChargeTimer:F1}초)</color>");
        }
        if (m_ChargeTimer >= m_ChargeDuration)
        {
            Debug.Log("<color=orange>[Teleporter] 충전 완료! 남은 적을 소탕하고 보스를 처치하세요!</color>");
            ChangeState(ETeleporterState.WaitingForClear);
        }
    }

    private int m_LastLoggedEnemyCount = -1;
    private void UpdateWaitingForClear()
    {
        // 스포너가 없는 씬(단독 테스트 등)에서는 적 0으로 간주
        int activeEnemies = EnemySpawner.Instance != null ? EnemySpawner.Instance.ActiveEnemyCount : 0;
        bool bossDead = IsBossDead();

        EventBus.Publish(new TeleporterUpdateEvent
        {
            State = ETeleporterState.WaitingForClear,
            RemainingEnemies = activeEnemies,
            IsBossDead = bossDead
        });

        if (activeEnemies != m_LastLoggedEnemyCount)
        {
            m_LastLoggedEnemyCount = activeEnemies;
            Debug.Log($"<color=cyan>[Teleporter] 클리어 대기중 -> 남은 적: {activeEnemies}마리 | 보스 처치 여부: {bossDead}</color>");
        }

        if (activeEnemies == 0 && bossDead)     // AND 두 조건 (RoR1: 전멸 + 보스처치)
        {
            Debug.Log("<color=green>[Teleporter]  모든 조건 클리어 텔레포터가 활성화되었습니다.</color>");
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
        Debug.Log($"<color=yellow>[Teleporter 상태 변경] -> {State}</color>");
        if (State == ETeleporterState.Cleared && m_SpriteRenderer != null && m_ClearedSprite != null)
        {
            m_SpriteRenderer.sprite = m_ClearedSprite;
        }

        EventBus.Publish(new TeleporterStateChangedEvent { State = State });

        EventBus.Publish(new TeleporterUpdateEvent { State = State });
    }

    private void OnBossDied(BossDiedEvent e)
    {
        m_IsBossDead = true;
    }

    private void OnEliteDied(EliteDiedEvent e)
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
