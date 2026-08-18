using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [Header("Teleporter Audio")]
    [SerializeField] private AudioClip m_TeleportInSound;

    [Header("Selected Player Settings")]
    public GameObject SelectedPlayerPrefab;

    private Coroutine m_HitStopCoroutine;
    public GameObject CurrentPlayer { get; private set; }
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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        EventBus.Unsubscribe<MonsterDiedEvent>(OnMonsterDied);
        SceneManager.sceneLoaded -= OnSceneLoaded;
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

    /// <summary>
    /// 선택창에서 플레이어를 선택하면 호출되는 메서드. SelectedPlayerPrefab을 설정하고 로그를 출력
    /// </summary>
    /// <param name="playerPrefab"></param>
    public void SetSelectedPlayer(GameObject playerPrefab)
    {
        SelectedPlayerPrefab = playerPrefab;
        Debug.Log($"<color=green>[GameManager] 플레이어 선택 완료: {playerPrefab.name}</color>");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //  인게임 스테이지일 때
        if (scene.name != "TitleScene" && scene.name != "PlayerSelectScene")
        {
            // 플레이어 존재 여부에 따라 스폰 또는 이동 처리
            if (CurrentPlayer != null)
            {
                GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
                CurrentPlayer.transform.position = spawnPoint != null ? spawnPoint.transform.position : Vector3.zero;

                Debug.Log($"<color=cyan>[GameManager]플레이어를 다음 스테이지({scene.name})로 이동</color>");
            }
            else
            {
                SpawnPlayer();
                State = EGameState.Ready;
                StartGame();
            }

            if (m_TeleportInSound != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(m_TeleportInSound);
            }
        }
        else
        {
            // 다음 새 게임을 위해 기존에 살아남아있던 플레이어 정보를 완전히 초기화합니다.
            if (CurrentPlayer != null)
            {
                Destroy(CurrentPlayer);
                CurrentPlayer = null;
                State = EGameState.Ready;
            }
        }
    }

    private void SpawnPlayer()
    {
        if (SelectedPlayerPrefab == null)
        {
            Debug.LogError("[GameManager] 선택된 플레이어 프리팹이 없습니다! SelectScene에서 제대로 전달되었는지 확인하세요.");
            return;
        }

        // 씬에서 "PlayerSpawnPoint"라는 태그를 가진 빈 오브젝트를 찾아 그 위치에 스폰
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.transform.position : Vector3.zero;

        CurrentPlayer = Instantiate(SelectedPlayerPrefab, spawnPosition, Quaternion.identity);
        DontDestroyOnLoad(CurrentPlayer);

        SaveLoadManager.Instance.CurrentRun.ResetRun(SelectedPlayerPrefab.name);
        Debug.Log($"<color=cyan>[GameManager] 플레이어 스폰 완료 위치: {spawnPosition}</color>");
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

        Object.FindAnyObjectByType<GameResultUI>(FindObjectsInactive.Include).DisplayResult(false);
    }

    private void OnMonsterDied(MonsterDiedEvent e)
    {
        AddGold(e.Gold);

        if (CurrentPlayer != null)
        {
            if (CurrentPlayer.TryGetComponent(out PlayerController player))
            {
                if (player.Stats != null)
                {
                    player.Stats.AddExp(e.Exp);
                }
            }
        }

    }
}
