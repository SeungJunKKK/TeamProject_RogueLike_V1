using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioMixer m_Mixer;
    private const string k_MasterParam = "MasterVolume";
    private const string k_BGMParam = "BGMVolume";
    private const string k_SFXParam = "SFXVolume";

    // playerprefs 용
    private const string k_MasterVolumeKey = "MasterVolume";
    private const string k_BGMVolumeKey = "BGMVolume";
    private const string k_SFXVolumeKey = "SFXVolume";
    private const float k_FadeDuration = 1.0f;
    private Coroutine m_PlaylistCoroutine;

    private AudioSource m_SFXSource;
    private AudioSource m_BGMSource;

    private Coroutine m_FadeOutCoroutine;

    [Header("Difficulty SFX")]
    public AudioClip DifficultyUpSound;

    [Header("Playlists")]
    public AudioClip LobbyBgm;
    public AudioClip[] MainStagePlaylist;
    public AudioClip[] BossStagePlaylist;

    [Header("Audio Mixers")]
    public AudioMixerGroup bgmMixerGroup;
    public AudioMixerGroup sfxMixerGroup;

    private void Start()
    {
        SetMasterVolume(PlayerPrefs.GetFloat(k_MasterVolumeKey, 1f));
        SetBGMVolume(PlayerPrefs.GetFloat(k_BGMVolumeKey, 1f));
        SetSFXVolume(PlayerPrefs.GetFloat(k_SFXVolumeKey, 1f));
    }

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null && Instance != this)
        {
            return;
        }

        m_SFXSource = gameObject.AddComponent<AudioSource>();
        m_SFXSource.playOnAwake = false;
        if (sfxMixerGroup != null)
        {
            m_SFXSource.outputAudioMixerGroup = sfxMixerGroup;
        }

        m_BGMSource = gameObject.AddComponent<AudioSource>();
        m_BGMSource.loop = true;
        m_BGMSource.playOnAwake = false;
        if (bgmMixerGroup != null)
        {
            m_BGMSource.outputAudioMixerGroup = bgmMixerGroup;
        }

        EventBus.Subscribe<SceneLoadStartedEvent>(OnSceneLoadStarted);
        EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        EventBus.Subscribe<TeleporterStateChangedEvent>(OnTeleporterStateChanged);
        EventBus.Subscribe<DifficultyChangedEvent>(OnDifficultyChanged);
        EventBus.Subscribe<SceneLoadCompletedEvent>(OnSceneLoadCompleted);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<SceneLoadStartedEvent>(OnSceneLoadStarted);
        EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        EventBus.Unsubscribe<TeleporterStateChangedEvent>(OnTeleporterStateChanged);
        EventBus.Unsubscribe<DifficultyChangedEvent>(OnDifficultyChanged);
        EventBus.Unsubscribe<SceneLoadCompletedEvent>(OnSceneLoadCompleted);
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

        m_BGMSource.loop = true;
        m_BGMSource.clip = clip;
        m_BGMSource.Play();
    }

    /// <summary>
    ///  여러 곡을 순서대로 바로 컷(Cut)하여 무한 반복 재생하는 함수
    /// </summary>
    public void PlayBGMList(AudioClip[] playlist)
    {
        if (playlist == null || playlist.Length == 0) return;

        CancelFade();

        if (m_PlaylistCoroutine != null)
        {
            StopCoroutine(m_PlaylistCoroutine);
        }

        m_PlaylistCoroutine = StartCoroutine(PlaylistRoutine(playlist));
    }

    private IEnumerator PlaylistRoutine(AudioClip[] playlist)
    {
        // 리스트를 재생할 땐 한 곡이 끝나고 다음 곡으로 넘어가야 하니 loop를 끈다.
        m_BGMSource.loop = false;
        int currentIndex = 0;

        while (true)
        {
            m_BGMSource.clip = playlist[currentIndex];
            m_BGMSource.Play();
            yield return new WaitForSecondsRealtime(m_BGMSource.clip.length);
            currentIndex = (currentIndex + 1) % playlist.Length;
        }
    }

    private void OnGameStateChanged(GameStateChangedEvent e)
    {
        if (e.Current == EGameState.Playing && e.Previous == EGameState.Ready)
        {
            PlayBGMList(MainStagePlaylist);
        }
    }
    private void OnSceneLoadCompleted(SceneLoadCompletedEvent e)
    {
        if (MainStagePlaylist != null && MainStagePlaylist.Length > 0)
        {
            PlayBGMList(MainStagePlaylist);
        }
    }

    private void OnTeleporterStateChanged(TeleporterStateChangedEvent e)
    {
        if (e.State == ETeleporterState.Charging)
        {
            PlayBGMList(BossStagePlaylist);
        }
        else if (e.State == ETeleporterState.Cleared)
        {
            PlayBGMList(MainStagePlaylist);
        }
    }

    private void OnDifficultyChanged(DifficultyChangedEvent e)
    {
        if (DifficultyUpSound != null)
        {
            PlaySFX(DifficultyUpSound);
        }
    }

    public void SetMasterVolume(float volume)   => ApplyVolume(k_MasterParam, k_MasterVolumeKey, volume);
    public void SetSFXVolume(float volume)      => ApplyVolume(k_SFXParam, k_SFXVolumeKey, volume);
    public void SetBGMVolume(float volume)      => ApplyVolume(k_BGMParam, k_BGMVolumeKey, volume);

    private void ApplyVolume(string param, string prefsKey, float linear)
    {
        linear = Mathf.Clamp01(linear);
        float dB = (linear <= 0.0001f) ? -80f : Mathf.Log10(linear) * 20f; // 0 -> -무한대 방지
        m_Mixer.SetFloat(param, dB);
        PlayerPrefs.SetFloat(prefsKey, linear);
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
        m_BGMSource.volume = 1f;  // 다음 BGM을 위해 볼륨을 원래대로 복원
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
        m_BGMSource.volume = 1f; // 볼륨을 원래대로 복원
    }
}
