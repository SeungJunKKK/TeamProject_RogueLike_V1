using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private Coroutine m_HitStopCoroutine;

    public EGameState State { get; private set; }
    public int Gold { get; private set; }
    public bool IsPaused => State == EGameState.Paused;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null && Instance != this)
        {
            return;
        }

        EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        EventBus.Subscribe<MonsterDiedEvent>(OnMonsterDied);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        EventBus.Unsubscribe<MonsterDiedEvent>(OnMonsterDied);
    }
    public void RequestHitStop(float duration)
    {
        if (State != EGameState.Playing)
        {
            return;
        }

        CancelHitStop();

        m_HitStopCoroutine = StartCoroutine(HitStopRoutine(duration));
    }

    public void StartGame()
    {
        if (State != EGameState.Ready)
        {
            return;
        }
        Gold = 0;
        ChangeState(EGameState.Playing);
    }

    public void SetPause(bool paused)
    {
        if (paused)
        {
            if (State != EGameState.Playing)
            {
                return;
            }
            CancelHitStop();
            ChangeState(EGameState.Paused);
            Time.timeScale = 0.0f;
        }
        else
        {
            if (State != EGameState.Paused)
            {
                return;
            }

            ChangeState(EGameState.Playing);
            Time.timeScale = 1.0f;
        }
    }

    /// <summary>
    /// 골드를 추가하고 GoldChangedEvent를 발행. 몬스터 처치 등에서 호출
    /// </summary>
    /// <param name="amount"></param>
    public void AddGold(int amount)
    {
        Gold += amount;
        EventBus.Publish(new GoldChangedEvent { Current = Gold, Delta = amount });
    }

    public bool TrySpendGold(int amount)
    {
        if (Gold < amount)
        {
            return false;
        }
        Gold -= amount;
        EventBus.Publish(new GoldChangedEvent { Current = Gold, Delta = -amount });
        return true;
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.05f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1.0f;
        m_HitStopCoroutine = null;
    }

    private void CancelHitStop()
    {
        if (m_HitStopCoroutine == null)
        {
            return;
        }
        StopCoroutine(m_HitStopCoroutine);
        m_HitStopCoroutine = null;
    }

    private void ChangeState(EGameState next)
    {
        if (State == next)
        {
            return;
        }
        EGameState prev = State;
        State = next;

        EventBus.Publish(new GameStateChangedEvent { Previous = prev, Current = State });
    }

    private void OnPlayerDied(PlayerDiedEvent e)
    {
        if (State != EGameState.Playing)
        {
            return;
        }
        CancelHitStop();
        ChangeState(EGameState.GameOver);
        Time.timeScale = 0.0f;
    }

    private void OnMonsterDied(MonsterDiedEvent e)
    {
        AddGold(e.Gold);
    }
}
