using UnityEngine;

public class DifficultyManager : Singleton<DifficultyManager>
{
    private const float k_BaseCoeff = 1.0f;
    private const float k_TimeFactor = 0.0506f;
    private const float k_DifficultyValue = 2f;
    private const float k_LevelDivisor = 0.33f;

    [SerializeField]
    private float[] m_LevelThresholds = { 2f, 3f, 4f, 5f, 6f, 8f, 10f, 13f, 16f }; // 임시 경계값. 추후 밸런싱 예정

    private float m_ElapsedSeconds;

    public float Coefficient { get; private set; }
    public EDifficultyLevel Level { get; private set; }

    /// <summary>
    /// 현재 적 레벨. 몬스터 스탯 계산의 기준 (최소 1).
    /// 최종 스탯 = 기본값 + 레벨당증가분 × (Level - 1)
    /// </summary>
    public int EnemyLevel
    {
        get
        {
            int level = 1 + Mathf.FloorToInt((Coefficient - k_BaseCoeff) / k_LevelDivisor);
            return Mathf.Clamp(level, 1, 99);
        }
    }
    public float ElapsedSeconds => m_ElapsedSeconds;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null && Instance != this)
        {
            return;
        }

        EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
    }

    /// <summary>
    /// 골드 드랍 배율. 몬스터가 사망 이벤트 발행 전에 곱한다.
    /// </summary>
    /// <returns>현재 난이도 계수</returns>
    public float GetGoldMultiplier()
    {
        return 2f * Coefficient;      // RoR2: gold = 2 × coeff × rewardCoefficient
    }

    /// <summary>
    /// 레벨 스케일링이 적용된 스탯값을 반환한다.
    /// </summary>
    /// <param name="baseValue">1레벨 기준 기본값</param>
    /// <param name="perLevel">레벨당 증가량</param>
    public float GetScaledStat(float baseValue, float perLevel)
    {
        return baseValue + perLevel * (EnemyLevel - 1);
    }

    /// <summary>
    /// 시간별 게이지 상승하는 프로그레스바용 값을 반환하는 헬퍼 함수
    /// </summary>
    /// <returns>0~1, 다음 단계까지의 진행도</returns>
    public float GetLevelProgress()
    {
        int index = (int)Level;
        if (index >= m_LevelThresholds.Length)
        {
            return 1f;// 최고 단계
        }
        float prev = index == 0 ? k_BaseCoeff : m_LevelThresholds[index - 1];
        float next = m_LevelThresholds[index];
        return Mathf.Clamp01((Coefficient - prev) / (next - prev));
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// [치트] 시간을 증가시킴.
    /// </summary>
    public void AddDebugTime(float seconds)
    {
        m_ElapsedSeconds += seconds;

        // 시간을 더한 직후 Coefficient 및 Level 즉시 계산 - 치트 직후 난이도 UI 갱신을 위해 추가했음
        Coefficient = k_BaseCoeff + (m_ElapsedSeconds / 60f) * k_TimeFactor * k_DifficultyValue;
        Level = CalculateLevel(Coefficient);
    }
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// [치트] 난이도 레벨을 강제로 1단계 올리고, 플레이어에게도 1레벨 업 상당의 경험치를 지급.
    /// </summary>
    public void AddDebugLevel()
    {
        // 난이도 레벨 강제 상승
        int nextLevelIndex = (int)Level + 1;
        int maxLevelIndex = System.Enum.GetValues(typeof(EDifficultyLevel)).Length - 1;

        if (nextLevelIndex > maxLevelIndex)
        {
            nextLevelIndex = maxLevelIndex;
        }

        Level = (EDifficultyLevel)nextLevelIndex;

        // 계수 및 레벨업 이벤트 발행 (우측 상단 난이도 UI)
        Coefficient = k_BaseCoeff + (m_ElapsedSeconds / 60f) * k_TimeFactor * k_DifficultyValue;
        EventBus.Publish(new DifficultyChangedEvent { Level = Level, Coefficient = Coefficient });

        // 플레이어 경험치/레벨 반영 (하단 HUD_Bottom UI)
        var player = FindAnyObjectByType<PlayerController>();
        if (player != null && player.Stats != null)
        {
            float requiredExp = player.Stats.GetRequiredExp(player.Stats.CurrentLevel);
            player.Stats.AddExp(requiredExp);
        }

        Debug.Log($"[Cheat] 난이도 강제 상승: {Level} & 플레이어 경험치 지급 완료");
    }
#endif

    private void Update()
    {
        if (GameManager.Instance.State != EGameState.Playing)
        {
            return;
        }
        m_ElapsedSeconds += Time.deltaTime;
        Coefficient = k_BaseCoeff + (m_ElapsedSeconds / 60f) * k_TimeFactor * k_DifficultyValue;

        EDifficultyLevel newLevel = CalculateLevel(Coefficient);

        if (newLevel != Level)
        {
            Level = newLevel;
            EventBus.Publish(new DifficultyChangedEvent { Level = Level, Coefficient = Coefficient });
        }
    }

    private EDifficultyLevel CalculateLevel(float coeff)
    {
        int index = 0;

        for (int i = 0; i < m_LevelThresholds.Length; ++i)
        {
            if (coeff >= m_LevelThresholds[i])
            {
                ++index;
            }
            else
            {
                break;
            }
        }

        return (EDifficultyLevel)index;
    }

    private void OnGameStateChanged(GameStateChangedEvent e)
    {
        // 새 게임 시작했을 때만 리셋
        if (e.Current == EGameState.Playing && e.Previous == EGameState.Ready)
        {
            m_ElapsedSeconds = 0;
            Coefficient = k_BaseCoeff;
            Level = EDifficultyLevel.VeryEasy;
        }
    }
}
