using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public bool IsPaused { get; private set; }

    private Coroutine m_HitStopCoroutine;

    public void RequestHitStop(float duration)
    {
        if (IsPaused) return;

        if (m_HitStopCoroutine != null)
            StopCoroutine(m_HitStopCoroutine);

        m_HitStopCoroutine = StartCoroutine(HitStopRoutine(duration));

    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.05f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1.0f;
        m_HitStopCoroutine = null;
    }

    public void SetPause(bool paused)
    {
        if (m_HitStopCoroutine != null)
        {
            StopCoroutine(m_HitStopCoroutine);
            m_HitStopCoroutine = null;
        }

        IsPaused = paused;
        Time.timeScale = paused ? 0.0f : 1.0f;
    }
}
