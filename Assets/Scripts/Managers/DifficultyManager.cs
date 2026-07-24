using UnityEngine;

public class DifficultyManager : Singleton<DifficultyManager>
{
    private const float k_BaseCoeff = 1.0f;
    private const float k_TimeFactor = 0.0506f;
    private const float k_DifficultyValue = 2f;

    [SerializeField]
    private float[] m_LevelThresholds = { 2f, 3f, 4f, 5f, 6f, 8f, 10f, 13f, 16f }; // 임시 경계값. 추후 밸런싱 예정

    private float m_ElapsedSeconds;

    public float Coefficient { get; private set; }
    public EDifficultyLevel Level { get; private set; }

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
    /// 몬스터 체력 배율. 스폰 시 최대 체력에 곱한다.
    /// </summary>
    /// <returns>현재 난이도 계수</returns>
    public float GetHPMultiplier()
    {
        return Coefficient;
    }

    /// <summary>
    /// 골드 드랍 배율. 몬스터가 사망 이벤트 발행 전에 곱한다.
    /// </summary>
    /// <returns>현재 난이도 계수</returns>
    public float GetGoldMultiplier()
    {
        return Coefficient;
    }

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
