using UnityEngine;
using System.IO;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    public PlayerSaveData CurrentSave { get; private set; }
    public CurrentRunData CurrentRun { get; private set; }
    private string m_SaveFilePath;

    protected override void Awake()
    {
        base.Awake();
        m_SaveFilePath = Path.Combine(Application.persistentDataPath, "SaveData.json");

        CurrentRun = new CurrentRunData(); // 이번 판 데이터 객체 생성
        LoadGame(); // 게임 켜질 때 JSON 세이브 로드

        EventBus.Subscribe<MonsterDiedEvent>(OnMonsterDied);
        EventBus.Subscribe<ItemPickedUpEvent>(OnItemPickedUp);
        EventBus.Subscribe<BossDiedEvent>(OnBossDied);
        EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);

        Debug.Log($"<color=yellow>내 세이브 파일 위치: {m_SaveFilePath}</color>");
    }
    private void OnDestroy()
    {
        EventBus.Unsubscribe<MonsterDiedEvent>(OnMonsterDied);
        EventBus.Unsubscribe<ItemPickedUpEvent>(OnItemPickedUp);
        EventBus.Unsubscribe<BossDiedEvent>(OnBossDied);
        EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnMonsterDied(MonsterDiedEvent e)
    {
        if (CurrentRun != null)
        {
            CurrentRun.Kills += 1;                  // 이번 판 누적 킬 수 1 증가
            CurrentRun.GoldCollected += e.Gold;     // 이번 판 누적 획득 골드 증가
            // Debug.Log($"<color=yellow>[기록장 갱신] 킬: {CurrentRun.Kills} / 누적 골드: {CurrentRun.GoldCollected}</color>");
        }
    }
    private void OnBossDied(BossDiedEvent e)
    {
        if (CurrentRun != null)
        {
            CurrentRun.BossesKilled++;
            Debug.Log($"<color=orange>[기록장 갱신] 보스 처치! 누적 보스 킬: {CurrentRun.BossesKilled}</color>");
        }
    }

    private void OnPlayerDied(PlayerDiedEvent e)
    {
        if (CurrentRun == null) return;

        CurrentRun.PlayTime = DifficultyManager.Instance.ElapsedSeconds;

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null && player.Stats != null)
        {
            CurrentRun.CurrentLevel = player.Stats.CurrentLevel;
        }

        //// 최종 점수 계산
        int finalScore = CurrentRun.CalculateTotalScore();

        // 3. 이번 판 기록과 기존 최고 기록을 비교하여 더 큰 값(Mathf.Max)을 저장
        CurrentSave.HighestTotalScore = Mathf.Max(CurrentSave.HighestTotalScore, finalScore);
        CurrentSave.LongestSurviveTime = Mathf.Max(CurrentSave.LongestSurviveTime, CurrentRun.PlayTime);
        CurrentSave.MaxLevelReached = Mathf.Max(CurrentSave.MaxLevelReached, CurrentRun.CurrentLevel);
        CurrentSave.MaxKillsInOneRun = Mathf.Max(CurrentSave.MaxKillsInOneRun, CurrentRun.Kills);
        CurrentSave.MaxBossesKilled = Mathf.Max(CurrentSave.MaxBossesKilled, CurrentRun.BossesKilled);
        CurrentSave.MaxGoldCollected = Mathf.Max(CurrentSave.MaxGoldCollected, CurrentRun.GoldCollected);
     
        SaveGame();

        Debug.Log($"<color=cyan>[SaveLoadManager] 게임 오버! 최고 점수: {CurrentSave.HighestTotalScore} 저장 완료!</color>");
    }

private void OnItemPickedUp(ItemPickedUpEvent e)
    {
        if (CurrentRun != null)
        {
            // 1) 이번 판 누적 아이템 획득 수 1 증가
            CurrentRun.ItemsCollected++;

            // 2) 도감(SaveData)에 아직 등록되지 않은 '새로운 아이템'인지 검사
            if (!CurrentSave.UnlockedItems.Contains(e.ItemName))
            {
                // 영구 보존용 세이브 파일에 아이템 이름표(ID) 기록
                CurrentSave.UnlockedItems.Add(e.ItemName);

                // 결과 창 우측(Unlocked)에 띄워주기 위해 '이번 판 해금 리스트'에도 기록
                CurrentRun.NewlyUnlockedItems.Add(e.ItemName);

                SaveGame();
                Debug.Log($"<color=magenta>[도감 등록] 신규 아이템을 해금했습니다!: {e.ItemName}</color>");
            }
        }
    }

    public void LoadGame()
    {
        if (File.Exists(m_SaveFilePath))
        {
            string json = File.ReadAllText(m_SaveFilePath);
            CurrentSave = JsonUtility.FromJson<PlayerSaveData>(json);
        }
        else
        {
            CurrentSave = new PlayerSaveData(); // 세이브가 없으면 새로 생성
        }
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(CurrentSave, true);
        Debug.Log($"<color=white>[저장 직전 데이터 확인]\n{json}</color>");

        File.WriteAllText(m_SaveFilePath, json);
        Debug.Log("<color=cyan>[SaveLoadManager] JSON 데이터 저장 완료!</color>");
    }
}