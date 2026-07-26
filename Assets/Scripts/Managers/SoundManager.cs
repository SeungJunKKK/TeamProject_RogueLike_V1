using System.Collections;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    private const string k_SFXVolumeKey = "SFXVolume";
    private const string k_BGMVolumeKey = "BGMVolume";
    private const float k_FadeDuration = 1.0f;

    private AudioSource m_SFXSource;
    private AudioSource m_BGMSource;

    private Coroutine m_FadeOutCoroutine;
    protected override void Awake()
    {
        base.Awake();

        if (Instance != null && Instance != this)
        {
            return;
        }

        m_SFXSource = gameObject.AddComponent<AudioSource>();
        m_SFXSource.playOnAwake = false;

        m_BGMSource = gameObject.AddComponent<AudioSource>();
        m_BGMSource.loop = true;
        m_BGMSource.playOnAwake = false;

        m_SFXSource.volume = PlayerPrefs.GetFloat(k_SFXVolumeKey, 1f);
        m_BGMSource.volume = PlayerPrefs.GetFloat(k_BGMVolumeKey, 1f);

        EventBus.Subscribe<SceneLoadStartedEvent>(OnSceneLoadStarted);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<SceneLoadStartedEvent>(OnSceneLoadStarted);
    }

    /// <summary>
    /// 효과음 재생. 연속 호출 해도 소리가 겹쳐서 재생
    /// </summary>
    /// <param name="clip"></param>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SoundManager] PlaySFX called with null clip.");
            return;
        }
        m_SFXSource.PlayOneShot(clip);
    }

    /// <summary>
    /// 배경음악 재생. 연속 호출 시 이전 곡은 중단되고 새 곡이 재생됨
    /// </summary>
    /// <param name="clip"></param>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SoundManager] PlayBGM called with null clip.");
            return;
        }

        CancelFade();   // 진행중임 fade 가 새 곡을 죽이지 못하게

        if (clip == m_BGMSource.clip && m_BGMSource.isPlaying)
        {
            Debug.LogWarning("[SoundManager] PlayBGM called with the same clip that is already playing.");
            return; // Already playing this BGM
        }

        m_BGMSource.clip = clip;
        m_BGMSource.Play();
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        m_SFXSource.volume = volume;
        PlayerPrefs.SetFloat(k_SFXVolumeKey, volume);
    }

    public void SetBGMVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        m_BGMSource.volume = volume;
        PlayerPrefs.SetFloat(k_BGMVolumeKey, volume);
    }

    private void OnSceneLoadStarted(SceneLoadStartedEvent e)
    {
        CancelFade();
        m_FadeOutCoroutine = StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float startVolume = m_BGMSource.volume; // 복원용 기억
        float elapsed = 0f;

        while (elapsed < k_FadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;  // 일시 정지 중에도 페이드가 끝나야 하기 때문
            m_BGMSource.volume = Mathf.Lerp(startVolume, 0.0f, elapsed / k_FadeDuration);
            yield return null;
        }

        m_BGMSource.Stop();
        m_BGMSource.volume = PlayerPrefs.GetFloat(k_BGMVolumeKey, 1f);  // 다음 BGM을 위해 볼륨을 원래대로 복원
        m_FadeOutCoroutine = null;
    }

    private void CancelFade()
    {
        if (m_FadeOutCoroutine == null)
        {
            return;
        }

        StopCoroutine(m_FadeOutCoroutine);
        m_FadeOutCoroutine = null;
        m_BGMSource.volume = PlayerPrefs.GetFloat(k_BGMVolumeKey, 1f); // 볼륨을 원래대로 복원
    }
}
