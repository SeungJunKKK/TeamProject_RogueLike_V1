using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public GameState State { get; private set; }
    public bool IsPaused =>  State == GameState.Paused;

    private Coroutine m_HitStopCoroutine;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null && Instance != this)
            return;

        EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
    }
    public void RequestHitStop(float duration)
    {
        if (State != GameState.Playing) return;

        StopHitStop();

        m_HitStopCoroutine = StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.05f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1.0f;
        m_HitStopCoroutine = null;
    }

    private void StopHitStop()
    {
        if (m_HitStopCoroutine == null)
            return;
        StopCoroutine(m_HitStopCoroutine);
        m_HitStopCoroutine = null;
    }

    private void ChangeState(GameState next)
    {
        if (State == next) return;
        GameState prev = State;
        State = next;

        EventBus.Publish(new GameStateChangedEvent { Previous = prev, Current = State});
    }

    private void OnPlayerDied(PlayerDiedEvent e)
    {
        if (State != GameState.Playing)
            return;
        StopHitStop();
        ChangeState(GameState.GameOver);
        Time.timeScale = 0.0f;
    }

    public void StartGame()
    {
        if (State != GameState.Ready)
            return;

        ChangeState(GameState.Playing);
    }

    public void SetPause(bool paused)
    {
        if (paused)
        {
            if (State != GameState.Playing)
                return;
            StopHitStop();
            ChangeState(GameState.Paused);
            Time.timeScale = 0.0f;
        }
        else
        {
            if (State != GameState.Paused)
                return;

            ChangeState(GameState.Playing);
            Time.timeScale = 1.0f;
        }
    }
}
