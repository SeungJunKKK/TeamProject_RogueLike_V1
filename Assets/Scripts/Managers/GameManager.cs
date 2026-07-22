using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public bool IsPaused => State == EGameState.Paused;
    private Coroutine m_HitStopCoroutine;

    public EGameState State { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null && Instance != this)
        {
            return;
        }

        EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
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
}
